using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class LearnerCourseService : ILearnerCourseService
    {
        private readonly IGenericRepository<LearnerCourse> _learnerCourseRepo;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepo;
        private readonly IGenericRepository<Course> _courseRepo;
        private readonly IUnitOfWork _unitOfWork;

        public LearnerCourseService(
            IGenericRepository<LearnerCourse> learnerCourseRepo,
            IGenericRepository<LearnerProfile> learnerProfileRepo,
            IGenericRepository<Course> courseRepo,
            IUnitOfWork unitOfWork)
        {
            _learnerCourseRepo = learnerCourseRepo;
            _learnerProfileRepo = learnerProfileRepo;
            _courseRepo = courseRepo;
            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // 🔹 ENROLL COURSE
        // ============================================================
        public async Task<ResponseDTO> EnrollAsync(Guid learnerProfileId, Guid courseId)
        {
            var learner = await _learnerProfileRepo.GetById(learnerProfileId);
            if (learner == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy hồ sơ học viên.");

            var course = await _courseRepo.GetById(courseId);
            if (course == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học.");

            // Kiểm tra level hợp lệ
            string[] levelOrder = { "A1", "A2", "B1", "B2", "C1", "C2" };
            int learnerIndex = Array.IndexOf(levelOrder, learner.Level);
            int courseIndex = Array.IndexOf(levelOrder, course.Level);

            if (courseIndex > learnerIndex)
                return Fail(BusinessCode.VALIDATION_FAILED,
                    $"Bạn chưa đủ điều kiện học Level {course.Level}. Hãy hoàn thành Level {learner.Level} trước.");

            // Kiểm tra đã enroll chưa
            var existed = await _learnerCourseRepo.GetFirstByExpression(x =>
                x.LearnerProfileId == learner.LearnerProfileId && x.NumberOfCourse == course.OrderIndex);

            if (existed != null)
                return Fail(BusinessCode.VALIDATION_FAILED, "Bạn đã đăng ký khóa học này rồi.");

            var entity = new LearnerCourse
            {
                LearnerCourseId = Guid.NewGuid(),
                LearnerProfileId = learner.LearnerProfileId,
                GeneratedDate = DateTime.UtcNow,
                NumberOfCourse = course.OrderIndex,
                Progress = 0,
                Status = LearnerCourseStatus.Enrolled.ToString()
            };

            await _learnerCourseRepo.Insert(entity);
            await _unitOfWork.SaveChangeAsync();

            return Success(BusinessCode.INSERT_SUCESSFULLY, $"Đăng ký khóa học {course.Title} thành công.");
        }

        // ============================================================
        // 🔹 UNENROLL COURSE
        // ============================================================
        public async Task<ResponseDTO> UnenrollAsync(Guid learnerId, Guid courseId)
        {
            var learner = await _learnerProfileRepo.GetByExpression(x => x.UserId == learnerId);
            if (learner == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy hồ sơ học viên.");

            var course = await _courseRepo.GetById(courseId);
            if (course == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học.");

            var record = await _learnerCourseRepo.GetFirstByExpression(x =>
                x.LearnerProfileId == learner.LearnerProfileId && x.NumberOfCourse == course.OrderIndex);

            if (record == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Bạn chưa đăng ký khóa học này.");

            record.Status = LearnerCourseStatus.Cancelled.ToString();
            await _learnerCourseRepo.Update(record);
            await _unitOfWork.SaveChangeAsync();

            return Success(BusinessCode.DELETE_SUCESSFULLY, "Hủy đăng ký khóa học thành công.");
        }

        // ============================================================
        // 🔹 UPDATE PROGRESS
        // ============================================================
        public async Task<ResponseDTO> UpdateProgressAsync(Guid learnerId, Guid courseId, double progress)
        {
            var learner = await _learnerProfileRepo.GetByExpression(x => x.UserId == learnerId);
            if (learner == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy hồ sơ học viên.");

            var course = await _courseRepo.GetById(courseId);
            if (course == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học.");

            var record = await _learnerCourseRepo.GetFirstByExpression(x =>
                x.LearnerProfileId == learner.LearnerProfileId && x.NumberOfCourse == course.OrderIndex);

            if (record == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Bạn chưa đăng ký khóa học này.");

            record.Progress = progress;
            if (progress >= 100)
            {
                record.Status = LearnerCourseStatus.Completed.ToString();

                // tự nâng Level học viên
                string[] order = { "A1", "A2", "B1", "B2", "C1", "C2" };
                int index = Array.IndexOf(order, learner.Level);
                if (index >= 0 && index < order.Length - 1)
                    learner.Level = order[index + 1];

                await _learnerProfileRepo.Update(learner);
            }

            await _learnerCourseRepo.Update(record);
            await _unitOfWork.SaveChangeAsync();

            return Success(BusinessCode.UPDATE_SUCESSFULLY, "Cập nhật tiến độ học thành công.");
        }





        public async Task<ResponseDTO> GetFullCoursesByLevelAsync(string level, string? keyword = null)
        {
            try
            {
                // ✅ Validate đầu vào
                if (string.IsNullOrWhiteSpace(level))
                    return Fail(BusinessCode.VALIDATION_FAILED, "Level không hợp lệ.");

                // ✅ Lấy danh sách khóa học theo level
                var courses = await _courseRepo.AsQueryable()
                    .AsNoTracking()
                    .Include(c => c.Chapters)
                        .ThenInclude(ch => ch.Exercises)
                            .ThenInclude(ex => ex.Questions)
                    .Where(c => c.Level.ToUpper() == level.ToUpper()
                             && (string.IsNullOrEmpty(keyword) || c.Title.Contains(keyword)))
                    .OrderBy(c => c.OrderIndex)
                    .ToListAsync();

                if (!courses.Any())
                    return Fail(BusinessCode.DATA_NOT_FOUND, $"Không tìm thấy khóa học cho Level {level}.");

                // ✅ Map dữ liệu
                var mapped = courses.Select(c => new ReadCourseFullDTO
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    NumberOfChapter = c.NumberOfChapter,
                    OrderIndex = c.OrderIndex,
                    Level = c.Level,
                    Price = c.Price,
                    IsFree = c.OrderIndex == 1, // có thể điều chỉnh logic free tại đây
                    Chapters = c.Chapters?.Select(ch => new ReadCourseChapterForCourseDTO
                    {
                        ChapterId = ch.ChapterId,
                        Title = ch.Title,
                        Description = ch.Description,
                        NumberOfExercise = ch.NumberOfExercise,
                        CreatedAt = ch.CreatedAt,
                        Exercises = ch.Exercises?.Select(ex => new ReadCourseExerciseForCourseDTO
                        {
                            ExerciseId = ex.ExerciseId,
                            Title = ex.Title,
                            Description = ex.Description,
                            OrderIndex = ex.OrderIndex,
                            NumberOfQuestion = ex.NumberOfQuestion,
                            Questions = ex.Questions?.Select(q => new ReadCourseQuestionForCourseDTO
                            {
                                QuestionId = q.QuestionId,
                                Text = q.Text,
                                Type = q.Type,
                                OrderIndex = q.OrderIndex,
                                PhonemeJson = q.PhonemeJson
                            }).ToList() ?? new List<ReadCourseQuestionForCourseDTO>()
                        }).ToList() ?? new List<ReadCourseExerciseForCourseDTO>()
                    }).ToList() ?? new List<ReadCourseChapterForCourseDTO>()
                }).ToList();

                // ✅ Trả kết quả
                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = $"Lấy danh sách khóa học level {level} thành công.",
                    Data = mapped
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, $"Không thể lấy danh sách khóa học: {ex.Message}");
            }
        }


        // ============================================================
        // Helper methods
        // ============================================================

        private ResponseDTO Success(BusinessCode code, string msg)
            => new() { IsSucess = true, BusinessCode = code, Message = msg };

        private ResponseDTO Fail(BusinessCode code, string msg)
            => new() { IsSucess = false, BusinessCode = code, Message = msg };
    }
}
