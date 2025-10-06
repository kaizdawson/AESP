using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearningPathExercise
    {
        [Key]
        public Guid LearningPathExerciseId { get; set; }

        [ForeignKey("LearningPathChapter")]
        public Guid LearningPathChapterId { get; set; }

        public Guid ExerciseId { get; set; }

        public double ScoreAchieved { get; set; }
        public int OrderIndex { get; set; }
        public string Status { get; set; }
        public int NumberOfQuestion { get; set; }

        [ForeignKey("ExerciseId")]
        public virtual Exercise Exercise { get; set; }
        public virtual LearningPathChapter LearningPathChapter { get; set; }
    }
}
