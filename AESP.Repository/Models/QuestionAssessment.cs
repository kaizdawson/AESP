using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class QuestionAssessment
    {
        [Key]
        public Guid QuestionAssessmentId { get; set; }

        public string Type { get; set; }
        public string Content { get; set; }

        [ForeignKey("Assessment")]
        public Guid? AssessmentId { get; set; }

        public virtual ICollection<AssessmentDetail> AssessmentDetails { get; set; }
    }
}
