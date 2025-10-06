using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Feedback
    {
        [Key]
        public Guid FeedbackId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public Guid TargetId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
