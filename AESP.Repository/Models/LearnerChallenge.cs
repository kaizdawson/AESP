using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearnerChallenge
    {
        [Key]
        public Guid LearnerChallengeId { get; set; }
        public Guid LearnerProfileId { get; set; }
        public Guid GoalId { get; set; }
        public Guid ChallengeId { get; set; }
        public DateTime CompletionStatus { get; set; }

        public LearnerProfile LearnerProfile { get; set; } = null!;
        public GoalSetting Goal { get; set; } = null!;
        public Challenge Challenge { get; set; } = null!;
    }
}
