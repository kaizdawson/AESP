using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
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
    public class LearnerTipService : ILearnerTipService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LearnerTipService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetMyTipHistoryAsync(Guid learnerProfileId, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 10)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _unitOfWork.GetDbContext();

                // Kiểm tra learner tồn tại
                var learner = await db.Set<LearnerProfile>()
                    .Include(lp => lp.User)
                    .FirstOrDefaultAsync(lp => lp.LearnerProfileId == learnerProfileId);

                if (learner == null || learner.IsDeleted)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy hồ sơ learner.";
                    return dto;
                }

                // Query các tip mà learner nhận được (LearnerProfileId là người nhận)
                var query = db.Set<TransferTransaction>()
                     .Include(t => t.ReviewerProfile)
                         .ThenInclude(rp => rp.User)
                     .Include(t => t.Review)
                         .ThenInclude(r => r.LearnerAnswer)
                             .ThenInclude(la => la.LearningPathQuestion)
                             .ThenInclude(lpq => lpq.Question)
                     .Include(t => t.Review)
                         .ThenInclude(r => r.LearnerAnswer)
                             .ThenInclude(la => la.LearnerProfile)
                             .ThenInclude(lp => lp.User)
                     .Include(t => t.Review)
                         .ThenInclude(r => r.Record)
                             .ThenInclude(rec => rec.RecordContent)
                                 .ThenInclude(rc => rc.LearnerRecord)
                                     .ThenInclude(lr => lr.LearnerProfile)
                                         .ThenInclude(lp => lp.User)
                     .Where(t =>
                         t.LearnerProfileId == learnerProfileId &&
                         t.Status == "Completed" &&
                         t.TransactionType == "ReviewerTip" &&
                         t.ReviewId != null)
                     .AsQueryable();

                // Filter thời gian
                if (fromDate.HasValue)
                    query = query.Where(t => t.CreatedAt >= fromDate.Value);
                if (toDate.HasValue)
                    query = query.Where(t => t.CreatedAt <= toDate.Value);

                // Tổng số
                var totalItems = await query.CountAsync();

                // Lấy dữ liệu phân trang
                var tips = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new LearnerTipHistoryDto
                    {
                        TipTransactionId = t.TransferTransactionId,
                        ReviewId = t.ReviewId!.Value,
                        ReviewerName = t.ReviewerProfile.User.FullName,
                        TipAmount = (int)t.AmountCoin,
                        TipMessage = t.Comment ?? "Cảm ơn bạn đã nói rất tốt!",
                        TipCreatedAt = t.CreatedAt,
                        ReviewType = t.Review.LearnerAnswerId != null ? "LearnerAnswer" : "Record",
                        ReviewScore = t.Review.Score,
                        ReviewComment = t.Review.Comment,
                        LearnerAudioUrl = t.Review.LearnerAnswer != null
                        ? t.Review.LearnerAnswer.AudioRecordingUrl
                        : (t.Review.Record != null ? t.Review.Record.AudioRecordingURL : null),
                        ReviewerAudioUrl = t.Review.RecordAudioUrl
                    })
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy lịch sử tip nhận được thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                    Items = tips
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy lịch sử tip: " + ex.Message;
            }
            return dto;
        }
    }
}
