using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class QuestionMedia
    {
        [Key]
        public Guid QuestionMediaId { get; set; }

        [ForeignKey("Question")]
        public Guid QuestionId { get; set; }

        public string Accent { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;

        public virtual Question Question { get; set; }
    }
}
