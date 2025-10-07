using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Question
    {
        [Key]
        public Guid QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int OrderIndex { get; set; }

        public string IPA { get; set; } = string.Empty;
        public string PhonemeJson { get; set; } = string.Empty;

        [ForeignKey("Exercise")]
        public Guid ExerciseId { get; set; }
        public virtual Exercise Exercise { get; set; }


        public virtual ICollection<QuestionMedia> QuestionMedias { get; set; }
        public virtual ICollection<AssessmentDetail> AssessmentDetails { get; set; }
        public virtual ICollection<LearnerAnswer> LearnerAnswers { get; set; }
        public virtual ICollection<PhonemeResult> PhonemeResults { get; set; }
        public virtual ICollection<PhonemeTemplate> PhonemeTemplates { get; set; }

    }
}
