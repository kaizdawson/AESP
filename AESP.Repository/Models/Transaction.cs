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
        public Guid TransactionId { get; set; }

        [Required]
        public double Amount { get; set; }
        public DateTime CreatedTransaction { get; set; }

        [ForeignKey("Wallet")]
        public Guid WalletId { get; set; }

        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string ReasonWithdrawReject { get; set; } = string.Empty;
        public string TransactionEnum { get; set; } = string.Empty;

        public virtual Wallet Wallet { get; set; }
    }
}
