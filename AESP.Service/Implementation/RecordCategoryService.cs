using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;

public class RecordCategoryService : IRecordCategoryService
{
    private readonly IGenericRepository<LearnerRecord> _categoryRepo;
    private readonly IGenericRepository<Record> _recordRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RecordCategoryService(
        IGenericRepository<LearnerRecord> categoryRepo,
        IGenericRepository<Record> recordRepo,
        IUnitOfWork unitOfWork)
    {
        _categoryRepo = categoryRepo;
        _recordRepo = recordRepo;
        _unitOfWork = unitOfWork;
    }

    // ========================================================
    // CREATE CATEGORY
    // ========================================================
    public async Task<ResponseDTO> CreateCategoryAsync(Guid learnerProfileId, CreateRecordCategoryDTO dto)
    {
        try
        {
            var cat = new LearnerRecord
            {
                LearnerRecordId = Guid.NewGuid(),
                LearnerId = learnerProfileId,
                Name = dto.Name,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepo.Insert(cat);
            await _unitOfWork.SaveChangeAsync();

            return Success("Tạo thư mục thành công.", new
            {
                cat.LearnerRecordId,
                cat.Name,
                cat.Status,
                cat.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    // ========================================================
    // RENAME CATEGORY
    // ========================================================
    public async Task<ResponseDTO> RenameCategoryAsync(Guid learnerProfileId, Guid categoryId, string newName)
    {
        try
        {
            var cat = await _categoryRepo.AsQueryable()
                .FirstOrDefaultAsync(x =>
                    x.LearnerRecordId == categoryId &&
                    x.LearnerId == learnerProfileId
                );

            if (cat == null)
                return Fail("Không tìm thấy thư mục.");

            cat.Name = newName;

            await _categoryRepo.Update(cat);
            await _unitOfWork.SaveChangeAsync();

            return Success("Đổi tên thư mục thành công.", new
            {
                cat.LearnerRecordId,
                cat.Name,
                cat.Status,
                cat.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    // ========================================================
    // DELETE CATEGORY
    // ========================================================
    public async Task<ResponseDTO> DeleteCategoryAsync(Guid learnerProfileId, Guid categoryId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var cat = await _categoryRepo.AsQueryable()
                .Include(x => x.Records)
                .FirstOrDefaultAsync(x =>
                    x.LearnerRecordId == categoryId &&
                    x.LearnerId == learnerProfileId
                );

            if (cat == null)
                return Fail("Không tìm thấy thư mục.");

            if (cat.Records.Any())
                await _recordRepo.DeleteRange(cat.Records);

            await _categoryRepo.Delete(cat);

            await _unitOfWork.SaveChangeAsync();
            await _unitOfWork.CommitAsync();

            return Success("Xóa thư mục thành công.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    // ========================================================
    // GET ALL CATEGORIES (MINE)
    // ========================================================
    public async Task<ResponseDTO> GetAllCategoriesAsync(Guid learnerProfileId)
    {
        try
        {
            var cats = await _categoryRepo.AsQueryable()
                .Where(x => x.LearnerId == learnerProfileId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.LearnerRecordId,
                    x.Name,
                    x.Status,
                    x.CreatedAt
                })
                .ToListAsync();

            return Success("Lấy dữ liệu thành công.", cats);
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    // ========================================================
    // HELPERS
    // ========================================================
    private ResponseDTO Success(string msg, object data = null)
        => new ResponseDTO
        {
            IsSucess = true,
            BusinessCode = BusinessCode.UPDATE_SUCESSFULLY,
            Message = msg,
            Data = data
        };

    private ResponseDTO Fail(string msg)
        => new ResponseDTO
        {
            IsSucess = false,
            BusinessCode = BusinessCode.VALIDATION_FAILED,
            Message = msg
        };
}
