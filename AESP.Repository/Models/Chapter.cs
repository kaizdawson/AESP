using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Chapter
    {
        [Key]
        public Guid ChapterId { get; set; }

        [ForeignKey("Course")]
        public Guid CourseId { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int NumberOfExercise { get; set; }

        public virtual Course Course { get; set; }
        public virtual ICollection<Exercise> Exercises { get; set; }
    }
}
