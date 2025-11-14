using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Purchase
    {
        [Key]
        public Guid PurchaseId { get; set; } = Guid.NewGuid();

        // --- FK ---
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        // Loại product: REVIEW_FEE / AI_CONVERSATION / COURSE
        public string ItemType { get; set; } = string.Empty;

        // --- Fields chính ---
        public string Status { get; set; } = "Pending"; // Pending / Completed / Failed

        public decimal AmountCoin { get; set; }        // số coin đã thanh toán

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int NumberOfReview { get; set; }

        // FK đến ReviewFee
        public Guid? ReviewFeeId { get; set; }

        [ForeignKey(nameof(ReviewFeeId))]
        public virtual ReviewFee? ReviewFee { get; set; }

        // FK đến AIConversationCharge
        public Guid? AIConversationChargeId { get; set; }

        [ForeignKey(nameof(AIConversationChargeId))]
        public virtual AIConversationCharge? AIConversationCharge { get; set; }

        // Navigation
        public virtual User User { get; set; }

        public virtual ICollection<CoinTransaction> CoinTransactions { get; set; } = new List<CoinTransaction>();

    }
}