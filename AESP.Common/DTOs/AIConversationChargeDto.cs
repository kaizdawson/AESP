namespace AESP.Common.DTOs
{
    public class AIConversationChargeDto
    {
        public Guid AIConversationChargeId { get; set; }
        public int AmountCoin { get; set; }
        public int AllowedMinutes { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
