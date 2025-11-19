using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class ReviewFeeDetailResponseDto
    {
        public Guid ReviewFeeDetailId { get; set; }
        public decimal PricePerReviewFee { get; set; }
        public DateTime AppliedDate { get; set; }
        public decimal PercentOfSystem { get; set; }
        public decimal PercentOfReviewer { get; set; }
    }
    public class ReviewFeePackageResponseDto
    {
        public Guid ReviewFeeId { get; set; }
        public int NumberOfReview { get; set; }

        // Chính sách giá hiện tại (hoặc gần nhất)
        public ReviewFeeDetailResponseDto? CurrentPricePolicy { get; set; }
    }
}
