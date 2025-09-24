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

        public string Skill { get; set; } = string.Empty;
        public double Score { get; set; }
        public string AI_Feedback { get; set; } = string.Empty;
        public string MentorFeedback { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;




        public Guid AssessmentId { get; set; }
        [ForeignKey(nameof(AssessmentId))]
        public Assessment Assessment { get; set; } = null!;


        public Guid QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public Question Question { get; set; } = null!;
    }
}
