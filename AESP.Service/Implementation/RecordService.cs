using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;

namespace AESP.Service.Implementation
{
    public class RecordService : IRecordService
    {
        private readonly IGenericRepository<Record> _recordRepo;
        private readonly IGenericRepository<LearnerRecord> _learnerRecordRepo;
        private readonly IUnitOfWork _unitOfWork;

        public RecordService(
            IGenericRepository<Record> recordRepo,
            IGenericRepository<LearnerRecord> learnerRecordRepo,
            IUnitOfWork unitOfWork)
        {
            _recordRepo = recordRepo;
            _learnerRecordRepo = learnerRecordRepo;
            _unitOfWork = unitOfWork;
        }

        // ============================
        // Create Record
        // ============================
        public async Task<ResponseDTO> CreateRecordAsync(Guid learnerProfileId, CreateRecordDTO dto)
        {
            try
            {
                var learnerRecord = await _learnerRecordRepo.AsQueryable()
                    .FirstOrDefaultAsync(x => x.LearnerId == learnerProfileId);

                if (learnerRecord == null)
                {
                    learnerRecord = new LearnerRecord
                    {
                        LearnerRecordId = Guid.NewGuid(),
                        LearnerId = learnerProfileId,
                        Name = "My Record",
                        Status = "Active"
                    };

                    await _learnerRecordRepo.Insert(learnerRecord);
                }

                var record = new Record
                {
                    RecordId = Guid.NewGuid(),
                    LearnerRecordId = learnerRecord.LearnerRecordId,
                    AudioRecordingURL = dto.AudioRecordingURL,
                    Content = dto.Content,
                    Status = "Draft",
                    Score = 0,
                    NumberOfReview = 0,
                    IsNeedReviewed = false
                };

                await _recordRepo.Insert(record);
                await _unitOfWork.SaveChangeAsync();

                return Success("Tạo bản ghi thành công.", record);
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        // ============================
        // Submit Record
        // ============================
        public async Task<ResponseDTO> SubmitRecordAsync(Guid learnerProfileId, Guid recordId, SubmitRecordDTO dto)
        {
            try
            {
                var record = await _recordRepo.AsQueryable()
                    .Include(r => r.LearnerRecord)
                    .FirstOrDefaultAsync(r => r.RecordId == recordId);

                if (record == null)
                    return Fail("Không tìm thấy record.");

                if (record.LearnerRecord.LearnerId != learnerProfileId)
                    return Fail("Không có quyền.");

                record.AudioRecordingURL = dto.AudioRecordingURL;
                record.Content = dto.Content;
                record.Score = dto.Score;
                record.AIFeedback = dto.AIFeedback;
                record.Status = "Submitted";

                await _recordRepo.Update(record);
                await _unitOfWork.SaveChangeAsync();

                return Success("Nộp record thành công.", record);
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        // ============================
        // Delete Record
        // ============================
        public async Task<ResponseDTO> DeleteRecordAsync(Guid learnerProfileId, Guid recordId)
        {
            try
            {
                var record = await _recordRepo.AsQueryable()
                    .Include(r => r.LearnerRecord)
                    .FirstOrDefaultAsync(r => r.RecordId == recordId);

                if (record == null)
                    return Fail("Không tìm thấy record.");

                if (record.LearnerRecord.LearnerId != learnerProfileId)
                    return Fail("Không có quyền.");

                await _recordRepo.Delete(record);
                await _unitOfWork.SaveChangeAsync();

                return Success("Xóa thành công.");
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        // ============================
        // GetAll Record of learner
        // ============================
        public async Task<ResponseDTO> GetAllRecordsAsync(Guid learnerProfileId)
        {
            try
            {
                var records = await _recordRepo.AsQueryable()
                    .Include(r => r.LearnerRecord)
                    .Where(r => r.LearnerRecord.LearnerId == learnerProfileId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Success("Lấy dữ liệu thành công.", records);
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        private ResponseDTO Success(string msg, object data = null)
            => new ResponseDTO { IsSucess = true, BusinessCode = BusinessCode.UPDATE_SUCESSFULLY, Message = msg, Data = data };

        private ResponseDTO Fail(string msg)
            => new ResponseDTO { IsSucess = false, BusinessCode = BusinessCode.VALIDATION_FAILED, Message = msg };
    }
}
