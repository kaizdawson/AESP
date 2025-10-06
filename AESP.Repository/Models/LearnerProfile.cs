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
        public Guid UserId { get; set; }
        public string Level { get; set; }
        public double PronunciationScore { get; set; }
        public int DailyMinutes { get; set; }


        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<Assessment> Assessments { get; set; }
        public virtual ICollection<LearnerCourse> LearnerCourses { get; set; }
        public virtual ICollection<LearnerAnswer> LearnerAnswers { get; set; }
        public virtual ICollection<LearnerRecordCategory> LearnerRecordCategories { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }
        public virtual ICollection<ProgressAnalytics> ProgressAnalytics { get; set; }
    }
}
