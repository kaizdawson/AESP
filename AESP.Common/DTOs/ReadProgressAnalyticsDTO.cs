using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class ReadProgressAnalyticsDTO
    {
        public Guid ProgressAnalyticsId { get; set; }
        public DateTime DateRecorded { get; set; }
        public double SpeakingTime { get; set; }           
        public int SessionsCompleted { get; set; }
        public double PronunciationScoreAvg { get; set; }
        public Guid LearnerProfileId { get; set; }  
    }
}
