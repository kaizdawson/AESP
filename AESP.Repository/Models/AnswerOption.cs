using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class AnswerOption
    {
        [Key]
        public Guid AnswerOptionId { get; set; }
        public string Text { get; set; } = string.Empty;


        public Guid QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public Question Questions { get; set; } = null!;


        public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
        public ICollection<CorrectMatch> CorrectMatches { get; set; } = new List<CorrectMatch>();



    }
}
