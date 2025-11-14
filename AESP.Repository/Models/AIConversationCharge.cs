using System;
using System.ComponentModel.DataAnnotations;

namespace AESP.Repository.Models
{
    public class AIConversationCharge : BaseEntity
    {
        [Key]
        public Guid AIConversationChargeId { get; set; } = Guid.NewGuid();

        public int AmountCoin { get; set; }

        public int AllowedMinutes { get; set; }

        public string Status { get; set; } = "Active";

    }
}
