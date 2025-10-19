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

        public AdminFeedbackService(
            IGenericRepository<Feedback> feedbackRepository,
            IGenericRepository<User> userRepository)
        {
            _feedbackRepository = feedbackRepository;
            _userRepository = userRepository;
        }

        public async Task<ResponseDTO> GetAllFeedbackAsync()
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _feedbackRepository.GetDbContext();

                var feedbacks = await db.Feedbacks
                    .Include(f => f.User)
                    .OrderByDescending(f => f.CreatedAt)
                    .Select(f => new FeedbackListDto
                    {
                        FeedbackId = f.FeedbackId,
                        SenderName = f.User.FullName,
                        Type = f.Type,
                        Rating = f.Rating,
                        Content = f.Content,
                        Status = f.Status,
                        CreatedAt = f.CreatedAt
                    })
                    .ToListAsync();

                if (!feedbacks.Any())
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không có feedback nào trong hệ thống.";
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách feedback thành công.";
                dto.Data = feedbacks;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách feedback: " + ex.Message;
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
                    dto.Message = "Không tìm thấy feedback.";
                    return dto;
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết feedback thành công.";
                dto.Data = new
                {
                    feedback.FeedbackId,
                    Sender = feedback.User.FullName,
                    feedback.Type,
                    feedback.Rating,
                    feedback.Content,
                    feedback.Status,
                    feedback.CreatedAt,
                    feedback.TargetId
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy chi tiết feedback: " + ex.Message;
            }

            return dto;
        }
    }
}
