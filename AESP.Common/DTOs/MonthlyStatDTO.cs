using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class MonthlyStatDTO
    {
        public int Month { get; set; }
        public int Count { get; set; }       // Số gói bán
        public decimal Revenue { get; set; } // Doanh thu
    }
}
