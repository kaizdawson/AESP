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

                //  Kiểm tra Reviewer tồn tại
                var reviewer = await db.ReviewerProfiles
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ReviewerProfileId == dto.TargetId);

                if (reviewer == null)
                {
                    response.IsSucess = false;
                    response.Message = "Không tìm thấy người đánh giá (Reviewer).";
                    response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    return response;
                }

                //  Kiểm tra User gửi feedback có tồn tại và đúng vai trò Learner
                var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId);
                if (user == null)
                {
                    response.IsSucess = false;
                    response.Message = "Người gửi feedback không tồn tại.";
                    response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    return response;
                }

                if (user.Role != "LEARNER")
                {
                    response.IsSucess = false;
                    response.Message = "Chỉ người học (Learner) mới có thể gửi feedback.";
                    response.BusinessCode = BusinessCode.ACCESS_DENIED;
                    return response;
                }

                //  Kiểm tra content và rating
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

                // Tạo feedback mới
                var feedback = new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    Content = dto.Content.Trim(),
                    Rating = dto.Rating,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active",
                    UserId = dto.UserId,
                    TargetId = dto.TargetId,
                    Type = "ReviewerFeedback"
                };

                await _feedbackRepository.Insert(feedback);
                await _unitOfWork.SaveChangeAsync();

                //  Tính lại Rating trung bình
                await RecalculateReviewerRatingAsync(dto.TargetId);

                response.IsSucess = true;
                response.Message = "Gửi feedback thành công và cập nhật điểm đánh giá reviewer.";
                response.BusinessCode = BusinessCode.CREATED_SUCCESSFULLY;
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.Message = "Lỗi khi thêm feedback: " + ex.Message;
                response.BusinessCode = BusinessCode.EXCEPTION;
            }

            return response;
        }

        private async Task RecalculateReviewerRatingAsync(Guid reviewerProfileId)
        {
            var db = _reviewerProfileRepository.GetDbContext();

            var feedbacks = await db.Feedbacks
                .Where(f => f.TargetId == reviewerProfileId && f.Type == "ReviewerFeedback" && f.Status == "Active")
                .ToListAsync();

            if (feedbacks.Count == 0) return;

            double avgRating = Math.Round(feedbacks.Average(f => f.Rating), 1);

            var reviewer = await db.ReviewerProfiles.FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);
            if (reviewer != null)
            {
                reviewer.Rating = avgRating;
                db.ReviewerProfiles.Update(reviewer);
                await db.SaveChangesAsync();
            }
        }
    }
}
