using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAdminReviewerService
    {
        Task<ResponseDTO> GetPendingReviewersAsync(int pageNumber, int pageSize);
        Task<ResponseDTO> ApproveReviewerByCertificateAsync(Guid certificateId);
        Task<ResponseDTO> RejectReviewerByCertificateAsync(Guid certificateId);

    }
}
