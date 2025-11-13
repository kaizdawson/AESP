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
        private readonly IUnitOfWork _unitOfWork;

        public LearnerAnswerService(
            IGenericRepository<LearnerAnswer> answerRepo,
            IGenericRepository<Question> questionRepo,
            IGenericRepository<LearningPathExercise> lpExerciseRepo,
            IGenericRepository<LearningPathChapter> lpChapterRepo,
            IGenericRepository<LearningPathCourse> lpCourseRepo,
            IUnitOfWork unitOfWork)
        {
            _answerRepo = answerRepo;
            _questionRepo = questionRepo;
            _lpExerciseRepo = lpExerciseRepo;
            _lpChapterRepo = lpChapterRepo;
            _lpCourseRepo = lpCourseRepo;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDTO> SubmitAnswerAsync(Guid learnerProfileId, Guid questionId, SubmitLearnerAnswerDTO dto)
        {
            try
            {
                // 1️⃣ Validate question
                var question = await _questionRepo.AsQueryable()
                    .Include(q => q.Exercise)
                    .FirstOrDefaultAsync(q => q.QuestionId == questionId);

                if (question == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy câu hỏi.");

                var exerciseId = question.ExerciseId;

                // 2️⃣ Find LP Exercise
                var lpExercise = await _lpExerciseRepo.AsQueryable()
                    .Include(x => x.LearningPathChapter)
                        .ThenInclude(ch => ch.LearningPathCourse)
                            .ThenInclude(c => c.LearnerCourse)
                    .FirstOrDefaultAsync(x =>
                        x.ExerciseId == exerciseId &&
                        x.LearningPathChapter.LearningPathCourse.LearnerCourse.LearnerProfileId == learnerProfileId);

                if (lpExercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập trong LearningPath.");

                // 3️⃣ Insert Answer
                var answer = new LearnerAnswer
                {
                    LearnerAnswerId = Guid.NewGuid(),
                    LearnerProfileId = learnerProfileId,
                    QuestionId = questionId,
                    LearningPathExerciseId = lpExercise.LearningPathExerciseId,
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

                // 4️⃣ Count total answers for exercise
                var allAnswers = await _answerRepo.AsQueryable()
                    .Where(a => a.LearningPathExerciseId == lpExercise.LearningPathExerciseId)
                    .ToListAsync();

                int numberDone = allAnswers.Count;
                int totalQuestions = lpExercise.NumberOfQuestion;

                // 5️⃣ Update EXERCISE: completed khi làm đủ câu hỏi, KHÔNG quan tâm điểm
                if (numberDone >= totalQuestions)
                {
                    lpExercise.Status = "Completed";
                    lpExercise.ScoreAchieved = allAnswers.Average(a => a.ScoreForVoice);
                }
                else
                {
                    lpExercise.Status = "InProgress";
                    lpExercise.ScoreAchieved = allAnswers.Average(a => a.ScoreForVoice);
                }

                await _lpExerciseRepo.Update(lpExercise);
                await _unitOfWork.SaveChangeAsync();


                // 6️⃣ Update CHAPTER
                var lpChapter = lpExercise.LearningPathChapter;

                var chapterExercises = await _lpExerciseRepo.AsQueryable()
                    .Where(e => e.LearningPathChapterId == lpChapter.LearningPathChapterId)
                    .ToListAsync();

                int totalEx = lpChapter.NumberOfModule;

                int completedEx = chapterExercises.Count(e => e.Status == "Completed");
                int startedEx = chapterExercises.Count(e => e.Status != "NotStarted");

                // progress = % bài tập đã hoàn thành
                lpChapter.Progress = (int)Math.Round((double)completedEx / totalEx * 100);

                if (completedEx == totalEx)
                    lpChapter.Status = "Completed";
                else if (startedEx > 0)
                    lpChapter.Status = "InProgress";
                else
                    lpChapter.Status = "Enrolled";

                await _lpChapterRepo.Update(lpChapter);
                await _unitOfWork.SaveChangeAsync();


                // 7️⃣ Update COURSE
                var lpCourse = lpChapter.LearningPathCourse;

                var chapters = await _lpChapterRepo.AsQueryable()
                    .Where(c => c.LearningPathCourseId == lpCourse.LearningPathCourseId)
                    .ToListAsync();

                int totalCh = lpCourse.NumberOfChapter;

                int completedCh = chapters.Count(c => c.Status == "Completed");
                int startedCh = chapters.Count(c => c.Status != "Enrolled");

                lpCourse.Progress = (int)Math.Round((double)completedCh / totalCh * 100);

                if (completedCh == totalCh)
                    lpCourse.Status = "Completed";
                else if (startedCh > 0)
                    lpCourse.Status = "InProgress";
                else
                    lpCourse.Status = "Enrolled";

                await _lpCourseRepo.Update(lpCourse);
                await _unitOfWork.SaveChangeAsync();


                // 8️⃣ Find next question
                var nextQuestion = await _questionRepo.AsQueryable()
                    .Where(q => q.ExerciseId == exerciseId && q.OrderIndex > question.OrderIndex)
                    .OrderBy(q => q.OrderIndex)
                    .FirstOrDefaultAsync();

                bool isLast = nextQuestion == null;

                // 9️⃣ Response
                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.UPDATE_SUCESSFULLY,
                    Message = isLast ? "Hoàn thành bài tập." : "Nộp câu trả lời thành công.",
                    Data = new
                    {
                        LearningPathExerciseId = lpExercise.LearningPathExerciseId,
                        ExerciseId = exerciseId,
                        SubmittedScore = dto.ScoreForVoice,
                        AverageScore = lpExercise.ScoreAchieved,
                        TotalQuestions = totalQuestions,
                        NumberDone = numberDone,
                        IsNeededReviewed = answer.IsNeededReviewed,

                        ExerciseStatus = lpExercise.Status,
                        ChapterStatus = lpChapter.Status,
                        CourseStatus = lpCourse.Status,

                        NextQuestion = isLast ? null : new
                        {
                            nextQuestion.QuestionId,
                            nextQuestion.Text,
                            nextQuestion.Type,
                            nextQuestion.OrderIndex
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Lỗi server: " + ex.Message);
            }
        }

        private ResponseDTO Fail(BusinessCode code, string msg)
            => new ResponseDTO { IsSucess = false, BusinessCode = code, Message = msg };
    }
}
