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
        public string Goal { get; set; } = string.Empty;
        public string Favorite { get; set; } = string.Empty;
        public double PronunciationScore { get; set; }


        public ICollection<ProgressAnalytics> ProgressAnalytics { get; set; } = new List<ProgressAnalytics>();

        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
        public ICollection<GroupSessionMember> GroupSessionMembers { get; set; } = new List<GroupSessionMember>();
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

        public ICollection<PracticeSession> PracticeSessions { get; set; } = new List<PracticeSession>();
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public ICollection<LearnerAchievement> LearnerAchievements { get; set; } = new List<LearnerAchievement>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<SendToLearner> SendToLearners { get; set; } = new List<SendToLearner>();
        public ICollection<GoalSetting> GoalSettings { get; set; } = new List<GoalSetting>();
        public ICollection<LearnerChallenge> LearnerChallenges { get; set; } = new List<LearnerChallenge>();
        public ICollection<LearningPath> LearningPaths { get; set; } = new List<LearningPath>();

        public virtual Leaderboard Leaderboard { get; set; } = null!;

    }
}
