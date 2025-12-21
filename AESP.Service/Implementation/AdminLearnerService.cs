using AESP.API.Helpers;
using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Common.Enums;
using AESP.Common.Helpers;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class AdminLearnerService : IAdminLearnerService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepository;
        private readonly IGenericRepository<Feedback> _feedbackRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IGenericRepository<Notification> _notificationRepository;
        private readonly IGenericRepository<LearningPathCourse> _learningPathCourseRepository;

        public AdminLearnerService(
             IGenericRepository<User> userRepository,
             IGenericRepository<LearnerProfile> learnerProfileRepository,
             IGenericRepository<Feedback> feedbackRepository,
             IGenericRepository<Notification> notificationRepository,
             IGenericRepository<LearningPathCourse> learningPathCourseRepository,
             IUnitOfWork unitOfWork,
             IEmailService emailService)
        {
            _userRepository = userRepository;
            _learnerProfileRepository = learnerProfileRepository;
            _feedbackRepository = feedbackRepository;
            _notificationRepository = notificationRepository;
            _learningPathCourseRepository = learningPathCourseRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }


        public async Task<ResponseDTO> BanLearnerAsync(Guid userId, string reason)
        {
            var dto = new ResponseDTO();

            try
            {
                var user = await _userRepository.GetById(userId);
                if (user == null)
                {
                    dto.IsSucess = false;
                    dto.Message = "Không tìm thấy người học.";
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    return dto;
                }

                var learnerProfile = await _learnerProfileRepository
                    .GetFirstByExpression(l => l.UserId == userId);

                bool isCurrentlyBanned = user.Status == "Banned" || user.IsDeleted;

                if (!isCurrentlyBanned)
                {
                    // ✅ Thực hiện khóa tài khoản
                    user.IsDeleted = true;
                    user.Status = "Banned";
                    await _userRepository.Update(user);

                    if (learnerProfile != null)
                    {
                        learnerProfile.IsDeleted = true;
                        await _learnerProfileRepository.Update(learnerProfile);
                    }

                    // Gửi email có lý do
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        string subject = "AESP System - Tài khoản học viên bị khóa";
                        string body = $@"
Xin chào {user.FullName},

Tài khoản học viên của bạn đã bị khóa bởi quản trị viên hệ thống.

🔹 Lý do: {reason}

Nếu bạn cho rằng đây là nhầm lẫn, vui lòng liên hệ lại để được xem xét.

Trân trọng,
Đội ngũ Quản trị AESP.";
                        await _emailService.SendEmailAsync(user.Email, subject, body);
                    }

                    // Ghi Notification
                    var notification = new Notification
                    {
                        NotificationId = Guid.NewGuid(),
                        UserId = user.UserId,
                        Message = $"Tài khoản của bạn đã bị khóa. Lý do: {reason}",
                        Type = "Account",
                        IsRead = false,
                        CreatedAt = DateTimeHelper.NowVN()
                    };
                    await _notificationRepository.Insert(notification);

                    await _unitOfWork.SaveChangeAsync();

                    dto.Message = "Đã khóa tài khoản học viên và gửi thông báo.";
                }
                else
                {
                    // ✅ Mở khóa tài khoản
                    user.IsDeleted = false;
                    user.Status = "Active";
                    await _userRepository.Update(user);

                    if (learnerProfile != null)
                    {
                        learnerProfile.IsDeleted = false;
                        await _learnerProfileRepository.Update(learnerProfile);
                    }

                    // Gửi email thông báo mở khóa
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        string subject = "AESP System - Tài khoản học viên đã được mở khóa";
                        string body = $@"
Xin chào {user.FullName},

Tài khoản học viên của bạn đã được mở khóa và có thể truy cập lại hệ thống bình thường.

Trân trọng,
Đội ngũ Quản trị AESP.";
                        await _emailService.SendEmailAsync(user.Email, subject, body);
                    }

                    // Ghi Notification mở khóa
                    var notification = new Notification
                    {
                        NotificationId = Guid.NewGuid(),
                        UserId = user.UserId,
                        Message = "Tài khoản của bạn đã được mở khóa bởi quản trị viên.",
                        Type = "Account",
                        IsRead = false,
                        CreatedAt = DateTimeHelper.NowVN()
                    };
                    await _notificationRepository.Insert(notification);

                    await _unitOfWork.SaveChangeAsync();

                    dto.Message = "Đã mở khóa tài khoản học viên và gửi thông báo.";
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.Message = "Lỗi khi khóa/mở khóa học viên: " + ex.Message;
                dto.BusinessCode = BusinessCode.EXCEPTION;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetActiveLearnersAsync(string? search, int pageNumber, int pageSize, string? filterStatus)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _learnerProfileRepository.GetDbContext();

                // --- B1: Query danh sách học viên ---
                var learnersQuery = db.LearnerProfiles
                    .Include(l => l.User)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    learnersQuery = learnersQuery.Where(l =>
                        l.User.FullName.ToLower().Contains(keyword) ||
                        l.User.Email.ToLower().Contains(keyword) ||
                        l.User.PhoneNumber.Contains(keyword));
                }

                var learners = await learnersQuery.ToListAsync();
                var learnerIds = learners.Select(l => l.LearnerProfileId).ToList();

                // --- B2: Lấy tất cả LearnerCourse của các học viên ---
                var learnerCourses = await db.LearnerCourses
                    .Where(lc => learnerIds.Contains(lc.LearnerProfileId))
                    .ToListAsync();

                // --- B3: Lấy tất cả LearningPathCourse có liên quan ---
                var learnerCourseIds = learnerCourses.Select(lc => lc.LearnerCourseId).ToList();
                var learningPathCourses = await db.LearningPathCourses
                    .Include(lp => lp.Course)
                    .Where(lp => learnerCourseIds.Contains(lp.LearnerCourseId))
                    .ToListAsync();

                var now = DateTimeHelper.NowVN();

                // --- B4: Mapping dữ liệu trả về ---
                var mapped = learners.Select(l =>
                {
                    var daysInactive = (now - (l.User.LastActiveAt ?? l.User.CreatedAt)).TotalDays;
                    var userStatus = l.User.IsDeleted ? "Banned" :
                                     (daysInactive > 30 ? "Inactive" : "Active");

                    // 🔹 Lấy tất cả LearnerCourseId của học viên này
                    var learnerCourseIdsOfLearner = learnerCourses
                        .Where(x => x.LearnerProfileId == l.LearnerProfileId)
                        .Select(x => x.LearnerCourseId)
                        .ToList();

                    // 🔹 Lấy tất cả LearningPathCourse liên quan
                    var lpOfLearner = learningPathCourses
                        .Where(lp => learnerCourseIdsOfLearner.Contains(lp.LearnerCourseId))
                        .ToList();

                    // 🔹 Xác định khóa học hiện tại (Pending hoặc Enrolled)
                    var currentLp = lpOfLearner
                        .Where(lp =>
                            StatusHelper.EqualsCourseStatus(lp.Status, CourseStatus.Pending) ||
                            StatusHelper.EqualsCourseStatus(lp.Status, CourseStatus.Enrolled))
                        .OrderByDescending(lp => lp.OrderIndex)
                        .FirstOrDefault();

                    return new
                    {
                        l.LearnerProfileId,
                        UserId = l.User.UserId,
                        FullName = l.User.FullName,
                        Email = l.User.Email,
                        Phone = l.User.PhoneNumber,
                        Level = l.Level,
                        PronunciationScore = l.PronunciationScore,
                        Status = userStatus,
                        JoinDate = l.User.CreatedAt,
                        LastActiveAt = l.User.LastActiveAt,
                        CurrentCourseTitle = currentLp?.Course?.Title ?? "(Chưa học khóa nào)",
                        CurrentCourseStatus = currentLp != null
                             ? StatusHelper.ToCourseStatus(currentLp.Status).ToString()
                             : "-",
                        CurrentCourseProgress = currentLp?.Progress ?? 0
                    };
                });

                // --- B5: Lọc theo trạng thái (Active / Banned / Inactive) ---
                if (!string.IsNullOrWhiteSpace(filterStatus))
                    mapped = mapped.Where(m => string.Equals(m.Status, filterStatus, StringComparison.OrdinalIgnoreCase));

                // --- B6: Phân trang ---
                var total = mapped.Count();
                var items = mapped
                    .OrderBy(m => m.Status == "Banned" ? 3 : m.Status == "Inactive" ? 2 : 1)
                    .ThenBy(m => m.FullName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // --- B7: Kết quả ---
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách học viên thành công.";
                dto.Data = new { PageNumber = pageNumber, PageSize = pageSize, TotalItems = total, Items = items };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách học viên: " + ex.Message;
            }

            return dto;
        }

         //============================================================
         //✅ 3️⃣ Chi tiết học viên(Profile + Course Progress + Pronunciation)
         //============================================================
        public async Task<ResponseDTO> GetLearnerDetailAsync(Guid learnerProfileId)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _learnerProfileRepository.GetDbContext();

                // --- B1: Load learner + quan hệ cần thiết ---
                var learner = await db.LearnerProfiles
                    .Include(l => l.User)
                    .Include(l => l.LearnerCourses)
                    .Include(l => l.Assessments)
                    .FirstOrDefaultAsync(l => l.LearnerProfileId == learnerProfileId);

                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucess = false,
                        BusinessCode = BusinessCode.DATA_NOT_FOUND,
                        Message = "Không tìm thấy người học."
                    };
                }

                // --- B2: Lấy danh sách tất cả LearningPathCourse của học viên ---
                var learnerCourseIds = learner.LearnerCourses.Select(lc => lc.LearnerCourseId).ToList();

                var learningPathCourses = await db.LearningPathCourses
                    .Include(lp => lp.Course)
                    .Where(lp => learnerCourseIds.Contains(lp.LearnerCourseId))
                    .ToListAsync();

                // --- B3: Xác định khóa học hiện tại ---
                var currentLp = learningPathCourses
                    .Where(lp => StatusHelper.InCourseStatus(lp.Status, CourseStatus.Pending, CourseStatus.Enrolled))
                    .OrderByDescending(lp => lp.OrderIndex)
                    .FirstOrDefault();

                ReadLearnerCourseDTOS? currentDto = null;
                if (currentLp != null)
                {
                    var course = currentLp.Course;
                    currentDto = new ReadLearnerCourseDTOS
                    {
                        Status = StatusHelper.ToCourseStatus(currentLp.Status),
                        Progress = currentLp.Progress,
                        Title = course?.Title ?? "(Không rõ)",
                        Price = course?.Price ?? 0,
                        Duration = course?.Duration ?? 0,
                        StartTime = currentLp.CreatedAt,
                        EndTime = currentLp.CreatedAt.AddDays(course?.Duration ?? 0)
                    };
                }

                // --- B4: Danh sách khóa học đã hoàn thành ---
                var completedDtos = learningPathCourses
                    .Where(lp => StatusHelper.EqualsCourseStatus(lp.Status, CourseStatus.Completed))
                    .OrderBy(lp => lp.OrderIndex)
                    .Select(lp => new ReadLearnerCourseDTOS
                    {
                        Status = CourseStatus.Completed,
                        Progress = lp.Progress,
                        Title = lp.Course?.Title ?? "(Không rõ)",
                        Price = lp.Course?.Price ?? 0,
                        Duration = lp.Course?.Duration ?? 0,
                        StartTime = lp.CreatedAt,
                        EndTime = lp.CreatedAt.AddDays(lp.Course?.Duration ?? 0)
                    })
                    .ToList();
                var allCourses = new List<ReadLearnerCourseDTOS>();

                if (currentDto != null)
                    allCourses.Add(currentDto);

                if (completedDtos.Any())
                    allCourses.AddRange(completedDtos);

                // --- B5: Tổng hợp dữ liệu ---
                var result = new ReadLearnerDetailDTO
                {
                    LearnerProfileId = learner.LearnerProfileId,
                    UserId = learner.User.UserId,
                    FullName = learner.User.FullName,
                    Email = learner.User.Email,
                    PhoneNumber = learner.User.PhoneNumber,
                    Level = learner.Level,
                    PronunciationScore = learner.PronunciationScore,
                    DailyMinutes = learner.DailyMinutes,
                    Status = learner.User.IsDeleted ? "Banned" : learner.User.Status,
                    JoinDate = learner.User.CreatedAt,
                    LastActiveAt = learner.User.LastActiveAt,
                    Courses = allCourses,
                    AssessmentCount = learner.Assessments.Count,
                    AvgScore = learner.Assessments.Any(a => a.Score.HasValue)? learner.Assessments.Where(a => a.Score.HasValue).Average(a => a.Score!.Value): 0
                };

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết người học thành công.";
                dto.Data = result;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy chi tiết người học: " + ex.Message;
            }

            return dto;
        }
    }
}
