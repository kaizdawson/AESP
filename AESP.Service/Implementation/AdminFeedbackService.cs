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
                        ReviewerName = f.Review != null ? f.Review.ReviewerProfile.User.FullName : null
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
                var feedback = await db.Feedbacks.FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

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

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Từ chối phản hồi thành công.";
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi từ chối phản hồi: " + ex.Message;
            }

            return dto;

        }
    }
}