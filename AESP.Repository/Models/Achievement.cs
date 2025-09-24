using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Achievement
    {

        [Key]
        public Guid AchievementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PointsReward { get; set; }
        public string IconUrl { get; set; } = string.Empty;

        public ICollection<LearnerAchievement> LearnerAchievements { get; set; } = new List<LearnerAchievement>();
    }
}
