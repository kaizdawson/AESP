using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearnerRecordCategory
    {
        [Key]
        public Guid LearnerRecordCategoryId { get; set; }

        [ForeignKey("LearnerProfile")]
        public Guid LearnerId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = string.Empty;

        public  LearnerProfile LearnerProfile { get; set; }

        public virtual ICollection<Record> Records { get; set; }
    }
}
