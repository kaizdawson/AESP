using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Review
    {
        [Key]
        public Guid ReviewId { get; set; }
        public Guid LearnerAnswerId { get; set; }
        public Guid ReviewerProfileId { get; set; }
        public Guid RecordId { get; set; }

        [ForeignKey("LearnerAnswerId")]
        public virtual LearnerAnswer LearnerAnswer { get; set; }
        [ForeignKey("ReviewerProfileId")]
        public virtual ReviewerProfile ReviewerProfile { get; set; }
        [ForeignKey("RecordId")]

        public string Comment { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Status { get; set; } = string.Empty;

        public virtual Record Record { get; set; }



        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }
}
