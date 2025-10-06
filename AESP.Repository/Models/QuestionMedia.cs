using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public string Accent { get; set; }
        public string AudioUrl { get; set; }
        public string VideoUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Source { get; set; }

        public virtual Question Question { get; set; }
    }
}
