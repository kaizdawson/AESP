using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class ToggleReviewRequestDTO
    {
        public bool IsNeededReview { get; set; }
        public int NumberOfReview { get; set; }
    }

}
