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
        public Guid PronunciationResultId { get; set; }
        public string WordOrPhoneme { get; set; }
        public string ExpectedSound { get; set; }
        public string LearnerSound { get; set; }
        public DateTime Timestamp { get; set; }
        public double AccuracyScore { get; set; }
        public Guid AssessmentId { get; set; }
        [ForeignKey(nameof(AssessmentId))]
        public Assessment Assessment { get; set; }
    }
}
