using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class ChoiceStudent
    {
        [Key]
        public Guid ChoiceStudentId { get; set; }
        public bool IsChosen { get; set; }
        public string Explain { get; set; } = string.Empty;

        public Guid StudentAnswerId { get; set; }
        [ForeignKey(nameof(StudentAnswerId))]
        public StudentAnswer StudentAnswer { get; set; } = null!;

        public Guid QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public Question Question { get; set; } = null!;
    }
}
