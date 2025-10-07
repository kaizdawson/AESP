using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Course 
    {
        [Key]
        public Guid CourseId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }
        public string Level { get; set; } = string.Empty;

        public virtual ICollection<Chapter> Chapters { get; set; }
        public virtual ICollection<LearningPathCourse> LearningPathCourses { get; set; }
    }
}
