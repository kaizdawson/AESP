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
    public class AdminFeedbackService : IAdminFeedbackService
    {
        private readonly IGenericRepository<Feedback> _feedbackRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminFeedbackService(
            IGenericRepository<Feedback> feedbackRepository,
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork)
        {
            _feedbackRepository = feedbackRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
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
                if (totalRejected >= 3)
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
                dto.Message = totalRejected >= 3
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