using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class AIConversationCharge
    {
        [Key]
        public Guid AIConversationChargeId { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public decimal AmountCoin { get; set; }

        public int AllowedMinutes { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public string Status { get; set; } = "Active"; // Active / Expired / Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string RoomId { get; set; } = string.Empty;

        // Lưu JSON nội dung hội thoại (AI message history, transcript,…)
        public string ContentJson { get; set; } = string.Empty;

        // --- Navigation ---
        public virtual User User { get; set; }

        public virtual ICollection<CoinTransaction> CoinTransactions { get; set; } = new List<CoinTransaction>();
    }
}