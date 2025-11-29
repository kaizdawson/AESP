using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class PendingReviewItemDto
    {
        public string Type { get; set; }
        public Guid Id { get; set; }
        public DateTime SubmittedAt { get; set; }

        public string QuestionText { get; set; }
        public string TranscribedText { get; set; }
        public string AIFeedback { get; set; }

        public double AIScore { get; set; }   // ✅ DÙNG DOUBLE CHUNG
        public string AudioUrl { get; set; }

        public int NumberOfReview { get; set; }
        public string LearnerFullName { get; set; }
    }
}
