using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class RecordContent : BaseEntity
    {
        [Key]
        public Guid RecordContentId { get; set; }

        [ForeignKey(nameof(LearnerRecord))]
        public Guid LearnerRecordId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public virtual LearnerRecord LearnerRecord { get; set; }
        public virtual ICollection<Record> Records { get; set; } = new List<Record>();
    }
}
