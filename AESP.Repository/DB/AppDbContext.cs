using Microsoft.EntityFrameworkCore;
using AESP.Repository.Models;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Emit;

namespace AESP.Repository.DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
         public DbSet<LearnerProfile> LearnerProfiles { get; set; }
        public DbSet<MentorProfile> MentorProfiles { get; set; }
        public DbSet<MentorSchedule> MentorSchedules { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PracticeSession> PracticeSessions { get; set; }
        public DbSet<PracticeDetail> PracticeDetails { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<ImageOption> ImageOptions { get; set; }
        public DbSet<ChoiceOption> ChoiceOptions { get; set; }
        public DbSet<CorrectMatch> CorrectMatches { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<EvaluationDetail> EvaluationDetails { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<AssessmentDetail> AssessmentDetails { get; set; }
        public DbSet<PronunciationResult> PronunciationResults { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<ChapterModule> ChapterModules { get; set; }
        public DbSet<LearningPath> LearningPaths { get; set; }
        public DbSet<LearningPathTopic> LearningPathTopics { get; set; }
        public DbSet<LearningPathChapter> LearningPathChapters { get; set; }
        public DbSet<LearningPathModule> LearningPathModules { get; set; }
        public DbSet<ProgressAnalytics> ProgressAnalytics { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Period> Periods { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Models.Type> Types { get; set; }
        public DbSet<SystemPolicy> SystemPolicies { get; set; }
        public DbSet<ContentLibrary> ContentLibraries { get; set; }
        public DbSet<SendToLearner> SendToLearners { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<LearnerAchievement> LearnerAchievements { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<LearnerChallenge> LearnerChallenges { get; set; }
        public DbSet<GoalSetting> GoalSettings { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }
        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<ConversationTopicCategory> ConversationTopicCategories { get; set; }
        public DbSet<ConversationTopic> ConversationTopics { get; set; }
        public DbSet<GroupSession> GroupSessions { get; set; }
        public DbSet<GroupSessionMember> GroupSessionMembers { get; set; }
        public DbSet<GroupPracticeDetail> GroupPracticeDetails { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<SkillMentor> SkillMentors { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }



    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
            modelBuilder.Entity<User>()
       .HasMany(u => u.Notifications)
       .WithOne(n => n.User)
       .HasForeignKey(n => n.UserId)
       .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Feedbacks)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.SystemPolicies)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.Role)
                      .IsRequired()
                      .HasMaxLength(50);
            });


            modelBuilder.Entity<RefreshToken>()
                 .HasOne(r => r.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(r => r.UserId);
        }
    }
}
