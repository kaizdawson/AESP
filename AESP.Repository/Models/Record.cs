using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Record
    {
        [Key]
        public Guid RecordId { get; set; }

        [ForeignKey("LearnerRecordCategory")]
        public Guid LearnerRecordCategoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string AudioRecordingURL { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public string AIFeedback { get; set; }
        public double Score { get; set; }

        public virtual LearnerRecordCategory LearnerRecordCategory { get; set; }
    }
}
