using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Implementation;
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
        private readonly IGenericRepository<LearningPathCourse> _learningPathCourseRepo;
        private readonly ILearningPathChapterService _learningPathChapterService;


        public LearnerCourseService(
       IGenericRepository<LearnerCourse> learnerCourseRepo,
       IGenericRepository<LearnerProfile> learnerProfileRepo,
       IGenericRepository<Course> courseRepo,
       IGenericRepository<LearningPathCourse> learningPathCourseRepo,
       ILearningPathChapterService learningPathChapterService,   // ✅ đúng interface
       IUnitOfWork unitOfWork)
        {
            _learnerCourseRepo = learnerCourseRepo;
            _learnerProfileRepo = learnerProfileRepo;
            _courseRepo = courseRepo;
            _learningPathCourseRepo = learningPathCourseRepo;
            _learningPathChapterService = learningPathChapterService; // ✅ đúng kiểu
            _unitOfWork = unitOfWork;
        }



        public async Task<ResponseDTO> EnrollAsync(Guid learnerProfileId, Guid courseId)
        {
            var learner = await _learnerProfileRepo.GetById(learnerProfileId);
            if (learner == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy hồ sơ học viên.");

            var course = await _courseRepo.GetById(courseId);
            if (course == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học.");

            string[] levelOrder = { "A1", "A2", "B1", "B2", "C1", "C2" };
            int courseIndex = Array.IndexOf(levelOrder, course.Level);

            // Lấy danh sách tất cả level mà learner đã có khóa học
            var learnerLevels = await (
                from lp in _learningPathCourseRepo.AsQueryable()
                join c in _courseRepo.AsQueryable() on lp.CourseId equals c.CourseId
                join lc in _learnerCourseRepo.AsQueryable() on lp.LearnerCourseId equals lc.LearnerCourseId
                where lc.LearnerProfileId == learner.LearnerProfileId
                select c.Level
            ).Distinct().ToListAsync();

            // Tìm level cao nhất mà learner đã hoàn thành tất cả khóa
            string highestCompletedLevel = learnerLevels
                .Where(level =>
                {
                    var levelCourses = (from lp in _learningPathCourseRepo.AsQueryable()
                                        join c in _courseRepo.AsQueryable() on lp.CourseId equals c.CourseId
                                        join lc in _learnerCourseRepo.AsQueryable() on lp.LearnerCourseId equals lc.LearnerCourseId
                                        where lc.LearnerProfileId == learner.LearnerProfileId
                                              && c.Level == level
                                        select lp).ToList();

                    return levelCourses.Any() &&
                           levelCourses.All(lp => lp.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
                })
                .OrderBy(level => Array.IndexOf(levelOrder, level))
                .LastOrDefault();

            // Ưu tiên Level thực tế trong hồ sơ học viên
            if (highestCompletedLevel == null ||
                Array.IndexOf(levelOrder, highestCompletedLevel) < Array.IndexOf(levelOrder, learner.Level))
            {
                highestCompletedLevel = learner.Level;
            }



            int highestIndex = Array.IndexOf(levelOrder, highestCompletedLevel);

            // ❌ Không cho học thấp hơn level đã đạt
            if (courseIndex < highestIndex)
                return Fail(BusinessCode.INVALID_ACTION,
                    $"Bạn đã vượt qua Level {highestCompletedLevel}. Không thể học lại Level {course.Level}.");

            // ❌ Không cho học level cao hơn nếu chưa hoàn thành level hiện tại
            if (courseIndex > highestIndex)
            {
                return Fail(BusinessCode.INVALID_ACTION,
                    $"Bạn cần hoàn thành toàn bộ khóa học Level {highestCompletedLevel} trước khi học Level {course.Level}.");
            }



            // ✅ Chỉ cho phép enroll khóa đầu tiên (OrderIndex = 1)
            if (course.OrderIndex != 1)
                return Fail(BusinessCode.INVALID_ACTION,
                    $"Chỉ có thể đăng ký khóa học đầu tiên (OrderIndex = 1) trong Level {course.Level}.");

            // ✅ Check nếu đã enroll khóa đầu tiên của level này
            var existedLearnerCourse = await (
                from lc in _learnerCourseRepo.AsQueryable()
                join lp in _learningPathCourseRepo.AsQueryable() on lc.LearnerCourseId equals lp.LearnerCourseId
                join c in _courseRepo.AsQueryable() on lp.CourseId equals c.CourseId
                where lc.LearnerProfileId == learner.LearnerProfileId
                      && c.Level == course.Level
                select lc
            ).FirstOrDefaultAsync();

            if (existedLearnerCourse != null)
                return Fail(BusinessCode.INVALID_ACTION,
                    $"Bạn đã đăng ký Level {course.Level} rồi. Hãy tiếp tục học trong lộ trình hiện tại.");

            // ✅ Tạo LearnerCourse mới cho level này
            var learnerCourse = new LearnerCourse
            {
                LearnerCourseId = Guid.NewGuid(),
                LearnerProfileId = learner.LearnerProfileId,
                GeneratedDate = DateTime.UtcNow,
                NumberOfCourse = course.OrderIndex
            };
            await _learnerCourseRepo.Insert(learnerCourse);

            // ✅ Tạo LearningPathCourse đầu tiên cho level này
            var newLp = new LearningPathCourse
            {
                LearningPathCourseId = Guid.NewGuid(),
                LearnerCourseId = learnerCourse.LearnerCourseId,
                CourseId = course.CourseId,
                OrderIndex = course.OrderIndex,
                NumberOfChapter = course.NumberOfChapter,
                Status = "Enrolled",
                Progress = 0
            };

            await _learningPathCourseRepo.Insert(newLp);
            await _unitOfWork.SaveChangeAsync();



            //// 🔹 Auto generate LearningPathChapter khi học viên enroll khóa đầu tiên
            //try
            //{
            //    await _learningPathChapterService.CreateByCourseAsync(newLp.LearningPathCourseId, learnerCourse.LearnerCourseId);
            //}
            //catch (Exception ex)
            //{
            //    // Không ảnh hưởng enroll chính, chỉ log nếu cần
            //    Console.WriteLine($"[WARN] Không thể tạo LearningPathChapter tự động: {ex.Message}");
            //}


            return Success(BusinessCode.INSERT_SUCESSFULLY,
                $"Đăng ký khóa học đầu tiên của Level {course.Level} thành công! Chúc bạn học tốt.");

        }



        // ============================================================
        // 🔹 UNENROLL COURSE
        // ============================================================
        public async Task<ResponseDTO> UnenrollAsync(Guid learnerId, Guid courseId)
        {
            var learner = await _learnerProfileRepo.GetByExpression(x => x.UserId == learnerId);
            if (learner == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy hồ sơ học viên.");

            var learnerCourse = await _learnerCourseRepo.GetFirstByExpression(x => x.LearnerProfileId == learner.LearnerProfileId);
            if (learnerCourse == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy lộ trình học.");

            var record = await _learningPathCourseRepo.GetFirstByExpression(x =>
                x.LearnerCourseId == learnerCourse.LearnerCourseId &&
                x.CourseId == courseId);

            if (record == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Bạn chưa đăng ký khóa học này.");

            record.Status = "Cancelled";
            await _learningPathCourseRepo.Update(record);
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

            var learnerCourse = await _learnerCourseRepo.GetFirstByExpression(x => x.LearnerProfileId == learner.LearnerProfileId);
            if (learnerCourse == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy lộ trình học.");

            var record = await _learningPathCourseRepo.GetFirstByExpression(x =>
                x.LearnerCourseId == learnerCourse.LearnerCourseId &&
                x.CourseId == courseId);

            if (record == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Bạn chưa đăng ký khóa học này.");

            record.Progress = progress;
            if (progress >= 100)
            {
                record.Status = "Completed";

                // tự nâng Level học viên
                string[] order = { "A1", "A2", "B1", "B2", "C1", "C2" };
                int index = Array.IndexOf(order, learner.Level);
                if (index >= 0 && index < order.Length - 1)
                {
                    learner.Level = order[index + 1];
                    await _learnerProfileRepo.Update(learner);
                }
            }

            await _learningPathCourseRepo.Update(record);
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
                                .ThenInclude(q => q.QuestionMedias) 
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
                                PhonemeJson = q.PhonemeJson,
                                QuestionMedia = q.QuestionMedias?.Select(m => new ReadQuestionMediaForCourseDTO
                                {
                                    QuestionMediaId = m.QuestionMediaId,
                                    Accent = m.Accent,
                                    AudioUrl = m.AudioUrl,
                                    VideoUrl = m.VideoUrl,
                                    ImageUrl = m.ImageUrl,
                                    Source = m.Source
                                }).ToList() ?? new List<ReadQuestionMediaForCourseDTO>()
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
