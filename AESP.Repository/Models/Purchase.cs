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
        public Guid PurchaseId { get; set; }

        [ForeignKey("LearnerProfile")]
        public Guid LearnerProfileId { get; set; }

        [ForeignKey("Subscription")]
        public Guid SubscriptionId { get; set; }

        [ForeignKey("ReviewFee")]
        public Guid ReviewFeeId { get; set; }

        public string PaymentStatus { get; set; }
        public DateTime PurchaseDate { get; set; }
        public double PriceServicePackage { get; set; }
        public double PriceReviewFee { get; set; }

        public  LearnerProfile LearnerProfile { get; set; }

        public  Subscription Subscription { get; set; }
        public  ReviewFee ReviewFee { get; set; }

    }
}
