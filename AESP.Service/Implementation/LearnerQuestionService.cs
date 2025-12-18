using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Implementation;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class LearnerQuestionService : ILearnerQuestionService
    {
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepo;
        private readonly IGenericRepository<Exercise> _exerciseRepo;
        private readonly IGenericRepository<Question> _questionRepo;
        private readonly IGenericRepository<LearningPathCourse> _learningPathCourseRepo;
        private readonly IGenericRepository<LearningPathExercise> _lpExerciseRepo;
        private readonly IGenericRepository<LearningPathQuestion> _lpQuestionRepo;
        private readonly IUnitOfWork _unitOfWork;


        public LearnerQuestionService(
     IGenericRepository<LearnerProfile> learnerProfileRepo,
     IGenericRepository<Exercise> exerciseRepo,
     IGenericRepository<Question> questionRepo,
     IGenericRepository<LearningPathCourse> learningPathCourseRepo,
     IGenericRepository<LearningPathExercise> lpExerciseRepo,
     IGenericRepository<LearningPathQuestion> lpQuestionRepo,
     IUnitOfWork unitOfWork
 )
        {
            _learnerProfileRepo = learnerProfileRepo;
            _exerciseRepo = exerciseRepo;
            _questionRepo = questionRepo;
            _learningPathCourseRepo = learningPathCourseRepo;
            _lpExerciseRepo = lpExerciseRepo;
            _lpQuestionRepo = lpQuestionRepo;
            _unitOfWork = unitOfWork;

        }

        public async Task<ResponseDTO> GetQuestionsByExerciseIdForLearnerAsync(Guid learnerProfileId, Guid exerciseId)
        {
            try
            {
                if (exerciseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "ExerciseId không hợp lệ.");

                // 🔹 Lấy exercise gốc
                var exercise = await _exerciseRepo.AsQueryable()
                    .Include(e => e.Chapter)
                    .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId);

                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập.");

                var courseId = exercise.Chapter.CourseId;

                // 🔹 Lấy LearningPathCourse đúng learner
                var lpCourse = await _learningPathCourseRepo.AsQueryable()
                    .Include(lp => lp.LearnerCourse)
                    .Include(lp => lp.LearningPathChapters)
                        .ThenInclude(ch => ch.LearningPathExercises)
                    .FirstOrDefaultAsync(lp =>
                        lp.CourseId == courseId &&
                        lp.LearnerCourse.LearnerProfileId == learnerProfileId
                    );

                if (lpCourse == null)
                    return Fail(BusinessCode.ACCESS_DENIED, "Bạn chưa đăng ký khóa học này.");

                // 🔥 Tìm đúng LearningPathExercise
                var lpExercise = lpCourse.LearningPathChapters
                    .SelectMany(ch => ch.LearningPathExercises ?? new List<LearningPathExercise>())
                    .FirstOrDefault(ex => ex.ExerciseId == exerciseId);

                if (lpExercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "LearningPathExercise chưa được sinh.");

                // 🔥 CHẶN truy cập khi chưa InProgress
                if (lpExercise.Status != "InProgress")
                    return Fail(BusinessCode.ACCESS_DENIED, "Bạn cần bắt đầu bài tập trước khi xem câu hỏi.");

                // 🔹 Lấy LPQ
                var lpQuestions = await _lpQuestionRepo.AsQueryable()
                    .Where(q => q.LearningPathExerciseId == lpExercise.LearningPathExerciseId)
                    .Include(q => q.Question)
                    .ToListAsync();

                // ❗ Không tự sinh — nếu không tồn tại thì trả lỗi
                if (!lpQuestions.Any())
                    return Fail(BusinessCode.DATA_NOT_FOUND,
                        "Chưa tạo câu hỏi cho bài tập này. (LPQuestion chưa được sinh)");

                // 🔥 Bảo vệ Question = null
                lpQuestions = lpQuestions.Where(q => q.Question != null).ToList();

                // 🔹 Map DTO
                var result = lpQuestions
                    .OrderBy(q => q.Question.OrderIndex)
                    .Select(q => new ReadQuestionDTO
                    {
                        QuestionId = q.QuestionId,
                        ExerciseId = exerciseId,
                        Text = q.Question.Text,
                        Type = q.Question.Type,
                        OrderIndex = q.Question.OrderIndex,
                        Media = new List<ReadQuestionMediaDTO>() // vì QuestionMedias đang rỗng
                    })
                    .ToList();

                return Success(BusinessCode.GET_DATA_SUCCESSFULLY,
                    "Lấy danh sách câu hỏi thành công.",
                    result);
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION,
                    $"Lỗi khi lấy danh sách câu hỏi: {ex.Message}");
            }
        }

        // ✅ Helpers
        private ResponseDTO Fail(BusinessCode code, string msg)
            => new() { IsSucess = false, BusinessCode = code, Message = msg };

        private ResponseDTO Success(BusinessCode code, string msg, object? data = null)
            => new() { IsSucess = true, BusinessCode = code, Message = msg, Data = data };
    }
}
