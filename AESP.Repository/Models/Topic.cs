using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Topic
    {
        [Key]
        public Guid TopicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int NumberOfChapter { get; set; }

        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<LearningPathTopic> LearningPathTopics { get; set; } = new List<LearningPathTopic>();


    }
}
