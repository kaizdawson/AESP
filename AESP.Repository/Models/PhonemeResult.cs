using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class PhonemeResult
    {
        [Key]
        public Guid PhonemeResultId { get; set; }
        public Guid LearnerAnswerId { get; set; }
        public Guid QuestionId { get; set; }

        [ForeignKey("LearnerAnswerId")]
        public virtual LearnerAnswer LearnerAnswer { get; set; }
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        public virtual ICollection<StressResult> StressResults { get; set; }
    }
}
