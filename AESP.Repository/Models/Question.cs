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
        public string AudioUrl { get; set; } = string.Empty;
        public int OrderIndex { get; set; }


        public Guid ModuleId { get; set; }
        [ForeignKey(nameof(ModuleId))]
        public ChapterModule ChapterModule { get; set; } = null!;


        public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
        public ICollection<ImageOption> ImageOptions { get; set; } = new List<ImageOption>();
        public ICollection<ChoiceOption> ChoiceOptions { get; set; } = new List<ChoiceOption>();
        public ICollection<CorrectMatch> CorrectMatches { get; set; } = new List<CorrectMatch>();
        public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
        public ICollection<AssessmentDetail> AssessmentDetails { get; set; } = new List<AssessmentDetail>();

    }
}
