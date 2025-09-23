using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class CorrectMatch
    {
        [Key]
        public Guid MatchId { get; set; }
        public string Explain { get; set; } = string.Empty;



        public Guid QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public Question Question { get; set; } = null!;

        public Guid AnswerOptionId { get; set; }
        [ForeignKey(nameof(AnswerOptionId))]
        public AnswerOption AnswerOption { get; set; } = null!;

        public Guid ImageOptionId { get; set; }
        [ForeignKey(nameof(ImageOptionId))]

        public ImageOption ImageOption { get; set; } = null!;


    }
}
