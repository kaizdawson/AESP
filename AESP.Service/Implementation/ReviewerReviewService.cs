using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Realtime.Interfaces;
using AESP.Repository.Contract;
using AESP.Repository.DB;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

                // ============================
                // 1) GET LEARNER ANSWERS
                // ============================
                var pendingAnswersQuery = db.Set<LearnerAnswer>()
                     .Include(la => la.LearningPathQuestion)
                     .ThenInclude(lpq => lpq.Question)
                     .Include(la => la.LearnerProfile)
                     .ThenInclude(lp => lp.User)
                     .AsNoTracking()
                    .Where(la =>
                        la.IsNeededReviewed == true &&
                        la.NumberofReview > 0 &&
                        !db.Set<Review>().Any(r =>
                            r.LearnerAnswerId == la.LearnerAnswerId &&
                            r.ReviewerProfileId == reviewerProfileId))
                    .Select(la => new
                    {
                        Type = "LearnerAnswer",
                        Id = la.LearnerAnswerId,
                        SubmittedAt = la.SubmittedAt,
                        Content = la.TranscribedText,
                        AudioUrl = la.AudioRecordingUrl,
                        NumberOfReview = la.NumberofReview,
                        LearnerFullName = la.LearnerProfile.User.FullName,
                        QuestionText = la.LearningPathQuestion.Question.Text
                    });

                // ============================
                // 2) GET RECORDS
                // ============================
                var pendingRecordsQuery = db.Set<Record>()
                    .Include(r => r.LearnerRecord)
                        .ThenInclude(lr => lr.LearnerProfile)
                            .ThenInclude(lp => lp.User)
                    .AsNoTracking()
                    .Where(r =>
                        r.IsNeedReviewed == true &&
                        r.NumberOfReview > 0 &&
                        !db.Set<Review>().Any(rv =>
                            rv.RecordId == r.RecordId &&
                            rv.ReviewerProfileId == reviewerProfileId))
                    .Select(r => new
                    {
                        Type = "Record",
                        Id = r.RecordId,
                        SubmittedAt = r.CreatedAt,
                        Content = r.Content,
                        AudioUrl = r.AudioRecordingURL,
                        NumberOfReview = r.NumberOfReview,
                        LearnerFullName = r.LearnerRecord.LearnerProfile.User.FullName,
                        QuestionText = (string?)null  // Record không có Question
                    });

                // ============================
                // 3) GỘP 2 QUERY
                // ============================
                var combinedQuery = pendingAnswersQuery
                    .Union(pendingRecordsQuery);

                var totalItems = await combinedQuery.CountAsync();

                var items = await combinedQuery
                    .OrderByDescending(x => x.SubmittedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // ============================
                // TRẢ VỀ RESPONSE
                // ============================
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách item cần review thành công.";
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
                 .ThenInclude(la => la.LearningPathQuestion)
                 .ThenInclude(lpq => lpq.Question)
                 .Include(r => r.LearnerAnswer)
                 .ThenInclude(la => la.LearnerProfile)
                 .ThenInclude(lp => lp.User)
                 .Include(r => r.Record)
                 .ThenInclude(rec => rec.LearnerRecord)
                 .ThenInclude(lr => lr.LearnerProfile)
                 .ThenInclude(lp => lp.User)
                 .AsNoTracking()
                 .Where(r => r.ReviewerProfileId == reviewerProfileId);

                var totalItems = await query.CountAsync();

                var items = await query
                    .OrderByDescending(r => r.ReviewId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.ReviewId,
                        r.Score,
                        r.Comment,
                        r.Status,

                        // Nếu là LearnerAnswer
                        LearnerAnswerId = r.LearnerAnswerId,
                        RecordId = r.RecordId,

                        CreatedAt = r.LearnerAnswer != null
                            ? r.LearnerAnswer.SubmittedAt
                            : (r.Record != null ? r.Record.CreatedAt : DateTime.UtcNow),

                        // Câu hỏi của Answer hoặc Content của Record
                        QuestionContent = r.LearnerAnswer != null
                             ? r.LearnerAnswer.LearningPathQuestion.Question.Text
                             : (r.Record != null ? r.Record.Content : null),

                        // Tên học viên
                        LearnerFullName = r.LearnerAnswer != null
                            ? r.LearnerAnswer.LearnerProfile.User.FullName
                            : (r.Record != null
                                ? r.Record.LearnerRecord.LearnerProfile.User.FullName
                                : null),

                        // Loại review: Answer / Record
                        ReviewType = r.LearnerAnswerId != null ? "LearnerAnswer" : "Record",

                        ReviewerEarnCoin = db.Set<TransferTransaction>()
                        .Where(t => t.ReviewId == r.ReviewId &&
                        t.ReviewerProfileId == r.ReviewerProfileId)
                         .Select(t => (int?)t.AmountCoin)
                        .FirstOrDefault() ?? 0
                    })
                    .ToListAsync();

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
            var db = (AppDbContext)_unitOfWork.GetDbContext();
            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                

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
                        .Include(x => x.LearnerProfile)
                        .FirstOrDefaultAsync(x => x.LearnerAnswerId == learnerAnswerId);

                    if (learnerAnswer == null)
                    {
                        dto.IsSucess = false;
                        dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                        dto.Message = "Không tìm thấy câu trả lời của học viên.";
                        return dto;
                    }

                    if (learnerAnswer.NumberofReview <= 0)
                    {
                        dto.IsSucess = false;
                        dto.BusinessCode = BusinessCode.INVALID_ACTION;
                        dto.Message = "Câu trả lời này đã được review đủ số lượt.";
                        return dto;
                    }

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

                    learnerAnswer.NumberofReview -= 1;
                    if (learnerAnswer.NumberofReview < 0)
                        learnerAnswer.NumberofReview = 0;

                    if (learnerAnswer.NumberofReview > 0)
                    {
                        learnerAnswer.IsNeededReviewed = true;
                        if (string.IsNullOrEmpty(learnerAnswer.Status))
                            learnerAnswer.Status = "InReview";
                    }
                    else
                    {
                        learnerAnswer.IsNeededReviewed = false;
                        learnerAnswer.Status = "Reviewed";
                    }

                    remainingReviews = learnerAnswer.NumberofReview;

                    db.Set<LearnerAnswer>().Update(learnerAnswer);

                    // ✅ Trả coin cho reviewer (LearnerAnswer)
                    await PayReviewerAsync(
                                   db,
                                   reviewerProfileId,
                                   learnerAnswer.LearnerProfileId,
                                   review.ReviewId,
                                   "Thanh toán coin cho reviewer sau khi review LearnerAnswer.");
                }


                // ---------------- CASE 2: REVIEW RECORD ----------------
                if (recordId != null && recordId != Guid.Empty)
                {
                    var record = await db.Set<Record>()
                        .Include(r => r.LearnerRecord)
                            .ThenInclude(cat => cat.LearnerProfile)
                        .FirstOrDefaultAsync(x => x.RecordId == recordId);

                    if (record == null)
                    {
                        dto.IsSucess = false;
                        dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                        dto.Message = "Không tìm thấy bản ghi âm của học viên.";
                        return dto;
                    }

                    if (record.NumberOfReview <= 0)
                    {
                        dto.IsSucess = false;
                        dto.BusinessCode = BusinessCode.INVALID_ACTION;
                        dto.Message = "Bản ghi âm này đã được review đủ số lượt.";
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

                    // Multi-review cho Record
                    record.NumberOfReview -= 1;
                    if (record.NumberOfReview < 0)
                        record.NumberOfReview = 0;

                    if (record.NumberOfReview > 0)
                    {
                        record.IsNeedReviewed = true;
                        if (string.IsNullOrEmpty(record.Status))
                            record.Status = "InReview";
                    }
                    else
                    {
                        record.IsNeedReviewed = false;
                        record.Status = "Reviewed";
                    }

                    remainingReviews = record.NumberOfReview;

                    db.Set<Record>().Update(record);

                    // ✅ Trả coin cho reviewer (Record)
                    await PayReviewerAsync(
                        db,
                        reviewerProfileId,
                        record.LearnerRecord.LearnerProfile.LearnerProfileId,
                        review.ReviewId,
                        "Thanh toán coin cho reviewer sau khi review Record.");
                }

                // ---------------- LƯU REVIEW ----------------
                await db.Set<Review>().AddAsync(review);
                await _unitOfWork.SaveChangeAsync();
                // Commit transaction sau khi mọi thứ OK
                await transaction.CommitAsync();

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
                await transaction.RollbackAsync();
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lưu review: " + ex.Message;
            }

            return dto;

        }

        private async Task PayReviewerAsync(
     AppDbContext db,
     Guid reviewerProfileId,
     Guid learnerProfileId,
     Guid reviewId,
     string reviewType) // "LearnerAnswer" hoặc "Record"
        {
            try
            {
                // =============================
                // 1. Lấy thông tin Reviewer
                // =============================
                var reviewerProfile = await db.Set<ReviewerProfile>()
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);

                if (reviewerProfile == null || reviewerProfile.User == null)
                    return;

                // =============================
                // 2. Lấy cấu hình giá mới nhất
                // =============================
                var now = DateTime.UtcNow;

                var feeDetail = await db.Set<ReviewFeeDetail>()
                    .Where(f => f.AppliedDate <= now)
                    .OrderByDescending(f => f.AppliedDate)
                    .FirstOrDefaultAsync();

                if (feeDetail == null)
                {
                    feeDetail = new ReviewFeeDetail
                    {
                        PricePerReviewFee = 1,
                        PercentOfReviewer = 1,
                        PercentOfSystem = 0
                    };
                }

                // =============================
                // 3. Tính coin chia cho Reviewer + Hệ thống
                // =============================
                var reviewerCoinDec = feeDetail.PricePerReviewFee * feeDetail.PercentOfReviewer;
                var adminCoinDec = feeDetail.PricePerReviewFee * feeDetail.PercentOfSystem;

                var reviewerCoin = (int)Math.Round(reviewerCoinDec, MidpointRounding.AwayFromZero);
                var adminCoin = (int)Math.Round(adminCoinDec, MidpointRounding.AwayFromZero);

                if (reviewerCoin <= 0) reviewerCoin = 1;

                // =============================
                // 4. Cộng coin cho Reviewer
                // =============================
                reviewerProfile.User.CoinBalance += reviewerCoin;
                db.Set<User>().Update(reviewerProfile.User);


                // =============================
                // 5. Cộng coin cho Admin (User có ROLE = ADMIN)
                // =============================
                if (adminCoin > 0)
                {
                    var adminUser = await db.Set<User>()
                        .FirstOrDefaultAsync(u => u.Role == "ADMIN");

                    if (adminUser != null)
                    {
                        adminUser.CoinBalance += adminCoin;
                        db.Set<User>().Update(adminUser);
                    }
                }

                // =============================
                // 6. Ghi log TransferTransactions
                // =============================
                var comment =
                    $"Hệ thống thanh toán {reviewerCoin} coin cho Reviewer {reviewerProfile.User.FullName} sau khi review {reviewType}";

                var transaction = new TransferTransaction
                {
                    TransferTransactionId = Guid.NewGuid(),
                    LearnerProfileId = learnerProfileId,
                    ReviewerProfileId = reviewerProfileId,
                    ReviewId = reviewId,
                    AmountCoin = reviewerCoin,
                    Comment = comment,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                };

                await db.Set<TransferTransaction>().AddAsync(transaction);
            }
            catch
            {
                // Không để lỗi coin làm hỏng flow review
            }
        }

    }
}
