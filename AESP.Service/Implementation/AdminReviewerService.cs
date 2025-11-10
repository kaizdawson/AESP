using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class AdminReviewerService : IAdminReviewerService
    {
        private readonly IGenericRepository<ReviewerProfile> _reviewerProfileRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Certificate> _certificateRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AdminReviewerService(IGenericRepository<ReviewerProfile> reviewerProfileRepository, IGenericRepository<User> userRepository, IGenericRepository<Certificate> certificateRepository, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _reviewerProfileRepository = reviewerProfileRepository;
            _userRepository = userRepository;
            _certificateRepository = certificateRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<ResponseDTO> ApproveReviewerByCertificateAsync(Guid certificateId)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var certificate = await _certificateRepository.GetById(certificateId);
                if (certificate == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy chứng chỉ.";
                    return dto;
                }

                var profile = await _reviewerProfileRepository.GetById(certificate.ReviewerProfileId);
                if (profile == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy hồ sơ reviewer tương ứng.";
                    return dto;
                }

                certificate.Status = "Approved";
                await _certificateRepository.Update(certificate);

                //  Chỉ duyệt reviewer nếu họ đang Pending
                if (profile.Status != "Pending")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_ERROR;
                    dto.Message = "Reviewer này không ở trạng thái Pending.";
                    return dto;
                }

                //  Đổi reviewer sang trạng thái Active
                profile.Status = "Active";
                await _reviewerProfileRepository.Update(profile);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Duyệt chứng chỉ thành công. Reviewer đã được Active.";
                dto.Data = new
                {
                    ReviewerProfileId = profile.ReviewerProfileId,
                    ReviewerStatus = profile.Status,
                    CertificateId = certificate.CertificateId,
                    CertificateStatus = certificate.Status
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi duyệt reviewer: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> BanReviewerAsync(Guid userId, string reason)
        {
            var dto = new ResponseDTO();

            try
            {
                // 🔹 Lấy thông tin User
                var user = await _userRepository.GetById(userId);
                if (user == null)
                {
                    dto.IsSucess = false;
                    dto.Message = "Không tìm thấy người dùng.";
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    return dto;
                }

                // 🔹 Lấy Reviewer Profile
                var reviewerProfile = await _reviewerProfileRepository
                    .GetFirstByExpression(r => r.UserId == userId);

                bool isCurrentlyBanned = user.IsDeleted || reviewerProfile?.IsDeleted == true || reviewerProfile?.Status == "Banned";

                // =======================
                // CASE 1: BAN REVIEWER
                // =======================
                if (!isCurrentlyBanned)
                {
                    user.IsDeleted = true;

                    if (reviewerProfile != null)
                    {
                        reviewerProfile.IsDeleted = true;
                        reviewerProfile.Status = "Banned";
                        await _reviewerProfileRepository.Update(reviewerProfile);
                    }

                    await _userRepository.Update(user);

                    // 📩 Gửi email thông báo có lý do
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        string subject = "AESP System - Tài khoản Reviewer bị khóa";
                        string body = $@"
Xin chào {user.FullName},

Tài khoản Reviewer của bạn trên hệ thống AESP đã bị khóa bởi quản trị viên.

🔹 Lý do: {reason}

Nếu bạn cho rằng đây là nhầm lẫn, vui lòng phản hồi email này để được xem xét lại.

Trân trọng,
Đội ngũ Quản trị AESP.";
                        await _emailService.SendEmailAsync(user.Email, subject, body);
                    }

                    // 🧾 Ghi Notification
                    var noti = new Notification
                    {
                        NotificationId = Guid.NewGuid(),
                        UserId = user.UserId,
                        Message = $"Tài khoản Reviewer của bạn đã bị khóa. Lý do: {reason}",
                        Type = "Account",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    var db = _reviewerProfileRepository.GetDbContext();
                    await db.Notifications.AddAsync(noti);

                    await _unitOfWork.SaveChangeAsync();

                    dto.IsSucess = true;
                    dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                    dto.Message = "Đã khóa reviewer và gửi thông báo qua email.";
                }

                // =======================
                // CASE 2: UNBAN REVIEWER
                // =======================
                else
                {
                    user.IsDeleted = false;

                    if (reviewerProfile != null)
                    {
                        reviewerProfile.IsDeleted = false;
                        reviewerProfile.Status = "Active";
                        await _reviewerProfileRepository.Update(reviewerProfile);
                    }

                    await _userRepository.Update(user);

                    // 📩 Gửi email mở khóa
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        string subject = "AESP System - Tài khoản Reviewer đã được mở khóa";
                        string body = $@"
Xin chào {user.FullName},

Tài khoản Reviewer của bạn đã được mở khóa và có thể hoạt động bình thường trở lại.

Trân trọng,
Đội ngũ Quản trị AESP.";
                        await _emailService.SendEmailAsync(user.Email, subject, body);
                    }

                    // 🧾 Notification mở khóa
                    var noti = new Notification
                    {
                        NotificationId = Guid.NewGuid(),
                        UserId = user.UserId,
                        Message = "Tài khoản Reviewer của bạn đã được mở khóa bởi quản trị viên.",
                        Type = "Account",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    var db = _reviewerProfileRepository.GetDbContext();
                    await db.Notifications.AddAsync(noti);

                    await _unitOfWork.SaveChangeAsync();

                    dto.IsSucess = true;
                    dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                    dto.Message = "Đã mở khóa reviewer và gửi thông báo qua email.";
                }
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.Message = "Lỗi khi cập nhật trạng thái reviewer: " + ex.Message;
                dto.BusinessCode = BusinessCode.EXCEPTION;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetActiveReviewersAsync(string? search, int pageNumber, int pageSize, string? filterStatus)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _reviewerProfileRepository.GetDbContext();

                // Lấy toàn bộ reviewer (đã duyệt) — không loại bỏ bị ban, để còn sort hiển thị
                var query = db.ReviewerProfiles
                    .Include(r => r.User)
                    .Where(r => r.User.Role == "REVIEWER" && r.Status == "Active" || r.Status == "Banned");

                // 🔍 Tìm kiếm theo tên hoặc email
                if (!string.IsNullOrEmpty(search))
                {
                    string keyword = search.Trim().ToLower();
                    query = query.Where(r => r.User.FullName.ToLower().Contains(keyword)
                                          || r.User.Email.ToLower().Contains(keyword));
                }

                var reviewers = await query.ToListAsync();
                DateTime now = DateTime.UtcNow;

                // ✅ Xử lý logic trạng thái động (3 loại)
                var mapped = reviewers.Select(r =>
                {
                    double daysInactive = (now - (r.User.LastActiveAt ?? r.User.CreatedAt)).TotalDays;

                    string status;
                    if (r.User.IsDeleted || r.IsDeleted)
                        status = "Banned";
                    else if (daysInactive > 30)
                        status = "Inactived";
                    else
                        status = "Actived";

                    return new
                    {
                        r.ReviewerProfileId,
                        UserId = r.User.UserId,
                        FullName = r.User.FullName,
                        Email = r.User.Email,
                        Phone = r.User.PhoneNumber,
                        Level = r.Level,
                        Experience = r.Experience,
                        Rating = r.Rating,
                        Status = status,
                        LastActiveAt = r.User.LastActiveAt,
                        CreatedAt = r.User.CreatedAt
                    };
                });

                // ✅ Lọc theo trạng thái FE truyền vào
                if (!string.IsNullOrEmpty(filterStatus))
                {
                    mapped = mapped.Where(m => m.Status.Equals(filterStatus, StringComparison.OrdinalIgnoreCase));
                }

                // ✅ Sort thứ tự ưu tiên: Actived -> Inactived -> Banned
                mapped = mapped.OrderBy(m => m.Status == "Banned" ? 3 : m.Status == "Inactived" ? 2 : 1)
                               .ThenByDescending(m => m.Rating);

                // ✅ Phân trang
                var totalItems = mapped.Count();
                var pagedItems = mapped.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách reviewer thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = pagedItems
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách reviewer: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetPendingReviewersAsync(int pageNumber, int pageSize)
        {
            ResponseDTO dto = new ResponseDTO();

            try
            {
                var dbContext = _reviewerProfileRepository.GetDbContext();

                // ✅ Bỏ lỗi khi có space hoặc khác hoa thường
                var query = dbContext.ReviewerProfiles
                    .Include(x => x.User)
                    .Include(x => x.Certificates)
                    .Where(x => x.Status.Trim().ToLower() == "pending");

                var reviewers = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (reviewers == null || !reviewers.Any())
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không có reviewer nào đang chờ duyệt.";
                    dto.Data = new
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Items = new List<object>()
                    };
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách reviewer đang chờ duyệt thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Items = reviewers.Select(x => new
                    {
                        x.ReviewerProfileId,
                        UserId = x.User.UserId,
                        FullName = x.User.FullName,
                        Email = x.User.Email,
                        Phone = x.User.PhoneNumber,
                        x.Experience,
                        x.Status,
                        HasCertificate = x.Certificates.Any(),
                        Certificates = x.Certificates.Select(c => new
                        {
                            c.CertificateId,
                            c.Name,
                            c.Url
                        })
                    })
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách reviewer: " + ex.Message;
            }

            return dto;

        }

        public async Task<ResponseDTO> GetReviewerDetailAsync(Guid reviewerProfileId)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _reviewerProfileRepository.GetDbContext();

                // 🔹 Lấy thông tin reviewer
                var reviewer = await db.ReviewerProfiles
                    .Include(r => r.User)
                    .Include(r => r.Certificates)
                    .FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);

                if (reviewer == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy thông tin reviewer.";
                    return dto;
                }

                // 🔹 Lấy feedback từ bảng Feedback (Learner -> Reviewer)
                var feedbacks = await db.Feedbacks
                    .Include(f => f.User)
                    .Where(f => f.TargetId == reviewerProfileId && f.Type == "ReviewerFeedback")
                    .OrderByDescending(f => f.CreatedAt)
                    .Select(f => new
                    {
                        LearnerName = f.User.FullName,
                        LearnerEmail = f.User.Email,
                        LearnerPhone = f.User.PhoneNumber,
                        Rating = f.Rating,
                        Comment = string.IsNullOrEmpty(f.Content) ? "(Không có nhận xét)" : f.Content,
                        Date = f.CreatedAt
                    })
                    .ToListAsync();

                // 🔹 Trả kết quả
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy thông tin chi tiết reviewer thành công.";
                dto.Data = new
                {
                    reviewer.ReviewerProfileId,
                    UserId = reviewer.User.UserId,
                    reviewer.User.FullName,
                    reviewer.User.Email,
                    reviewer.Experience,
                    reviewer.User.PhoneNumber,
                    reviewer.Level,
                    reviewer.Rating,
                    reviewer.Status,
                    reviewer.User.CreatedAt,
                    Certificates = reviewer.Certificates.Select(c => new
                    {
                        c.CertificateId,
                        c.Name,
                        c.Url
                    }),
                    Feedbacks = feedbacks.Select(f => new
                    {
                        Learner = new
                        {
                            f.LearnerName,
                            f.LearnerEmail,
                            f.LearnerPhone
                        },
                        Rating = f.Rating,
                        Comment = f.Comment,
                        Date = f.Date.ToString("yyyy-MM-dd")
                    })
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy chi tiết reviewer: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> RejectReviewerByCertificateAsync(Guid certificateId)
        {
            ResponseDTO dto = new ResponseDTO();

            try
            {
                var certificate = await _certificateRepository.GetById(certificateId);
                if (certificate == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy chứng chỉ.";
                    return dto;
                }

                var profile = await _reviewerProfileRepository
                    .GetByExpression(x => x.ReviewerProfileId == certificate.ReviewerProfileId, x => x.User);

                if (profile == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy hồ sơ reviewer.";
                    return dto;
                }
                certificate.Status = "Rejected";
                await _certificateRepository.Update(certificate);

                if (profile.Status == "Pending")
                {
                    await _reviewerProfileRepository.Update(profile);
                }

                await _unitOfWork.SaveChangeAsync();

                // ✅ Gửi email thông báo cho reviewer
                if (!string.IsNullOrEmpty(profile.User?.Email))
                {
                    string subject = "AESP System - Chứng chỉ của bạn bị từ chối";
                    string body =
$@"Xin chào {profile.User.FullName},

Chứng chỉ bạn gửi lên hệ thống đã bị từ chối do không hợp lệ hoặc không đạt yêu cầu.

Vui lòng đăng nhập vào hệ thống AESP và gửi lại chứng chỉ mới để được xét duyệt lại.

Trân trọng,
Đội ngũ Quản trị viên AESP System.";

                    await _emailService.SendEmailAsync(profile.User.Email, subject, body);
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Từ chối reviewer thành công.";
                dto.Data = new
                {
                    ReviewerProfileId = profile.ReviewerProfileId,
                    ReviewerStatus = profile.Status,
                    CertificateId = certificate.CertificateId,
                    CertificateStatus = certificate.Status
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi từ chối reviewer: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> UpdateReviewerLevelAsync(Guid reviewerProfileId, string newLevel)
        {
            ResponseDTO dto = new ResponseDTO();

            try
            {
                if (string.IsNullOrWhiteSpace(newLevel))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Level không được để trống.";
                    return dto;
                }

                var profile = await _reviewerProfileRepository.GetFirstByExpression(
                    x => x.ReviewerProfileId == reviewerProfileId,
                    x => x.User
                );

                if (profile == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy hồ sơ reviewer.";
                    return dto;
                }

                profile.Level = newLevel.Trim();

                await _reviewerProfileRepository.Update(profile);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = $"Cập nhật cấp độ reviewer thành công.";
                dto.Data = new
                {
                    profile.ReviewerProfileId,
                    FullName = profile.User.FullName,
                    NewLevel = profile.Level
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi cập nhật cấp độ reviewer: " + ex.Message;
            }

            return dto;
        }
    }
}
