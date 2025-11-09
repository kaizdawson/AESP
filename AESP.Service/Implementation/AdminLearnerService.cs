using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Common.Enums;
using AESP.Common.Helpers;
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
    public class AdminLearnerService : IAdminLearnerService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepository;
        private readonly IGenericRepository<Feedback> _feedbackRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IGenericRepository<Notification> _notificationRepository;

        public AdminLearnerService(
            IGenericRepository<User> userRepository,
            IGenericRepository<LearnerProfile> learnerProfileRepository,
            IGenericRepository<Feedback> feedbackRepository,
             IGenericRepository<Notification> notificationRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _learnerProfileRepository = learnerProfileRepository;
            _feedbackRepository = feedbackRepository;
            _notificationRepository = notificationRepository;
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
                        CreatedAt = DateTime.UtcNow
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
                        CreatedAt = DateTime.UtcNow
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

                var query = db.LearnerProfiles
                    .Include(l => l.User)
                    .Include(l => l.LearnerCourses)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    query = query.Where(l =>
                        l.User.FullName.ToLower().Contains(keyword) ||
                        l.User.Email.ToLower().Contains(keyword) ||
                        l.User.PhoneNumber.Contains(keyword));
                }

                // Load course meta 1 lần, tránh query lặp
                var courseMetas = await db.Courses
                    .Select(c => new { c.OrderIndex, c.Title, c.Price, c.Duration })
                    .ToListAsync();

                var now = DateTime.UtcNow;

                // EF không hiểu helper → chuyển ra memory sau khi đã filter đủ
                var learners = await query.ToListAsync();

                var mapped = learners.Select(l =>
                {
                    var daysInactive = (now - (l.User.LastActiveAt ?? l.User.CreatedAt)).TotalDays;
                    var userStatus = l.User.IsDeleted ? "Banned" : (daysInactive > 30 ? "Inactive" : "Active");

                    // Khóa đang học (Enrolled) mới là “current”
                    var current = l.LearnerCourses
                        .Where(c => StatusHelper.EqualsCourseStatus(c.Status, CourseStatus.Enrolled))
                        .OrderByDescending(c => c.GeneratedDate)
                        .FirstOrDefault();

                    ReadLearnerCourseDTOS? currentDto = null;
                    if (current != null)
                    {
                        var meta = courseMetas.FirstOrDefault(m => m.OrderIndex == current.NumberOfCourse);
                        currentDto = new ReadLearnerCourseDTOS
                        {
                            LearnerCourseId = current.LearnerCourseId,
                            NumberOfCourse = current.NumberOfCourse,
                            Status = StatusHelper.ToCourseStatus(current.Status),
                            Progress = current.Progress,
                            Title = meta?.Title ?? $"Course #{current.NumberOfCourse}",
                            Price = meta?.Price ?? 0,
                            Duration = meta?.Duration ?? 0,
                            StartTime = current.GeneratedDate,
                            EndTime = current.GeneratedDate.AddDays(meta?.Duration ?? 0)
                        };
                    }

                    return new
                    {
                        l.LearnerProfileId,
                        FullName = l.User.FullName,
                        Email = l.User.Email,
                        Phone = l.User.PhoneNumber,
                        Level = l.Level,
                        PronunciationScore = l.PronunciationScore,
                        Status = userStatus,
                        JoinDate = l.User.CreatedAt,
                        LastActiveAt = l.User.LastActiveAt,

                        // Tối giản cho list
                        CurrentCourseTitle = currentDto?.Title ?? "(Chưa ghi danh khóa nào)",
                        CurrentCourseStatus = currentDto?.Status.ToString() ?? "-",
                        CurrentCourseStart = currentDto?.StartTime,
                        CurrentCourseEnd = currentDto?.EndTime
                    };
                });

                if (!string.IsNullOrWhiteSpace(filterStatus))
                    mapped = mapped.Where(m => string.Equals(m.Status, filterStatus, StringComparison.OrdinalIgnoreCase));

                mapped = mapped
                    .OrderBy(m => m.Status == "Banned" ? 3 : m.Status == "Inactive" ? 2 : 1)
                    .ThenBy(m => m.FullName);

                var total = mapped.Count();
                var items = mapped.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách người học thành công.";
                dto.Data = new { PageNumber = pageNumber, PageSize = pageSize, TotalItems = total, Items = items };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách người học: " + ex.Message;
            }
            return dto;
        }

        // ============================================================
        // ✅ 3️⃣ Chi tiết học viên (Profile + Course Progress + Pronunciation)
        // ============================================================
        public async Task<ResponseDTO> GetLearnerDetailAsync(Guid learnerProfileId)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _learnerProfileRepository.GetDbContext();

                // --- B1: Load learner + các quan hệ cần thiết ---
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


                var courseMetas = await db.Courses
           .Select(c => new { c.OrderIndex, c.Title, c.Price, c.Duration })
           .ToListAsync();

                // Xác định course hiện tại = Enrolled mới là “đang học”
                var current = learner.LearnerCourses
                    .Where(c => StatusHelper.EqualsCourseStatus(c.Status, CourseStatus.Enrolled))
                    .OrderByDescending(c => c.GeneratedDate)
                    .FirstOrDefault();

                ReadLearnerCourseDTOS? currentDto = null;
                if (current != null)
                {
                    var meta = courseMetas.FirstOrDefault(m => m.OrderIndex == current.NumberOfCourse);
                    currentDto = new ReadLearnerCourseDTOS
                    {
                        LearnerCourseId = current.LearnerCourseId,
                        NumberOfCourse = current.NumberOfCourse,
                        Status = StatusHelper.ToCourseStatus(current.Status),
                        Progress = current.Progress,
                        Title = meta?.Title ?? $"Course #{current.NumberOfCourse}",
                        Price = meta?.Price ?? 0,
                        Duration = meta?.Duration ?? 0,
                        StartTime = current.GeneratedDate,
                        EndTime = current.GeneratedDate.AddDays(meta?.Duration ?? 0)
                    };
                }

                // Danh sách đã hoàn thành
                var completedDtos = learner.LearnerCourses
                    .Where(c => StatusHelper.EqualsCourseStatus(c.Status, CourseStatus.Completed))
                    .OrderByDescending(c => c.GeneratedDate)
                    .Select(c =>
                    {
                        var meta = courseMetas.FirstOrDefault(m => m.OrderIndex == c.NumberOfCourse);
                        return new ReadLearnerCourseDTOS
                        {
                            LearnerCourseId = c.LearnerCourseId,
                            NumberOfCourse = c.NumberOfCourse,
                            Status = CourseStatus.Completed,
                            Progress = c.Progress,
                            Title = meta?.Title ?? $"Course #{c.NumberOfCourse}",
                            Price = meta?.Price ?? 0,
                            Duration = meta?.Duration ?? 0,
                            StartTime = c.GeneratedDate,
                            EndTime = c.GeneratedDate.AddDays(meta?.Duration ?? 0)
                        };
                    })
                    .ToList();

                var result = new ReadLearnerDetailDTO
                {
                    LearnerProfileId = learner.LearnerProfileId,
                    FullName = learner.User.FullName,
                    Email = learner.User.Email,
                    PhoneNumber = learner.User.PhoneNumber,
                    Level = learner.Level,
                    PronunciationScore = learner.PronunciationScore,
                    DailyMinutes = learner.DailyMinutes,
                    Status = learner.User.IsDeleted ? "Banned" : learner.User.Status,
                    JoinDate = learner.User.CreatedAt,
                    LastActiveAt = learner.User.LastActiveAt,
                    CurrentCourse = currentDto,
                    CompletedCourses = completedDtos,
                    AssessmentCount = learner.Assessments.Count,
                    AvgScore = learner.Assessments.Any() ? learner.Assessments.Average(a => a.Score) : 0
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
