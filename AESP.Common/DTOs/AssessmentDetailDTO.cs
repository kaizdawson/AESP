using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class CreateAssessmentDetailDTO
    {
        public Guid AssessmentId { get; set; }                  // ✅ FK bắt buộc
        public Guid QuestionAssessmentId { get; set; }          // ✅ FK bắt buộc
        public double Score { get; set; }
        public string Type { get; set; } = string.Empty;
        public string AI_Feedback { get; set; } = string.Empty;
        public string AnswerAudio { get; set; } = string.Empty;
    }

    public class UpdateAssessmentDetailDTO
    {
        public double? Score { get; set; }
        public string? Type { get; set; }
        public string? AI_Feedback { get; set; }
        public string? AnswerAudio { get; set; }
    }

    public class ReadAssessmentDetailDTO
    {
        public Guid AssessmentDetailId { get; set; }
        public Guid AssessmentId { get; set; }                  // ✅ Hiển thị rõ FK
        public Guid QuestionAssessmentId { get; set; }          // ✅ Hiển thị rõ FK
        public double Score { get; set; }
        public string Type { get; set; } = string.Empty;
        public string AI_Feedback { get; set; } = string.Empty;
        public string AnswerAudio { get; set; } = string.Empty;
    }
}
