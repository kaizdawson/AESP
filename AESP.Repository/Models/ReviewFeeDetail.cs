using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public  class ReviewFeeDetail
    {
        [Key]
        public Guid ReviewFeeDetailId { get; set; }
        public decimal PricePerReviewFee { get; set; }

        public DateTime AppliedDate { get; set; }

        public decimal PercentOfSystem { get; set; }

        public decimal PercentOfReviewer { get; set; }

        // FK 
        [ForeignKey(nameof(ReviewFee))]
        public Guid ReviewFeeId { get; set; }

        // Navigation
        public virtual ReviewFee ReviewFee { get; set; }
    }
}
