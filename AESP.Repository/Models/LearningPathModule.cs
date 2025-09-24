using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearningPathModule
    {
        [Key]
        public Guid LearningPathModuleId { get; set; }

        public string Status { get; set; }
        public int OrderIndex { get; set; }
        public double ScoreAchieved { get; set; }



        public Guid LearningPathChapterId { get; set; }
        [ForeignKey(nameof(LearningPathChapterId))]
        public LearningPathChapter LearningPathChapter { get; set; }


        public ICollection<StudentAnswer> StudentAnswers { get; set; }


        public Guid ModuleId { get; set; }
        [ForeignKey(nameof(ModuleId))]
        public ChapterModule ChapterModule { get; set; }
    }
}
