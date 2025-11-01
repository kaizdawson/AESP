using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using Microsoft.EntityFrameworkCore;
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
                if (request == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Dữ liệu không hợp lệ.";
                    return dto;
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Tên gói không được để trống.";
                    return dto;
                }

                // ⚙️ Validate cơ bản
                if (request.Price < 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Giá phải >= 0.";
                    return dto;
                }

                if (request.NumberOfCoin <= 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Số lượng coin phải > 0.";
                    return dto;
                }

                if (request.BonusPercent < 0 || request.BonusPercent > 100)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Phần trăm thưởng phải nằm trong khoảng 0–100.";
                    return dto;
                }

                // ⚙️ Kiểm tra trùng tên
                var existed = await _servicePackageRepository.GetByExpression(
                    p => p.Name.ToLower() == request.Name.Trim().ToLower());

                if (existed != null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DUPLICATE_DATA;
                    dto.Message = "Tên gói dịch vụ đã tồn tại.";
                    return dto;
                }

                // ✅ Tạo entity mới
                var entity = new ServicePackage
                {
                    ServicePackageId = Guid.NewGuid(),
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim() ?? string.Empty,
                    Price = request.Price,
                    NumberOfCoin = request.NumberOfCoin,
                    BonusPercent = request.BonusPercent,
                    Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _servicePackageRepository.Insert(entity);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                dto.Message = "Tạo gói dịch vụ thành công.";
                dto.Data = new
                {
                    entity.ServicePackageId,
                    entity.Name,
                    entity.Description,
                    entity.Price,
                    entity.NumberOfCoin,
                    entity.BonusPercent,
                    entity.Status,
                    entity.CreatedAt
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
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
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

        //public async Task<ResponseDTO> GetByIdAsync(Guid id)
        //{
        //    var dto = new ResponseDTO();

        //    try
        //    {
        //        var db = _servicePackageRepository.GetDbContext();

        //        var package = await db.ServicePackages
        //         .Include(p => p.Subscriptions)
        //         .ThenInclude(s => s.LearnerProfile)
        //         .ThenInclude(lp => lp.User)
        //         .FirstOrDefaultAsync(p => p.ServicePackageId == id);

        //        if (package == null)
        //        {
        //            dto.IsSucess = false;
        //            dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
        //            dto.Message = "Không tìm thấy gói dịch vụ.";
        //            return dto;
        //        }

        //        var learners = package.Subscriptions.Select(s => new
        //        {
        //            LearnerProfileId = s.LearnerProfileId,
        //            FullName = s.LearnerProfile.User.FullName,
        //            Email = s.LearnerProfile.User.Email,
        //            Phone = s.LearnerProfile.User.PhoneNumber,
        //            Status = s.Status
        //        }).ToList();

        //        dto.IsSucess = true;
        //        dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
        //        dto.Message = "Lấy chi tiết gói dịch vụ thành công.";
        //        dto.Data = new
        //        {
        //            package.ServicePackageId,
        //            package.Name,
        //            package.Description,
        //            package.Level,
        //            package.Price,
        //            package.Duration,
        //            package.NumberOfReview,
        //            LearnerCount = learners.Count(),
        //            Learners = learners
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        dto.IsSucess = false;
        //        dto.BusinessCode = BusinessCode.EXCEPTION;
        //        dto.Message = "Lỗi khi lấy chi tiết gói dịch vụ: " + ex.Message;
        //    }

        //    return dto;
        //}

        //public async Task<ResponseDTO> GetListAsync(string? search, int pageNumber, int pageSize)
        //{
        //    var dto = new ResponseDTO();

        //    try
        //    {
        //        var db = _servicePackageRepository.GetDbContext();

        //        var query = db.ServicePackages.AsQueryable();



        //        // Search theo tên
        //        if (!string.IsNullOrEmpty(search))
        //        {
        //            string keyword = search.Trim().ToLower();
        //            query = query.Where(p => p.Name.ToLower().Contains(keyword));
        //        }

        //        var total = await query.CountAsync();

        //        var packages = await query
        //            .OrderBy(p => p.Level)
        //            .ThenBy(p => p.Name)
        //            .Skip((pageNumber - 1) * pageSize)
        //            .Take(pageSize)
        //            .Select(p => new
        //            {
        //                p.ServicePackageId,
        //                p.Name,
        //                p.Description,
        //                p.Level,
        //                p.Price,
        //                p.Duration,
        //                p.NumberOfReview,
        //                p.CreatedAt,
        //                p.UpdatedAt
        //            })
        //            .ToListAsync();

        //        dto.IsSucess = true;
        //        dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
        //        dto.Message = "Lấy danh sách gói dịch vụ thành công.";
        //        dto.Data = new
        //        {
        //            PageNumber = pageNumber,
        //            PageSize = pageSize,
        //            TotalItems = total,
        //            Items = packages
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        dto.IsSucess = false;
        //        dto.BusinessCode = BusinessCode.EXCEPTION;
        //        dto.Message = "Lỗi khi lấy danh sách gói dịch vụ: " + ex.Message;
        //    }

        //    return dto;
        //}

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

                // ⚙️ Validate cơ bản
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Tên gói dịch vụ không được để trống.";
                    return dto;
                }

                if (request.Price < 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Giá phải >= 0.";
                    return dto;
                }

                if (request.NumberOfCoin <= 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Số lượng coin phải lớn hơn 0.";
                    return dto;
                }

                if (request.BonusPercent < 0 || request.BonusPercent > 100)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Phần trăm thưởng phải nằm trong khoảng 0–100.";
                    return dto;
                }

                // ⚙️ Kiểm tra trùng tên (trừ chính nó)
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

                // ✅ Cập nhật dữ liệu
                entity.Name = request.Name.Trim();
                entity.Description = request.Description?.Trim() ?? string.Empty;
                entity.Price = (decimal)request.Price;
                entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim();
                entity.NumberOfCoin = request.NumberOfCoin;
                entity.BonusPercent = request.BonusPercent;
                entity.UpdatedAt = DateTime.UtcNow;

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
                    entity.Price,
                    entity.NumberOfCoin,
                    entity.BonusPercent,
                    entity.Status,
                    entity.UpdatedAt
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
