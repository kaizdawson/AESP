using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearningPathCourse
    {
        [Key]
        public Guid LearningPathCourseId { get; set; }

        [ForeignKey("LearnerCourse")]
        public Guid LearnerCourseId { get; set; }

        public string Status { get; set; }
        public double Progress { get; set; }
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }
        public Guid CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
        public virtual LearnerCourse LearnerCourse { get; set; }

        public virtual ICollection<LearningPathChapter> LearningPathChapters { get; set; }
    }
}
