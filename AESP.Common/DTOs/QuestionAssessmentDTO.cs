using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class CreateQuestionAssessmentDTO
    {
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateQuestionAssessmentDTO
    {
        public string? Type { get; set; }
        public string? Content { get; set; }
    }

    public class ReadQuestionAssessmentDTO
    {
        public Guid QuestionAssessmentId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
