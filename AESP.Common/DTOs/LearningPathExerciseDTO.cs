using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class ReadLearningPathExerciseDTO
    {
        public Guid LearningPathExerciseId { get; set; }
        public Guid ExerciseId { get; set; }
        public double ScoreAchieved { get; set; }
        public string Status { get; set; }

      
    }
}
