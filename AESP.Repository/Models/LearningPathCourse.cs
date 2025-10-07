using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
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

        public string Status { get; set; } = string.Empty;
        public double Progress { get; set; }
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }
        public Guid CourseId { get; set; }

        [ForeignKey("CourseId")]
        public  Course Course { get; set; }
        public  LearnerCourse LearnerCourse { get; set; }

        public virtual ICollection<LearningPathChapter> LearningPathChapters { get; set; }
    }
}
