using AESP.Common.DTOs;
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

            var detail = await _reviewfeeDetailRepo.GetFirstByExpression(x => x.ReviewFeeId == reviewFeeId);
            if (detail == null)
                return (false, "Không tìm thấy chi tiết gói review.");

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
                Status = "Completed",
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                ReviewFeeId = reviewFeeId,
                AmountCoin = amount
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

          
            var detail = await _reviewfeeDetailRepo.GetFirstByExpression(x => x.ReviewFeeId == reviewFeeId);
            if (detail == null)
                return (false, "Không tìm thấy chi tiết gói review.");

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
                Status = "Completed",
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                ReviewFeeId = reviewFeeId,
                AmountCoin = amount

            };

            await _purchaseRepo.Insert(purchase);

            await _unitOfWork.SaveChangeAsync();

            return (true, "Mua gói review cho record thành công.");
        }

    }
}
