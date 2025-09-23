using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Report
    {
        [Key]
        public Guid ReportId { get; set; }
        public string PerformanceSummary { get; set; }
        public string Recommendations { get; set; }



        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; }


        public Guid PeriodId { get; set; }
        [ForeignKey(nameof(PeriodId))]
        public Period Period { get; set; }
    }
}
