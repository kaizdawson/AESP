using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class ReviewerProfileService : IReviewerProfileService
    {
        private readonly IGenericRepository<ReviewerProfile> _reviewerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewerProfileService(IGenericRepository<ReviewerProfile> reviewerProfileRepository, IUnitOfWork unitOfWork)
        {
            _reviewerProfileRepository = reviewerProfileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetByUserIdAsync(Guid userId)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                // Load kèm Certificates, Wallet, Reviews (nếu cần)
                var profile = await _reviewerProfileRepository.GetFirstByExpression(
                    x => x.UserId == userId,
                    x => x.Certificates,
                    x => x.Wallet,
                    x => x.Reviews
                );

                if (profile == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy hồ sơ Reviewer.";
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy hồ sơ Reviewer thành công.";
                dto.Data = new
                {
                    profile.ReviewerProfileId,
                    profile.UserId,
                    profile.Experience,
                    profile.Rating,
                    profile.Status,
                    
                    Certificates = profile.Certificates?.Select(c => new
                    {
                        c.CertificateId,
                        c.Name,
                        c.Url
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy hồ sơ reviewer: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> UpdateProfileAsync(Guid userId, ReviewerProfileUpdateDto request)
        {
            ResponseDTO dto = new ResponseDTO();

            try
            {
                var profile = await _reviewerProfileRepository.GetFirstByExpression(
                    x => x.UserId == userId,
                    x => x.Certificates,
                    x => x.User
                );

                if (profile == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy hồ sơ Reviewer.";
                    return dto;
                }

                // ✅ Chặn update khi vẫn là Draft (chưa nộp chứng chỉ)
                if (profile.Status == "Draft")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_ACTION;
                    dto.Message = "Vui lòng nộp chứng chỉ trước khi cập nhật hồ sơ.";
                    return dto;
                }

                // ✅ Validate input
                if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length < 3)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Họ và tên phải có ít nhất 3 ký tự.";
                    return dto;
                }

                var phone = request.PhoneNumber?.Trim();
                if (string.IsNullOrEmpty(phone) || !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9}$"))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Số điện thoại không hợp lệ. Phải có 10 chữ số và bắt đầu bằng 0.";
                    return dto;
                }

                if (string.IsNullOrWhiteSpace(request.Experience) || request.Experience.Length < 10)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Kinh nghiệm phải có ít nhất 10 ký tự mô tả.";
                    return dto;
                }

                // ✅ Cập nhật dữ liệu
                profile.Experience = request.Experience.Trim();
                profile.User.FullName = request.FullName.Trim();
                profile.User.PhoneNumber = request.PhoneNumber.Trim();

                // Không đổi status ở đây nữa
                await _reviewerProfileRepository.Update(profile);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Cập nhật hồ sơ Reviewer thành công.";
                dto.Data = new
                {
                    profile.ReviewerProfileId,
                    profile.Status,
                    profile.Experience,
                    FullName = profile.User.FullName,
                    PhoneNumber = profile.User.PhoneNumber,
                    Certificates = profile.Certificates?.Select(c => new
                    {
                        c.CertificateId,
                        c.Name,
                        c.Url
                    })
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi cập nhật hồ sơ Reviewer: " + ex.Message;
            }

            return dto;

        }
    }
}
