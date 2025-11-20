using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AESP.Repository.Models
{
    public class TransferTransaction
    {
        [Key]
        public Guid TransferTransactionId { get; set; } = Guid.NewGuid();




        [ForeignKey(nameof(LearnerProfile))]
        public Guid LearnerProfileId { get; set; }    // learner_id trong UML

        [ForeignKey(nameof(ReviewerProfile))]
        public Guid ReviewerProfileId { get; set; }   // reviewer_id trong UML

        [ForeignKey(nameof(Review))]
        public Guid? ReviewId { get; set; }

        public decimal AmountCoin { get; set; }

        public string Comment { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string TransactionType { get; set; } = "ReviewPayment";

        // --- Navigation ---
        public virtual LearnerProfile LearnerProfile { get; set; }
        public virtual ReviewerProfile ReviewerProfile { get; set; }
        public virtual Review? Review { get; set; }



    }
}