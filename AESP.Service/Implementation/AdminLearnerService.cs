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

        //public async Task<ResponseDTO> GetActiveLearnersAsync(string? search, int pageNumber, int pageSize, string? filterStatus)
        //{
        //    var dto = new ResponseDTO();
        //    try
        //    {
        //        var db = _learnerProfileRepository.GetDbContext();

        //        var query = db.LearnerProfiles
        //            .Include(l => l.User)
        //            .Include(l => l.Subscriptions)
        //                .ThenInclude(s => s.ServicePackage)
        //            .AsQueryable();

        //        if (!string.IsNullOrEmpty(search))
        //        {
        //            string keyword = search.Trim().ToLower();
        //            query = query.Where(l => l.User.FullName.ToLower().Contains(keyword)
        //                                  || l.User.Email.ToLower().Contains(keyword)
        //                                  || l.User.PhoneNumber.Contains(keyword));
        //        }

        //        var learners = await query.ToListAsync();
        //        DateTime now = DateTime.UtcNow;

        //        var mapped = learners.Select(l =>
        //        {
        //            double daysInactive = (now - (l.User.LastActiveAt ?? l.User.CreatedAt)).TotalDays;
        //            string status;

        //            if (l.User.IsDeleted)
        //                status = "Banned";
        //            else if (daysInactive > 30)
        //                status = "Inactived";
        //            else
        //                status = "Actived";

        //            return new
        //            {
        //                l.LearnerProfileId,
        //                FullName = l.User.FullName,
        //                Email = l.User.Email,
        //                Phone = l.User.PhoneNumber,
        //                Level = l.Level,
        //                l.PronunciationScore,
        //                Status = status,
        //                CreatedAt = l.User.CreatedAt,
        //                LastActiveAt = l.User.LastActiveAt,
        //                CurrentPackage = l.Subscriptions
        //                    .OrderByDescending(s => s.StartDate)
        //                    .FirstOrDefault()?.ServicePackage?.Name ?? "(Chưa đăng ký)"
        //            };
        //        });

        //        if (!string.IsNullOrEmpty(filterStatus))
        //        {
        //            mapped = mapped.Where(m => m.Status.Equals(filterStatus, StringComparison.OrdinalIgnoreCase));
        //        }

        //        mapped = mapped.OrderBy(m => m.Status == "Banned" ? 3 : m.Status == "Inactived" ? 2 : 1)
        //                       .ThenBy(m => m.FullName);

        //        var totalItems = mapped.Count();
        //        var pagedItems = mapped.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        //        dto.IsSucess = true;
        //        dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
        //        dto.Message = "Lấy danh sách người học thành công.";
        //        dto.Data = new
        //        {
        //            PageNumber = pageNumber,
        //            PageSize = pageSize,
        //            TotalItems = totalItems,
        //            Items = pagedItems
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        dto.IsSucess = false;
        //        dto.BusinessCode = BusinessCode.EXCEPTION;
        //        dto.Message = "Lỗi khi lấy danh sách người học: " + ex.Message;
        //    }

        //    return dto;
        //}

        //public async Task<ResponseDTO> GetLearnerDetailAsync(Guid learnerProfileId)
        //{
        //    var dto = new ResponseDTO();
        //    try
        //    {
        //        var db = _learnerProfileRepository.GetDbContext();

        //        var learner = await db.LearnerProfiles
        //            .Include(l => l.User)
        //            .Include(l => l.Subscriptions)
        //                .ThenInclude(s => s.ServicePackage)
        //            .FirstOrDefaultAsync(l => l.LearnerProfileId == learnerProfileId);

        //        if (learner == null)
        //        {
        //            dto.IsSucess = false;
        //            dto.Message = "Không tìm thấy người học.";
        //            dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
        //            return dto;
        //        }

        //        // ====== Logic xử lý ======
        //        var now = DateTime.UtcNow;

        //        // Gói học hiện tại (gói đang hoạt động, có EndDate > hôm nay)
        //        var activeSub = learner.Subscriptions
        //            .Where(s => s.Status == "Active" && s.EndDate.HasValue && s.EndDate.Value > now)
        //            .OrderByDescending(s => s.StartDate)
        //            .FirstOrDefault();

        //        // Tính toán số ngày còn lại
        //        var packages = learner.Subscriptions
        //            .OrderByDescending(s => s.StartDate)
        //            .Select(s =>
        //            {
        //                string status;
        //                if (s.Status == "Active" && s.EndDate.HasValue && s.EndDate.Value > now)
        //                    status = "Đang học";
        //                else if (s.EndDate.HasValue && s.EndDate.Value < now)
        //                    status = "Hoàn thành";
        //                else
        //                    status = "Chưa kích hoạt";

        //                int daysLeft = s.EndDate.HasValue
        //                    ? Math.Max(0, (int)(s.EndDate.Value - now).TotalDays)
        //                    : 0;

        //                return new
        //                {
        //                    s.SubscriptionId,
        //                    PackageName = s.ServicePackage?.Name ?? "(Không xác định)",
        //                    Duration = s.ServicePackage?.Duration ?? 0,
        //                    Price = s.ServicePackage?.Price ?? 0,
        //                    Status = status,
        //                    StartDate = s.StartDate?.ToString("yyyy-MM-dd"),
        //                    EndDate = s.EndDate?.ToString("yyyy-MM-dd"),
        //                    DaysLeft = daysLeft
        //                };
        //            }).ToList();

        //        // Mapping dữ liệu trả về FE
        //        dto.IsSucess = true;
        //        dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
        //        dto.Message = "Lấy chi tiết người học thành công.";
        //        dto.Data = new
        //        {
        //            learner.LearnerProfileId,
        //            learner.User.FullName,
        //            learner.User.Email,
        //            learner.User.PhoneNumber,
        //            learner.Level,
        //            learner.PronunciationScore,
        //            learner.DailyMinutes,
        //            Status = learner.User.IsDeleted ? "Bị chặn" : learner.User.Status,
        //            JoinDate = learner.User.CreatedAt.ToString("yyyy-MM-dd"),
        //            PronunciationLevel = learner.PronunciationScore switch
        //            {
        //                >= 9 => "Nâng cao (Advanced)",
        //                >= 7 => "Khá tốt (Upper Intermediate)",
        //                >= 4 => "Trung bình (Intermediate)",
        //                > 0 => "Cơ bản (Beginner)",
        //                _ => "Chưa có dữ liệu đánh giá"
        //            },

        //            CurrentPackage = activeSub != null ? activeSub.ServicePackage?.Name : "(Chưa có gói hoạt động)",
        //            Packages = packages
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        dto.IsSucess = false;
        //        dto.Message = "Lỗi khi lấy chi tiết người học: " + ex.Message;
        //        dto.BusinessCode = BusinessCode.EXCEPTION;
        //    }

        //    return dto;
        //}
    }
}
