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

                var reviewer = await db.Set<ReviewerProfile>()
           .Include(r => r.User)
           .FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);

                if (reviewer == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy reviewer.";
                    return dto;
                }

                if (reviewer.Status != "Active" || reviewer.IsDeleted || reviewer.User.IsDeleted)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.ACCESS_DENIED;
                    dto.Message = "Reviewer chưa được duyệt hoặc đã bị khóa, không thể xem danh sách bài cần review.";
                    return dto;
                }
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

                        QuestionText = la.LearningPathQuestion.Question.Text,
                        TranscribedText = la.TranscribedText,

                        AudioUrl = la.AudioRecordingUrl,
                        NumberOfReview = la.NumberofReview,
                        LearnerFullName = la.LearnerProfile.User.FullName
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

                        QuestionText = r.Content,
                        TranscribedText = r.TranscribedText,

                        AudioUrl = r.AudioRecordingURL,
                        NumberOfReview = r.NumberOfReview,
                        LearnerFullName = r.LearnerRecord.LearnerProfile.User.FullName,
                       
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

        public async Task<ResponseDTO> GetReviewerStatisticsAsync(Guid reviewerProfileId)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _unitOfWork.GetDbContext();

                var reviewer = await db.Set<ReviewerProfile>()
    .Include(r => r.User)
    .FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);

                if (reviewer == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy reviewer.";
                    return dto;
                }

                if (reviewer.Status != "Active" || reviewer.IsDeleted || reviewer.User.IsDeleted)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.ACCESS_DENIED;
                    dto.Message = "Reviewer chưa được duyệt hoặc đã bị khóa, không thể xem thống kê.";
                    return dto;
                }

                // ================================
                // 1. Tổng phản hồi (Feedback từ Learner, đã Active)
                // ================================
                var totalFeedback = await db.Set<Feedback>()
                    .Include(f => f.Review)
                    .CountAsync(f =>
                        f.Type == "ReviewerFeedback" &&
                        f.Status == "Active" &&
                        f.Review.ReviewerProfileId == reviewerProfileId);

                // ================================
                // 2. Tổng bài đã review
                // ================================
                var totalReviews = await db.Set<Review>()
                    .CountAsync(r => r.ReviewerProfileId == reviewerProfileId);

                // ================================
                // 3. Điểm trung bình Rating
                // ================================
                var ratingList = await db.Set<Feedback>()
                    .Include(f => f.Review)
                    .Where(f =>
                        f.Type == "ReviewerFeedback" &&
                        f.Status == "Active" &&
                        f.Review.ReviewerProfileId == reviewerProfileId)
                    .Select(f => (double?)f.Rating)
                    .ToListAsync();

                double avgRating = ratingList.Count == 0
                    ? 0
                    : Math.Round(ratingList.Average() ?? 0, 1);

                // ================================
                // 4. Số tiền trong ví
                // ================================
                var reviewerInfo = await db.Set<ReviewerProfile>()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);

                var coinBalance = reviewer?.User?.CoinBalance ?? 0;

                // ================================
                // TRẢ VỀ
                // ================================
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy thống kê reviewer thành công.";
                dto.Data = new
                {
                    TotalFeedback = totalFeedback,
                    TotalReviews = totalReviews,
                    AverageRating = avgRating,
                    CoinBalance = coinBalance
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy thống kê reviewer: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetReviewerWalletAsync(Guid reviewerProfileId, int pageNumber, int pageSize)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _unitOfWork.GetDbContext();

                // =============================
                // 1. Reviewer + User
                // =============================
                var reviewer = await db.Set<ReviewerProfile>()
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);

                if (reviewer == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy reviewer.";
                    return dto;
                }

                if (reviewer.Status != "Active" || reviewer.IsDeleted || reviewer.User.IsDeleted)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.ACCESS_DENIED;
                    dto.Message = "Reviewer chưa được duyệt hoặc đã bị khóa, không thể xem ví.";
                    return dto;
                }

                if (reviewer == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy reviewer.";
                    return dto;
                }

                var user = reviewer.User;

                // =============================
                // 2. Tổng coin kiếm được
                // =============================
                int totalEarnedCoin = await db.Set<TransferTransaction>()
                    .Where(t =>
                        t.ReviewerProfileId == reviewerProfileId &&
                        t.Status == "Completed")
                    .SumAsync(t => (int?)t.AmountCoin) ?? 0;

                decimal totalEarnedMoney = totalEarnedCoin * 1000;

                // =============================
                // 3. Danh sách giao dịch rút tiền
                // =============================
                var query = db.Set<Transaction>()
                    .Where(t => t.Type == "Withdrawal" && t.UserId == user.UserId)
                    .OrderByDescending(t => t.CreatedTransaction)
                    .AsQueryable();

                var totalItems = await query.CountAsync();

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        t.TransactionId,
                        Coin = t.AmountCoin,
                        Money = t.AmountMoney,
                        t.BankName,
                        t.AccountNumber,
                        t.Status,
                        t.OrderCode,
                        CreatedAt = t.CreatedTransaction,
                        t.Description
                    })
                    .ToListAsync();

                // =============================
                // 4. Số dư hiện tại (coin + tiền)
                // =============================
                int balanceCoin = user.CoinBalance;
                decimal balanceMoney = balanceCoin * 1000;

                // =============================
                // TRẢ VỀ
                // =============================
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy ví reviewer thành công.";
                dto.Data = new
                {
                    TotalEarnedCoin = totalEarnedCoin,
                    TotalEarnedMoney = totalEarnedMoney,

                    CurrentBalanceCoin = balanceCoin,
                    CurrentBalanceMoney = balanceMoney,

                    Transactions = new
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalItems = totalItems,
                        Items = items
                    }
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy ví reviewer: " + ex.Message;
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
                var reviewer = await db.Set<ReviewerProfile>()
    .Include(r => r.User)
    .FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);

                if (reviewer == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy reviewer.";
                    return dto;
                }

                if (reviewer.Status != "Active" || reviewer.IsDeleted || reviewer.User.IsDeleted)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.ACCESS_DENIED;
                    dto.Message = "Reviewer chưa được duyệt hoặc đã bị khóa, không thể xem lịch sử.";
                    return dto;
                }

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

                        LearnerAudioUrl = r.LearnerAnswer != null
            ? r.LearnerAnswer.AudioRecordingUrl
            : (r.Record != null ? r.Record.AudioRecordingURL : null),

                        ReviewerAudioUrl = r.RecordAudioUrl,

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

        public async Task<ResponseDTO> SubmitReviewAsync(Guid reviewerProfileId, Guid? learnerAnswerId, Guid? recordId, double score, string comment, string? recordAudioUrl)
        {
            var dto = new ResponseDTO();
            var db = (AppDbContext)_unitOfWork.GetDbContext();
            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                if (reviewerProfileId == Guid.Empty)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "ReviewerProfileId không hợp lệ.";
                    return dto;
                }

                var reviewer = await db.Set<ReviewerProfile>()
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);

                if (reviewer == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy reviewer.";
                    return dto;
                }

                if (reviewer.Status != "Active" || reviewer.IsDeleted || reviewer.User.IsDeleted)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.ACCESS_DENIED;
                    dto.Message = "Reviewer chưa được duyệt hoặc đã bị khóa, không thể thực hiện review.";
                    return dto;
                }


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
                if (!string.IsNullOrWhiteSpace(recordAudioUrl))
                {
                    // Nếu chỉ là "string" hoặc text không phải URL -> báo lỗi
                    if (!Uri.TryCreate(recordAudioUrl, UriKind.Absolute, out var uriResult)
                        || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        dto.IsSucess = false;
                        dto.BusinessCode = BusinessCode.INVALID_INPUT;
                        dto.Message = "RecordAudioUrl phải là URL hợp lệ hoặc để trống.";
                        return dto;
                    }
                }
                else
                {
                    // FE không gửi hoặc gửi rỗng -> set null
                    recordAudioUrl = null;
                }

                // ================= KHỞI TẠO REVIEW =================
                var review = new Review
                {
                    ReviewId = Guid.NewGuid(),
                    ReviewerProfileId = reviewerProfileId,
                    Score = score,
                    Comment = comment,
                    Status = "Completed",
                    RecordAudioUrl = recordAudioUrl
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
                    review.RecordAudioUrl,
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

        public async Task<ResponseDTO> TipAfterReviewAsync(Guid reviewerProfileId, ReviewerTipAfterReviewDTO dto)
        {
            var response = new ResponseDTO();
            var db = (AppDbContext)_unitOfWork.GetDbContext();

            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var reviewerProfile = await db.ReviewerProfiles
           .Include(r => r.User)
           .FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);

                if (reviewerProfile == null || reviewerProfile.Status != "Active" || reviewerProfile.IsDeleted || reviewerProfile.User.IsDeleted)
                {
                    response.IsSucess = false;
                    response.BusinessCode = BusinessCode.ACCESS_DENIED;
                    response.Message = "Reviewer chưa được duyệt hoặc đã bị khóa, không thể thưởng coin.";
                    return response;
                }
                // 1. Kiểm tra Review có tồn tại + đúng reviewer + đã Completed
                var review = await db.Reviews
                    .Include(r => r.ReviewerProfile).ThenInclude(rp => rp.User)
                    .Include(r => r.LearnerAnswer).ThenInclude(la => la.LearnerProfile).ThenInclude(lp => lp.User)
                    .Include(r => r.Record).ThenInclude(rec => rec.LearnerRecord).ThenInclude(lr => lr.LearnerProfile).ThenInclude(lp => lp.User)
                    .FirstOrDefaultAsync(r =>
                        r.ReviewId == dto.ReviewId &&
                        r.ReviewerProfileId == reviewerProfileId &&
                        r.Status == "Completed");

                if (review == null)
                {
                    response.IsSucess = false;
                    response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    response.Message = "Không tìm thấy review hợp lệ hoặc bạn không có quyền tặng thưởng cho bài này.";
                    return response;
                }

           

                // 3. Kiểm tra số dư reviewer
                var reviewerUser = review.ReviewerProfile.User;
                if (reviewerUser.CoinBalance < dto.AmountCoin)
                {
                    response.IsSucess = false;
                    response.BusinessCode = BusinessCode.INSUFFICIENT_BALANCE;
                    response.Message = "Số dư coin không đủ để thưởng.";
                    return response;
                }

                // 4. Lấy learner UserId
                Guid learnerUserId = review.LearnerAnswer != null
                    ? review.LearnerAnswer.LearnerProfile.User.UserId
                    : review.Record.LearnerRecord.LearnerProfile.User.UserId;

                Guid learnerProfileId = review.LearnerAnswer != null
                    ? review.LearnerAnswer.LearnerProfileId
                    : review.Record.LearnerRecord.LearnerId;

                // 5. Trừ & cộng coin
                reviewerUser.CoinBalance -= dto.AmountCoin;

                var learnerUser = await db.Users.FirstOrDefaultAsync(u => u.UserId == learnerUserId);
                learnerUser.CoinBalance += dto.AmountCoin;

                // 6. Ghi log TransferTransaction
                var transfer = new TransferTransaction
                {
                    TransferTransactionId = Guid.NewGuid(),
                    LearnerProfileId = learnerProfileId,
                    ReviewerProfileId = reviewerProfileId,
                    ReviewId = dto.ReviewId,                    // quan trọng: gắn vào review
                    AmountCoin = dto.AmountCoin,
                    Comment = $"Reviewer thưởng kèm review: {dto.Message}",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow,
                    TransactionType = "ReviewerTip"
                };
                await db.TransferTransactions.AddAsync(transfer);

                // 7. Gửi Notification cho learner
                var notification = new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    UserId = learnerUserId,
                    Message = $"Reviewer {reviewerUser.FullName} đã thưởng bạn {dto.AmountCoin} coin vì phần nói rất tuyệt vời!\n\"{dto.Message}\"",
                    Type = "ReviewerTip",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await db.Notifications.AddAsync(notification);

                // 8. Save + Commit
                await _unitOfWork.SaveChangeAsync();
                await transaction.CommitAsync();

                response.IsSucess = true;
                response.BusinessCode = BusinessCode.CREATED_SUCCESSFULLY;
                response.Message = $"Thưởng thành công {dto.AmountCoin} coin!";
                response.Data = new
                {
                    TipCoin = dto.AmountCoin,
                    LearnerFullName = learnerUser.FullName,
                    RemainingCoin = reviewerUser.CoinBalance
                };

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.EXCEPTION;
                response.Message = "Lỗi khi thưởng coin: " + ex.Message;
                return response;
            }
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
                    CreatedAt = DateTime.UtcNow,
                    TransactionType = "ReviewPayment"
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
