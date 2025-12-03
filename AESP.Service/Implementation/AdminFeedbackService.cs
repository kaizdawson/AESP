using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.DB;
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
    public class AdminFeedbackService : IAdminFeedbackService
    {
        private readonly IGenericRepository<Feedback> _feedbackRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AdminFeedbackService(
            IGenericRepository<Feedback> feedbackRepository,
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _feedbackRepository = feedbackRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<ResponseDTO> ApproveFeedbackAsync(Guid feedbackId)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _feedbackRepository.GetDbContext();
                var feedback = await db.Feedbacks
            .Include(f => f.Review)
            .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

                if (feedback == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy phản hồi.";
                    return dto;
                }

                if (feedback.Status == "Active")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_ACTION;
                    dto.Message = "Feedback đã được duyệt trước đó.";
                    return dto;
                }

                // Cập nhật
                feedback.Status = "Active";

                await _feedbackRepository.Update(feedback);
                await _unitOfWork.SaveChangeAsync();

                // Sau khi admin duyệt → cập nhật điểm reviewer
                await RecalculateReviewerRatingAsync(feedback.Review.ReviewerProfileId);

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Duyệt phản hồi thành công.";
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi duyệt phản hồi: " + ex.Message;
            }
            return dto;
        }

        public async Task<ResponseDTO> ApproveReviewReportAsync(Guid feedbackId)
        {
            var dto = new ResponseDTO();
            var db = (AppDbContext)_feedbackRepository.GetDbContext();

            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var feedback = await db.Feedbacks
                    .Include(f => f.Review)
                        .ThenInclude(r => r.ReviewerProfile)
                            .ThenInclude(rp => rp.User)
                    .Include(f => f.Review)
                        .ThenInclude(r => r.LearnerAnswer)
                            .ThenInclude(la => la.LearnerProfile)
                                .ThenInclude(lp => lp.User)
                    .Include(f => f.Review)
                        .ThenInclude(r => r.Record)
                            .ThenInclude(rec => rec.LearnerRecord)
                                .ThenInclude(lr => lr.LearnerProfile)
                                    .ThenInclude(lp => lp.User)
                    .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId && f.Type == "ReviewerReport");

                if (feedback == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy report.";
                    return dto;
                }

                if (feedback.Status == "Active" || feedback.Status == "Rejected")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_ACTION;
                    dto.Message = "Report này đã được xử lý trước đó.";
                    return dto;
                }

                var review = feedback.Review;
                if (review == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy bản chấm liên quan.";
                    return dto;
                }

                var reviewerProfile = review.ReviewerProfile;
                var reviewerUser = reviewerProfile?.User;
                if (reviewerUser == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy thông tin reviewer.";
                    return dto;
                }

                // Lấy learner
                User learnerUser;
                Guid learnerProfileId;
                if (review.LearnerAnswer != null)
                {
                    learnerUser = review.LearnerAnswer.LearnerProfile.User;
                    learnerProfileId = review.LearnerAnswer.LearnerProfileId;
                }
                else if (review.Record != null)
                {
                    learnerUser = review.Record.LearnerRecord.LearnerProfile.User;
                    learnerProfileId = review.Record.LearnerRecord.LearnerProfile.LearnerProfileId;
                }
                else
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Review không gắn với learner nào.";
                    return dto;
                }

                // 1) Đánh dấu report đã được duyệt
                feedback.Status = "Active";
                db.Feedbacks.Update(feedback);

                // 2) Đổi trạng thái Review
                review.Status = "Reported_Approved"; // hoặc "ReportedApproved"
                db.Reviews.Update(review);

                // 3) Hoàn lại 1 lượt review cho learner (không hoàn coin)
                if (review.LearnerAnswer != null)
                {
                    var ans = review.LearnerAnswer;
                    ans.NumberofReview += 1;
                    ans.IsNeededReviewed = true;
                    ans.Status = "InReview";
                    db.LearnerAnswers.Update(ans);
                }
                else if (review.Record != null)
                {
                    var rec = review.Record;
                    rec.NumberOfReview += 1;
                    rec.IsNeedReviewed = true;
                    rec.Status = "InReview";
                    db.Records.Update(rec);
                }

                // 4) Thu hồi coin reviewer – dựa trên TransferTransaction ReviewPayment
                var reviewPayment = await db.TransferTransactions
                    .Where(t =>
                        t.ReviewId == review.ReviewId &&
                        t.ReviewerProfileId == reviewerProfile.ReviewerProfileId &&
                        t.TransactionType == "ReviewPayment" &&
                        t.Status == "Completed")
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                int refundCoin = reviewPayment != null ? (int)Math.Floor(reviewPayment.AmountCoin): 0;

                if (refundCoin > 0)
                {
                    // Không cho âm quá sâu – trừ tối đa bằng số coin hiện tại
                    var actualDeduct = Math.Min(refundCoin, reviewerUser.CoinBalance);
                    reviewerUser.CoinBalance -= actualDeduct;
                    var systemAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

                    var systemAdmin = await db.Users
                        .FirstOrDefaultAsync(u => u.UserId == systemAdminId);

                    if (systemAdmin != null)
                    {
                        systemAdmin.CoinBalance += actualDeduct;
                        db.Users.Update(systemAdmin);
                    }
                    db.Users.Update(reviewerUser);

                    // Log thêm 1 dòng TransferTransaction thể hiện thu hồi
                    var penaltyTransaction = new TransferTransaction
                    {
                        TransferTransactionId = Guid.NewGuid(),
                        LearnerProfileId = learnerProfileId,
                        ReviewerProfileId = reviewerProfile.ReviewerProfileId,
                        ReviewId = review.ReviewId,
                        AmountCoin = actualDeduct,
                        Comment = $"Thu hồi {actualDeduct} coin do review bị learner report và được admin chấp nhận.",
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow,
                        TransactionType = "ReviewPenalty"
                    };
                    await db.TransferTransactions.AddAsync(penaltyTransaction);
                }

                await _unitOfWork.SaveChangeAsync();
                await transaction.CommitAsync();

                // 5) Gửi email cho Learner
                if (!string.IsNullOrEmpty(learnerUser.Email))
                {
                    string subject = "AESP - Report của bạn đã được chấp nhận";
                    string body =
        $@"Xin chào {learnerUser.FullName},

Report của bạn đối với bài chấm đã được admin xem xét và CHẤP NHẬN.

- Bạn đã được cộng lại 1 lượt review cho bài nói đó.
- Bài chấm của reviewer đã được ghi nhận là có vấn đề và hệ thống đã thu hồi coin tương ứng.

Cảm ơn bạn đã giúp chúng tôi cải thiện chất lượng hệ thống.

Trân trọng,
Đội ngũ AESP.";
                    await _emailService.SendEmailAsync(learnerUser.Email, subject, body);
                }

                // 6) Gửi email cho Reviewer
                if (!string.IsNullOrEmpty(reviewerUser.Email))
                {
                    string subject = "AESP - Bài chấm của bạn bị report và đã được chấp nhận";
                    string body =
        $@"Xin chào {reviewerUser.FullName},

Một bài chấm của bạn đã bị learner report và report đó đã được admin xác nhận là HỢP LÝ.

Hệ thống đã thu hồi coin tương ứng cho bài chấm này. Vui lòng chú ý hơn trong các lần review tiếp theo.

Trân trọng,
Đội ngũ AESP.";
                    await _emailService.SendEmailAsync(reviewerUser.Email, subject, body);
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Duyệt report thành công. Đã thu hồi coin của reviewer và hoàn lượt review cho learner.";
                return dto;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi duyệt report: " + ex.Message;
                return dto;
            }
        }
        

        public async Task<ResponseDTO> GetAllFeedbackAsync(
              string? keyword,
              string? status,
              int pageNumber = 1,
              int pageSize = 10)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _feedbackRepository.GetDbContext();

                var query = db.Feedbacks
                    .Include(f => f.User)
                 .Include(f => f.Review) // thêm dòng này
                 .ThenInclude(r => r.ReviewerProfile) // thêm dòng này
                 .ThenInclude(rp => rp.User) // thêm dòng này
                    .OrderByDescending(f => f.CreatedAt)
                    .AsQueryable();

                // 🔍 Lọc theo từ khóa
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var lowerKeyword = keyword.ToLower();
                    query = query.Where(f =>
                        f.User.FullName.ToLower().Contains(lowerKeyword) ||
                        f.Content.ToLower().Contains(lowerKeyword));
                }

                // 🔍 Lọc theo trạng thái
                if (!string.IsNullOrWhiteSpace(status))
                {
                    switch (status.ToLower())
                    {
                        case "approved":
                            query = query.Where(f => f.Status == "Active");
                            break;
                        case "rejected":
                            query = query.Where(f => f.Status == "Rejected");
                            break;
                        case "all":
                        default:
                            // không lọc
                            break;
                    }
                }

                // 🔢 Phân trang
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var feedbacks = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new
                    {
                        f.FeedbackId,
                        SenderName = f.User.FullName,
                        SenderEmail = f.User.Email,
                        f.Type,
                        f.Rating,
                        f.Content,
                        f.Status,
                        f.CreatedAt,
                        f.ReviewId,
                        ReviewerName = f.Review != null ? f.Review.ReviewerProfile.User.FullName : null,
                        ReviewScore = f.Review.Score,
                        ReviewComment = f.Review.Comment,

                        // ✅ AUDIO PHẢN HỒI CỦA REVIEWER (CHÍNH XÁC)
                        ReviewerRecordAudioUrl = f.Review.RecordAudioUrl,

                        // ===== BÀI GỐC CỦA LEARNER =====
                        ReviewType =
            f.Review.LearnerAnswerId != null ? "LearnerAnswer" :
            f.Review.RecordId != null ? "Record" : "Unknown",

                        // ✅ CÂU HỎI (NẾU LÀ LEARNERANSWER)
                        QuestionContent =
            f.Review.LearnerAnswerId != null
                ? f.Review.LearnerAnswer.LearningPathQuestion.Question.Text
                : null,

                        // ✅ AUDIO BÀI NÓI CỦA LEARNER (NẾU LÀ RECORD)
                        LearnerRecordAudioUrl =
            f.Review.RecordId != null
                ? f.Review.Record.AudioRecordingURL
                : null
                    })
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách phản hồi thành công.";
                dto.Data = new
                {
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Items = feedbacks
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách phản hồi: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetFeedbackDetailAsync(Guid feedbackId)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _feedbackRepository.GetDbContext();

                var feedback = await db.Feedbacks
                    .Include(f => f.User)
                    .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

                if (feedback == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy phản hồi.";
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết phản hồi thành công.";
                dto.Data = new
                {
                    feedback.FeedbackId,
                    SenderName = feedback.User.FullName,
                    feedback.Type,
                    feedback.Rating,
                    feedback.Content,
                    feedback.Status,
                    feedback.CreatedAt,
                    feedback.ReviewId
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy chi tiết phản hồi: " + ex.Message;
            }

            return dto;

        }

        public async Task<ResponseDTO> RejectFeedbackAsync(Guid feedbackId, string reason)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _feedbackRepository.GetDbContext();
                var feedback = await db.Feedbacks
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

                if (feedback == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy phản hồi.";
                    return dto;
                }

                feedback.Status = "Rejected";
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    feedback.Content += $"\n\n[Lý do từ chối: {reason}]";
                }

                await _feedbackRepository.Update(feedback);
                await _unitOfWork.SaveChangeAsync();

                // ✅ 2. ĐẾM SỐ LẦN BỊ TỪ CHỐI CỦA USER NÀY
                var totalRejected = await db.Feedbacks
                    .Where(f =>
                        f.UserId == feedback.UserId &&
                        f.Type == "ReviewerFeedback" &&
                        f.Status == "Rejected")
                    .CountAsync();

                // ✅ 3. NẾU >= 3 → KHÓA TÀI KHOẢN
                if (totalRejected >= 5)
                {
                    var user = await db.Users
                        .FirstOrDefaultAsync(u => u.UserId == feedback.UserId);

                    if (user != null)
                    {
                        user.Status = "Banned";   // ✅ ĐÚNG LOGIC HỆ THỐNG BẠN
                        user.IsDeleted = false;     // không phải xóa, chỉ khóa

                        db.Users.Update(user);
                        await db.SaveChangesAsync();
                    }
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = totalRejected >= 5
           ? "Từ chối phản hồi thành công. Tài khoản learner đã bị khóa."
           : "Từ chối phản hồi thành công.";
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi từ chối phản hồi: " + ex.Message;
            }

            return dto;

        }

        public async Task<ResponseDTO> RejectReviewReportAsync(Guid feedbackId, string reason)
        {
            var dto = new ResponseDTO();
            var db = (AppDbContext)_feedbackRepository.GetDbContext();

            try
            {
                var feedback = await db.Feedbacks
                    .Include(f => f.User)
                    .Include(f => f.Review)
                    .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId && f.Type == "ReviewerReport");

                if (feedback == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy report.";
                    return dto;
                }

                if (feedback.Status == "Active" || feedback.Status == "Rejected")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_ACTION;
                    dto.Message = "Report này đã được xử lý trước đó.";
                    return dto;
                }

                var review = feedback.Review;

                // 1) Đánh dấu report bị từ chối
                feedback.Status = "Rejected";
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    feedback.Content += $"\n\n[Lý do từ chối report: {reason}]";
                }
                db.Feedbacks.Update(feedback);

                // 2) Trả trạng thái Review về Completed (nếu đang Reported_Pending)
                if (review != null && review.Status == "Reported_Pending")
                {
                    review.Status = "Completed";
                    db.Reviews.Update(review);
                }

                await _unitOfWork.SaveChangeAsync();

                // 3) Gửi mail cho learner
                var learnerUser = feedback.User;
                if (!string.IsNullOrEmpty(learnerUser?.Email))
                {
                    string subject = "AESP - Report của bạn không được chấp nhận";
                    string body =
        $@"Xin chào {learnerUser.FullName},

Report của bạn đối với bài chấm vừa rồi đã được admin xem xét, 
và kết quả là KHÔNG được chấp nhận.

Lý do (nếu có): {reason}

Nếu bạn còn thắc mắc, vui lòng liên hệ bộ phận hỗ trợ để được giải đáp thêm.

Trân trọng,
Đội ngũ AESP.";
                    await _emailService.SendEmailAsync(learnerUser.Email, subject, body);
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Từ chối report thành công.";
                return dto;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi từ chối report: " + ex.Message;
                return dto;
            }
        }

        private async Task RecalculateReviewerRatingAsync(Guid reviewerProfileId)
        {
            var db = _feedbackRepository.GetDbContext();

            // 1️⃣ Lấy tất cả feedback thuộc những review mà reviewer này chấm
            var feedbacks = await db.Feedbacks
                .Include(f => f.Review)
               .Where(f =>
                  f.Review.ReviewerProfileId == reviewerProfileId &&
                  f.Type == "LearnerFeedback" &&
                  f.Status == "Active")
                  .ToListAsync();

            if (feedbacks.Count == 0)
                return;

            // 2️⃣ Tính điểm trung bình
            double avgRating = Math.Round(feedbacks.Average(f => f.Rating), 1);

            // 3️⃣ Update rating của reviewer
            var reviewer = await db.ReviewerProfiles
                .FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);

            if (reviewer != null)
            {
                reviewer.Rating = avgRating;
                db.ReviewerProfiles.Update(reviewer);
                await db.SaveChangesAsync();
            }
        }
    }
}