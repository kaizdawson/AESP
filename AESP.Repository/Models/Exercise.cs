using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Exercise
    {
        [Key]
        public Guid ExerciseId { get; set; }

        [ForeignKey("Chapter")]
        public Guid ChapterId { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }
        public int NumberOfQuestion { get; set; }

        public virtual Chapter Chapter { get; set; }

        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<LearningPathExercise> LearningPathExercises { get; set; }
    }
}
