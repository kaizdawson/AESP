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



        //public async Task<ResponseDTO> EnrollAsync(Guid learnerProfileId, Guid courseId)
        //{
        //    var learner = await _learnerProfileRepo.GetById(learnerProfileId);
        //    if (learner == null)
        //        return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy hồ sơ học viên.");

        //    var course = await _courseRepo.GetById(courseId);
        //    if (course == null)
        //        return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học.");

        //    string[] levelOrder = { "A1", "A2", "B1", "B2", "C1", "C2" };
        //    int courseIndex = Array.IndexOf(levelOrder, course.Level);
        //    int learnerIndex = Array.IndexOf(levelOrder, learner.Level);

        //    // ✅ Kiểm tra learner đã từng enroll level này chưa
        //    var existedLearnerCourse = await (
        //        from lc in _learnerCourseRepo.AsQueryable()
        //        join lp in _learningPathCourseRepo.AsQueryable() on lc.LearnerCourseId equals lp.LearnerCourseId
        //        join c in _courseRepo.AsQueryable() on lp.CourseId equals c.CourseId
        //        where lc.LearnerProfileId == learner.LearnerProfileId && c.Level == course.Level
        //        select lc
        //    ).FirstOrDefaultAsync();

        //    // ❌ Không được học level thấp hơn level hiện tại,
        //    // 👉 Trừ khi learner đã từng enroll level đó trước đây.
        //    if (courseIndex < learnerIndex && existedLearnerCourse == null)
        //    {
        //        return Fail(BusinessCode.INVALID_ACTION,
        //            $"Bạn hiện đang ở Level {learner.Level}. Không thể học Level thấp hơn ({course.Level}).");
        //    }

        //    // ✅ Nếu đã enroll rồi → cho phép quay lại học/view mà không tạo mới
        //    if (existedLearnerCourse != null)
        //    {
        //        return new ResponseDTO
        //        {
        //            IsSucess = true,
        //            BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
        //            Message = $"Bạn đã đăng ký Level {course.Level} trước đó. Có thể quay lại học hoặc xem lại nội dung.",
        //            Data = new
        //            {
        //                LearnerCourseId = existedLearnerCourse.LearnerCourseId,
        //                Level = course.Level
        //            }
        //        };
        //    }


        //    // ✅ Không cho học level cao hơn nếu chưa hoàn thành level hiện tại
        //    if (courseIndex > learnerIndex)
        //    {
        //        var currentLevelPassed = await (
        //            from lp in _learningPathCourseRepo.AsQueryable()
        //            join c in _courseRepo.AsQueryable() on lp.CourseId equals c.CourseId
        //            join lc in _learnerCourseRepo.AsQueryable() on lp.LearnerCourseId equals lc.LearnerCourseId
        //            where lc.LearnerProfileId == learner.LearnerProfileId && c.Level == learner.Level
        //            select lp.Status
        //        ).ToListAsync();

        //        bool hasPassedCurrentLevel = currentLevelPassed.Any(s =>
        //            s.Equals("Completed", StringComparison.OrdinalIgnoreCase));

        //        if (!hasPassedCurrentLevel)
        //        {
        //            return Fail(BusinessCode.INVALID_ACTION,
        //                $"Bạn cần hoàn thành Level {learner.Level} trước khi học Level {course.Level}.");
        //        }
        //    }

        //    // ✅ Chỉ cho phép enroll khóa đầu tiên (OrderIndex = 1)
        //    if (course.OrderIndex != 1)
        //        return Fail(BusinessCode.INVALID_ACTION,
        //            $"Chỉ có thể đăng ký khóa học đầu tiên (OrderIndex = 1) trong Level {course.Level}.");

        //    // ✅ Tạo LearnerCourse mới cho level này
        //    var learnerCourse = new LearnerCourse
        //    {
        //        LearnerCourseId = Guid.NewGuid(),
        //        LearnerProfileId = learner.LearnerProfileId,
        //        GeneratedDate = DateTime.UtcNow,
        //        NumberOfCourse = course.OrderIndex
        //    };
        //    await _learnerCourseRepo.Insert(learnerCourse);

        //    // ✅ Tạo LearningPathCourse đầu tiên cho level này
        //    var newLp = new LearningPathCourse
        //    {
        //        LearningPathCourseId = Guid.NewGuid(),
        //        LearnerCourseId = learnerCourse.LearnerCourseId,
        //        CourseId = course.CourseId,
        //        OrderIndex = course.OrderIndex,
        //        NumberOfChapter = course.NumberOfChapter,
        //        Status = "Enrolled",
        //        Progress = 0
        //    };

        //    await _learningPathCourseRepo.Insert(newLp);
        //    await _unitOfWork.SaveChangeAsync();

        //    return new ResponseDTO
        //    {
        //        IsSucess = true,
        //        BusinessCode = BusinessCode.INSERT_SUCESSFULLY,
        //        Message = $"Đăng ký khóa học đầu tiên của Level {course.Level} thành công! Chúc bạn học tốt.",
        //        Data = new
        //        {
        //            LearnerCourseId = learnerCourse.LearnerCourseId,
        //            Level = course.Level
        //        }
        //    };

        //}





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
            int learnerIndex = Array.IndexOf(levelOrder, learner.Level);

            // ============================================================
            // 1️⃣ CHECK ĐÃ ENROLL LEVEL NÀY CHƯA
            // ============================================================
            var existedLearnerCourse = await (
                from lc in _learnerCourseRepo.AsQueryable()
                join lp in _learningPathCourseRepo.AsQueryable()
                    on lc.LearnerCourseId equals lp.LearnerCourseId
                join c in _courseRepo.AsQueryable() on lp.CourseId equals c.CourseId
                where lc.LearnerProfileId == learner.LearnerProfileId && c.Level == course.Level
                select new { lc, lp }
            ).FirstOrDefaultAsync();

            if (existedLearnerCourse != null)
            {
                return Success(BusinessCode.GET_DATA_SUCCESSFULLY,
                    $"Bạn đã đăng ký Level {course.Level} trước đó.",
                    new
                    {
                        Level = course.Level,
                        LearningPathCourseId = existedLearnerCourse.lp.LearningPathCourseId,
                        CourseId = course.CourseId,
                        Status = existedLearnerCourse.lp.Status
                    });
            }

            // ============================================================
            // 2️⃣ VALIDATE LEVEL
            // ============================================================
            if (courseIndex < learnerIndex)
                return Fail(BusinessCode.INVALID_ACTION,
                    $"Bạn đang ở Level {learner.Level}, không thể học Level thấp hơn ({course.Level}).");

            if (courseIndex > learnerIndex)
            {
                var completed = await (
                    from lp in _learningPathCourseRepo.AsQueryable()
                    join c in _courseRepo.AsQueryable() on lp.CourseId equals c.CourseId
                    join lc in _learnerCourseRepo.AsQueryable() on lp.LearnerCourseId equals lc.LearnerCourseId
                    where lc.LearnerProfileId == learner.LearnerProfileId && c.Level == learner.Level
                    select lp.Status
                ).AnyAsync(s => s == "Completed");

                if (!completed)
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Bạn phải hoàn thành Level {learner.Level} trước.");
            }

            // ============================================================
            // 3️⃣ CHỈ CHO ĐK KHÓA ĐẦU TIÊN (ORDERINDEX = 1)
            // ============================================================
            if (course.OrderIndex != 1)
                return Fail(BusinessCode.INVALID_ACTION,
                    "Chỉ được đăng ký khóa đầu tiên (OrderIndex = 1) của Level.");

            // ============================================================
            // 4️⃣ TẠO LEARNERCOURSE
            // ============================================================
            var learnerCourse = new LearnerCourse
            {
                LearnerCourseId = Guid.NewGuid(),
                LearnerProfileId = learner.LearnerProfileId,
                GeneratedDate = DateTime.UtcNow,
                NumberOfCourse = 1
            };
            await _learnerCourseRepo.Insert(learnerCourse);

            // ============================================================
            // 5️⃣ TẠO LEARNINGPATHCOURSE
            // ============================================================
            var lpCourse = new LearningPathCourse
            {
                LearningPathCourseId = Guid.NewGuid(),
                LearnerCourseId = learnerCourse.LearnerCourseId,
                CourseId = course.CourseId,
                OrderIndex = 1,
                NumberOfChapter = course.NumberOfChapter,
                Status = "InProgress",
                Progress = 0
            };
            await _learningPathCourseRepo.Insert(lpCourse);

            // Lưu trước khi tạo chapter/exercise
            await _unitOfWork.SaveChangeAsync();

            // ============================================================
            // 6️⃣ GỌI SERVICE TẠO LEARNINGPATHCHAPTER + LEARNINGPATHEXERCISE
            // ============================================================
            await _learningPathChapterService.CreateByCourseAsync(
                lpCourse.LearningPathCourseId,
                learnerCourse.LearnerCourseId
            );

            // ============================================================
            // 7️⃣ RESPONSE – TRẢ VỀ 4 FIELD NHƯ YÊU CẦU
            // ============================================================
            return Success(BusinessCode.INSERT_SUCESSFULLY,
                "Đăng ký khóa học đầu tiên của Level {course.Level} thành công! Chúc bạn học tốt.",
                new
                {
                    Level = course.Level,
                    LearningPathCourseId = lpCourse.LearningPathCourseId,
                    CourseId = course.CourseId,
                    Status = "InProgress"
                });
        }



        // ============================================================
        // 🔹 UNENROLL COURSE
        // ============================================================
        //public async Task<ResponseDTO> UnenrollAsync(Guid learnerId, Guid courseId)
        //{
        //    var learner = await _learnerProfileRepo.GetByExpression(x => x.UserId == learnerId);
        //    if (learner == null)
        //        return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy hồ sơ học viên.");

        //    var learnerCourse = await _learnerCourseRepo.GetFirstByExpression(x => x.LearnerProfileId == learner.LearnerProfileId);
        //    if (learnerCourse == null)
        //        return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy lộ trình học.");

        //    var record = await _learningPathCourseRepo.GetFirstByExpression(x =>
        //        x.LearnerCourseId == learnerCourse.LearnerCourseId &&
        //        x.CourseId == courseId);

        //    if (record == null)
        //        return Fail(BusinessCode.DATA_NOT_FOUND, "Bạn chưa đăng ký khóa học này.");

        //    record.Status = "Cancelled";
        //    await _learningPathCourseRepo.Update(record);
        //    await _unitOfWork.SaveChangeAsync();

        //    return Success(BusinessCode.DELETE_SUCESSFULLY, "Hủy đăng ký khóa học thành công.");
        //}

        // ============================================================
        // 🔹 UPDATE PROGRESS
        // ============================================================
        //public async Task<ResponseDTO> UpdateProgressAsync(Guid learnerId, Guid courseId, double progress)
        //{
        //    var learner = await _learnerProfileRepo.GetByExpression(x => x.UserId == learnerId);
        //    if (learner == null)
        //        return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy hồ sơ học viên.");

        //    var learnerCourse = await _learnerCourseRepo.GetFirstByExpression(x => x.LearnerProfileId == learner.LearnerProfileId);
        //    if (learnerCourse == null)
        //        return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy lộ trình học.");

        //    var record = await _learningPathCourseRepo.GetFirstByExpression(x =>
        //        x.LearnerCourseId == learnerCourse.LearnerCourseId &&
        //        x.CourseId == courseId);

        //    if (record == null)
        //        return Fail(BusinessCode.DATA_NOT_FOUND, "Bạn chưa đăng ký khóa học này.");

        //    record.Progress = progress;
        //    if (progress >= 100)
        //    {
        //        record.Status = "Completed";

        //        // tự nâng Level học viên
        //        string[] order = { "A1", "A2", "B1", "B2", "C1", "C2" };
        //        int index = Array.IndexOf(order, learner.Level);
        //        if (index >= 0 && index < order.Length - 1)
        //        {
        //            learner.Level = order[index + 1];
        //            await _learnerProfileRepo.Update(learner);
        //        }
        //    }

        //    await _learningPathCourseRepo.Update(record);
        //    await _unitOfWork.SaveChangeAsync();

        //    return Success(BusinessCode.UPDATE_SUCESSFULLY, "Cập nhật tiến độ học thành công.");
        //}





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
                             && (string.IsNullOrEmpty(keyword) || c.Status.Contains(keyword)))
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
                    Description = c.Description,
                    Status = c.Status,

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
        // 🔹 HỌC LẠI EXERCISE ĐỂ CẢI THIỆN ĐIỂM
        // ============================================================
        public async Task<ResponseDTO> RelearnAndUpdateScoreAsync(Guid learnerProfileId, Guid exerciseId, double? newScore = null)
        {
            try
            {
                // 1️⃣ Tìm LearnerCourse
                var learnerCourse = await _learnerCourseRepo.AsQueryable()
                    .FirstOrDefaultAsync(lc => lc.LearnerProfileId == learnerProfileId);

                if (learnerCourse == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy lộ trình học của học viên.");

                // 2️⃣ Tìm bài tập
                var exercise = await _unitOfWork.GetDbContext()
                    .Set<LearningPathExercise>()
                    .Include(e => e.LearningPathChapter)
                        .ThenInclude(ch => ch.LearningPathCourse)
                            .ThenInclude(lpc => lpc.Course)
                    .Include(e => e.LearningPathChapter.LearningPathCourse.LearnerCourse)
                    .FirstOrDefaultAsync(e =>
                        e.ExerciseId == exerciseId &&
                        e.LearningPathChapter.LearningPathCourse.LearnerCourse.LearnerProfileId == learnerProfileId);

                if (exercise == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy bài tập trong lộ trình học.");

                // 3️⃣ KIỂM TRA THỜI HẠN KHÓA HỌC
                var lpCourse = exercise.LearningPathChapter.LearningPathCourse;
                var courseInfo = lpCourse.Course;

                // Duration > 0 thì mới tính hạn
                if (courseInfo.Duration > 0)
                {
                    // Lấy từ CreatedAt của LearningPathCourse
                    var startDate = lpCourse.CreatedAt;
                    var expireDate = startDate.AddDays(courseInfo.Duration);

                    if (DateTime.UtcNow > expireDate)
                    {
                        return Fail(BusinessCode.INVALID_ACTION,
                            $"Khóa học đã hết hạn vào ngày {expireDate:dd/MM/yyyy}. Bạn không thể học lại bài tập này.");
                    }
                }

                // 4️⃣ Mỗi lần học lại → tăng số lần
                exercise.RelearnCount++;

                // 5️⃣ Nếu có điểm mới → cập nhật
                if (newScore.HasValue)
                {
                    exercise.ScoreAchieved = newScore.Value;
                }

                await _unitOfWork.GetDbContext().SaveChangesAsync();

                // 6️⃣ RETURN HOÀN THÀNH
                return Success(BusinessCode.UPDATE_SUCESSFULLY,
                    "Cập nhật học lại thành công.",
                    new
                    {
                        exercise.LearningPathExerciseId,
                        exercise.ExerciseId,
                        exercise.ScoreAchieved,
                        exercise.RelearnCount
                    });
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Lỗi khi xử lý học lại: " + ex.Message);
            }
        }


        public async Task<ResponseDTO> GetMyLevelsAsync(Guid learnerProfileId)
        {
            try
            {
                string[] levelOrder = { "A1", "A2", "B1", "B2", "C1", "C2" };

                // 🟢 Lấy danh sách tất cả course mà learner đã enroll
                var enrolled = await (
                    from lc in _learnerCourseRepo.AsQueryable()
                    join lp in _learningPathCourseRepo.AsQueryable()
                        on lc.LearnerCourseId equals lp.LearnerCourseId
                    join c in _courseRepo.AsQueryable()
                        on lp.CourseId equals c.CourseId
                    where lc.LearnerProfileId == learnerProfileId
                    select new
                    {
                        Level = c.Level,
                        LearnerCourseId = lc.LearnerCourseId,
                        LearningPathCourseId = lp.LearningPathCourseId,
                        CourseId = c.CourseId,
                        Status = lp.Status
                    }
                ).ToListAsync();

                // 🟢 Duyệt toàn bộ level → gom tất cả course trong level đó
                var result = levelOrder.Select(level => new
                {
                    Level = level,
                    Courses = enrolled
                        .Where(x => x.Level == level)
                        .Select(x => new
                        {
                            x.LearnerCourseId,
                            x.LearningPathCourseId,
                            x.CourseId,
                            x.Status
                        }).ToList()
                }).ToList();

                return new ResponseDTO
                {
                    IsSucess = true,
                    BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY,
                    Message = "Lấy danh sách Level của học viên thành công.",
                    Data = new { Levels = result }
                };
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Lỗi khi lấy danh sách Level: " + ex.Message);
            }
        }




        // ============================================================
        // Helper methods
        // ============================================================

        private ResponseDTO Success(BusinessCode code, string msg, object data)
     => new() { IsSucess = true, BusinessCode = code, Message = msg, Data = data };


        private ResponseDTO Fail(BusinessCode code, string msg)
            => new() { IsSucess = false, BusinessCode = code, Message = msg };
    }
}
