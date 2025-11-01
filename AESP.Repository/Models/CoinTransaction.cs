using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class CoinTransaction
    {
        [Key]
        public Guid CoinTransactionId { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Transaction))]
        public Guid? TransactionId { get; set; }

        [ForeignKey(nameof(Purchase))]
        public Guid? PurchaseId { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(AIConversationCharge))]
        public Guid? AIConversationChargeId { get; set; }

        [ForeignKey(nameof(TransferTransaction))]
        public Guid? TransferTransactionId { get; set; }


        public string Type { get; set; } = string.Empty; // (cong / tru)

        public decimal Amount { get; set; }

        public decimal BalanceAfter { get; set; }

        public string Reason { get; set; } = string.Empty; // gui tien rut tien chuyen tien 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Navigation ---
        public virtual Transaction? Transaction { get; set; }
        public virtual Purchase? Purchase { get; set; }
        public virtual User User { get; set; }
        public virtual AIConversationCharge? AIConversationCharge { get; set; }
        public virtual TransferTransaction? TransferTransaction { get; set; }
    }
}
