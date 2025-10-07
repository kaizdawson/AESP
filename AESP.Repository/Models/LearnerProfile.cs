using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class LearnerProfile
    {
        [Key]
        public Guid LearnerProfileId { get; set; }
        public string Level { get; set; } = string.Empty;
        public double PronunciationScore { get; set; }

        public int DailyMinutes { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public ICollection<ProgressAnalytics> ProgressAnalytics { get; set; } = new List<ProgressAnalytics>();

        public virtual ICollection<Assessment> Assessments { get; set; }
        public virtual ICollection<LearnerCourse> LearnerCourses { get; set; }
        public virtual ICollection<LearnerAnswer> LearnerAnswers { get; set; }
        public virtual ICollection<LearnerRecordCategory> LearnerRecordCategories { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }


    }
}
