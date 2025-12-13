using AESP.API.Helpers;
using AESP.Common.DTOs;
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
    public class RecordChargeService : IRecordChargeService
    {
        private readonly IGenericRepository<RecordCharge> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public RecordChargeService(
            IGenericRepository<RecordCharge> repo,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> CreateAsync(RecordChargeCreateOrUpdateDto dto)
        {
            var response = new ResponseDTO();

            try
            {
                var entity = new RecordCharge
                {
                    AmountCoin = dto.AmountCoin,
                    AllowedRecordCount = dto.AllowedRecordCount,
                    Status = "Active",
                    CreatedAt = DateTimeHelper.NowVN(),
                    UpdatedAt = DateTimeHelper.NowVN()
                };

                await _repo.Insert(entity);
                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.Message = "Tạo gói record thành công.";
                response.Data = entity;
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDTO> DeleteAsync(Guid id)
        {
            var response = new ResponseDTO();

            try
            {
                var db = _repo.GetDbContext();

                var entity = await db.Set<RecordCharge>()
                    .FirstOrDefaultAsync(x => x.RecordChargeId == id && !x.IsDeleted);

                if (entity == null)
                {
                    response.IsSucess = false;
                    response.Message = "Không tìm thấy gói record.";
                    return response;
                }

                entity.IsDeleted = true;
                entity.UpdatedAt = DateTimeHelper.NowVN();

                await _repo.Update(entity);
                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.Message = "Xóa mềm gói record thành công.";
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDTO> GetAllActiveAsync()
        {
            var response = new ResponseDTO();

            try
            {
                var db = _repo.GetDbContext();

                var items = await db.Set<RecordCharge>()
                    .Where(x => !x.IsDeleted && x.Status == "Active")
                    .OrderBy(x => x.AmountCoin)
                    .Select(x => new
                    {
                        x.RecordChargeId,
                        x.AmountCoin,
                        x.AllowedRecordCount
                    })
                    .ToListAsync();

                response.IsSucess = true;
                response.Message = "Lấy danh sách gói record active thành công.";
                response.Data = items;
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDTO> GetAllAsync(int pageNumber, int pageSize)
        {
            var response = new ResponseDTO();

            try
            {
                var db = _repo.GetDbContext();

                var query = db.Set<RecordCharge>()
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt);

                var totalPackages = await query.CountAsync();

                var totalActivePackages = await query
            .CountAsync(x => x.Status == "Active");

                var totalItems = await query.CountAsync();

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new RecordChargeDto
                    {
                        RecordChargeId = x.RecordChargeId,
                        AmountCoin = x.AmountCoin,
                        AllowedRecords = x.AllowedRecordCount,
                        Status = x.Status,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        IsDeleted = x.IsDeleted
                    })
                    .ToListAsync();

                response.IsSucess = true;
                response.Message = "Lấy danh sách gói record thành công.";
                response.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = items,
                    TotalPackages = totalPackages,
                    TotalActivePackages = totalActivePackages
                };
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDTO> GetDetailAsync(Guid recordChargeId, int pageNumber, int pageSize)
        {
            var response = new ResponseDTO();

            try
            {
                var db = _repo.GetDbContext();

                var recordCharge = await db.Set<RecordCharge>()
                    .FirstOrDefaultAsync(x => x.RecordChargeId == recordChargeId && !x.IsDeleted);

                if (recordCharge == null)
                {
                    response.IsSucess = false;
                    response.Message = "Không tìm thấy gói record.";
                    return response;
                }

                var purchaseQuery = db.Purchases
                    .Include(p => p.User)
                        .ThenInclude(u => u.LearnerProfile)
                    .Where(p =>
                        p.RecordChargeId == recordChargeId &&
                        p.Status == "Success"
                    );

                var totalBuyer = await purchaseQuery
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();

                var totalCoin = await purchaseQuery.SumAsync(p => p.AmountCoin);

                var totalItems = await purchaseQuery.CountAsync();

                var buyers = await purchaseQuery
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        FullName = p.User.FullName,
                        Email = p.User.Email,
                        Coin = p.AmountCoin,
                        PurchaseDate = p.CreatedAt
                    })
                    .ToListAsync();

                response.IsSucess = true;
                response.Message = "Lấy chi tiết gói record thành công.";
                response.Data = new
                {
                    Summary = new
                    {
                        TotalBuyer = totalBuyer,
                        TotalCoin = totalCoin,
                        AllowedRecordCount = recordCharge.AllowedRecordCount
                    },
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Buyers = buyers
                };
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDTO> ToggleStatusAsync(Guid id)
        {
            var response = new ResponseDTO();

            try
            {
                var db = _repo.GetDbContext();

                var entity = await db.Set<RecordCharge>()
                    .FirstOrDefaultAsync(x => x.RecordChargeId == id && !x.IsDeleted);

                if (entity == null)
                {
                    response.IsSucess = false;
                    response.Message = "Không tìm thấy gói record.";
                    return response;
                }

                entity.Status = entity.Status == "Active" ? "InActive" : "Active";
                entity.UpdatedAt = DateTimeHelper.NowVN();

                await _repo.Update(entity);
                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.Message = "Đổi trạng thái thành công.";
                response.Data = new
                {
                    entity.RecordChargeId,
                    entity.Status
                };
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDTO> UpdateAsync(Guid id, RecordChargeCreateOrUpdateDto dto)
        {
            var response = new ResponseDTO();

            try
            {
                var db = _repo.GetDbContext();

                var entity = await db.Set<RecordCharge>()
                    .FirstOrDefaultAsync(x => x.RecordChargeId == id && !x.IsDeleted);

                if (entity == null)
                {
                    response.IsSucess = false;
                    response.Message = "Không tìm thấy gói record.";
                    return response;
                }

                entity.AmountCoin = dto.AmountCoin;
                entity.AllowedRecordCount = dto.AllowedRecordCount;
                entity.UpdatedAt = DateTimeHelper.NowVN();

                await _repo.Update(entity);
                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.Message = "Cập nhật gói record thành công.";
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
