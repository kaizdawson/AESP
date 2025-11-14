using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Realtime.Interfaces;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AESP.Service.Implementation
{
    public class ReviewerReviewService : IReviewerReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRealtimeNotifier _notifier;

        public ReviewerReviewService(IUnitOfWork unitOfWork, IRealtimeNotifier notifier)
        {
            _unitOfWork = unitOfWork;
            _notifier = notifier;
        }

        public async Task<ResponseDTO> GetPendingReviewsAsync(Guid reviewerProfileId, int pageNumber = 1, int pageSize = 10)
        {
            var dto = new ResponseDTO();

            try
            {
                if (reviewerProfileId == Guid.Empty)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "ReviewerProfileId không hợp lệ.";
                    return dto;
                }

                var db = _unitOfWork.GetDbContext();

                var query = db.Set<LearnerAnswer>()
                    .Include(la => la.Question)
                    .Include(la => la.LearnerProfile)
                        .ThenInclude(lp => lp.User)
                    .AsNoTracking()
                    .Where(la =>
                        la.IsNeededReviewed == true &&
                        la.NumberofReview > 0 &&
                        !db.Set<Review>().Any(r =>
                            r.LearnerAnswerId == la.LearnerAnswerId &&
                            r.ReviewerProfileId == reviewerProfileId));

                var totalItems = await query.CountAsync();

                var items = await query
                    .OrderByDescending(la => la.SubmittedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(la => new
                    {
                        la.LearnerAnswerId,
                        la.LearnerProfileId,
                        la.QuestionId,
                        la.SubmittedAt,
                        la.AudioRecordingUrl,
                        la.TranscribedText,
                        la.ScoreForVoice,
                        la.ExplainTheWrongForVoiceAI,
                        la.IsNeededReviewed,
                        la.Status,
                        la.NumberofReview,
                        QuestionText = la.Question.Text,
                        LearnerFullName = la.LearnerProfile.User.FullName
                    })
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách câu trả lời cần review thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách cần review: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetReviewHistoryAsync(Guid reviewerProfileId, int pageNumber = 1, int pageSize = 10)
        {
            var dto = new ResponseDTO();

            try
            {
                if (reviewerProfileId == Guid.Empty)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "ReviewerProfileId không hợp lệ.";
                    return dto;
                }

                var db = _unitOfWork.GetDbContext();

                var query = db.Set<Review>()
                    .Include(r => r.LearnerAnswer)
                        .ThenInclude(la => la.Question)
                    .Include(r => r.ReviewerProfile)
                    .Include(r => r.Record)
                    .AsNoTracking()
                    .Where(r => r.ReviewerProfileId == reviewerProfileId);

                // 🔹 Tổng số lượng review
                var totalItems = await query.CountAsync();

                // 🔹 Phân trang và chọn trường
                var items = await query
                    .OrderByDescending(r => r.ReviewId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.ReviewId,
                        r.LearnerAnswerId,
                        r.RecordId,
                        r.Score,
                        r.Comment,
                        r.Status,
                        CreatedAt = r.LearnerAnswer != null
                            ? r.LearnerAnswer.SubmittedAt
                            : (r.Record != null ? r.Record.CreatedAt : DateTime.UtcNow),
                        QuestionContent = r.LearnerAnswer != null
                            ? r.LearnerAnswer.Question.Text
                            : (r.Record != null ? r.Record.Content : null),
                        LearnerFullName = r.LearnerAnswer != null
                            ? r.LearnerAnswer.LearnerProfile.User.FullName
                            : (r.Record != null ? r.Record.LearnerRecordCategory.LearnerProfile.User.FullName : null)
                    })
                    .ToListAsync();

                // 🔹 Kết quả trả về đúng format
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách lịch sử review thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy lịch sử review: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> SubmitReviewAsync(Guid reviewerProfileId, Guid? learnerAnswerId, Guid? recordId, double score, string comment)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _unitOfWork.GetDbContext();

                // ================= VALIDATION =================
                if ((learnerAnswerId == null || learnerAnswerId == Guid.Empty) &&
                    (recordId == null || recordId == Guid.Empty))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Cần cung cấp LearnerAnswerId hoặc RecordId.";
                    return dto;
                }

                if (reviewerProfileId == Guid.Empty)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "ReviewerProfileId không hợp lệ.";
                    return dto;
                }

                if (score < 0 || score > 10)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Điểm đánh giá phải nằm trong khoảng 0 - 10.";
                    return dto;
                }

                if (string.IsNullOrWhiteSpace(comment))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Nhận xét không được để trống.";
                    return dto;
                }

                // ================= KHỞI TẠO REVIEW =================
                var review = new Review
                {
                    ReviewId = Guid.NewGuid(),
                    ReviewerProfileId = reviewerProfileId,
                    Score = score,
                    Comment = comment,
                    Status = "Completed"
                };

                int remainingReviews = 0;

                // ---------------- CASE 1: REVIEW LEARNERANSWER ----------------
                if (learnerAnswerId != null && learnerAnswerId != Guid.Empty)
                {
                    var learnerAnswer = await db.Set<LearnerAnswer>()
                        .FirstOrDefaultAsync(x => x.LearnerAnswerId == learnerAnswerId);

                    if (learnerAnswer == null)
                    {
                        dto.IsSucess = false;
                        dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                        dto.Message = "Không tìm thấy câu trả lời của học viên.";
                        return dto;
                    }

                    // Đã hết lượt review mà vẫn cố review tiếp
                    if (learnerAnswer.NumberofReview <= 0)
                    {
                        dto.IsSucess = false;
                        dto.BusinessCode = BusinessCode.INVALID_ACTION;
                        dto.Message = "Câu trả lời này đã được review đủ số lượt.";
                        return dto;
                    }

                    // Check trùng reviewer
                    bool alreadyReviewed = await db.Set<Review>()
                        .AnyAsync(r =>
                            r.LearnerAnswerId == learnerAnswerId &&
                            r.ReviewerProfileId == reviewerProfileId);

                    if (alreadyReviewed)
                    {
                        dto.IsSucess = false;
                        dto.BusinessCode = BusinessCode.INVALID_ACTION;
                        dto.Message = "Bạn đã review câu trả lời này rồi.";
                        return dto;
                    }

                    review.LearnerAnswerId = learnerAnswerId.Value;

                    // Trừ lượt review
                    learnerAnswer.NumberofReview -= 1;
                    if (learnerAnswer.NumberofReview < 0)
                        learnerAnswer.NumberofReview = 0;

                    // Nếu còn lượt → vẫn còn trong queue
                    if (learnerAnswer.NumberofReview > 0)
                    {
                        learnerAnswer.IsNeededReviewed = true;
                        if (string.IsNullOrEmpty(learnerAnswer.Status))
                            learnerAnswer.Status = "InReview";
                    }
                    else
                    {
                        // Hết lượt → không còn trong queue
                        learnerAnswer.IsNeededReviewed = false;
                        learnerAnswer.Status = "Reviewed";
                    }

                    remainingReviews = learnerAnswer.NumberofReview;

                    db.Set<LearnerAnswer>().Update(learnerAnswer);
                }

                // ---------------- CASE 2: REVIEW RECORD ----------------
                if (recordId != null && recordId != Guid.Empty)
                {
                    var record = await db.Set<Record>()
                        .Include(r => r.LearnerRecordCategory)
                            .ThenInclude(cat => cat.LearnerProfile)
                        .FirstOrDefaultAsync(x => x.RecordId == recordId);

                    if (record == null)
                    {
                        dto.IsSucess = false;
                        dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                        dto.Message = "Không tìm thấy bản ghi âm của học viên.";
                        return dto;
                    }

                    bool alreadyReviewed = await db.Set<Review>()
                        .AnyAsync(r =>
                            r.RecordId == recordId &&
                            r.ReviewerProfileId == reviewerProfileId);

                    if (alreadyReviewed)
                    {
                        dto.IsSucess = false;
                        dto.BusinessCode = BusinessCode.INVALID_ACTION;
                        dto.Message = "Bạn đã review bản ghi âm này rồi.";
                        return dto;
                    }

                    review.RecordId = recordId.Value;

                    // Với Record: hiện tại giả định mỗi bản ghi chỉ cần 1 review
                    record.Status = "Reviewed";
                    db.Set<Record>().Update(record);

                    remainingReviews = 0;
                }

                // ---------------- LƯU REVIEW ----------------
                await db.Set<Review>().AddAsync(review);
                await _unitOfWork.SaveChangeAsync();

                // ---------------- GỬI REALTIME CHO REVIEWER KHÁC ----------------
                // - learnerAnswerId != null: FE dùng learnerAnswerId + remaining để update / xoá item
                // - nếu chỉ review Record → learnerAnswerId = Guid.Empty, FE có thể bỏ qua event này
                await _notifier.NotifyReviewCompletedAsync(
                    learnerAnswerId ?? Guid.Empty,
                    remainingReviews
                );

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.CREATED_SUCCESSFULLY;
                dto.Message = "Review thành công.";
                dto.Data = new
                {
                    review.ReviewId,
                    review.LearnerAnswerId,
                    review.RecordId,
                    review.Score,
                    review.Comment,
                    review.Status,
                    RemainingReviews = remainingReviews
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lưu review: " + ex.Message;
            }

            return dto;

        }
    }
}
       
    

   

