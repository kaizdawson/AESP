using AESP.API.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearnerRecord : BaseEntity
    {
        [Key]
        public Guid LearnerRecordId { get; set; }

        [ForeignKey(nameof(LearnerProfile))]
        public Guid LearnerId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int NumberOfRecord { get; set; } = 0;

        // Navigation
        public virtual LearnerProfile LearnerProfile { get; set; }
        public virtual ICollection<RecordContent> RecordContents { get; set; } = new List<RecordContent>();
    }

}
