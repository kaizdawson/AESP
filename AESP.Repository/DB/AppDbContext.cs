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
        public DbSet<ReviewerProfile> ReviewerProfiles { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<AssessmentDetail> AssessmentDetails { get; set; }
        public DbSet<QuestionAssessment> QuestionAssessments { get; set; }

        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionMedia> QuestionMedias { get; set; }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Exercise> Exercises { get; set; }

        public DbSet<LearnerCourse> LearnerCourses { get; set; }
        public DbSet<LearningPathCourse> LearningPathCourses { get; set; }
        public DbSet<LearningPathChapter> LearningPathChapters { get; set; }
        public DbSet<LearningPathExercise> LearningPathExercises { get; set; }

        public DbSet<LearnerAnswer> LearnerAnswers { get; set; }
        public DbSet<PhonemeTemplate> PhonemeTemplates { get; set; }
        public DbSet<PhonemeResult> PhonemeResults { get; set; }
        public DbSet<Stress> Stresses { get; set; }
        public DbSet<StressResult> StressResults { get; set; }

        public DbSet<PronunciationResult> PronunciationResults { get; set; }
        public DbSet<ProgressAnalytics> ProgressAnalytics { get; set; }

        public DbSet<Record> Records { get; set; }
        public DbSet<LearnerRecordCategory> LearnerRecordCategories { get; set; }

        public DbSet<Review> Reviews { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Certificate> Certificates { get; set; }

        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<SubServicePackage> SubServicePackages { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<ReviewFee> ReviewFees { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }




        // ==================== Model Config ==================== //
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ❌ Disable all cascade deletes to prevent accidental mass deletions
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // ✅ Enable cascade delete only for safe relations

            // Khi xóa User → xóa Feedback + Notification + RefreshToken + Profile
            modelBuilder.Entity<User>()
                .HasMany(u => u.Feedbacks)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Khi xóa LearnerProfile → xóa các liên quan
            modelBuilder.Entity<LearnerProfile>()
                .HasMany(l => l.Assessments)
                .WithOne(a => a.LearnerProfile)
                .HasForeignKey(a => a.LearnerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LearnerProfile>()
                .HasMany(l => l.LearnerAnswers)
                .WithOne(a => a.Learner)
                .HasForeignKey(a => a.LearnerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LearnerProfile>()
                .HasMany(l => l.ProgressAnalytics)
                .WithOne(p => p.Learner)
                .HasForeignKey(p => p.LearnerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Khi xóa Wallet → xóa Transaction
            modelBuilder.Entity<Wallet>()
                .HasMany(w => w.Transactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            // Khi xóa Course → xóa Chapter → xóa Exercise
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Chapters)
                .WithOne(ch => ch.Course)
                .HasForeignKey(ch => ch.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Chapter>()
                .HasMany(ch => ch.Exercises)
                .WithOne(e => e.Chapter)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Khi xóa ReviewerProfile → xóa Certificate
            modelBuilder.Entity<ReviewerProfile>()
                .HasMany(r => r.Certificates)
                .WithOne(c => c.ReviewerProfile)
                .HasForeignKey(c => c.ReviewerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Khi xóa Assessment → xóa PronunciationResult và AssessmentDetail
            modelBuilder.Entity<Assessment>()
                .HasMany(a => a.PronunciationResults)
                .WithOne(p => p.Assessment)
                .HasForeignKey(p => p.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Assessment>()
                .HasMany(a => a.AssessmentDetails)
                .WithOne(d => d.Assessment)
                .HasForeignKey(d => d.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);
            // ================== LearningPath Hierarchy ==================
            modelBuilder.Entity<LearningPathCourse>()
                .HasMany(lp => lp.LearningPathChapters)
                .WithOne(ch => ch.LearningPathCourse)
                .HasForeignKey(ch => ch.LearningPathCourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LearningPathChapter>()
                .HasMany(ch => ch.LearningPathExercises)
                .WithOne(ex => ex.LearningPathChapter)
                .HasForeignKey(ex => ex.LearningPathChapterId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
