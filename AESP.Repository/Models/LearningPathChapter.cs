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

        [ForeignKey("LearningPathCourse")]
        public Guid LearningPathCourseId { get; set; }

        public string Status { get; set; }
        public double Progress { get; set; }
        public int NumberOfModule { get; set; }
        public int OrderIndex { get; set; }
        public Guid ChapterId { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter Chapter { get; set; }
        public virtual LearningPathCourse LearningPathCourse { get; set; }

        public virtual ICollection<LearningPathExercise> LearningPathExercises { get; set; }
    }
}
