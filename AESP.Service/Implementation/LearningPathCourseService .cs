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
    public class LearningPathCourseService : ILearningPathCourseService
    {
        private readonly IGenericRepository<LearningPathCourse> _repo;
        private readonly IGenericRepository<Course> _courseRepo;
        private readonly IGenericRepository<LearnerCourse> _learnerCourseRepo;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepo;
        private readonly ILearningPathChapterService _learningPathChapterService;

        private readonly IUnitOfWork _unitOfWork;

        public LearningPathCourseService(
            IGenericRepository<LearningPathCourse> repo,
            IGenericRepository<Course> courseRepo,
            IGenericRepository<LearnerCourse> learnerCourseRepo,
            IGenericRepository<LearnerProfile> learnerProfileRepo,
            ILearningPathChapterService learningPathChapterService,

            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _courseRepo = courseRepo;
            _learnerCourseRepo = learnerCourseRepo;
            _learnerProfileRepo = learnerProfileRepo;
            _learningPathChapterService = learningPathChapterService;

            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // 🔹 GET ALL (có thể lọc theo LearnerCourseId)
        // ============================================================
        public async Task<ResponseDTO> GetAllAsync(Guid? learnerCourseId = null)
        {
            var query = _repo.AsQueryable();

            if (learnerCourseId.HasValue)
                query = query.Where(x => x.LearnerCourseId == learnerCourseId.Value);

            var data = await query
                .Include(x => x.Course)
                .Select(x => new ReadLearningPathCourseDTO
                {
                    LearningPathCourseId = x.LearningPathCourseId,
                    LearnerCourseId = x.LearnerCourseId,
                    CourseId = x.CourseId,
                    CourseTitle = x.Course.Title,
                    Description = x.Course.Description,
                    Status = x.Status,
                    Progress = x.Progress,
                    NumberOfChapter = x.NumberOfChapter,
                    OrderIndex = x.OrderIndex
                })
                .OrderBy(x => x.OrderIndex)
                .ToListAsync();

            return Success(BusinessCode.GET_DATA_SUCCESSFULLY, "Lấy danh sách khóa học trong lộ trình thành công.", data);
        }

        // ============================================================
        // 🔹 GET BY ID
        // ============================================================
        public async Task<ResponseDTO> GetByIdAsync(Guid id)
        {
            var entity = await _repo.AsQueryable()
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.LearningPathCourseId == id);

            if (entity == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học trong lộ trình.");

            var dto = new ReadLearningPathCourseDTO
            {
                LearningPathCourseId = entity.LearningPathCourseId,
                LearnerCourseId = entity.LearnerCourseId,
                CourseId = entity.CourseId,
                CourseTitle = entity.Course.Title,
                Description = entity.Course.Description,

                Status = entity.Status,
                Progress = entity.Progress,
                NumberOfChapter = entity.NumberOfChapter,
                OrderIndex = entity.OrderIndex
            };

            return Success(BusinessCode.GET_DATA_SUCCESSFULLY, "Lấy chi tiết khóa học trong lộ trình thành công.", dto);
        }

        // ============================================================
        // 🔹 CREATE (KHÔNG AUTO)
        // ============================================================
        public async Task<ResponseDTO> CreateAsync(CreateLearningPathCourseDTO dto)
        {
            try
            {
                // 1️⃣ VALIDATION
                if (dto.LearnerCourseId == Guid.Empty || dto.CourseId == Guid.Empty)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Thiếu thông tin khóa học hoặc lộ trình học viên.");

                var learnerCourse = await _learnerCourseRepo.AsQueryable()
                    .Include(lc => lc.LearnerProfile)
                        .ThenInclude(lp => lp.User)
                    .FirstOrDefaultAsync(lc => lc.LearnerCourseId == dto.LearnerCourseId);

                if (learnerCourse == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy lộ trình học của học viên.");

                var course = await _courseRepo.GetById(dto.CourseId);
                if (course == null)
                    return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học.");

                // ❌ Không cho phép mở khóa học đầu tiên trong level bằng API CreateAsync
                if (course.OrderIndex == 1)
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Khóa học '{course.Title}' là khóa đầu tiên của Level {course.Level}. Vui lòng đăng ký qua tính năng Enroll.");

                // 2️⃣ KIỂM TRA LEVEL
                var learnerLevel = learnerCourse.LearnerProfile.Level;
                string[] levels = { "A1", "A2", "B1", "B2", "C1", "C2" };
                int learnerIndex = Array.IndexOf(levels, learnerLevel);
                int courseIndex = Array.IndexOf(levels, course.Level);

                if (courseIndex < learnerIndex)
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Bạn không thể học lại khóa ở level thấp hơn ({course.Level}) so với level hiện tại ({learnerLevel}).");

                if (courseIndex > learnerIndex)
                    return Fail(BusinessCode.INVALID_ACTION,
                        $"Bạn cần hoàn thành tất cả khóa học level {learnerLevel} trước khi học level {course.Level}.");

                int orderIndex = course.OrderIndex;

                // 3️⃣ KIỂM TRA TRÙNG DỮ LIỆU
                if (await _repo.AsQueryable().AnyAsync(x =>
                    x.LearnerCourseId == dto.LearnerCourseId && x.CourseId == dto.CourseId))
                    return Fail(BusinessCode.DUPLICATE_DATA, "Khóa học này đã có trong lộ trình.");

                if (await _repo.AsQueryable()
                    .Include(x => x.Course)
                    .AnyAsync(x =>
                        x.LearnerCourseId == dto.LearnerCourseId &&
                        x.OrderIndex == orderIndex &&
                        x.Course.Level == course.Level))
                    return Fail(BusinessCode.DUPLICATE_DATA,
                        $"OrderIndex {orderIndex} đã được sử dụng trong level {course.Level} này.");

                // 4️⃣ KIỂM TRA KHÓA TRƯỚC ĐÃ HOÀN THÀNH CHƯA
                if (orderIndex > 1)
                {
                    var prev = await _repo.AsQueryable().FirstOrDefaultAsync(x =>
                        x.LearnerCourseId == dto.LearnerCourseId && x.OrderIndex == orderIndex - 1);

                    if (prev == null || !prev.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                        return Fail(BusinessCode.INVALID_ACTION, "Bạn cần hoàn thành khóa học trước đó trước khi mở khóa tiếp theo.");
                }

                //// 5️⃣ XÁC ĐỊNH MIỄN PHÍ & XỬ LÝ XU
                //var levelCourses = await _courseRepo.AsQueryable()
                //    .Where(c => c.Level == course.Level)
                //    .OrderBy(c => c.OrderIndex)
                //    .ToListAsync();

                //bool isFreeCourseOfLevel = (levelCourses.FirstOrDefault()?.CourseId == course.CourseId);

                //var learnerUser = learnerCourse.LearnerProfile.User;
                //if (!isFreeCourseOfLevel && course.Price > 0)
                //{
                //    int price = (int)Math.Round(course.Price);
                //    if (learnerUser.CoinBalance < price)
                //        return Fail(BusinessCode.INVALID_ACTION, "Không đủ xu để mở khóa học này.");

                //    learnerUser.CoinBalance -= price;
                //    _courseRepo.GetDbContext().Set<User>().Update(learnerUser);
                //}


                // 5️⃣ XÁC ĐỊNH MIỄN PHÍ & XỬ LÝ XU
                var levelCourses = await _courseRepo.AsQueryable()
                    .Where(c => c.Level == course.Level)
                    .OrderBy(c => c.OrderIndex)
                    .ToListAsync();

                bool isFreeCourseOfLevel = (levelCourses.FirstOrDefault()?.CourseId == course.CourseId);

                var learnerUser = learnerCourse.LearnerProfile.User;

                if (!isFreeCourseOfLevel && course.Price > 0)
                {
                    int price = (int)Math.Round(course.Price);

                    if (learnerUser.CoinBalance < price)
                        return Fail(BusinessCode.INVALID_ACTION, "Không đủ xu để mở khóa học này.");

                    // trừ xu
                    learnerUser.CoinBalance -= price;
                    _courseRepo.GetDbContext().Set<User>().Update(learnerUser);

                    // ⭐ TẠO PURCHASE RECORD
                    var purchase = new Purchase
                    {
                        PurchaseId = Guid.NewGuid(),
                        UserId = learnerUser.UserId,
                        CourseId = course.CourseId,
                        AmountCoin = price,
                        Status = "Success",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _courseRepo.GetDbContext().Set<Purchase>().AddAsync(purchase);
                }

                // 6️⃣ TẠO KHÓA HỌC TRONG LỘ TRÌNH
                var entity = new LearningPathCourse
                {
                    LearningPathCourseId = Guid.NewGuid(),
                    LearnerCourseId = dto.LearnerCourseId,
                    CourseId = dto.CourseId,
                    OrderIndex = orderIndex,
                    Status = "InProgress",
                    Progress = 0,
                    NumberOfChapter = course.NumberOfChapter
                };

                await _repo.Insert(entity);
                await _unitOfWork.SaveChangeAsync();

                // 7️⃣ CẬP NHẬT TỔNG SỐ KHÓA HỌC TRONG LEVEL
                var totalCoursesInLevel = await _courseRepo.AsQueryable()
                    .CountAsync(c => c.Level == course.Level);

                learnerCourse.NumberOfCourse = totalCoursesInLevel;
                await _learnerCourseRepo.Update(learnerCourse);
                await _unitOfWork.SaveChangeAsync();

                //// 8️⃣ 🔹 TỰ ĐỘNG SINH LearningPathChapter (và sau này auto tạo LearningPathExercise)
                //try
                //{
                //    await _learningPathChapterService.CreateByCourseAsync(entity.LearningPathCourseId, dto.LearnerCourseId);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"[WARN] Không thể tạo LearningPathChapter tự động: {ex.Message}");
                //}

                // 9️⃣ TRẢ VỀ KẾT QUẢ
                string message = isFreeCourseOfLevel
                    ? "Mở khóa học miễn phí đầu tiên trong level thành công. Chúc bạn học tốt."
                    : "Mở khóa học thành công. Đã trừ xu tương ứng. Chúc bạn học tốt. ";

                return Success(BusinessCode.INSERT_SUCESSFULLY, message, new
                {
                    entity.LearningPathCourseId,
                    entity.LearnerCourseId,
                    entity.CourseId,
                    entity.OrderIndex,
                    entity.Status,
                    RemainingCoins = learnerUser.CoinBalance
                });
            }
            catch (Exception ex)
            {
                return Fail(BusinessCode.EXCEPTION, "Lỗi khi mở khóa học trong lộ trình: " + ex.Message);
            }
        }

        // ============================================================
        // 🔹 UPDATE
        // ============================================================
        public async Task<ResponseDTO> UpdateAsync(Guid id, UpdateLearningPathCourseDTO dto)
        {
            var entity = await _repo.AsQueryable()
                .Include(x => x.Course)
                .Include(x => x.LearnerCourse)
                    .ThenInclude(lc => lc.LearnerProfile)
                .FirstOrDefaultAsync(x => x.LearningPathCourseId == id);

            if (entity == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy khóa học trong lộ trình.");

            // ✅ Đồng bộ lại OrderIndex theo Course
            var realOrderIndex = entity.Course.OrderIndex;
            entity.OrderIndex = realOrderIndex;

            // --- Kiểm tra trùng OrderIndex trong cùng Level ---
            var duplicateOrder = await _repo.AsQueryable()
                .Include(x => x.Course)
                .AnyAsync(x =>
                    x.LearnerCourseId == entity.LearnerCourseId &&
                    x.OrderIndex == realOrderIndex &&
                    x.Course.Level == entity.Course.Level &&
                    x.LearningPathCourseId != entity.LearningPathCourseId);

            if (duplicateOrder)
                return Fail(BusinessCode.DUPLICATE_DATA,
                    $"OrderIndex {realOrderIndex} đã được sử dụng trong level {entity.Course.Level} này.");

            // --- UPDATE PROGRESS ---
            if (dto.Progress.HasValue)
            {
                if (dto.Progress.Value < 0 || dto.Progress.Value > 100)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Progress phải nằm trong khoảng 0 - 100.");
                entity.Progress = dto.Progress.Value;
            }

            // --- UPDATE STATUS ---
            var allowedStatuses = new[] { "NotStarted", "Enrolled", "InProgress", "Completed" };
            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (!allowedStatuses.Contains(dto.Status))
                    return Fail(BusinessCode.INVALID_DATA, $"Trạng thái '{dto.Status}' không hợp lệ.");

                entity.Status = char.ToUpper(dto.Status[0]) + dto.Status.Substring(1).ToLower();
            }

            // ============================================================
            // 🔹 AUTO UPGRADE LEVEL KHI HOÀN THÀNH KHÓA CUỐI CÙNG TRONG LEVEL
            // ============================================================
            //if (entity.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            //{
            //    var learnerProfileId = entity.LearnerCourse.LearnerProfile.LearnerProfileId;
            //    var currentLevel = entity.Course.Level;

            //    // 🔹 Lấy tất cả course thuộc level hiện tại
            //    var levelCourses = await _courseRepo.AsQueryable()
            //        .Where(c => c.Level == currentLevel)
            //        .OrderBy(c => c.OrderIndex)
            //        .ToListAsync();

            //    // 🔹 Xác định khóa cuối cùng trong level
            //    int maxOrderIndex = levelCourses.Max(c => c.OrderIndex);

            //    // 🔹 Nếu chính khóa này là khóa cuối cùng trong level
            //    if (entity.Course.OrderIndex == maxOrderIndex)
            //    {
            //        // Kiểm tra xem học viên đã hoàn thành hết chưa
            //        var completedCount = await _repo.AsQueryable()
            //            .Include(lp => lp.Course)
            //            .Include(lp => lp.LearnerCourse)
            //            .CountAsync(lp =>
            //                lp.LearnerCourse.LearnerProfileId == learnerProfileId &&
            //                lp.Course.Level == currentLevel &&
            //                lp.Status.ToLower() == "completed");

            //        var totalCourses = levelCourses.Count;

            //        if (completedCount == totalCourses && totalCourses > 0)
            //        {
            //            string[] levels = { "A1", "A2", "B1", "B2", "C1", "C2" };
            //            int idx = Array.IndexOf(levels, currentLevel);

            //            if (idx >= 0 && idx < levels.Length - 1)
            //            {
            //                var learnerProfile = await _learnerProfileRepo.GetById(learnerProfileId);
            //                if (learnerProfile != null)
            //                {
            //                    learnerProfile.Level = levels[idx + 1];
            //                    await _learnerProfileRepo.Update(learnerProfile);
            //                }
            //            }
            //        }

            //    }
            //}



            if (entity.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                var learnerProfileId = entity.LearnerCourse.LearnerProfile.LearnerProfileId;
                var currentLevel = entity.Course.Level;

                // --- Lưu ngay trước khi kiểm tra để đảm bảo DB có trạng thái mới nhất ---
                await _repo.Update(entity);
                await _unitOfWork.SaveChangeAsync();

                // 🔹 Lấy tất cả course thuộc level hiện tại
                var levelCourses = await _courseRepo.AsQueryable()
                    .Where(c => c.Level == currentLevel)
                    .OrderBy(c => c.OrderIndex)
                    .ToListAsync();

                int maxOrderIndex = levelCourses.Max(c => c.OrderIndex);

                if (entity.Course.OrderIndex == maxOrderIndex)
                {
                    var completedCount = await _repo.AsQueryable()
                        .Include(lp => lp.Course)
                        .Include(lp => lp.LearnerCourse)
                        .CountAsync(lp =>
                            lp.LearnerCourse.LearnerProfileId == learnerProfileId &&
                            lp.Course.Level == currentLevel &&
                            lp.Status.ToLower() == "completed");

                    var totalCourses = levelCourses.Count;

                    if (completedCount == totalCourses && totalCourses > 0)
                    {
                        string[] levels = { "A1", "A2", "B1", "B2", "C1", "C2" };
                        int idx = Array.IndexOf(levels, currentLevel);

                        if (idx >= 0 && idx < levels.Length - 1)
                        {
                            var learnerProfile = await _learnerProfileRepo.GetById(learnerProfileId);
                            if (learnerProfile != null)
                            {
                                learnerProfile.Level = levels[idx + 1];
                                await _learnerProfileRepo.Update(learnerProfile);
                                await _unitOfWork.SaveChangeAsync(); // lưu luôn thay đổi level
                            }
                        }
                    }
                }
            }
            else
            {
                await _repo.Update(entity);
                await _unitOfWork.SaveChangeAsync();
            }


            // --- Lưu LearningPathCourse sau cùng ---
            await _repo.Update(entity);
            await _unitOfWork.SaveChangeAsync();

            return Success(BusinessCode.UPDATE_SUCESSFULLY, "Cập nhật khóa học trong lộ trình thành công.", new
            {
                entity.LearningPathCourseId,
                entity.CourseId,
                entity.OrderIndex,
                entity.Status,
                entity.Progress
            });
        }




        public async Task<ResponseDTO> GetFullLearningPathCourseAsync(
            Guid? learningPathCourseId,
            Guid? courseId,
            string? status)
        {
            // =============================================
            // 1️⃣ Xác định learningPathCourseId
            // =============================================
            LearningPathCourse lpCourse = null;

            if (learningPathCourseId.HasValue && learningPathCourseId.Value != Guid.Empty)
            {
                lpCourse = await _repo.AsQueryable()
                    .Include(x => x.Course)
                    .FirstOrDefaultAsync(x => x.LearningPathCourseId == learningPathCourseId.Value);
            }
            else if (courseId.HasValue && courseId.Value != Guid.Empty)
            {
                lpCourse = await _repo.AsQueryable()
                    .Include(x => x.Course)
                    .FirstOrDefaultAsync(x => x.CourseId == courseId.Value);
            }

            if (lpCourse == null)
                return Fail(BusinessCode.DATA_NOT_FOUND, "Không tìm thấy LearningPathCourse.");

            // =============================================
            // 2️⃣ Lọc theo status nếu truyền
            // =============================================
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!lpCourse.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                {
                    return Fail(BusinessCode.DATA_NOT_FOUND,
                        $"Không tìm thấy LearningPathCourse với status = {status}.");
                }
            }

            var db = _unitOfWork.GetDbContext();

            // =============================================
            // 3️⃣ Load Chapters + Exercises + Title + Description
            // =============================================
            var chapters = await db.Set<LearningPathChapter>()
                .Where(x => x.LearningPathCourseId == lpCourse.LearningPathCourseId)
                .OrderBy(x => x.OrderIndex)
                .Select(x => new
                {
                    x.LearningPathChapterId,
                    x.ChapterId,
                    x.OrderIndex,
                    x.Status,
                    x.Progress,
                    x.NumberOfModule,

                    // ⭐ THÊM TITLE + DESCRIPTION TỪ BẢNG CHAPTER
                    ChapterTitle = db.Set<Chapter>()
                        .Where(ch => ch.ChapterId == x.ChapterId)
                        .Select(ch => ch.Title)
                        .FirstOrDefault(),

                    ChapterDescription = db.Set<Chapter>()
                        .Where(ch => ch.ChapterId == x.ChapterId)
                        .Select(ch => ch.Description)
                        .FirstOrDefault(),

                    Exercises = db.Set<LearningPathExercise>()
                        .Where(e => e.LearningPathChapterId == x.LearningPathChapterId)
                        .OrderBy(e => e.OrderIndex)
                        .Select(e => new
                        {
                            e.LearningPathExerciseId,
                            e.ExerciseId,
                            e.OrderIndex,
                            e.Status,
                            e.ScoreAchieved,
                            e.NumberOfQuestion,

                            // ⭐ THÊM TITLE + DESCRIPTION TỪ BẢNG EXERCISE
                            ExerciseTitle = db.Set<Exercise>()
                                .Where(ex => ex.ExerciseId == e.ExerciseId)
                                .Select(ex => ex.Title)
                                .FirstOrDefault(),

                            ExerciseDescription = db.Set<Exercise>()
                                .Where(ex => ex.ExerciseId == e.ExerciseId)
                                .Select(ex => ex.Description)
                                .FirstOrDefault(),
                        }).ToList()
                })
                .ToListAsync();

            // =============================================
            // 4️⃣ Trả kết quả
            // =============================================
            return Success(BusinessCode.GET_DATA_SUCCESSFULLY, "Lấy đầy đủ LearningPathCourse thành công.", new
            {
                lpCourse.LearningPathCourseId,
                lpCourse.LearnerCourseId,
                lpCourse.CourseId,
                lpCourse.Status,
                lpCourse.Progress,
                lpCourse.NumberOfChapter,
                lpCourse.OrderIndex,

                Course = new
                {
                    lpCourse.Course.CourseId,
                    lpCourse.Course.Title,
                    lpCourse.Course.Description,
                    lpCourse.Course.Level,
                    lpCourse.Course.Price
                },

                Chapters = chapters
            });
        }





        // ============================================================
        // 🔹 Helper
        // ============================================================
        private static ResponseDTO Success(BusinessCode code, string msg, object? data = null)
            => new ResponseDTO { IsSucess = true, BusinessCode = code, Message = msg, Data = data };

        private static ResponseDTO Fail(BusinessCode code, string msg)
            => new ResponseDTO { IsSucess = false, BusinessCode = code, Message = msg };
    }
}
