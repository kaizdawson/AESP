using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class PracticeSession
    {
        [Key]
        public Guid PracticeSessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;


        public Guid ConversationTopicId { get; set; }
        [ForeignKey(nameof(ConversationTopicId))]
        public ConversationTopic ConversationTopic { get; set; } = null!;

        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; } = null!;
        public Guid RoomId { get; set; }
        [ForeignKey(nameof(RoomId))]
        public Room Room { get; set; } = null!;
        public ICollection<PracticeDetail> PracticeDetails { get; set; } = new List<PracticeDetail>();

        public Booking Booking { get; set; }
    }
}
