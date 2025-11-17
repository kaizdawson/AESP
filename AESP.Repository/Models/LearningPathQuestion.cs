using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearningPathQuestion
    {
        [Key]
        public Guid LearningPathQuestionId { get; set; } = Guid.NewGuid();

        public Guid LearningPathExerciseId { get; set; }

        [ForeignKey(nameof(LearningPathExerciseId))]
        public LearningPathExercise LearningPathExercise { get; set; }

        public Guid QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public Question Question { get; set; }


        public int NumberOfRetake { get; set; } = 0;

        public int Score { get; set; } = 0;

        public string Status { get; set; } = "NotStarted";
        // NotStarted / InProgress / Completed

        public virtual ICollection<LearnerAnswer> LearnerAnswers { get; set; }
    }
}
