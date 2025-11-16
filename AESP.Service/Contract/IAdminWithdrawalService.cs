using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAdminWithdrawalService
    {
        Task<ResponseDTO> GetPendingWithdrawalsAsync(int pageNumber, int pageSize);
        Task<ResponseDTO> ApproveWithdrawalAsync(Guid transactionId);
        Task<ResponseDTO> RejectWithdrawalAsync(Guid transactionId, string reason);
    }
}
