using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearnerCourse
    {
        [Key]
        public Guid LearnerCourseId { get; set; }

        [ForeignKey("LearnerProfile")]
        public Guid LearnerProfileId { get; set; }

        public DateTime GeneratedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int NumberOfCourse { get; set; }
        public double Progress { get; set; }

        public virtual LearnerProfile LearnerProfile { get; set; }
    }
}
