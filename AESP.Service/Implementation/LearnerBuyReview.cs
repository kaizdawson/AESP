using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;

namespace AESP.Service.Implementation
{
    public class LearnerBuyReview : ILearnerBuyReview
    {
        private readonly IGenericRepository<ReviewFee> _reviewfeeRepo;
        private readonly IGenericRepository<ReviewFeeDetail> _reviewfeeDetailRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<LearnerAnswer> _learnerAnswerRepo;
        private readonly IGenericRepository<Purchase> _purchaseRepo;
        private readonly IGenericRepository<Record> _recordRepo;
        private readonly IUnitOfWork _unitOfWork;

        public LearnerBuyReview(
            IGenericRepository<ReviewFee> reviewfeeRepo,
            IGenericRepository<ReviewFeeDetail> reviewfeeDetailRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<LearnerAnswer> learnerAnswerRepo,
            IGenericRepository<Record> recordRepo,
            IGenericRepository<Purchase> purchaseRepo,
            IUnitOfWork unitOfWork)
        {
            _reviewfeeRepo = reviewfeeRepo;
            _reviewfeeDetailRepo = reviewfeeDetailRepo;
            _userRepo = userRepo;
            _learnerAnswerRepo = learnerAnswerRepo;
            _purchaseRepo = purchaseRepo;
            _recordRepo = recordRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ReviewFeeMenuDto>> GetReviewFeeMenuAsync()
        {
            var fees = _reviewfeeRepo.AsQueryable();
            var feeDetails = _reviewfeeDetailRepo.AsQueryable();

            var query =
                from fee in fees
                join detail in feeDetails
                    on fee.ReviewFeeId equals detail.ReviewFeeId
                select new ReviewFeeMenuDto
                {
                    ReviewFeeId = fee.ReviewFeeId,
                    NumberOfReview = fee.NumberOfReview,
                    PricePerReviewFee = detail.PricePerReviewFee,
                    AmountMoney = fee.NumberOfReview * detail.PricePerReviewFee
                };

            return await query.ToListAsync();
        }

        public async Task<(bool isSuccess, string message)> BuyReviewFeeAsync(
    Guid userId, Guid reviewFeeId, Guid learnerAnswerId)
        {
            var user = await _userRepo.GetById(userId);
            if (user == null)
                return (false, "User không tồn tại.");

            var learnerAnswer = await _learnerAnswerRepo.GetById(learnerAnswerId);
            if (learnerAnswer == null)
                return (false, "Không tìm thấy câu trả lời của learner.");

            var fee = await _reviewfeeRepo.GetById(reviewFeeId);
            if (fee == null)
                return (false, "Không tìm thấy gói review.");

            var detail = await _reviewfeeDetailRepo
         .AsQueryable()
         .Where(x => x.ReviewFeeId == reviewFeeId && x.AppliedDate <= DateTime.UtcNow)
         .OrderByDescending(x => x.AppliedDate)
         .FirstOrDefaultAsync();

            if (detail == null)
                return (false, "Không tìm thấy chi tiết gói review đang áp dụng.");

            int numberOfReview = (int)fee.NumberOfReview;
            int amount = (int)(fee.NumberOfReview * detail.PricePerReviewFee);

            if (user.CoinBalance < amount)
                return (false, "Số dư không đủ để mua gói.");

            user.CoinBalance -= amount;
            await _userRepo.Update(user);

            // LearnerAnswer phải có NumberOfReview trong model
            learnerAnswer.NumberofReview += numberOfReview;
            learnerAnswer.IsNeededReviewed = learnerAnswer.NumberofReview > 0;
            await _learnerAnswerRepo.Update(learnerAnswer);

            var purchase = new Purchase
            {
                PurchaseId = Guid.NewGuid(),
                Status = "Success",
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                ReviewFeeId = reviewFeeId,
                AmountCoin = amount,
                // ✅ ĐÓNG BĂNG GIÁ TẠI THỜI ĐIỂM MUA
                PricePerReviewAtPurchase = detail.PricePerReviewFee,
                PercentOfReviewerAtPurchase = detail.PercentOfReviewer
            };

            await _purchaseRepo.Insert(purchase);

            await _unitOfWork.SaveChangeAsync();  

            return (true, "Mua gói thành công.");
        }


        public async Task<(bool isSuccess, string message)> BuyReviewFeeForRecordAsync(
    Guid userId, Guid reviewFeeId, Guid recordId)
        {
      
            var user = await _userRepo.GetById(userId);
            if (user == null)
                return (false, "User không tồn tại.");

           
            var record = await _recordRepo.GetById(recordId);
            if (record == null)
                return (false, "Không tìm thấy record.");

        
            var fee = await _reviewfeeRepo.GetById(reviewFeeId);
            if (fee == null)
                return (false, "Không tìm thấy gói review.");


            var detail = await _reviewfeeDetailRepo
       .AsQueryable()
       .Where(x => x.ReviewFeeId == reviewFeeId && x.AppliedDate <= DateTime.UtcNow)
       .OrderByDescending(x => x.AppliedDate)
       .FirstOrDefaultAsync();

            if (detail == null)
                return (false, "Không tìm thấy chi tiết gói review đang áp dụng.");

            int numberOfReview = (int)fee.NumberOfReview;
            int amount = (int)(fee.NumberOfReview * detail.PricePerReviewFee);

      
            if (user.CoinBalance < amount)
                return (false, $"Số dư không đủ để mua gói. Cần {amount} coin, hiện có {user.CoinBalance} coin.");

   
            user.CoinBalance -= amount;
            await _userRepo.Update(user);


            record.NumberOfReview += numberOfReview;
            record.IsNeedReviewed = record.NumberOfReview > 0;
            await _recordRepo.Update(record);


            var purchase = new Purchase
            {
                PurchaseId = Guid.NewGuid(),
                Status = "Success",
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                ReviewFeeId = reviewFeeId,
                AmountCoin = amount,
                PricePerReviewAtPurchase = detail.PricePerReviewFee,
                PercentOfReviewerAtPurchase = detail.PercentOfReviewer
            };

            await _purchaseRepo.Insert(purchase);

            await _unitOfWork.SaveChangeAsync();

            return (true, "Mua gói review cho record thành công.");
        }

        public async Task<ResponseDTO> GetLearnerReviewHistoryAsync(Guid learnerProfileId, int pageNumber = 1, int pageSize = 10, string? status = null, string? keyword = null, string? feedbackType = null)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _unitOfWork.GetDbContext();

                var baseQuery = db.Set<Review>()
                    .Include(r => r.ReviewerProfile)
                        .ThenInclude(rp => rp.User)
                    .Include(r => r.Feedbacks)
                    .Include(r => r.LearnerAnswer)
                        .ThenInclude(la => la.LearningPathQuestion)
                            .ThenInclude(lpq => lpq.Question)
                    .Include(r => r.Record)
                        .ThenInclude(rec => rec.LearnerRecord)
                            .ThenInclude(lr => lr.LearnerProfile)
                    .Where(r =>
                        (r.LearnerAnswer != null && r.LearnerAnswer.LearnerProfileId == learnerProfileId)
                        || (r.Record != null && r.Record.LearnerRecord.LearnerProfile.LearnerProfileId == learnerProfileId)
                    )
                    .AsQueryable();

                // ✅ FILTER THEO TRẠNG THÁI FEEDBACK (ĐÚNG NGHIỆP VỤ)
                if (!string.IsNullOrWhiteSpace(status))
                {
                    status = status.ToLower();

                    if (status == "approved")
                    {
                        baseQuery = baseQuery.Where(r =>
                            r.Feedbacks.Any(f =>
                                (f.Type == "ReviewerFeedback" || f.Type == "ReviewerReport") &&
                                f.Status == "Active"));
                    }
                    else if (status == "pending")
                    {
                        baseQuery = baseQuery.Where(r =>
                            r.Feedbacks.Any(f =>
                                (f.Type == "ReviewerFeedback" || f.Type == "ReviewerReport") &&
                                f.Status == "Pending"));
                    }
                    else if (status == "rejected")
                    {
                        baseQuery = baseQuery.Where(r =>
                            r.Feedbacks.Any(f =>
                                (f.Type == "ReviewerFeedback" || f.Type == "ReviewerReport") &&
                                f.Status == "Rejected"));
                    }
                    else if (status == "notsent")
                    {
                        baseQuery = baseQuery.Where(r =>
                            !r.Feedbacks.Any(f =>
                                f.Type == "ReviewerFeedback" || f.Type == "ReviewerReport"));
                    }
                }
                // ✅ FILTER THEO TYPE FEEDBACK (ReviewerFeedback / ReviewerReport)
                if (!string.IsNullOrWhiteSpace(feedbackType))
                {
                    feedbackType = feedbackType.Trim().ToLower();

                    baseQuery = baseQuery.Where(r =>
                        r.Feedbacks.Any(f => f.Type == feedbackType));
                }

                // ✅ SEARCH ĐÚNG NGHIỆP VỤ
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.ToLower();

                    baseQuery = baseQuery.Where(r =>
                        (r.Comment != null && r.Comment.ToLower().Contains(keyword)) ||
                        (r.LearnerAnswer != null &&
                            r.LearnerAnswer.LearningPathQuestion.Question.Text.ToLower().Contains(keyword)) ||
                        (r.Record != null && r.Record.Content.ToLower().Contains(keyword))
                    );
                }

                var totalItems = await baseQuery.CountAsync();

                var items = await baseQuery
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.ReviewId,
                        r.Score,
                        r.Comment,

                        ReviewAudioUrl = r.RecordAudioUrl,
                        r.LearnerAnswerId,
                        r.RecordId,

                        CreatedAt = r.CreatedAt,

                        QuestionContent = r.LearnerAnswer != null
                            ? r.LearnerAnswer.LearningPathQuestion.Question.Text
                            : r.Record.Content,

                        ReviewerFullName = r.ReviewerProfile.User.FullName,

                        ReviewType = r.LearnerAnswerId != null ? "LearnerAnswer" : "Record",
                        FeedbackType = r.Feedbacks
                        .Where(f => f.Type == "ReviewerFeedback" || f.Type == "ReviewerReport")
                        .OrderByDescending(f => f.CreatedAt)
                        .Select(f => f.Type)
                         .FirstOrDefault(),

                        FeedbackStatus = r.Feedbacks
                        .Where(f => f.Type == "ReviewerFeedback" || f.Type == "ReviewerReport")
                        .OrderByDescending(f => f.CreatedAt)
                        .Select(f =>
                            f.Status == "Active" ? "Approved" :
                            f.Status == "Pending" ? "Pending" :
                            f.Status == "Rejected" ? "Rejected" : string.IsNullOrWhiteSpace(f.Status) ? "NotSent" : f.Status)
                            .FirstOrDefault() ?? "NotSent",

                        FeedbackRating = r.Feedbacks
                        .Where(f => f.Type == "ReviewerFeedback" || f.Type == "ReviewerReport")
                        .OrderByDescending(f => f.CreatedAt)
                        .Select(f => (int?)f.Rating)
                        .FirstOrDefault(),

                        FeedbackContent = r.Feedbacks
                    .Where(f => f.Type == "ReviewerFeedback" || f.Type == "ReviewerReport")
                    .OrderByDescending(f => f.CreatedAt)
                    .Select(f => f.Content)
                    .FirstOrDefault()
                    })
                    .ToListAsync();

                // ✅ DASHBOARD COUNT ĐÚNG NGHIỆP VỤ
                var totalReview = await baseQuery.CountAsync();

                var completed = await baseQuery.CountAsync(r =>
                    r.Feedbacks.Any(f => (f.Type == "ReviewerFeedback" || f.Type == "ReviewerReport") && f.Status == "Active")
                );

                var rejected = await baseQuery.CountAsync(r =>
                    r.Feedbacks.Any(f =>
                    (f.Type == "ReviewerFeedback" || f.Type == "ReviewerReport") &&
                         f.Status == "Rejected")
                );

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy lịch sử review của learner thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,

                    TotalReview = totalReview,
                    Completed = completed,
                    Rejected = rejected,

                    Items = items
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = ex.Message;
            }

            return dto;
        }
    }
}
