using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearningPath
    {
        [Key]
        public Guid PathId { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string TargetLevel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double NumberOfTopic { get; set; }
        public string Progress { get; set; } = string.Empty;




        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; } = null!;
        public ICollection<LearningPathTopic> LearningPathTopics { get; set; } = new List<LearningPathTopic>();

    }
}
