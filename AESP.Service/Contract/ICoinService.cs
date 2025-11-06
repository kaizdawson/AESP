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


    }
}
