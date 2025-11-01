using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Transaction
    {
        [Key]
        public Guid TransactionId { get; set; } = Guid.NewGuid();

        public decimal AmountMoney { get; set; }    // amount-money trong UML

        public decimal AmountCoin { get; set; }     // amount_coin trong UML

        public DateTime CreatedTransaction { get; set; } = DateTime.UtcNow;

        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string ReasonWithdrawReject { get; set; } = string.Empty;

        public string TransactionEnum { get; set; } = string.Empty;

        [ForeignKey(nameof(ServicePackage))]
        public Guid? ServicePackageId { get; set; }

        // --- Navigation ---
        public virtual ServicePackage? ServicePackage { get; set; }

        public virtual ICollection<CoinTransaction> CoinTransactions { get; set; } = new List<CoinTransaction>();

    }
}