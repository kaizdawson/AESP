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

                // ✅ Chỉ duyệt reviewer nếu họ đang Pending
                if (profile.Status != "Pending")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_ERROR;
                    dto.Message = "Reviewer này không ở trạng thái Pending.";
                    return dto;
                }

                // ✅ Đổi reviewer sang trạng thái Active
                profile.Status = "Active";
                await _reviewerProfileRepository.Update(profile);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Duyệt reviewer thành công.";
                dto.Data = new
                {
                    profile.ReviewerProfileId,
                    profile.Status,
                    Certificate = new
                    {
                        certificate.CertificateId,
                        certificate.Name,
                        certificate.Url
                    }
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

        public async Task<ResponseDTO> GetPendingReviewersAsync(int pageNumber, int pageSize)
        {
            ResponseDTO dto = new ResponseDTO();

            try
            {
                var dbContext = _reviewerProfileRepository.GetDbContext();

                var query = dbContext.ReviewerProfiles
                    .Include(x => x.User)
                    .Include(x => x.Certificates)
                    .Where(x => x.Status == "Pending");

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

                profile.Status = "Rejected";
                await _reviewerProfileRepository.Update(profile);
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
                    profile.ReviewerProfileId,
                    profile.Status,
                    Certificate = new
                    {
                        certificate.CertificateId,
                        certificate.Name,
                        certificate.Url
                    }
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
    }
}
