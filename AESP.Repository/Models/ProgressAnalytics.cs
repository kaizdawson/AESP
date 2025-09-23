using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class ProgressAnalytics
    {
        [Key]
        public Guid ProgressAnalyticsId { get; set; }
        public DateTime DateRecorded { get; set; }
        public double SpeakingTime { get; set; }
        public int SessionsCompleted { get; set; }
        public double PronunciationScoreAvg { get; set; }
        public double GrammarAccuracy { get; set; }
        public double VocabularyUsage { get; set; }
        public double ConfidenceLevel { get; set; }
        public int StreakDays { get; set; }
        public string HeatmapData { get; set; }


        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; }
    }
}
