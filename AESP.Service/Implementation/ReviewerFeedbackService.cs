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
    public class ReviewerFeedbackService : IReviewerFeedbackService
    {
        private readonly IGenericRepository<Feedback> _feedbackRepository;
        private readonly IGenericRepository<ReviewerProfile> _reviewerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewerFeedbackService(
            IGenericRepository<Feedback> feedbackRepository,
            IGenericRepository<ReviewerProfile> reviewerProfileRepository,
            IUnitOfWork unitOfWork)
        {
            _feedbackRepository = feedbackRepository;
            _reviewerProfileRepository = reviewerProfileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetReviewerFeedbackAsync(Guid reviewerProfileId, int pageNumber, int pageSize)
        {
            var dto = new ResponseDTO();

            try
            {
                // Lấy đúng AppDbContext từ generic repo (GIỐNG HỆT FeedbackService)
                var db = _feedbackRepository.GetDbContext();

                // 1) Kiểm tra reviewer hợp lệ + Active
                var reviewer = await db.ReviewerProfiles
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ReviewerProfileId == reviewerProfileId);

                if (reviewer == null || reviewer.IsDeleted || reviewer.Status != "Active")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.ACCESS_DENIED;
                    dto.Message = "Reviewer không hợp lệ hoặc đã bị khóa.";
                    return dto;
                }

                // 2) Lấy feedback đã được Admin duyệt (Status = Active)
                var query = db.Feedbacks
                    .Include(f => f.User)      // Learner gửi feedback
                    .Include(f => f.Review)
                        .ThenInclude(r => r.LearnerAnswer)
                            .ThenInclude(a => a.LearnerProfile)
                            .ThenInclude(lp => lp.User)
                    .Include(f => f.Review)
                        .ThenInclude(r => r.Record)
                            .ThenInclude(rec => rec.LearnerRecord)
                            .ThenInclude(lr => lr.LearnerProfile)
                            .ThenInclude(lp => lp.User)
                    .Where(f =>
                        f.Status == "Active" &&
                        f.Type == "ReviewerFeedback" &&
                        f.Review.ReviewerProfileId == reviewerProfileId)
                    .OrderByDescending(f => f.CreatedAt);

                var totalItems = await query.CountAsync();
                if (totalItems == 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Reviewer chưa có feedback nào.";
                    dto.Data = new
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalItems = 0,
                        Items = new List<object>()
                    };
                    return dto;
                }
                    var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new
                    {
                        f.FeedbackId,
                        f.Content,
                        f.Rating,
                        f.CreatedAt,
                        LearnerName = f.User.FullName,
                        LearnerEmail = f.User.Email,
                        ReviewId = f.ReviewId,
                        ReviewType = f.Review.LearnerAnswerId != null ? "LearnerAnswer" : "Record",
                        QuestionOrContent = f.Review.LearnerAnswer != null
                            ? f.Review.LearnerAnswer.LearningPathQuestion.Question.Text
                            : (f.Review.Record != null ? f.Review.Record.Content : null)
                    })
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách feedback thành công.";
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
                dto.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return dto;
        }
    }
}
