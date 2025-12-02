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

                // 1️ Kiểm tra Review tồn tại
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

                // 2️ Kiểm tra Review này có thuộc Learner đang đăng nhập không
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

                // 3️ Validate Content
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
                var existedFeedback = await _feedbackRepository.AsQueryable()
                 .Where(f => f.ReviewId == dto.ReviewId && f.Type == "ReviewerFeedback")
                 .OrderByDescending(f => f.CreatedAt)
                 .FirstOrDefaultAsync();

                if (existedFeedback != null)
                {
                    if (existedFeedback.Status == "Pending")
                    {
                        return new ResponseDTO
                        {
                            IsSucess = false,
                            BusinessCode = BusinessCode.INVALID_ACTION,
                            Message = "Feedback đang chờ admin duyệt, không thể gửi lại."
                        };
                    }

                    if (existedFeedback.Status == "Active")
                    {
                        return new ResponseDTO
                        {
                            IsSucess = false,
                            BusinessCode = BusinessCode.INVALID_ACTION,
                            Message = "Feedback đã được duyệt, không thể gửi lại."
                        };
                    }

                    if (existedFeedback.Status == "Rejected")
                    {
                        return new ResponseDTO
                        {
                            IsSucess = false,
                            BusinessCode = BusinessCode.INVALID_ACTION,
                            Message = "Feedback đã bị từ chối, không được gửi lại."
                        };
                    }

                }
                if (review.LearnerAnswer != null)
                {
                    var learner = review.LearnerAnswer.LearnerProfile.User;
                    if (learner.Status == "Inactive")
                    {
                        return new ResponseDTO
                        {
                            IsSucess = false,
                            BusinessCode = BusinessCode.ACCESS_DENIED,
                            Message = "Tài khoản của bạn đã bị khóa do vi phạm nhiều lần."
                        };
                    }
                }

                // 4️⃣ Tạo Feedback
                var feedback = new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    Content = dto.Content.Trim(),
                    Rating = dto.Rating,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pending",
                    UserId = userId,
                    ReviewId = dto.ReviewId,
                    Type = "ReviewerFeedback"
                };

                await _feedbackRepository.Insert(feedback);
                await _unitOfWork.SaveChangeAsync();

                // 5️ Cập nhật rating reviewer
               // await RecalculateReviewerRatingAsync(review.ReviewerProfileId);

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

      
    }
}
