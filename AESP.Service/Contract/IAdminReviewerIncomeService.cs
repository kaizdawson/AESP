using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAdminReviewerIncomeService
    {
        Task<ResponseDTO> GetSummaryAsync();
        Task<ResponseDTO> GetReviewerListAsync(string? search, int pageNumber, int pageSize);
        Task<ResponseDTO> GetReviewerDetailAsync(Guid reviewerProfileId);
    }
}
