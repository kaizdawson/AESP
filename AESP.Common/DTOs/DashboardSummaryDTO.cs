using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class DashboardSummaryDTO
    {
        public int TotalLearners { get; set; }
        public int TotalActiveLearners { get; set; }
        public int TotalServicePackages { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
