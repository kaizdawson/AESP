using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class CertificateService : ICertificateService
    {
        private readonly IGenericRepository<Certificate> _certificateRepository;
        private readonly IGenericRepository<ReviewerProfile> _reviewerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;

        public CertificateService(IGenericRepository<Certificate> certificateRepository, IGenericRepository<ReviewerProfile> reviewerProfileRepository, IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
        {
            _certificateRepository = certificateRepository;
            _reviewerProfileRepository = reviewerProfileRepository;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ResponseDTO> DeleteAsync(Guid certificateId)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var cert = await _certificateRepository.GetById(certificateId);
                if (cert == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy chứng chỉ.";
                    return dto;
                }

                await _certificateRepository.Delete(cert);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
                dto.Message = "Xóa chứng chỉ thành công.";
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi xóa chứng chỉ: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetByReviewerProfileIdAsync(Guid reviewerProfileId)
        {
            ResponseDTO response = new ResponseDTO();
            try
            {
                var reviewerProfile = await _reviewerProfileRepository.GetById(reviewerProfileId);
                if (reviewerProfile == null)
                {
                    response.IsSucess = false;
                    response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    response.Message = "Reviewer profile không tồn tại.";
                    return response;
                }

                var certificates = await _certificateRepository
                    .GetAllDataByExpression(x => x.ReviewerProfileId == reviewerProfileId, 0, 0, null, true);

                if (certificates.Items == null || !certificates.Items.Any())
                {
                    response.IsSucess = true;
                    response.Data = new List<Certificate>();
                    response.BusinessCode = BusinessCode.INVALID_ACTION;
                    response.Message = "Reviewer chưa upload chứng chỉ nào.";
                    return response;
                }

                response.IsSucess = true;
                response.Data = certificates.Items;
                response.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                response.Message = "Lấy danh sách chứng chỉ thành công.";
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.INTERNAL_ERROR;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDTO> UploadCertificateAsync(Guid reviewerProfileId, IFormFile file)
        {
            ResponseDTO dto = new ResponseDTO();
            try
            {
                var profile = await _reviewerProfileRepository.GetById(reviewerProfileId);
                if (profile == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.AUTH_NOT_FOUND;
                    dto.Message = "Không tìm thấy hồ sơ reviewer.";
                    return dto;
                }

                if (file == null || file.Length == 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Vui lòng chọn file hợp lệ.";
                    return dto;
                }

                // ✅ Upload file lên Cloudinary
                var uploadResult = await _cloudinaryService.UploadFileAsync(file, "certificates");
                if (!uploadResult.IsSuccess)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.EXCEPTION;
                    dto.Message = "Upload thất bại: " + uploadResult.Message;
                    return dto;
                }

                // ✅ Lưu vào DB
                var cert = new Certificate
                {
                    CertificateId = Guid.NewGuid(),
                    ReviewerProfileId = reviewerProfileId,
                    Name = file.FileName,
                    Url = uploadResult.Url
                };

                await _certificateRepository.Insert(cert);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                dto.Message = "Upload chứng chỉ thành công.";
                dto.Data = new
                {
                    cert.CertificateId,
                    cert.Name,
                    cert.Url,
                    cert.ReviewerProfileId
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi upload chứng chỉ: " + ex.Message;
            }

            return dto;
        }
    }
}
