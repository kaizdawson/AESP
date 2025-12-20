using AESP.API.Helpers;
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
    .ThenInclude(r => r.RecordContent)
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
                    learnerUserId = review.Record.RecordContent.LearnerRecord.LearnerProfile.UserId;

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
                    CreatedAt = DateTimeHelper.NowVN(),
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

        public async Task<ResponseDTO> ReportReviewAsync(ReportReviewDto dto, Guid userId)
        {
            var response = new ResponseDTO();

            try
            {
                var db = _reviewerProfileRepository.GetDbContext();

                // 1) Kiểm tra Review tồn tại
                var review = await db.Reviews
                    .Include(r => r.ReviewerProfile).ThenInclude(rp => rp.User)
                    .Include(r => r.LearnerAnswer).ThenInclude(a => a.LearnerProfile).ThenInclude(lp => lp.User)
                    .Include(r => r.Record).ThenInclude(rec => rec.RecordContent).ThenInclude(rc => rc.LearnerRecord).ThenInclude(lr => lr.LearnerProfile).ThenInclude(lp => lp.User)
                    .FirstOrDefaultAsync(r => r.ReviewId == dto.ReviewId);

                if (review == null)
                {
                    response.IsSucess = false;
                    response.Message = "Không tìm thấy bản chấm (Review).";
                    response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    return response;
                }

                // 2) Check review này có thuộc learner đang đăng nhập không
                Guid? learnerUserId = null;

                if (review.LearnerAnswer != null)
                    learnerUserId = review.LearnerAnswer.LearnerProfile.UserId;

                if (review.Record != null)
                    learnerUserId = review.Record.RecordContent.LearnerRecord.LearnerProfile.UserId;

                if (learnerUserId == null || learnerUserId.Value != userId)
                {
                    response.IsSucess = false;
                    response.Message = "Bạn không thể report bản chấm không thuộc về bạn.";
                    response.BusinessCode = BusinessCode.ACCESS_DENIED;
                    return response;
                }

                // 3) Validate nội dung lý do report
                if (string.IsNullOrWhiteSpace(dto.Reason))
                {
                    response.IsSucess = false;
                    response.Message = "Lý do report không được để trống.";
                    response.BusinessCode = BusinessCode.INVALID_DATA;
                    return response;
                }

                // 4) Không cho report nhiều lần 1 review
                var existedReport = await _feedbackRepository.AsQueryable()
                    .Where(f => f.ReviewId == dto.ReviewId && f.Type == "ReviewerReport")
                    .OrderByDescending(f => f.CreatedAt)
                    .FirstOrDefaultAsync();

                if (existedReport != null)
                {
                    if (existedReport.Status == "Pending")
                    {
                        return new ResponseDTO
                        {
                            IsSucess = false,
                            BusinessCode = BusinessCode.INVALID_ACTION,
                            Message = "Report này đang chờ admin xử lý, không thể gửi lại."
                        };
                    }

                    if (existedReport.Status == "Active")
                    {
                        return new ResponseDTO
                        {
                            IsSucess = false,
                            BusinessCode = BusinessCode.INVALID_ACTION,
                            Message = "Report cho bản chấm này đã được xử lý, không thể gửi lại."
                        };
                    }

                    if (existedReport.Status == "Rejected")
                    {
                        return new ResponseDTO
                        {
                            IsSucess = false,
                            BusinessCode = BusinessCode.INVALID_ACTION,
                            Message = "Report trước đó đã bị từ chối, không thể gửi lại."
                        };
                    }
                }

                // 5) Tạo Feedback kiểu REPORT
                var feedback = new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    Content = dto.Reason.Trim(),
                    Rating = 1,                    // ✅ Fix cứng = 1
                    CreatedAt = DateTimeHelper.NowVN(),
                    Status = "Pending",
                    UserId = userId,
                    ReviewId = dto.ReviewId,
                    Type = "ReviewerReport"        // ✅ Phân biệt với ReviewerFeedback
                };

                await _feedbackRepository.Insert(feedback);

                // 6) Set trạng thái Review để FE/Admin biết đã bị report
                review.Status = "Reported_Pending";
                db.Reviews.Update(review);

                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.Message = "Gửi report thành công. Admin sẽ xem xét và phản hồi.";
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
