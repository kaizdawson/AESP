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

        [Required]
        public string Text { get; set; }

        public string Type { get; set; }
        public int OrderIndex { get; set; }
        public string IPA { get; set; }
        public string PhonemeJson { get; set; }

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
