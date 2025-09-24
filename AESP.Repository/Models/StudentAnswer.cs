using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class StudentAnswer
    {
        [Key]
        public Guid StudentAnswerId { get; set; }
        public string AudioRecordingUrl { get; set; } = string.Empty;
        public string TranscriptText { get; set; } = string.Empty;
        public double ScoreForVoice { get; set; }
        public string ExplainTheWrongForVoice { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }



        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; } = null!;


        public Guid QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public Question Question { get; set; } = null!;


        public Guid? AnswerOptionId { get; set; }
        [ForeignKey(nameof(AnswerOptionId))]
        public AnswerOption? AnswerOption { get; set; }


        public Guid? ImageOptionId { get; set; }
        [ForeignKey(nameof(ImageOptionId))]
        public ImageOption? ImageOption { get; set; }

        public Guid? ChoiceId { get; set; }
        [ForeignKey(nameof(ChoiceId))]
        public ChoiceOption? ChoiceOption { get; set; }

        public Guid? PathModuleId { get; set; }
        [ForeignKey(nameof(PathModuleId))]
        public LearningPathModule? LearningPathModule { get; set; }

        public virtual ICollection<EvaluationDetail> EvaluationDataVoice { get; set; } = new List<EvaluationDetail>();
        public ICollection<ChoiceStudent> ChoiceStudents { get; set; } = new List<ChoiceStudent>();
    }
}
