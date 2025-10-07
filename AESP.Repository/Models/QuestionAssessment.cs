using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class QuestionAssessment 
    {
        [Key]
        public Guid QuestionAssessmentId { get; set; }

        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        

        public virtual ICollection<AssessmentDetail> AssessmentDetails { get; set; }
    }
}
