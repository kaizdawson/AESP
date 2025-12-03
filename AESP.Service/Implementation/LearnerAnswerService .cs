using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;

namespace AESP.Service.Implementation
{
    public class LearnerAnswerService : ILearnerAnswerService
    {
        private readonly IGenericRepository<LearnerAnswer> _answerRepo;
        private readonly IGenericRepository<Question> _questionRepo;
        private readonly IGenericRepository<LearningPathExercise> _lpExerciseRepo;
        private readonly IGenericRepository<LearningPathChapter> _lpChapterRepo;
        private readonly IGenericRepository<LearningPathCourse> _lpCourseRepo;
        private readonly IGenericRepository<LearningPathQuestion> _lpQuestionRepo;
        private readonly IUnitOfWork _unitOfWork;

        public LearnerAnswerService(
            IGenericRepository<LearnerAnswer> answerRepo,
            IGenericRepository<Question> questionRepo,
            IGenericRepository<LearningPathExercise> lpExerciseRepo,
            IGenericRepository<LearningPathChapter> lpChapterRepo,
            IGenericRepository<LearningPathCourse> lpCourseRepo,
            IGenericRepository<LearningPathQuestion> lpQuestionRepo,
            IUnitOfWork unitOfWork)
        {
            _answerRepo = answerRepo;
            _questionRepo = questionRepo;
            _lpExerciseRepo = lpExerciseRepo;
            _lpChapterRepo = lpChapterRepo;
            _lpCourseRepo = lpCourseRepo;
            _lpQuestionRepo = lpQuestionRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> SubmitAnswerAsync(
       Guid learnerProfileId,
       Guid learningPathQuestionId,
       SubmitLearnerAnswerDTO dto)
        {
            try
            {
                // 1️⃣ Load LPQuestion
                var lpQuestion = await _lpQuestionRepo.AsQueryable()
                    .Include(q => q.Question)
                    .Include(q => q.LearningPathExercise)
                        .ThenInclude(e => e.LearningPathChapter)
                                    .ThenInclude(ch => ch.LearningPathCourse)  // ⭐ BẮT BUỘC

                    .FirstOrDefaultAsync(q => q.LearningPathQuestionId == learningPathQuestionId);

                if (lpQuestion == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy câu hỏi trong LearningPath.");

                var question = lpQuestion.Question;
                var lpExercise = lpQuestion.LearningPathExercise;
                var exerciseId = lpExercise.ExerciseId;

                // 2️⃣ Insert learner answer
                var answer = new LearnerAnswer
                {
                    LearnerAnswerId = Guid.NewGuid(),
                    LearnerProfileId = learnerProfileId,
                    LearningPathQuestionId = learningPathQuestionId,
                    AudioRecordingUrl = dto.AudioRecordingUrl,
                    TranscribedText = dto.TranscribedText,
                    ScoreForVoice = dto.ScoreForVoice,
                    ExplainTheWrongForVoiceAI = dto.ExplainTheWrongForVoiceAI,
                    Status = "Submitted",
                    SubmittedAt = DateTime.UtcNow,
                    IsNeededReviewed = false,
                    NumberofReview = 0
                };

                await _answerRepo.Insert(answer);
                await _unitOfWork.SaveChangeAsync();

                // 3️⃣ Update LPQuestion
                lpQuestion.Status = "Completed";
                lpQuestion.Score = dto.ScoreForVoice;
                lpQuestion.NumberOfRetake += 1;
                await _lpQuestionRepo.Update(lpQuestion);
                await _unitOfWork.SaveChangeAsync();

                // 4️⃣ Update LPExercise
                var allLpQuestions = await _lpQuestionRepo.AsQueryable()
                    .Where(q => q.LearningPathExerciseId == lpExercise.LearningPathExerciseId)
                    .ToListAsync();

                var completed = allLpQuestions.Count(q => q.Status == "Completed");

                // ✅ TÍNH ĐIỂM TRUNG BÌNH EXERCISE
                lpExercise.ScoreAchieved = allLpQuestions.Any()
                    ? allLpQuestions.Average(q => q.Score)
                    : 0;

                // ✅ MẶC ĐỊNH: CHƯA ĐỦ CÂU → IN PROGRESS
                lpExercise.Status = "InProgress";

                await _lpExerciseRepo.Update(lpExercise);
                await _unitOfWork.SaveChangeAsync();

                // ✅ CHỈ KHI LÀM XONG TẤT CẢ CÂU → MỚI XÉT COMPLETED QUA RULE 50%
                if (completed == lpExercise.NumberOfQuestion)
                {
                    // ✅ ÁP DỤNG RULE 50% Ở ĐÂY
                    if (lpExercise.ScoreAchieved < 50)
                    {
                        // ❌ Dưới 50 → BẮT BUỘC GIỮ InProgress
                        lpExercise.Status = "InProgress";
                        await _lpExerciseRepo.Update(lpExercise);
                        await _unitOfWork.SaveChangeAsync();

                        return Fail(
                            BusinessCode.INVALID_ACTION,
                            $"Điểm trung bình hiện tại là {Math.Round(lpExercise.ScoreAchieved, 2)}%. Cần tối thiểu 50% để hoàn thành bài tập."
                        );
                    }

                    // ✅ >= 50 → CHO PHÉP COMPLETED
                    lpExercise.Status = "Completed";
                    await _lpExerciseRepo.Update(lpExercise);
                    await _unitOfWork.SaveChangeAsync();
                }



                // ⭐⭐⭐ 4.1️⃣ UPDATE CHAPTER ⭐⭐⭐
                var lpChapter = lpExercise.LearningPathChapter;

                var chapterExercises = await _lpExerciseRepo.AsQueryable()
                    .Where(e => e.LearningPathChapterId == lpChapter.LearningPathChapterId)
                    .ToListAsync();

                var chapterCompletedCount = chapterExercises.Count(e => e.Status == "Completed");

                lpChapter.Status = chapterCompletedCount == lpChapter.NumberOfModule ? "Completed" : "InProgress";

                lpChapter.Progress = lpChapter.NumberOfModule == 0
                    ? 0
                    : (double)chapterCompletedCount / lpChapter.NumberOfModule * 100;

                await _lpChapterRepo.Update(lpChapter);
                await _unitOfWork.SaveChangeAsync();


                // ⭐⭐⭐ 4.2️⃣ UPDATE COURSE ⭐⭐⭐
                var lpCourse = lpChapter.LearningPathCourse;

                var courseChapters = await _lpChapterRepo.AsQueryable()
                    .Where(c => c.LearningPathCourseId == lpCourse.LearningPathCourseId)
                    .ToListAsync();

                var courseCompletedCount = courseChapters.Count(c => c.Status == "Completed");

                lpCourse.Status = courseCompletedCount == lpCourse.NumberOfChapter ? "Completed" : "InProgress";

                lpCourse.Progress = lpCourse.NumberOfChapter == 0
                    ? 0
                    : (double)courseCompletedCount / lpCourse.NumberOfChapter * 100;

                await _lpCourseRepo.Update(lpCourse);
                await _unitOfWork.SaveChangeAsync();


                // 5️⃣ Next question
                var nextQuestion = await _lpQuestionRepo.AsQueryable()
                    .Include(x => x.Question)
                    .Where(x => x.LearningPathExerciseId == lpExercise.LearningPathExerciseId &&
                                x.Question.OrderIndex > question.OrderIndex)
                    .OrderBy(x => x.Question.OrderIndex)
                    .FirstOrDefaultAsync();

                bool isLast = nextQuestion == null;

                // 6️⃣ RESPONSE
                return Success(
                    BusinessCode.UPDATE_SUCESSFULLY,
                    isLast ? "Hoàn thành bài tập." : "Nộp câu trả lời thành công.",
                    new
                    {
                        LearnerAnswerId = answer.LearnerAnswerId,   
                        LearningPathExerciseId = lpExercise.LearningPathExerciseId,
                        ExerciseId = exerciseId,
                        SubmittedScore = dto.ScoreForVoice,
                        AverageScore = lpExercise.ScoreAchieved,
                        TotalQuestions = lpExercise.NumberOfQuestion,
                        NumberDone = completed,
                        ExerciseStatus = lpExercise.Status,
                        NextQuestion = isLast ? null : new
                        {
                            LearningPathQuestionId = nextQuestion.LearningPathQuestionId,
                            QuestionId = nextQuestion.QuestionId,
                            nextQuestion.Question.Text,
                            nextQuestion.Question.Type,
                            nextQuestion.Question.OrderIndex
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, ex.Message);
            }
        }



        public async Task<ResponseDTO> CheckAndUpgradeLevelAsync(Guid learnerProfileId)
        {
            try
            {
                var learner = await _unitOfWork.GetDbContext().Set<LearnerProfile>()
                    .FirstOrDefaultAsync(x => x.LearnerProfileId == learnerProfileId);

                if (learner == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy hồ sơ học viên.");

                string[] levels = { "A1", "A2", "B1", "B2", "C1", "C2" };
                int currentIndex = Array.IndexOf(levels, learner.Level);

                if (currentIndex == -1 || currentIndex == levels.Length - 1)
                    return Fail(BusinessCode.INVALID_ACTION, "Bạn đã ở level cao nhất hoặc level không hợp lệ.");

                string currentLevel = learner.Level;

                // ========================================
                // 1. Lấy tất cả course của level hiện tại
                // ========================================
                var courses = await _unitOfWork.GetDbContext().Set<Course>()
                    .Where(c => c.Level == currentLevel)
                    .OrderBy(c => c.OrderIndex)
                    .ToListAsync();

                if (!courses.Any())
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Level hiện tại không có khóa học.");

                // ========================================
                // 2. Load toàn bộ LP Course
                // ========================================
                var lpCourses = await _unitOfWork.GetDbContext().Set<LearningPathCourse>()
                    .Where(lp => lp.LearnerCourse.LearnerProfileId == learnerProfileId &&
                                 lp.Course.Level == currentLevel)
                    .Include(lp => lp.LearningPathChapters)
                        .ThenInclude(ch => ch.LearningPathExercises)
                    .ToListAsync();

                if (lpCourses.Count != courses.Count)
                    return Fail(BusinessCode.INVALID_ACTION, "Bạn chưa mở hoặc chưa hoàn thành toàn bộ khóa học của level.");

                // ========================================
                // 3. Validate Completed từng Course/Chapter/Exercise
                // ========================================
                foreach (var lpCourse in lpCourses)
                {
                    if (lpCourse.Status != "Completed")
                        return Fail(BusinessCode.INVALID_ACTION, $"Khóa học '{lpCourse.Course.Title}' chưa hoàn thành.");

                    foreach (var chapter in lpCourse.LearningPathChapters)
                    {
                        if (chapter.Status != "Completed")
                            return Fail(BusinessCode.INVALID_ACTION, "Vẫn còn chương chưa hoàn thành.");

                        foreach (var ex in chapter.LearningPathExercises)
                        {
                            if (ex.Status != "Completed")
                                return Fail(BusinessCode.INVALID_ACTION, "Vẫn còn bài tập chưa hoàn thành.");
                        }
                    }
                }

                // ========================================
                // ⭐ 4. TÍNH TRUNG BÌNH SCORE CỦA LEVEL
                // ========================================
                double totalScore = 0;
                int exerciseCount = 0;

                foreach (var lpCourse in lpCourses)
                {
                    foreach (var chapter in lpCourse.LearningPathChapters)
                    {
                        foreach (var ex in chapter.LearningPathExercises)
                        {
                            totalScore += ex.ScoreAchieved;
                            exerciseCount++;
                        }
                    }
                }

                double avgScore = exerciseCount == 0 ? 0 : totalScore / exerciseCount;

                // ❌ Nếu TBC < 50 → không cho lên level
                if (avgScore < 50)
                {
                    return Fail(
                        BusinessCode.INVALID_ACTION,
                        $"Điểm trung bình Level {currentLevel} là {Math.Round(avgScore, 2)}. Cần đạt >= 50 để lên Level tiếp theo."
                    );
                }

                // ========================================
                // 5. UP LEVEL
                // ========================================
                learner.Level = levels[currentIndex + 1];
                learner.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.GetDbContext().Update(learner);
                await _unitOfWork.SaveChangeAsync();

                return Success(
                    BusinessCode.UPDATE_SUCESSFULLY,
                    $"Chúc mừng! Bạn đã lên {learner.Level}.",
                    new
                    {
                        NewLevel = learner.Level,
                        AverageScore = Math.Round(avgScore, 2)
                    }
                );
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Lỗi hệ thống: " + ex.Message);
            }
        }





        private ResponseDTO Success(BusinessCode code, string msg, object data = null)
    => new ResponseDTO { IsSucess = true, BusinessCode = code, Message = msg, Data = data };

        private ResponseDTO Fail(BusinessCode code, string msg)
            => new ResponseDTO { IsSucess = false, BusinessCode = code, Message = msg };

    }
}
