using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearningPathTopic
    {
        [Key]
        public Guid LearningPathTopicId { get; set; }
        public string Status { get; set; }
        public int OrderIndex { get; set; }
        public string Progress { get; set; }
        public int NumberOfChapter { get; set; }




        public Guid PathId { get; set; }
        [ForeignKey(nameof(PathId))]
        public LearningPath LearningPath { get; set; }

        public Guid TopicId { get; set; }
        [ForeignKey(nameof(TopicId))]
        public Topic Topic { get; set; }

        public ICollection<LearningPathChapter> LearningPathChapters { get; set; }

    }
}
