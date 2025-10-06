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

        [ForeignKey("LearnerProfile")]
        public Guid LearnerProfileId { get; set; }

        public double Score { get; set; }
        public string Feedback { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Type { get; set; }
        public int NumberOfQuestion { get; set; }

        public virtual LearnerProfile LearnerProfile { get; set; }

        public virtual ICollection<PronunciationResult> PronunciationResults { get; set; }
        public virtual ICollection<AssessmentDetail> AssessmentDetails { get; set; }
    }
}
