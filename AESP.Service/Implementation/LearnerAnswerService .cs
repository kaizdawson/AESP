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
                // 1️⃣ Load LPQuestion (chỉ cần lấy Question + Exercise)
                var lpQuestion = await _lpQuestionRepo.AsQueryable()
                    .Include(q => q.Question)
                    .Include(q => q.LearningPathExercise)
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
                    QuestionId = question.QuestionId,
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

                lpExercise.Status = completed == lpExercise.NumberOfQuestion ? "Completed" : "InProgress";
                lpExercise.ScoreAchieved = allLpQuestions.Average(q => q.Score);
                await _lpExerciseRepo.Update(lpExercise);
                await _unitOfWork.SaveChangeAsync();

                // 5️⃣ Next LPQuestion (theo OrderIndex)
                var nextQuestion = await _lpQuestionRepo.AsQueryable()
                    .Include(x => x.Question)
                    .Where(x => x.LearningPathExerciseId == lpExercise.LearningPathExerciseId &&
                                x.Question.OrderIndex > question.OrderIndex)
                    .OrderBy(x => x.Question.OrderIndex)
                    .FirstOrDefaultAsync();

                bool isLast = nextQuestion == null;

                // 6️⃣ FINAL RESPONSE — CHUẨN FORMAT FE CẦN
                return Success(
                    BusinessCode.UPDATE_SUCESSFULLY,
                    isLast ? "Hoàn thành bài tập." : "Nộp câu trả lời thành công.",
                    new
                    {
                        LearningPathExerciseId = lpExercise.LearningPathExerciseId,
                        ExerciseId = exerciseId,

                        // ⭐ Thông tin kết quả
                        SubmittedScore = dto.ScoreForVoice,
                        AverageScore = lpExercise.ScoreAchieved,
                        TotalQuestions = lpExercise.NumberOfQuestion,
                        NumberDone = completed,

                        ExerciseStatus = lpExercise.Status,

                        // ⭐ Câu hỏi tiếp theo (format gọn gàng)
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

        private ResponseDTO Success(BusinessCode code, string msg, object data = null)
    => new ResponseDTO { IsSucess = true, BusinessCode = code, Message = msg, Data = data };

        private ResponseDTO Fail(BusinessCode code, string msg)
            => new ResponseDTO { IsSucess = false, BusinessCode = code, Message = msg };

    }
}
