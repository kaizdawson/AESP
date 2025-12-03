using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class UpdateUpcomingReviewFeeDetailDto
    {
        public Guid ReviewFeeDetailId { get; set; }   // ✅ BẮT BUỘC
        public decimal PricePerReviewFee { get; set; }
        public DateTime AppliedDate { get; set; }
        public decimal PercentOfSystem { get; set; }
        public decimal PercentOfReviewer { get; set; }
    }
}
