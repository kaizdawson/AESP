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

        [ForeignKey("Assessment")]
        public Guid AssessmentId { get; set; }

        [ForeignKey("QuestionAssessment")]
        public Guid QuestionAssessmentId { get; set; }

        public string Type { get; set; }             // Loại câu hỏi (Word / Sentence / Pronunciation)
        public double Score { get; set; }            // Điểm cho từng câu
        public string AI_Feedback { get; set; }      // Phản hồi từ AI
        public string AnswerAudio { get; set; }      // Link âm thanh câu trả lời của học viên

        public virtual Assessment Assessment { get; set; }
        public virtual QuestionAssessment QuestionAssessment { get; set; }
    }
}
