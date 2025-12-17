using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using AESP.API.Helpers;

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
            var existingFolderCount = await _categoryRepo.AsQueryable()
                .CountAsync(x => x.LearnerId == learnerProfileId);
   
            var initialFree = existingFolderCount == 0 ? 5 : 0;

            var cat = new LearnerRecord
            {
                LearnerRecordId = Guid.NewGuid(),
                LearnerId = learnerProfileId,
                Name = dto.Name,
                Status = "Draft",
                CreatedAt = DateTimeHelper.NowVN(),
                NumberOfRecord = initialFree,
                IsDeleted = false
            };

            await _categoryRepo.Insert(cat);
            await _unitOfWork.SaveChangeAsync();

            return Success("Tạo thư mục thành công.", new
            {
                cat.LearnerRecordId,
                cat.Name,
                cat.Status,
                cat.CreatedAt,
                cat.NumberOfRecord
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
                x.LearnerId == learnerProfileId &&
                !x.IsDeleted);


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

            if (cat == null || cat.IsDeleted)
                return Fail("Không tìm thấy thư mục.");

            cat.IsDeleted = true;
            cat.UpdatedAt = DateTimeHelper.NowVN();

            await _categoryRepo.Update(cat);


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
                .Where(x => x.LearnerId == learnerProfileId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.LearnerRecordId,
                    x.Name,
                    x.Status,
                    x.CreatedAt,
                    x.NumberOfRecord
                })
                .ToListAsync();

            return Success("Lấy dữ liệu thành công.", cats);
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<ResponseDTO> PurchaseRecordChargeAsync(
     Guid learnerProfileId,
     Guid userId,
     Guid folderId,
     PurchaseRecordChargeDTO dto)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var db = _unitOfWork.GetDbContext();

            var user = await db.Set<User>()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                return Fail("Không tìm thấy user.");

            var recordCharge = await db.Set<RecordCharge>()
                .FirstOrDefaultAsync(x =>
                    x.RecordChargeId == dto.RecordChargeId &&
                    !x.IsDeleted &&
                    x.Status == "Active");

            if (recordCharge == null)
                return Fail("Gói record không tồn tại hoặc đã bị vô hiệu.");

            if (user.CoinBalance < recordCharge.AmountCoin)
                return Fail("Số dư không đủ, vui lòng nạp thêm coin.");

            var folder = await _categoryRepo.AsQueryable()
                .FirstOrDefaultAsync(x =>
                    x.LearnerRecordId == folderId &&
                    x.LearnerId == learnerProfileId &&
                    !x.IsDeleted);

            if (folder == null)
                return Fail("Không tìm thấy thư mục.");

            user.CoinBalance -= recordCharge.AmountCoin;
            folder.NumberOfRecord += recordCharge.AllowedRecordCount;

            var purchase = new Purchase
            {
                PurchaseId = Guid.NewGuid(),
                Status = "Success",
                CreatedAt = DateTimeHelper.NowVN(),
                UserId = userId,
                AmountCoin = recordCharge.AmountCoin,
                RecordChargeId = recordCharge.RecordChargeId
            };

            db.Set<Purchase>().Add(purchase);

            await _unitOfWork.SaveChangeAsync();
            await _unitOfWork.CommitAsync();

            return Success("Mua gói record thành công.", new
            {
                FolderId = folder.LearnerRecordId,
                AddedRecord = recordCharge.AllowedRecordCount,
                TotalRecord = folder.NumberOfRecord,
                RemainingCoin = user.CoinBalance
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Fail(ex.Message);
        }
    }

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
