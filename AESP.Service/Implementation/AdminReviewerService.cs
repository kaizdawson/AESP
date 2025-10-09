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
        private readonly IUnitOfWork _unitOfWork;

        public AdminReviewerService(IGenericRepository<ReviewerProfile> reviewerProfileRepository, IGenericRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            _reviewerProfileRepository = reviewerProfileRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> ApproveReviewerAsync(Guid reviewerProfileId)
        {
            ResponseDTO dto = new ResponseDTO();

            try
            {
                var reviewer = await _reviewerProfileRepository.GetByExpression(
                    x => x.ReviewerProfileId == reviewerProfileId,
                    x => x.User,
                    x => x.Certificates
                );

                if (reviewer == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Reviewer không tồn tại hoặc đã bị xóa.";
                    return dto;
                }

                if (reviewer.Status == "Active")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.ALREADY_ACTIVE;
                    dto.Message = "Reviewer đã được duyệt trước đó.";
                    return dto;
                }

                if (reviewer.Certificates == null || !reviewer.Certificates.Any())
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_ERROR;
                    dto.Message = "Reviewer chưa có chứng chỉ. Không thể duyệt.";
                    return dto;
                }

                reviewer.Status = "Active";
                await _reviewerProfileRepository.Update(reviewer);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Duyệt reviewer thành công. Reviewer có thể bắt đầu hoạt động.";

                dto.Data = new
                {
                    reviewer.ReviewerProfileId,
                    reviewer.UserId,
                    FullName = reviewer.User?.FullName,
                    Email = reviewer.User?.Email,
                    reviewer.Status,
                    CertificateCount = reviewer.Certificates.Count
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
                        x.Levels,
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

        public async Task<ResponseDTO> RejectReviewerAsync(Guid reviewerProfileId)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var profile = await _reviewerProfileRepository.GetById(reviewerProfileId);
                if (profile == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy hồ sơ reviewer để từ chối.";
                    return dto;
                }

                profile.Status = "Rejected";
                await _reviewerProfileRepository.Update(profile);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Từ chối reviewer thành công.";
                dto.Data = new
                {
                    profile.ReviewerProfileId,
                    profile.Status
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi từ chối reviewer: " + ex.Message;
            }

            return dto;
        }
    }
}
