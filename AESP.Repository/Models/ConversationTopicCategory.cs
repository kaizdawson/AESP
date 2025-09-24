using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class ConversationTopicCategory
    {
        [Key]
        public Guid ConversationTopicCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<ConversationTopic> ConversationTopics { get; set; } = new List<ConversationTopic>();
    }
}
