using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearnerAnswer
    {
        [Key]
        public Guid LearnerAnswerId { get; set; }

        public Guid LearnerProfileId { get; set; }

        public Guid LearningPathQuestionId { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public string AudioRecordingUrl { get; set; } = string.Empty;

        public string TranscribedText { get; set; } = string.Empty;

        public int ScoreForVoice { get; set; }

        public string ExplainTheWrongForVoiceAI { get; set; } = string.Empty;

        public bool IsNeededReviewed { get; set; } = false;

        public string Status { get; set; } = string.Empty;

        public int NumberofReview { get; set; }
        // ===== RELATION =====

        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; }

        // 🆕 Mỗi Answer thuộc 1 LearningPathQuestion
        [ForeignKey(nameof(LearningPathQuestionId))]
        public LearningPathQuestion LearningPathQuestion { get; set; }

        



        public virtual ICollection<Review> Reviews { get; set; }

        public virtual ICollection<PhonemeResult> PhonemeResults { get; set; }
    }
}
