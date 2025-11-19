using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class BuyReviewFeeRequest
    {
        public Guid ReviewFeeId { get; set; }
        public Guid LearnerAnswerId { get; set; }
    }

}
