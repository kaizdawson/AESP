using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class ChoiceOption
    {
        [Key]
        public Guid ChoiceId { get; set; }
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
        public string Explain { get; set; } = string.Empty;


        public Guid QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public Question Question { get; set; } = null!;


        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}
