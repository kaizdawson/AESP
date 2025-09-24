using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearningPathChapter
    {
        [Key]
        public Guid LearningPathChapterId { get; set; }
        public string Status { get; set; }
        public int OrderIndex { get; set; }
        public string Progress { get; set; }
        public int NumberOfModule { get; set; }

        public Guid PathId { get; set; }

        // FK -> LearningPathTopic
        public Guid LearningPathTopicId { get; set; }
        [ForeignKey(nameof(LearningPathTopicId))]
        public LearningPathTopic LearningPathTopic { get; set; } = null!;

        // FK -> Chapter
        public Guid ChapterId { get; set; }   // 🔑 Thêm dòng này
        [ForeignKey(nameof(ChapterId))]
        public Chapter Chapter { get; set; } = null!;

        public ICollection<LearningPathModule> LearningPathModules { get; set; } = new List<LearningPathModule>();
    }
}
