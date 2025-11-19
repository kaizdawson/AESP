using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class ReviewFeeMenuDto
    {
        public Guid ReviewFeeId { get; set; }
        public int NumberOfReview { get; set; }
        public decimal PricePerReviewFee { get; set; }
        public decimal AmountMoney { get; set; }
    }

}
