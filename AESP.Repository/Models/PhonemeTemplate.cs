using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class PhonemeTemplate
    {
        [Key]
        public Guid PhonemeId { get; set; }

        [ForeignKey("Question")]
        public Guid QuestionId { get; set; }

        [Required]
        public string Symbol { get; set; }
        public int OrderIndex { get; set; }

        public virtual Question Question { get; set; }
        public virtual ICollection<Stress> Stresses { get; set; }
    }
}
