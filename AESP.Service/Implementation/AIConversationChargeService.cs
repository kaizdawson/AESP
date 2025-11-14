using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;

namespace AESP.Service.Implementation
{
    public class AIConversationChargeService : IAIConversationChargeService
    {
        private readonly IGenericRepository<AIConversationCharge> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public AIConversationChargeService(
            IGenericRepository<AIConversationCharge> repo,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetAllAsync(int pageNumber, int pageSize)
        {
            var response = new ResponseDTO();

            try
            {
                var db = _repo.GetDbContext();

                var query = db.AIConversationCharge
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt);

                var totalItems = await query.CountAsync();

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new AIConversationChargeDto
                    {
                        AIConversationChargeId = x.AIConversationChargeId,
                        AmountCoin = x.AmountCoin,
                        AllowedMinutes = x.AllowedMinutes,
                        Status = x.Status,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        IsDeleted = x.IsDeleted
                    })

                    .ToListAsync();

                response.IsSucess = true;
                response.Message = "Lấy danh sách thành công.";
                response.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = items
                };
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

                var items = await db.AIConversationCharge
                    .Where(x => !x.IsDeleted && x.Status == "Active")
                    .OrderBy(x => x.AmountCoin)
                    .Select(x => new
                    {
                        x.AIConversationChargeId,
                        x.AmountCoin,
                        x.AllowedMinutes
                    })
                    .ToListAsync();

                response.IsSucess = true;
                response.Message = "Lấy danh sách active thành công.";
                response.Data = items;
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.Message = ex.Message;
            }

            return response;
        }


        public async Task<ResponseDTO> CreateAsync(AIConversationChargeCreateOrUpdateDto dto)
        {
            var response = new ResponseDTO();

            try
            {
                var entity = new AIConversationCharge
                {
                    AmountCoin = dto.AmountCoin,
                    AllowedMinutes = dto.AllowedMinutes,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repo.Insert(entity);
                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.Message = "Tạo gói thành công.";
                response.Data = entity;
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.Message = ex.Message;
            }

            return response;
        }
        public async Task<ResponseDTO> UpdateAsync(Guid id, AIConversationChargeCreateOrUpdateDto dto)
        {
            var response = new ResponseDTO();

            try
            {
                var db = _repo.GetDbContext();

                var entity = await db.AIConversationCharge
                    .FirstOrDefaultAsync(x => x.AIConversationChargeId == id && !x.IsDeleted);

                if (entity == null)
                {
                    response.IsSucess = false;
                    response.Message = "Không tìm thấy gói để cập nhật.";
                    return response;
                }

                entity.AmountCoin = dto.AmountCoin;
                entity.AllowedMinutes = dto.AllowedMinutes;
                entity.UpdatedAt = DateTime.UtcNow;

                await _repo.Update(entity);
                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.Message = "Cập nhật thành công.";
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

                var entity = await db.AIConversationCharge
                    .FirstOrDefaultAsync(x => x.AIConversationChargeId == id && !x.IsDeleted);

                if (entity == null)
                {
                    response.IsSucess = false;
                    response.Message = "Không tìm thấy gói.";
                    return response;
                }

                entity.Status = entity.Status == "Active" ? "InActive" : "Active";
                entity.UpdatedAt = DateTime.UtcNow;

                await _repo.Update(entity);
                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.Message = "Đổi trạng thái thành công.";
                response.Data = new
                {
                    entity.AIConversationChargeId,
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


        public async Task<ResponseDTO> DeleteAsync(Guid id)
        {
            var response = new ResponseDTO();

            try
            {
                var db = _repo.GetDbContext();

                var entity = await db.AIConversationCharge
                    .FirstOrDefaultAsync(x => x.AIConversationChargeId == id && !x.IsDeleted);

                if (entity == null)
                {
                    response.IsSucess = false;
                    response.Message = "Không tìm thấy gói.";
                    return response;
                }

                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;

                await _repo.Update(entity);
                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.Message = "Xóa mềm thành công.";
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
