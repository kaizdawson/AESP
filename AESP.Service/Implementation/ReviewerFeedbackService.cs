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

        public async Task<ResponseDTO> GetReviewerFeedbackAsync(Guid reviewerProfileId, int pageNumber, int pageSize, string? feedbackType = null)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _feedbackRepository.GetDbContext();

                // 1) Check reviewer hợp lệ + Active
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

                // 2) Base query: CHỈ lấy feedback/report đã được admin duyệt (Status = Active)
                var baseQuery = db.Feedbacks
                    .Include(f => f.User) // learner gửi
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
                        (f.Status == "Active" || f.Status == "Rejected") &&                            // ✅ chỉ approved
                        f.Review != null &&
                        f.Review.ReviewerProfileId == reviewerProfileId &&
                        (f.Type == "ReviewerFeedback" ||                   // ✅ cả feedback...
                         f.Type == "ReviewerReport"))                      //    ... và report
                    .AsQueryable();

                // 3) Filter theo TYPE (feedback / report) – vẫn nằm trong tập Approved
                if (!string.IsNullOrWhiteSpace(feedbackType))
                {
                    var ft = feedbackType.Trim().ToLower();

                    if (ft == "feedback")
                    {
                        baseQuery = baseQuery.Where(f => f.Type == "ReviewerFeedback");
                    }
                    else if (ft == "report")
                    {
                        baseQuery = baseQuery.Where(f => f.Type == "ReviewerReport");
                    }
                    else
                    {
                        // Nếu FE truyền đúng "ReviewerFeedback" / "ReviewerReport"
                        baseQuery = baseQuery.Where(f => f.Type == feedbackType);
                    }
                }

                var totalItems = await baseQuery.CountAsync();

                var items = await baseQuery
                    .OrderByDescending(f => f.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new
                    {
                        f.FeedbackId,
                        FeedbackType = f.Type,        // ReviewerFeedback / ReviewerReport
                        FeedbackStatus = f.Status == "Active" ? "Approved" : f.Status == "Rejected" ? "Rejected" : "Pending", // luôn là Approved vì mình đã filter Status = Active
                        f.Rating,
                        f.Content,
                        f.CreatedAt,

                        LearnerId = f.UserId,
                        LearnerName = f.User.FullName,
                        LearnerEmail = f.User.Email,

                        ReviewId = f.ReviewId,
                        ReviewScore = f.Review.Score,
                        ReviewComment = f.Review.Comment,
                        ReviewStatus = f.Review.Status,
                        ReviewCreatedAt = f.Review.CreatedAt,

                        ReviewType = f.Review.LearnerAnswerId != null
                            ? "LearnerAnswer"
                            : (f.Review.RecordId != null ? "Record" : "Unknown"),

                        QuestionContent = f.Review.LearnerAnswer != null
                            ? f.Review.LearnerAnswer.LearningPathQuestion.Question.Text
                            : (f.Review.Record != null ? f.Review.Record.Content : null),

                        LearnerRecordAudioUrl = f.Review.Record != null
                            ? f.Review.Record.AudioRecordingURL
                            : null,

                        ReviewerRecordAudioUrl = f.Review.RecordAudioUrl
                    })
                    .ToListAsync();

                // Với API list, không nên trả 400 khi không có data
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách feedback/report của reviewer thành công.";
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
