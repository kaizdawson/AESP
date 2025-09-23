using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Leaderboard
    {
        [Key]
        public Guid LeaderboardId { get; set; }
        public int Points { get; set; }
        public int Rank { get; set; }
        public int Streak { get; set; }

        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile? LearnerProfile { get; set; }
    }
}
