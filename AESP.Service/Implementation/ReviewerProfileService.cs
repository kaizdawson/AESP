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
                    profile.Levels,
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

        public async Task<ResponseDTO> UpdateProfileAsync(Guid userId, UpdateReviewerProfileDTO request)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                // ✅ 1. Kiểm tra hồ sơ có tồn tại
                var profile = await _reviewerProfileRepository.GetFirstByExpression(
                    x => x.UserId == userId,
                    x => x.Certificates
                );

                if (profile == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy hồ sơ Reviewer.";
                    return dto;
                }

                // ✅ 2. Kiểm tra trạng thái hồ sơ (chỉ cho phép cập nhật khi Pending hoặc Rejected)
                if (profile.Status == "Active")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_ACTION;
                    dto.Message = "Hồ sơ Reviewer đã được duyệt, không thể chỉnh sửa.";
                    return dto;
                }

                // ✅ 3. Validate nội dung
                if (string.IsNullOrWhiteSpace(request.Experience) || request.Experience.Length < 10)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Kinh nghiệm phải có ít nhất 10 ký tự mô tả.";
                    return dto;
                }

                var validLevels = new[] { "A1", "A2", "B1", "B2", "C1", "C2" };
                if (!validLevels.Contains(request.Levels))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Trình độ không hợp lệ. Vui lòng chọn từ A1 đến C2.";
                    return dto;
                }

                // ✅ 4. Cập nhật thông tin
                profile.Experience = request.Experience.Trim();
                profile.Levels = request.Levels.Trim().ToUpper();
                profile.Status = "Pending"; // reset để admin duyệt lại

                await _reviewerProfileRepository.Update(profile);
                await _unitOfWork.SaveChangeAsync();

                // ✅ 5. Trả về kết quả
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Cập nhật hồ sơ Reviewer thành công. Vui lòng chờ Admin duyệt.";
                dto.Data = new
                {
                    profile.ReviewerProfileId,
                    profile.Status,
                    profile.Experience,
                    profile.Levels,
                    Certificates = profile.Certificates.Select(c => new
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
