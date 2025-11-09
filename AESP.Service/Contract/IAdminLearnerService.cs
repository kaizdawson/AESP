using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAdminLearnerService
    {
        Task<ResponseDTO> BanLearnerAsync(Guid userId, string reason);
        Task<ResponseDTO> GetLearnerDetailAsync(Guid learnerProfileId);
        Task<ResponseDTO> GetActiveLearnersAsync(string? search, int pageNumber, int pageSize, string? filterStatus);
    }
}
