using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using AESP.API.Helpers;

public class RecordService : IRecordService
{
    private readonly IGenericRepository<Record> _recordRepo;
    private readonly IGenericRepository<LearnerRecord> _folderRepo;
    private readonly IGenericRepository<RecordContent> _recordContentRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RecordService(
        IGenericRepository<Record> recordRepo,
        IGenericRepository<LearnerRecord> folderRepo,
        IGenericRepository<RecordContent> recordContentRepo,
        IUnitOfWork unitOfWork)
    {
        _recordRepo = recordRepo;
        _folderRepo = folderRepo;
        _recordContentRepo = recordContentRepo;
        _unitOfWork = unitOfWork;
    }

    // ============================================================
    // SUBMIT (CREATE OR UPDATE)
    // ============================================================
    public async Task<ResponseDTO> SubmitRecordAsync(Guid learnerProfileId, Guid recordContentId, SubmitRecordDTO dto)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var recordContent = await _recordContentRepo.AsQueryable()
    .Include(rc => rc.LearnerRecord)
    .FirstOrDefaultAsync(rc =>
        rc.RecordContentId == recordContentId &&
        rc.LearnerRecord.LearnerId == learnerProfileId &&
    !rc.IsDeleted
    );


            if (recordContent == null)
                return Fail("Không tìm thấy nội dung record hoặc không có quyền.");


            // 🚀 Always create NEW record
            var record = new Record
            {
                RecordId = Guid.NewGuid(),
                RecordContentId = recordContent.RecordContentId,
                AudioRecordingURL = dto.AudioRecordingURL,
                TranscribedText = dto.TranscribedText,
                Score = dto.Score,
                AIFeedback = dto.AIFeedback,
                Status = "Submitted",
                CreatedAt = DateTimeHelper.NowVN(),
                NumberOfReview = 0,
                IsNeedReviewed = false
            };

            await _recordRepo.Insert(record);

            await _unitOfWork.SaveChangeAsync();
            await UpdateFolderStatusAsync(recordContent.LearnerRecord);

            await _unitOfWork.SaveChangeAsync();
            await _unitOfWork.CommitAsync();

            return Success("Tạo record thành công.", new
            {
                record.RecordId,
                record.RecordContentId,
                record.AudioRecordingURL,
                record.TranscribedText,
                record.Score,
                record.AIFeedback,
                record.Status,
                record.CreatedAt
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    // ============================================================
    // AI REVIEW (UPDATE SCORE + AIFEEDBACK)
    // ============================================================
    public async Task<ResponseDTO> UpdateRecordAIResultAsync(Guid learnerProfileId, Guid recordId, UpdateRecordAIResultDTO dto)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var record = await _recordRepo.AsQueryable()
    .Include(r => r.RecordContent)
        .ThenInclude(rc => rc.LearnerRecord)
    .FirstOrDefaultAsync(r => r.RecordId == recordId);


            if (record == null)
                return Fail("Không tìm thấy record.");

            if (record.RecordContent.LearnerRecord.LearnerId != learnerProfileId)
                return Fail("Không có quyền.");

            if (record.RecordContent.IsDeleted)
                return Fail("Record content đã bị xóa.");

            record.Score = dto.Score;
            record.AIFeedback = dto.AIFeedback;
            record.AudioRecordingURL = dto.AudioRecordingURL;
            record.Status = "Reviewed";
            record.NumberOfReview = Math.Max(0, record.NumberOfReview - 1);

            if (record.NumberOfReview == 0)
                record.IsNeedReviewed = false;

            await _recordRepo.Update(record);
            await _unitOfWork.SaveChangeAsync();
            await UpdateFolderStatusAsync(record.RecordContent.LearnerRecord);

            await _unitOfWork.SaveChangeAsync();
            await _unitOfWork.CommitAsync();

            return Success("Cập nhật kết quả review thành công.", new
            {
                record.RecordId,
                record.Score,
                record.AIFeedback,
                record.AudioRecordingURL,
                record.Status,
                record.NumberOfReview
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Fail(ex.Message);
        }
    }

    // ============================================================
    // GET ALL RECORDS BY FOLDER
    // ============================================================
    public async Task<ResponseDTO> GetRecordsByCategoryAsync(
    Guid learnerProfileId,
    Guid folderId)
    {
        try
        {
           
            var folder = await _folderRepo.AsQueryable()
                .FirstOrDefaultAsync(f =>
                    f.LearnerRecordId == folderId &&
                    f.LearnerId == learnerProfileId);

            if (folder == null)
                return Fail("Không tìm thấy thư mục hoặc không có quyền.");


            var result = await (
    from rc in _recordContentRepo.AsQueryable()
    where rc.LearnerRecordId == folderId && !rc.IsDeleted

    let latestRecord = _recordRepo.AsQueryable()
        .Where(r => r.RecordContentId == rc.RecordContentId)
        .OrderByDescending(r => r.CreatedAt)
        .FirstOrDefault()

    select new
    {
        rc.RecordContentId,
        rc.Content,

        RecordId = latestRecord != null ? latestRecord.RecordId : (Guid?)null,
        AudioRecordingURL = latestRecord != null ? latestRecord.AudioRecordingURL : string.Empty,
        TranscribedText = latestRecord != null ? latestRecord.TranscribedText : string.Empty,
        Score = latestRecord != null ? latestRecord.Score : 0,
        AIFeedback = latestRecord != null ? latestRecord.AIFeedback : string.Empty,
        Status = latestRecord != null ? latestRecord.Status : "Draft",
        CreatedAt = latestRecord != null ? latestRecord.CreatedAt : (DateTime?)null,
        NumberOfReview = latestRecord != null ? latestRecord.NumberOfReview : 0,
        IsNeedReviewed = latestRecord != null && latestRecord.IsNeedReviewed
    }
)
.OrderByDescending(x => x.CreatedAt ?? DateTime.MinValue)
.ToListAsync();


            return Success("Lấy danh sách record theo folder thành công.", result);
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }


    



    public async Task<ResponseDTO> CreateRecordContentOnlyAsync(Guid learnerProfileId, Guid folderId, CreateRecordContentOnlyDTO dto)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {

            var folder = await _folderRepo.AsQueryable()
                .FirstOrDefaultAsync(x =>
                    x.LearnerRecordId == folderId &&
                    x.LearnerId == learnerProfileId
                );

            if (folder == null)
                return Fail("Không tìm thấy thư mục hoặc không có quyền.");

            if (folder.NumberOfRecord <= 0)
            {
                return Fail("Bạn cần mua thêm số lượng record.");
            }


            folder.NumberOfRecord -= 1;

            if (folder.Status == "Draft")
            {
                folder.Status = "InProgress";
                folder.UpdatedAt = DateTimeHelper.NowVN();
            }

            await _folderRepo.Update(folder);


            var recordContent = new RecordContent
            {
                RecordContentId = Guid.NewGuid(),
                LearnerRecordId = folderId,
                Content = dto.Content,
                IsDeleted = false
            };

            await _recordContentRepo.Insert(recordContent);


            await _unitOfWork.SaveChangeAsync();
            await UpdateFolderStatusAsync(folder);

            await _unitOfWork.SaveChangeAsync();

            await _unitOfWork.CommitAsync();

            return Success("Tạo record content-only thành công.", new
            {
                recordContent.RecordContentId,
                recordContent.Content,
                RemainingFreeRecord = folder.NumberOfRecord
            });

        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Fail(ex.Message);
        }
    }


    public async Task<ResponseDTO> UpdateRecordContentAsync(
    Guid learnerProfileId,
    Guid recordContentId,
    UpdateRecordContentDTO dto)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
           
            var recordContent = await _recordContentRepo.AsQueryable()
                .Include(rc => rc.LearnerRecord)
                .FirstOrDefaultAsync(rc =>
                    rc.RecordContentId == recordContentId &&
                    rc.LearnerRecord.LearnerId == learnerProfileId &&
    !rc.IsDeleted
                );

            if (recordContent == null)
                return Fail("Không tìm thấy content hoặc không có quyền.");

       
            recordContent.Content = dto.Content;
            recordContent.UpdatedAt = DateTimeHelper.NowVN();
            await _recordContentRepo.Update(recordContent);

       
            var newRecord = new Record
            {
                RecordId = Guid.NewGuid(),
                RecordContentId = recordContent.RecordContentId,

                AudioRecordingURL = string.Empty,
                TranscribedText = string.Empty,
                Score = 0,
                AIFeedback = string.Empty,

                Status = "Draft",
                CreatedAt = DateTimeHelper.NowVN(),
                NumberOfReview = 0,
                IsNeedReviewed = false
            };

            await _recordRepo.Insert(newRecord);

          
            await _unitOfWork.SaveChangeAsync();
            await UpdateFolderStatusAsync(recordContent.LearnerRecord);

            await _unitOfWork.SaveChangeAsync();
            await _unitOfWork.CommitAsync();

            return Success("Cập nhật content và tạo record mới thành công.", new
            {
                recordContent.RecordContentId,
                recordContent.Content,
                NewRecordId = newRecord.RecordId
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Fail(ex.Message);
        }
    }





    public async Task<ResponseDTO> SubmitRecordUpdateAsync(Guid learnerProfileId, Guid recordId, SubmitRecordUpdateDTO dto)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var record = await _recordRepo.AsQueryable()
     .Include(r => r.RecordContent)
         .ThenInclude(rc => rc.LearnerRecord)
     .FirstOrDefaultAsync(r => r.RecordId == recordId);

            if (record == null)
                return Fail("Không tìm thấy record.");


            if (record.RecordContent.LearnerRecord.LearnerId != learnerProfileId)
                return Fail("Không có quyền.");

            if (record.RecordContent.IsDeleted)
                return Fail("Record content đã bị xóa.");

            record.AudioRecordingURL = dto.AudioRecordingURL;
            record.Score = dto.Score;
            record.AIFeedback = dto.AIFeedback;
            record.TranscribedText = dto.TranscribedText;
            record.Status = "Submitted";

            await _recordRepo.Update(record);
            await _unitOfWork.SaveChangeAsync();
            await UpdateFolderStatusAsync(record.RecordContent.LearnerRecord);

            await _unitOfWork.SaveChangeAsync();
            await _unitOfWork.CommitAsync();

            return Success("Cập nhật record thành công.", new
            {
                record.RecordId,
                record.AudioRecordingURL,
                record.Score,
                record.AIFeedback,
                record.TranscribedText,
                record.Status   
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Fail(ex.Message);
        }
    }



    private async Task UpdateFolderStatusAsync(LearnerRecord folder)
    {
        var contents = await _recordContentRepo.AsQueryable()
            .Where(rc => rc.LearnerRecordId == folder.LearnerRecordId &&
        !rc.IsDeleted)
            .Select(rc => new
            {
                rc.RecordContentId,
                LatestRecordStatus = _recordRepo.AsQueryable()
                    .Where(r => r.RecordContentId == rc.RecordContentId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.Status)
                    .FirstOrDefault()
            })
            .ToListAsync();

        if (!contents.Any())
        {
            folder.Status = "Draft";
        }
        else if (contents.All(x =>
            x.LatestRecordStatus == "Submitted" ||
            x.LatestRecordStatus == "Reviewed"))
        {
            folder.Status = "Done";
        }
        else
        {
            folder.Status = "InProgress";
        }

        folder.UpdatedAt = DateTimeHelper.NowVN();
        await _folderRepo.Update(folder);
    }


    // ============================================================
    // DELETE RECORD CONTENT (AGGREGATE)
    // ============================================================
    public async Task<ResponseDTO> DeleteRecordContentAsync(
    Guid learnerProfileId,
    Guid recordContentId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1️⃣ Lấy RecordContent + check quyền + chưa bị xóa
            var recordContent = await _recordContentRepo.AsQueryable()
                .Include(rc => rc.LearnerRecord)
                .FirstOrDefaultAsync(rc =>
                    rc.RecordContentId == recordContentId &&
                    rc.LearnerRecord.LearnerId == learnerProfileId &&
                    !rc.IsDeleted
                );

            if (recordContent == null)
                return Fail("Không tìm thấy record content hoặc không có quyền.");

            // 2️⃣ SOFT DELETE DUY NHẤT RecordContent
            recordContent.IsDeleted = true;
            recordContent.UpdatedAt = DateTimeHelper.NowVN();
            await _recordContentRepo.Update(recordContent);

            // 3️⃣ Lưu DB
            await _unitOfWork.SaveChangeAsync();

            // 4️⃣ Cập nhật trạng thái folder (bỏ qua record content đã bị xóa)
            await UpdateFolderStatusAsync(recordContent.LearnerRecord);
            await _unitOfWork.SaveChangeAsync();

            // 5️⃣ Commit
            await _unitOfWork.CommitAsync();

            return Success("Xóa record content thành công.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Fail(ex.Message);
        }
    }




    // ============================================================
    // Helpers
    // ============================================================
    private ResponseDTO Success(string msg, object? data = null)
        => new ResponseDTO { IsSucess = true, BusinessCode = BusinessCode.UPDATE_SUCESSFULLY, Message = msg, Data = data };

    private ResponseDTO Fail(string msg)
        => new ResponseDTO { IsSucess = false, BusinessCode = BusinessCode.VALIDATION_FAILED, Message = msg };
}
