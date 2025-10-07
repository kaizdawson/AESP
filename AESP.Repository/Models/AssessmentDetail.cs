using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class AssessmentDetail
    {
        [Key]
        public Guid AssessmentDetailId { get; set; }
        public double Score { get; set; }
        public string Type { get; set; } = string.Empty;
        public string AI_Feedback { get; set; } = string.Empty;
        public string AnswerAudio { get; set; } = string.Empty;


        public Guid AssessmentId { get; set; }
        [ForeignKey(nameof(AssessmentId))]
        public Assessment Assessment { get; set; } = null!;


        public Guid QuestionAssessmentId { get; set; }
        [ForeignKey(nameof(QuestionAssessmentId))]
        public QuestionAssessment QuestionAssessment { get; set; } = null!;
    }
}
