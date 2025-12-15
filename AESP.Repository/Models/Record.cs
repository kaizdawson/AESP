using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Record
    {
        [Key]
        public Guid RecordId { get; set; }

        [ForeignKey(nameof(LearnerRecord))]
        public Guid LearnerRecordId { get; set; }

        


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string AudioRecordingURL { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TranscribedText { get; set; } = string.Empty;
        public string AIFeedback { get; set; } = string.Empty;
        public double Score { get; set; }
        public int NumberOfReview { get; set; }

        public bool IsNeedReviewed { get; set; }

        public virtual LearnerRecord LearnerRecord { get; set; }

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
