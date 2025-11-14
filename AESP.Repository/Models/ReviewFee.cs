using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class ReviewFee
    {
        [Key]
        public Guid ReviewFeeId { get; set; }

        // Số lượng bài review trong gói
        public int NumberOfReview { get; set; }

        // Navigation
        public virtual ICollection<ReviewFeeDetail> ReviewFeeDetails { get; set; } = new List<ReviewFeeDetail>();

        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}
