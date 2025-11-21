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
        Task<ResponseDTO> GetAllWithdrawalAsync(string? keyword, string? status, int pageNumber , int pageSize);
        Task<ResponseDTO> GetWithdrawalSummaryAsync();
        Task<ResponseDTO> GetAllTransferTransactionsAsync(string? keyword, string? type, int pageNumber , int pageSize);
    }
}
