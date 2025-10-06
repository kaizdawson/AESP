using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class PronunciationResult
    {
        [Key]
        public Guid ResultId { get; set; }

        [ForeignKey("Assessment")]
        public Guid AssessmentId { get; set; }

        public string WordOrPhoneme { get; set; }
        public string ExpectedSound { get; set; }
        public string LearnerSound { get; set; }
        public DateTime Timestamp { get; set; }
        public double AccuracyScore { get; set; }

        public virtual Assessment Assessment { get; set; }
    }
}
