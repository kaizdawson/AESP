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
        rc.LearnerRecord.LearnerId == learnerProfileId
    );


            if (recordContent == null)
                return Fail("Không tìm thấy nội dung record hoặc không có quyền.");


            // 🚀 Always create NEW record
            var record = new Record
            {
                RecordId = Guid.NewGuid(),
                RecordContentId = recordContent.RecordContentId,
                AudioRecordingURL = dto.AudioRecordingURL,
                Content = dto.Content,
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
                record.Content,
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
    public async Task<ResponseDTO> GetRecordsByCategoryAsync(Guid learnerProfileId, Guid folderId)
    {
        try
        {
            var list = await _recordRepo.AsQueryable()
    .Include(r => r.RecordContent)
        .ThenInclude(rc => rc.LearnerRecord)
    .Where(r =>
        r.RecordContent.LearnerRecordId == folderId &&
        r.RecordContent.LearnerRecord.LearnerId == learnerProfileId
    )
    .OrderByDescending(r => r.CreatedAt)
    .Select(r => new
    {
        r.RecordId,
        r.RecordContentId,
        r.AudioRecordingURL,
        r.Score,
        r.AIFeedback,
        r.TranscribedText,
        r.Status,
        r.CreatedAt,
        r.NumberOfReview,
        r.Content
    })
    .ToListAsync();


            return Success("Lấy danh sách record thành công.", list);
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    // ============================================================
    // DELETE RECORD
    // ============================================================
    public async Task<ResponseDTO> DeleteRecordAsync(Guid learnerProfileId, Guid recordId)
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


            await _recordRepo.Delete(record);
            await _unitOfWork.SaveChangeAsync();
            await UpdateFolderStatusAsync(record.RecordContent.LearnerRecord);


            await _unitOfWork.SaveChangeAsync();
            await _unitOfWork.CommitAsync();

            return Success("Xóa record thành công.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
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
                Content = dto.Content
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
    Guid recordId,
    UpdateRecordContentDTO dto)
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

            record.Content = dto.Content;
            record.AudioRecordingURL = string.Empty;
            record.Score = 0;
            record.AIFeedback = string.Empty;
            record.TranscribedText = string.Empty;
            record.Status = "Draft";
            record.NumberOfReview = 0;
            record.IsNeedReviewed = false;

            await _recordRepo.Update(record);
            await _unitOfWork.SaveChangeAsync();

            await UpdateFolderStatusAsync(record.RecordContent.LearnerRecord);
            await _unitOfWork.SaveChangeAsync();
            await _unitOfWork.CommitAsync();

            return Success("Cập nhật content và reset record thành công.", new
            {
                record.RecordId,
                record.Content,
                record.Status
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


    public async Task<ResponseDTO> GetLatestRecordByRecordContentAsync(
    Guid learnerProfileId,
    Guid recordContentId)
    {
        try
        {
          
            var recordContent = await _recordContentRepo.AsQueryable()
                .Include(rc => rc.LearnerRecord)
                .FirstOrDefaultAsync(rc =>
                    rc.RecordContentId == recordContentId &&
                    rc.LearnerRecord.LearnerId == learnerProfileId
                );

            if (recordContent == null)
                return Fail("Không tìm thấy content hoặc không có quyền.");

            
            var latestRecord = await _recordRepo.AsQueryable()
                .Where(r => r.RecordContentId == recordContentId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.RecordId,
                    r.RecordContentId,
                    r.AudioRecordingURL,
                    r.Content,
                    r.Score,
                    r.AIFeedback,
                    r.TranscribedText,
                    r.Status,
                    r.CreatedAt,
                    r.NumberOfReview,
                    r.IsNeedReviewed
                })
                .FirstOrDefaultAsync();

           
            if (latestRecord == null)
            {
                return Success("Chưa có record.", new
                {
                    RecordContentId = recordContent.RecordContentId,
                    Record = (object?)null
                });
            }

            return Success("Lấy record mới nhất thành công.", latestRecord);
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }


    private async Task UpdateFolderStatusAsync(LearnerRecord folder)
    {
        var records = await _recordRepo.AsQueryable()
    .Include(r => r.RecordContent)
    .Where(r => r.RecordContent.LearnerRecordId == folder.LearnerRecordId)
    .ToListAsync();



        if (!records.Any())
        {
            folder.Status = "Draft";
        }
        
        else if (records.All(r => r.Status == "Submitted"))
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
    // Helpers
    // ============================================================
    private ResponseDTO Success(string msg, object? data = null)
        => new ResponseDTO { IsSucess = true, BusinessCode = BusinessCode.UPDATE_SUCESSFULLY, Message = msg, Data = data };

    private ResponseDTO Fail(string msg)
        => new ResponseDTO { IsSucess = false, BusinessCode = BusinessCode.VALIDATION_FAILED, Message = msg };
}
