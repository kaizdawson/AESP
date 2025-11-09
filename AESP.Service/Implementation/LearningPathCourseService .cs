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
        private readonly IUnitOfWork _unitOfWork;

        public LearningPathCourseService(
            IGenericRepository<LearningPathCourse> repo,
            IGenericRepository<Course> courseRepo,
            IGenericRepository<LearnerCourse> learnerCourseRepo,
            IGenericRepository<LearnerProfile> learnerProfileRepo,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _courseRepo = courseRepo;
            _learnerCourseRepo = learnerCourseRepo;
            _learnerProfileRepo = learnerProfileRepo;
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

                int orderIndex = course.OrderIndex;

                // Kiểm tra trùng course hoặc orderindex
                if (await _repo.AsQueryable().AnyAsync(x =>
                    x.LearnerCourseId == dto.LearnerCourseId && x.CourseId == dto.CourseId))
                    return Fail(BusinessCode.DUPLICATE_DATA, "Khóa học này đã có trong lộ trình.");

                // ✅ Sửa chỗ này - chỉ check trùng OrderIndex trong cùng level
                if (await _repo.AsQueryable()
                    .Include(x => x.Course)
                    .AnyAsync(x => x.LearnerCourseId == dto.LearnerCourseId
                                && x.OrderIndex == orderIndex
                                && x.Course.Level == course.Level))
                    return Fail(BusinessCode.DUPLICATE_DATA,
                        $"OrderIndex {orderIndex} đã được sử dụng trong level {course.Level} này.");


                // Kiểm tra khóa trước đã completed chưa
                if (orderIndex > 1)
                {
                    var prev = await _repo.AsQueryable().FirstOrDefaultAsync(x =>
                        x.LearnerCourseId == dto.LearnerCourseId && x.OrderIndex == orderIndex - 1);

                    if (prev == null || !string.Equals(prev.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                        return Fail(BusinessCode.INVALID_ACTION, "Bạn cần hoàn thành khóa học trước đó trước khi mở khóa tiếp theo.");
                }

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

                    learnerUser.CoinBalance -= price;
                    _courseRepo.GetDbContext().Set<User>().Update(learnerUser);
                }

                var entity = new LearningPathCourse
                {
                    LearningPathCourseId = Guid.NewGuid(),
                    LearnerCourseId = dto.LearnerCourseId,
                    CourseId = dto.CourseId,
                    OrderIndex = orderIndex,
                    Status = "NotStarted",
                    Progress = 0,
                    NumberOfChapter = course.NumberOfChapter
                };

                await _repo.Insert(entity);
                await _unitOfWork.SaveChangeAsync();

                string message = isFreeCourseOfLevel
                    ? "Mở khóa học miễn phí đầu tiên trong level thành công."
                    : "Mở khóa học thành công. Đã trừ xu tương ứng.";

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

            if (dto.OrderIndex <= 0)
                return Fail(BusinessCode.VALIDATION_FAILED, "Thứ tự (OrderIndex) phải lớn hơn 0.");

            var duplicateOrder = await _repo.AsQueryable()
     .Include(x => x.Course)
     .AnyAsync(x =>
         x.LearnerCourseId == entity.LearnerCourseId &&
         x.OrderIndex == dto.OrderIndex &&
         x.Course.Level == entity.Course.Level && // chỉ check trùng trong cùng level
         x.LearningPathCourseId != entity.LearningPathCourseId);

            if (duplicateOrder)
                return Fail(BusinessCode.DUPLICATE_DATA,
                    $"OrderIndex {dto.OrderIndex} đã được sử dụng trong level {entity.Course.Level} này.");


            if (dto.Progress.HasValue)
            {
                if (dto.Progress.Value < 0 || dto.Progress.Value > 100)
                    return Fail(BusinessCode.VALIDATION_FAILED, "Progress phải nằm trong khoảng 0 - 100.");
                entity.Progress = dto.Progress.Value;
            }

            var allowedStatuses = new[] { "NotStarted", "Enrolled", "InProgress", "Completed" };
            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (!allowedStatuses.Contains(dto.Status))
                    return Fail(BusinessCode.INVALID_DATA, $"Trạng thái '{dto.Status}' không hợp lệ.");

                entity.Status = dto.Status;
            }

            entity.OrderIndex = dto.OrderIndex;

            // ✅ Nếu học xong (Completed) → kiểm tra qua level
            if (entity.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                var learner = entity.LearnerCourse.LearnerProfile;
                var currentLevel = entity.Course.Level;

                var levelCourses = await _courseRepo.AsQueryable()
                    .Where(c => c.Level == currentLevel)
                    .Select(c => c.CourseId)
                    .ToListAsync();

                var completedCount = await _repo.AsQueryable()
                    .CountAsync(lp => lp.LearnerCourse.LearnerProfileId == learner.LearnerProfileId
                                   && lp.Status.ToLower() == "completed"
                                   && levelCourses.Contains(lp.CourseId));

                if (completedCount == levelCourses.Count && levelCourses.Count > 0)
                {
                    string[] levels = { "A1", "A2", "B1", "B2", "C1", "C2" };
                    int idx = Array.IndexOf(levels, learner.Level);
                    if (idx >= 0 && idx < levels.Length - 1)
                    {
                        learner.Level = levels[idx + 1];
                        await _learnerProfileRepo.Update(learner);
                    }
                }
            }

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

        // ============================================================
        // 🔹 Helper
        // ============================================================
        private static ResponseDTO Success(BusinessCode code, string msg, object? data = null)
            => new ResponseDTO { IsSucess = true, BusinessCode = code, Message = msg, Data = data };

        private static ResponseDTO Fail(BusinessCode code, string msg)
            => new ResponseDTO { IsSucess = false, BusinessCode = code, Message = msg };
    }
}
