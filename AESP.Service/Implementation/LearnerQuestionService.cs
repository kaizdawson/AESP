using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
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

        public LearnerQuestionService(
            IGenericRepository<LearnerProfile> learnerProfileRepo,
            IGenericRepository<Exercise> exerciseRepo,
            IGenericRepository<Question> questionRepo,
            IGenericRepository<LearningPathCourse> learningPathCourseRepo)
        {
            _learnerProfileRepo = learnerProfileRepo;
            _exerciseRepo = exerciseRepo;
            _questionRepo = questionRepo;
            _learningPathCourseRepo = learningPathCourseRepo;
        }

        public async Task<ResponseDTO> GetQuestionsByExerciseIdForLearnerAsync(Guid learnerProfileId, Guid exerciseId)
        {
            try
            {
                if (exerciseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "ExerciseId không hợp lệ.");

                var learner = await _learnerProfileRepo.GetById(learnerProfileId);
                if (learner == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy hồ sơ học viên.");

                // 🔹 Lấy Exercise + Chapter + Course
                var exercise = await _exerciseRepo.AsQueryable()
                    .Include(e => e.Chapter)
                        .ThenInclude(ch => ch.Course)
                    .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId);

                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập.");

                var courseId = exercise.Chapter.CourseId;

                // 🔹 Kiểm tra quyền truy cập (learner đã enroll course này chưa?)
                bool hasAccess = await _learningPathCourseRepo.AsQueryable()
                    .Include(lp => lp.LearnerCourse)
                    .AnyAsync(lp =>
                        lp.LearnerCourse.LearnerProfileId == learnerProfileId &&
                        lp.CourseId == courseId &&
                        (lp.Status == "Enrolled" || lp.Status == "InProgress" || lp.Status == "Completed"));

                if (!hasAccess)
                    return Fail(BusinessCode.ACCESS_DENIED, "Bạn chưa được phép truy cập bài tập này.");

                // 🔹 Lấy danh sách câu hỏi và media
                var questions = await _questionRepo.AsQueryable()
                    .Include(q => q.QuestionMedias)
                    .Where(q => q.ExerciseId == exerciseId)
                    .OrderBy(q => q.OrderIndex)
                    .ToListAsync();

                var result = questions.Select(q => new ReadQuestionDTO
                {
                    QuestionId = q.QuestionId,
                    ExerciseId = q.ExerciseId,
                    Text = q.Text,
                    Type = q.Type,
                    OrderIndex = q.OrderIndex,
                    PhonemeJson = q.PhonemeJson,
                    Media = q.QuestionMedias.Select(m => new ReadQuestionMediaDTO
                    {
                        QuestionMediaId = m.QuestionMediaId,
                        Accent = m.Accent,
                        AudioURL = m.AudioUrl,
                        VideoURL = m.VideoUrl,
                        ImageURL = m.ImageUrl,
                        Source = m.Source
                    }).ToList()
                }).ToList();

                return Success(BusinessCode.GET_DATA_SUCCESSFULLY,
                    "Lấy danh sách câu hỏi thành công.",
                    result);
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Lỗi khi lấy danh sách câu hỏi: {ex.Message}");
            }
        }

        // ✅ Helpers
        private ResponseDTO Fail(BusinessCode code, string msg)
            => new() { IsSucess = false, BusinessCode = code, Message = msg };

        private ResponseDTO Success(BusinessCode code, string msg, object? data = null)
            => new() { IsSucess = true, BusinessCode = code, Message = msg, Data = data };
    }
}
