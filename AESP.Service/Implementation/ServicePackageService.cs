using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class ServicePackageService : IServicePackageService
    {
        private readonly IGenericRepository<ServicePackage> _servicePackageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ServicePackageService(
            IGenericRepository<ServicePackage> servicePackageRepository,
            IUnitOfWork unitOfWork)
        {
            _servicePackageRepository = servicePackageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> CreateAsync(CreateServicePackageDto request)
        {
            var dto = new ResponseDTO();

            try
            {
                // ✅ Kiểm tra dữ liệu null hoặc thiếu
                if (request == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Dữ liệu đầu vào không được để trống.";
                    return dto;
                }

                // ✅ Kiểm tra Name rỗng
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Tên gói dịch vụ không được để trống.";
                    return dto;
                }

                // ✅ Kiểm tra tên trùng
                var existed = await _servicePackageRepository.GetByExpression(
                    x => x.Name.ToLower() == request.Name.Trim().ToLower()
                );

                if (existed != null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Tên gói dịch vụ đã tồn tại.";
                    return dto;
                }

                // ✅ Gói hợp lệ → tạo entity
                var entity = new ServicePackage
                {
                    ServicePackageId = Guid.NewGuid(),
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim() ?? "",
                    Level = request.Level?.Trim() ?? "",
                    Price = request.Price,
                    Duration = request.Duration,
                    NumberOfReview = request.NumberOfReview,
                    Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim()
                };

                await _servicePackageRepository.Insert(entity);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.CREATE_SUCCESSFULLY;
                dto.Message = "Tạo gói dịch vụ thành công.";
                dto.Data = new
                {
                    entity.ServicePackageId,
                    entity.Name,
                    entity.Level,
                    entity.Price,
                    entity.Status
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi tạo gói dịch vụ: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> DeleteAsync(Guid id)
        {
            var dto = new ResponseDTO();

            try
            {
                var entity = await _servicePackageRepository.GetById(id);
                if (entity == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy gói dịch vụ.";
                    return dto;
                }

                await _servicePackageRepository.Delete(entity);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
                dto.Message = "Xóa gói dịch vụ thành công.";
                dto.Data = new { id };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi xóa gói dịch vụ: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetByIdAsync(Guid id)
        {
            var dto = new ResponseDTO();

            try
            {
                var entity = await _servicePackageRepository.GetById(id);
                if (entity == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy gói dịch vụ.";
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết gói dịch vụ thành công.";
                dto.Data = new
                {
                    entity.ServicePackageId,
                    entity.Name,
                    entity.Description,
                    entity.Level,
                    entity.Price,
                    entity.Duration,
                    entity.NumberOfReview,
                    entity.Status
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy chi tiết gói dịch vụ: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetListAsync()
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _servicePackageRepository.GetDbContext();

                var packages = db.ServicePackages
                    .OrderByDescending(p => p.Price)
                    .Select(x => new
                    {
                        x.ServicePackageId,
                        x.Name,
                        x.Description,
                        x.Level,
                        x.Price,
                        x.Duration,
                        x.NumberOfReview,
                        x.Status
                    })
                    .ToList();

                if (packages == null || !packages.Any())
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không có gói dịch vụ nào trong hệ thống.";
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách gói dịch vụ thành công.";
                dto.Data = packages;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách gói dịch vụ: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> UpdateAsync(Guid id, UpdateServicePackageDto request)
        {
            var dto = new ResponseDTO();

            try
            {
                var entity = await _servicePackageRepository.GetById(id);
                if (entity == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy gói dịch vụ.";
                    return dto;
                }

                // kiểm tra trùng tên (trừ chính nó)
                var name = request.Name.Trim().ToLower();
                var duplicate = await _servicePackageRepository.GetByExpression(
                    x => x.ServicePackageId != id && x.Name.ToLower() == name);

                if (duplicate != null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Tên gói dịch vụ đã tồn tại.";
                    return dto;
                }

                entity.Name = request.Name.Trim();
                entity.Description = request.Description?.Trim() ?? string.Empty;
                entity.Level = request.Level?.Trim() ?? string.Empty;
                entity.Price = request.Price;
                entity.Duration = request.Duration;
                entity.NumberOfReview = request.NumberOfReview;
                entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status!.Trim();

                await _servicePackageRepository.Update(entity);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Cập nhật gói dịch vụ thành công.";
                dto.Data = new
                {
                    entity.ServicePackageId,
                    entity.Name,
                    entity.Description,
                    entity.Level,
                    entity.Price,
                    entity.Duration,
                    entity.NumberOfReview,
                    entity.Status
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi cập nhật gói dịch vụ: " + ex.Message;
            }

            return dto;
        }
    }

}
