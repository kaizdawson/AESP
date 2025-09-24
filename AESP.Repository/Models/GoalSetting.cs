using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class GoalSetting
    {
        [Key]
        public Guid GoalId { get; set; }
        public string TargetLevel { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public DateTime? DailyMinutes { get; set; }
        public string Status { get; set; } = string.Empty;

        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; } = null!;

        public ICollection<LearnerChallenge> LearnerChallenges { get; set; } = new List<LearnerChallenge>();

    }
}
