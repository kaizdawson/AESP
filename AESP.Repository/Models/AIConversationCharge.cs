using System;
using System.ComponentModel.DataAnnotations;

namespace AESP.Repository.Models
{
    public class AIConversationCharge : BaseEntity
    {
        [Key]
        public Guid AIConversationChargeId { get; set; } = Guid.NewGuid();

        public decimal AmountCoin { get; set; }

        public int AllowedMinutes { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Purchase sẽ tham chiếu đến bảng này
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}
