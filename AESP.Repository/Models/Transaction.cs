using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string ReasonWithdrawReject { get; set; }
        public string TransactionEnum { get; set; }

        public virtual Wallet Wallet { get; set; }
    }
}
