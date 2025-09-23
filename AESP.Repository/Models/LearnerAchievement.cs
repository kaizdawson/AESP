using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearnerAchievement
    {
        [Key]
        public Guid LearnerAchievementId { get; set; }
        public DateTime EarnedDate { get; set; }


        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; }

        public Guid AchievementId { get; set; }
        [ForeignKey(nameof(AchievementId))]
        public Achievement Achievement { get; set; }
    }
}
