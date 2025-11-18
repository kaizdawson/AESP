using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class UpdateReviewFeeDetailDto
    {
        public Guid ReviewFeeId { get; set; }

        // Chi tiết giá và chính sách mới
        public decimal PricePerReviewFee { get; set; }

        // Ngày áp dụng mới (có thể là ngày trong tương lai)
        public DateTime AppliedDate { get; set; }

        public decimal PercentOfSystem { get; set; }

        public decimal PercentOfReviewer { get; set; }
    }
}
