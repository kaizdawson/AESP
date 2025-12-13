using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class RecordCharge : BaseEntity
    {
        [Key]
        public Guid RecordChargeId { get; set; } = Guid.NewGuid();

        // Giá gói (coin)
        public int AmountCoin { get; set; }

        // Số record được phép tạo
        public int AllowedRecordCount { get; set; }

        // Trạng thái gói
        public string Status { get; set; } = "Active";
    }
}
