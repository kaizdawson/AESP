using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAdminFeedbackService
    {
        Task<ResponseDTO> GetAllFeedbackAsync(
           string? keyword,
           string? status,
           int pageNumber = 1,
           int pageSize = 10);
        Task<ResponseDTO> GetFeedbackDetailAsync(Guid feedbackId);
        Task<ResponseDTO> RejectFeedbackAsync(Guid feedbackId, string reason);
        Task<ResponseDTO> ApproveFeedbackAsync(Guid feedbackId);
    }
}