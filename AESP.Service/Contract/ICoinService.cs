using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ICoinService
    {
        Task<decimal> GetUserCoinBalanceAsync(Guid userId);
        Task<object> AddCoinAsync(Guid servicePackageId, Guid userId);


        Task AddBalanceFromPayOSAsync(string orderCode, decimal amount, string payosOrderCode);

        Task CancelTransactionByOrderCodeAsync(string orderCode);
        Task<string> GetTransactionStatusAsync(string orderCode);

        Task<int> PayCoinAsync(Guid userId, Guid aiChargeId);

        Task<object> WithdrawCoinAsync(Guid userId, int coin, string bankName, string accountNumber);

        Task<IEnumerable<object>> GetDepositHistoryAsync(Guid userId);
        Task<IEnumerable<object>> GetWithdrawHistoryAsync(Guid userId);

        Task<IEnumerable<object>> GetActiveAIConversationPackagesAsync();
        Task<ResponseDTO> UpdateWithdrawalAsync(Guid transactionId, Guid userId, int newAmountMoney, string bankName, string accountNumber);
        Task<ResponseDTO> GetAllTransactionsAsync(int pageNumber = 1, int pageSize = 10, string? status = null, string? search = null);
        Task<byte[]> ExportTransactionPdfAsync();

    }
}
