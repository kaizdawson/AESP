using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Assessment
    {
        [Key]
        public Guid AssessmentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Score { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
        public double NumberOfQuestion { get; set; }


        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; } = null!;
        public ICollection<AssessmentDetail> AssessmentDetails { get; set; } = new List<AssessmentDetail>();
        public ICollection<PronunciationResult> PronunciationResults { get; set; } = new List<PronunciationResult>();

    }
}
