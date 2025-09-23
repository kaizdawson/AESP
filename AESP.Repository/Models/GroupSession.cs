using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class GroupSession
    {
        [Key]
        public Guid GroupSessionId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }



        public Guid ConversationTopicId { get; set; }
        [ForeignKey(nameof(ConversationTopicId))]
        public ConversationTopic ConversationTopic { get; set; } = null!;

        public ICollection<GroupSessionMember> GroupSessionMembers { get; set; } = new List<GroupSessionMember>();
        public ICollection<GroupPracticeDetail> GroupPracticeDetails { get; set; } = new List<GroupPracticeDetail>();
    }
}
