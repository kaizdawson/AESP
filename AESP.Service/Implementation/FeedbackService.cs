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
    public class FeedbackService : IFeedbackService
    {
        private readonly IGenericRepository<Feedback> _feedbackRepository;
        private readonly IGenericRepository<ReviewerProfile> _reviewerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FeedbackService(
            IGenericRepository<Feedback> feedbackRepository,
            IGenericRepository<ReviewerProfile> reviewerProfileRepository,
            IUnitOfWork unitOfWork)
        {
            _feedbackRepository = feedbackRepository;
            _reviewerProfileRepository = reviewerProfileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> AddFeedbackAsync(FeedbackDTO dto, Guid userId)
        {
            var response = new ResponseDTO();
            try
            {
                var db = _reviewerProfileRepository.GetDbContext();

                // 1️⃣ Kiểm tra Review tồn tại
                var review = await db.Reviews
                    .Include(r => r.ReviewerProfile)
                    .Include(r => r.LearnerAnswer)
                        .ThenInclude(a => a.LearnerProfile)
                    .Include(r => r.Record)
                        .ThenInclude(rc => rc.LearnerRecord)
                        .ThenInclude(lr => lr.LearnerProfile)
                    .FirstOrDefaultAsync(r => r.ReviewId == dto.ReviewId);

                if (review == null)
                {
                    response.IsSucess = false;
                    response.Message = "Không tìm thấy bản chấm (Review).";
                    response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    return response;
                }

                // 2️⃣ Kiểm tra Review này có thuộc Learner đang đăng nhập không
                Guid? learnerUserId = null;

                if (review.LearnerAnswer != null)
                    learnerUserId = review.LearnerAnswer.LearnerProfile.UserId;

                if (review.Record != null)
                    learnerUserId = review.Record.LearnerRecord.LearnerProfile.UserId;

                if (learnerUserId == null || learnerUserId.Value != userId)
                {
                    response.IsSucess = false;
                    response.Message = "Bạn không thể feedback bản chấm không thuộc về bạn.";
                    response.BusinessCode = BusinessCode.ACCESS_DENIED;
                    return response;
                }

                // 3️⃣ Validate Content
                if (string.IsNullOrWhiteSpace(dto.Content))
                {
                    response.IsSucess = false;
                    response.Message = "Nội dung feedback không được để trống.";
                    response.BusinessCode = BusinessCode.INVALID_DATA;
                    return response;
                }

                if (dto.Rating < 1 || dto.Rating > 5)
                {
                    response.IsSucess = false;
                    response.Message = "Rating phải nằm trong khoảng 1 đến 5.";
                    response.BusinessCode = BusinessCode.INVALID_DATA;
                    return response;
                }

                // 4️⃣ Tạo Feedback
                var feedback = new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    Content = dto.Content.Trim(),
                    Rating = dto.Rating,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active",
                    UserId = userId,
                    ReviewId = dto.ReviewId,
                    Type = "ReviewerFeedback"
                };

                await _feedbackRepository.Insert(feedback);
                await _unitOfWork.SaveChangeAsync();

                // 5️⃣ Cập nhật rating reviewer
                await RecalculateReviewerRatingAsync(review.ReviewerProfileId);

                response.IsSucess = true;
                response.Message = "Gửi feedback thành công.";
                response.BusinessCode = BusinessCode.CREATED_SUCCESSFULLY;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.EXCEPTION;
                response.Message = ex.InnerException?.Message ?? ex.Message;
                return response;
            }
        }

        private async Task RecalculateReviewerRatingAsync(Guid reviewerProfileId)
        {
            var db = _reviewerProfileRepository.GetDbContext();

            // 1️⃣ Lấy tất cả feedback thuộc những review mà reviewer này chấm
            var feedbacks = await db.Feedbacks
                .Include(f => f.Review)
                .Where(f =>
                    f.Review.ReviewerProfileId == reviewerProfileId &&
                    f.Type == "ReviewerFeedback" &&
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
