using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Course : BaseEntity
    {
        [Key]
        public Guid CourseId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }
        public string Level { get; set; } = string.Empty;


        public decimal Price { get; set; } = 0;

        [Range(1, 365, ErrorMessage = "Thời lượng phải từ 1 đến 365 ngày.")]
        public int Duration { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        // 🆕 Thêm mô tả khóa học
        [MaxLength(2000)]
        public string? Description { get; set; }

        public virtual ICollection<Chapter> Chapters { get; set; }

        public virtual ICollection<LearningPathCourse> LearningPathCourses { get; set; }
    }
}
