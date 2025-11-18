using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class CreateReviewFeePackageDto
    {
        // Thông tin ReviewFee (Gói)
        public int NumberOfReview { get; set; }

        // Thông tin ReviewFeeDetail (Chi tiết giá và chính sách)
        public decimal PricePerReviewFee { get; set; }
        public decimal PercentOfSystem { get; set; } // Phần trăm hệ thống giữ lại (e.g., 0.3)
        public decimal PercentOfReviewer { get; set; } // Phần trăm cho Reviewer (e.g., 0.7)
    }
}
