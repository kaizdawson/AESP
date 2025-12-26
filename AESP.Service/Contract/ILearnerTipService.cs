using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ILearnerTipService
    {
        Task<ResponseDTO> GetMyTipHistoryAsync(Guid learnerProfileId, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 10);
    }
}
