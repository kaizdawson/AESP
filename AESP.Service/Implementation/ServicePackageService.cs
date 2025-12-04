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

                if (request.Price <= 0 || request.NumberOfCoin <= 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Giá và số lượng coin phải > 0.";
                    return dto;
                }

              

                var existed = await _servicePackageRepository.GetByExpression(
                    p => p.Name.ToLower() == request.Name.Trim().ToLower());

                if (existed != null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DUPLICATE_DATA;
                    dto.Message = "Tên gói dịch vụ đã tồn tại.";
                    return dto;
                }

                var entity = new ServicePackage
                {
                    ServicePackageId = Guid.NewGuid(),
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim() ?? string.Empty,
                    Price = request.Price,
                    BaseNumberOfCoin = request.NumberOfCoin,
                    NumberOfCoin = request.NumberOfCoin,
                    BonusPercent = request.BonusPercent,
                    Status = "Active",
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

                // Soft delete: set IsDeleted + Inactive
                entity.IsDeleted = true;
                entity.Status = "Inactive";
                entity.UpdatedAt = DateTime.UtcNow;

                await _servicePackageRepository.Update(entity);
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

        public async  Task<ResponseDTO> GetAllActiveAsync()
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _servicePackageRepository.GetDbContext();

                var items = await db.ServicePackages
                    .Where(x => !x.IsDeleted && x.Status == "Active")
                    .OrderBy(x => x.Price)
                    .Select(x => new
                    {
                        x.ServicePackageId,
                        x.Name,
                        x.Description,
                        x.Price,
                        x.NumberOfCoin,
                        x.BonusPercent
                    })
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách gói đang hoạt động thành công.";
                dto.Data = items;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách active: " + ex.Message;
            }

            return dto;
        
        }

        public async Task<ResponseDTO> GetAllAsync(string? search, int pageNumber = 1, int pageSize = 10, string? filter = null)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _servicePackageRepository.GetDbContext();

                var query = db.ServicePackages
                    .AsQueryable()
                    .Where(x => !x.IsDeleted);

                // 🔍 Tìm kiếm theo tên
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string keyword = search.Trim().ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(keyword));
                }

                // 🔹 Lọc trạng thái (Active / Inactive)
                if (!string.IsNullOrWhiteSpace(filter) && filter.ToLower() != "all")
                {
                    string f = filter.Trim().ToLower();
                    query = query.Where(p => p.Status.ToLower() == f);
                }

                // 🔹 Tổng số lượng item
                var totalItems = await query.CountAsync();

                // 🔹 Phân trang
                var baseItems = await query
                    .OrderBy(p => p.Price)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var resultItems = new List<object>();

                foreach (var p in baseItems)
                {
                    // ✅ GỘP LUÔN THỐNG KÊ TẠI ĐÂY
                    var totalBuyer = await db.Transactions
                        .Where(t =>
                            t.ServicePackageId == p.ServicePackageId &&
                            t.Type == "Deposit" &&
                            t.Status == "Paid")
                        .Select(t => t.UserId)
                        .Distinct()
                        .CountAsync();

                    var totalRevenueMoney = await db.Transactions
                        .Where(t =>
                            t.ServicePackageId == p.ServicePackageId &&
                            t.Type == "Deposit" &&
                            t.Status == "Paid")
                        .SumAsync(t => (decimal?)t.AmountMoney) ?? 0;

                    var totalRevenueCoin = await db.Transactions
                        .Where(t =>
                            t.ServicePackageId == p.ServicePackageId &&
                            t.Type == "Deposit" &&
                            t.Status == "Paid")
                        .SumAsync(t => (decimal?)t.AmountCoin) ?? 0;

                    resultItems.Add(new
                    {
                        p.ServicePackageId,
                        p.Name,
                        p.Description,
                        p.Price,
                        p.NumberOfCoin,
                        p.BonusPercent,
                        p.Status,
                        p.CreatedAt,
                        p.UpdatedAt,

                        // ✅ THỐNG KÊ ĐÃ GỘP
                        TotalBuyer = totalBuyer,
                        TotalRevenueMoney = totalRevenueMoney,
                        TotalRevenueCoin = totalRevenueCoin
                    });
                }

                // 🔹 Kết quả trả về
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách gói dịch vụ + thống kê thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = resultItems
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách gói dịch vụ: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetBuyersOfServicePackageAsync(Guid servicePackageId)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _servicePackageRepository.GetDbContext();

                var buyers = await db.Transactions
                    .Include(t => t.User)
                    .Where(t =>
                        t.ServicePackageId == servicePackageId &&
                        t.Type == "Deposit" &&
                        t.Status == "Paid")
                    .OrderByDescending(t => t.CreatedTransaction)
                    .Select(t => new
                    {
                        t.UserId,
                        t.User.FullName,
                        t.User.Email,
                        t.AmountMoney,
                        t.AmountCoin,
                        t.OrderCode,
                        t.CreatedTransaction
                    })
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách người mua gói thành công.";
                dto.Data = buyers;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách người mua: " + ex.Message;
            }

            return dto;
        }

        //public async Task<ResponseDTO> GetServicePackageStatisticAsync()
        //{
        //    var dto = new ResponseDTO();

        //    try
        //    {
        //        var db = _servicePackageRepository.GetDbContext();

        //        var data = await db.ServicePackages
        //            .Where(sp => !sp.IsDeleted)
        //            .Select(sp => new
        //            {
        //                sp.ServicePackageId,
        //                sp.Name,
        //                sp.Price,

        //                TotalBuyer = db.Transactions
        //                    .Where(t =>
        //                        t.ServicePackageId == sp.ServicePackageId &&
        //                        t.Type == "Deposit" &&
        //                        t.Status == "Paid")
        //                    .Select(t => t.UserId)
        //                    .Distinct()
        //                    .Count(),

        //                TotalRevenueMoney = db.Transactions
        //                    .Where(t =>
        //                        t.ServicePackageId == sp.ServicePackageId &&
        //                        t.Type == "Deposit" &&
        //                        t.Status == "Paid")
        //                    .Sum(t => (decimal?)t.AmountMoney) ?? 0,

        //                TotalRevenueCoin = db.Transactions
        //                    .Where(t =>
        //                        t.ServicePackageId == sp.ServicePackageId &&
        //                        t.Type == "Deposit" &&
        //                        t.Status == "Paid")
        //                    .Sum(t => (decimal?)t.AmountCoin) ?? 0
        //            })
        //            .ToListAsync();

        //        dto.IsSucess = true;
        //        dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
        //        dto.Message = "Lấy thống kê Service Package thành công.";
        //        dto.Data = data;
        //    }
        //    catch (Exception ex)
        //    {
        //        dto.IsSucess = false;
        //        dto.BusinessCode = BusinessCode.EXCEPTION;
        //        dto.Message = "Lỗi khi thống kê Service Package: " + ex.Message;
        //    }

        //    return dto;
        //}

        public async Task<ResponseDTO> ToggleStatusAsync(Guid id)
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

                entity.Status = entity.Status == "Active" ? "Inactive" : "Active";
                entity.UpdatedAt = DateTime.UtcNow;

                await _servicePackageRepository.Update(entity);
                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Thay đổi trạng thái thành công.";
                dto.Data = new
                {
                    entity.ServicePackageId,
                    entity.Status
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi đổi trạng thái: " + ex.Message;
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

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Tên gói dịch vụ không được để trống.";
                    return dto;
                }

                if (request.Price <= 0 || request.NumberOfCoin <= 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Giá và số lượng coin phải > 0.";
                    return dto;
                }

                if (request.BonusPercent < 0 || request.BonusPercent > 100)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Phần trăm thưởng phải nằm trong khoảng 0–100.";
                    return dto;
                }

                var duplicate = await _servicePackageRepository.GetByExpression(
                    x => x.ServicePackageId != id && x.Name.ToLower() == request.Name.Trim().ToLower());
                if (duplicate != null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DUPLICATE_DATA;
                    dto.Message = "Tên gói dịch vụ đã tồn tại.";
                    return dto;
                }

                // ✅ Cập nhật thông tin tĩnh (không bonus)
                entity.Name = request.Name.Trim();
                entity.Description = request.Description?.Trim() ?? string.Empty;
                entity.Price = request.Price;
                entity.BaseNumberOfCoin = request.NumberOfCoin;
                entity.NumberOfCoin = request.NumberOfCoin; // ✅ FE gửi giá trị cuối cùng
                entity.BonusPercent = request.BonusPercent;
                entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim();
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

        //public async Task<ResponseDTO> UpdateBonusPercentAsync(Guid id, UpdateBonusPercentDto request)
        //{
        //    var dto = new ResponseDTO();

        //    try
        //    {
        //        var entity = await _servicePackageRepository.GetById(id);
        //        if (entity == null)
        //        {
        //            dto.IsSucess = false;
        //            dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
        //            dto.Message = "Không tìm thấy gói dịch vụ.";
        //            return dto;
        //        }

        //        if (request.BonusPercent < 0 || request.BonusPercent > 100)
        //        {
        //            dto.IsSucess = false;
        //            dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
        //            dto.Message = "Phần trăm thưởng phải nằm trong khoảng 0–100.";
        //            return dto;
        //        }
              

        //        var bonusCoin = (int)Math.Round(entity.BaseNumberOfCoin * (request.BonusPercent / 100));
        //        entity.BonusPercent = request.BonusPercent;
        //        entity.NumberOfCoin = entity.BaseNumberOfCoin + bonusCoin;
        //        entity.UpdatedAt = DateTime.UtcNow;

        //        await _servicePackageRepository.Update(entity);
        //        await _unitOfWork.SaveChangeAsync();

        //        dto.IsSucess = true;
        //        dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
        //        dto.Message = $"Đã cập nhật bonus {request.BonusPercent}% cho gói {entity.Name}.";
        //        dto.Data = new
        //        {
        //            entity.ServicePackageId,
        //            entity.Name,
        //            entity.NumberOfCoin,
        //            entity.BonusPercent,
        //            entity.UpdatedAt
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        dto.IsSucess = false;
        //        dto.BusinessCode = BusinessCode.EXCEPTION;
        //        dto.Message = "Lỗi khi cập nhật phần trăm thưởng: " + ex.Message;
        //    }

        //    return dto;
        //}
    }
}

       
