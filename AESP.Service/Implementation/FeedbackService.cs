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

        public async Task<ResponseDTO> AddFeedbackAsync(FeedbackDTO dto)
        {
            var response = new ResponseDTO();
            try
            {
                var db = _reviewerProfileRepository.GetDbContext();

                // 1️⃣ Kiểm tra Review tồn tại
                var review = await db.Reviews
                    .Include(r => r.ReviewerProfile)
                    .ThenInclude(rp => rp.User)
                    .FirstOrDefaultAsync(r => r.ReviewId == dto.ReviewId);

                if (review == null)
                {
                    return new ResponseDTO
                    {
                        IsSucess = false,
                        Message = "Không tìm thấy bản chấm (Review).",
                        BusinessCode = BusinessCode.DATA_NOT_FOUND
                    };
                }

                // 2️⃣ Reviewer của review này
                var reviewer = review.ReviewerProfile;

                if (reviewer == null)
                {
                    return new ResponseDTO
                    {
                        IsSucess = false,
                        Message = "Không tìm thấy Reviewer của bài review này.",
                        BusinessCode = BusinessCode.DATA_NOT_FOUND
                    };
                }

                // 3️⃣ Kiểm tra User gửi feedback
                var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId);
                if (user == null)
                {
                    return new ResponseDTO
                    {
                        IsSucess = false,
                        Message = "Người gửi feedback không tồn tại.",
                        BusinessCode = BusinessCode.DATA_NOT_FOUND
                    };
                }

                if (user.Role != "LEARNER")
                {
                    return new ResponseDTO
                    {
                        IsSucess = false,
                        Message = "Chỉ Learner mới có thể gửi feedback.",
                        BusinessCode = BusinessCode.ACCESS_DENIED
                    };
                }

                // 4️⃣ Validate Feedback
                if (string.IsNullOrWhiteSpace(dto.Content))
                {
                    return new ResponseDTO
                    {
                        IsSucess = false,
                        Message = "Nội dung feedback không được để trống.",
                        BusinessCode = BusinessCode.INVALID_DATA
                    };
                }

                if (dto.Rating < 1 || dto.Rating > 5)
                {
                    return new ResponseDTO
                    {
                        IsSucess = false,
                        Message = "Rating phải nằm trong khoảng 1 đến 5.",
                        BusinessCode = BusinessCode.INVALID_DATA
                    };
                }

                // 5️⃣ Tạo Feedback
                var feedback = new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    Content = dto.Content.Trim(),
                    Rating = dto.Rating,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active",
                    UserId = dto.UserId,
                    ReviewId = dto.ReviewId,
                    Type = "ReviewerFeedback"
                };

                await _feedbackRepository.Insert(feedback);
                await _unitOfWork.SaveChangeAsync();

                // 6️⃣ Cập nhật rating reviewer
                await RecalculateReviewerRatingAsync(reviewer.ReviewerProfileId);

                return new ResponseDTO
                {
                    IsSucess = true,
                    Message = "Gửi feedback thành công và cập nhật điểm đánh giá reviewer.",
                    BusinessCode = BusinessCode.CREATED_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucess = false,
                    Message = "Lỗi khi thêm feedback: " + ex.Message,
                    BusinessCode = BusinessCode.EXCEPTION
                };
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
