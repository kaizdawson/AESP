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
        public string PaymentStatus { get; set; }
        public DateTime PurchaseDate { get; set; }
        public double Amount { get; set; }
        public string Method { get; set; }

        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; }



    }
}
