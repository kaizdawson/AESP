using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class BuyRecordReviewFeeRequest
    {
        public Guid ReviewFeeId { get; set; }
        public Guid RecordId { get; set; }
    }

}
