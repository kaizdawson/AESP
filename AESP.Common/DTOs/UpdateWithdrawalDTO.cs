using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class UpdateWithdrawalDTO
    {
      

        [Required]
        [Range(1000, int.MaxValue, ErrorMessage = "Số tiền rút tối thiểu là 1,000 VNĐ.")]
        public int NewAmountMoney { get; set; }

        [Required]
        [MaxLength(100)]
        public string BankName { get; set; }

        [Required]
        [MaxLength(100)]
        public string AccountNumber { get; set; }
    }
}
