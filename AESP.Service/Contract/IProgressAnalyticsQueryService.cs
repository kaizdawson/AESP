using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IProgressAnalyticsQueryService
    {
        Task<ResponseDTO> GetByLearnerProfileIdAsync(Guid learnerProfileId);
        Task<ResponseDTO> GetMyProgressAsync(Guid userId);
    }

}
