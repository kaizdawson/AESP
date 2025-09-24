using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class ConversationTopic
    {
        [Key]
        public Guid ConversationTopicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;



        public Guid ConversationTopicCategoryId { get; set; }
        [ForeignKey(nameof(ConversationTopicCategoryId))]
        public ConversationTopicCategory ConversationTopicCategory { get; set; } = null!;


        public ICollection<GroupSession> GroupSessions { get; set; } = new List<GroupSession>();
        public ICollection<PracticeSession> PracticeSessions { get; set; } = new List<PracticeSession>();

    }
}
