using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class WithdrawRequest
    {
        public int Coin { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
    }

}
