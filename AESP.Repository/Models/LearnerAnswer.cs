using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearnerAnswer
    {
        [Key]
        public Guid LearnerAnswerId { get; set; }
        public Guid LearnerProfileId { get; set; }
        public Guid QuestionId { get; set; }

        [ForeignKey("LearnerProfileId")]
        public virtual LearnerProfile Learner { get; set; }
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<PhonemeResult> PhonemeResults { get; set; }
    }
}
