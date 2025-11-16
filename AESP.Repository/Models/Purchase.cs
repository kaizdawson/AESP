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


        // --- Fields chính ---
        public string Status { get; set; } = "Pending"; // Pending / Completed / Failed

        public decimal AmountCoin { get; set; }        // số coin đã thanh toán

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // FK đến ReviewFee
        public Guid? ReviewFeeId { get; set; }

        [ForeignKey(nameof(ReviewFeeId))]
        public virtual ReviewFee? ReviewFee { get; set; }

        // FK đến AIConversationCharge
        public Guid? AIConversationChargeId { get; set; }

        [ForeignKey(nameof(AIConversationChargeId))]
        public virtual AIConversationCharge? AIConversationCharge { get; set; }

        public Guid? CourseId { get; set; }
        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; }
        // Navigation
        public virtual User User { get; set; }

      

    }
}