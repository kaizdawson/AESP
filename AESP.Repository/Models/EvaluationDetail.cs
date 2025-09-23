using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class EvaluationDetail
    {
        [Key]
        public Guid EvaluationId { get; set; }
        public string Word { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public double Pronunciation { get; set; }



        public Guid StudentAnswerId { get; set; }
        [ForeignKey(nameof(StudentAnswerId))]
        public StudentAnswer? StudentAnswer { get; set; }
    }
}
