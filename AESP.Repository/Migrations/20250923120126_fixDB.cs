using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class fixDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PointsReward = table.Column<int>(type: "int", nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementId);
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    ChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PointsReward = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.ChallengeId);
                });

            migrationBuilder.CreateTable(
                name: "ConversationTopicCategories",
                columns: table => new
                {
                    ConversationTopicCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationTopicCategories", x => x.ConversationTopicCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "LearnerProfiles",
                columns: table => new
                {
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Goal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Favorite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PronunciationScore = table.Column<double>(type: "float", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerProfiles", x => x.LearnerProfileId);
                    table.ForeignKey(
                        name: "FK_LearnerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MentorProfiles",
                columns: table => new
                {
                    MentorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Skills = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeachingMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VoiceStyle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    About = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorProfiles", x => x.MentorProfileId);
                    table.ForeignKey(
                        name: "FK_MentorProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Periods",
                columns: table => new
                {
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periods", x => x.PeriodId);
                });

            migrationBuilder.CreateTable(
                name: "ServicePackages",
                columns: table => new
                {
                    ServicePackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    WithMentor = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfMentorMeeting = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackages", x => x.ServicePackageId);
                });

            migrationBuilder.CreateTable(
                name: "SystemPolicies",
                columns: table => new
                {
                    SystemPolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemPolicies", x => x.SystemPolicyId);
                    table.ForeignKey(
                        name: "FK_SystemPolicies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfChapter = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.TopicId);
                });

            migrationBuilder.CreateTable(
                name: "Types",
                columns: table => new
                {
                    TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Types", x => x.TypeId);
                });

            migrationBuilder.CreateTable(
                name: "ConversationTopics",
                columns: table => new
                {
                    ConversationTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConversationTopicCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationTopics", x => x.ConversationTopicId);
                    table.ForeignKey(
                        name: "FK_ConversationTopics_ConversationTopicCategories_ConversationTopicCategoryId",
                        column: x => x.ConversationTopicCategoryId,
                        principalTable: "ConversationTopicCategories",
                        principalColumn: "ConversationTopicCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfQuestion = table.Column<double>(type: "float", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.AssessmentId);
                    table.ForeignKey(
                        name: "FK_Assessments_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoalSettings",
                columns: table => new
                {
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DailyMinutes = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalSettings", x => x.GoalId);
                    table.ForeignKey(
                        name: "FK_GoalSettings_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leaderboards",
                columns: table => new
                {
                    LeaderboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Streak = table.Column<int>(type: "int", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaderboards", x => x.LeaderboardId);
                    table.ForeignKey(
                        name: "FK_Leaderboards_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearnerAchievements",
                columns: table => new
                {
                    LearnerAchievementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EarnedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerAchievements", x => x.LearnerAchievementId);
                    table.ForeignKey(
                        name: "FK_LearnerAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "AchievementId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearnerAchievements_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearningPaths",
                columns: table => new
                {
                    PathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfTopic = table.Column<double>(type: "float", nullable: false),
                    Progress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPaths", x => x.PathId);
                    table.ForeignKey(
                        name: "FK_LearningPaths_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgressAnalytics",
                columns: table => new
                {
                    ProgressAnalyticsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateRecorded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SpeakingTime = table.Column<double>(type: "float", nullable: false),
                    SessionsCompleted = table.Column<int>(type: "int", nullable: false),
                    PronunciationScoreAvg = table.Column<double>(type: "float", nullable: false),
                    GrammarAccuracy = table.Column<double>(type: "float", nullable: false),
                    VocabularyUsage = table.Column<double>(type: "float", nullable: false),
                    ConfidenceLevel = table.Column<double>(type: "float", nullable: false),
                    StreakDays = table.Column<int>(type: "int", nullable: false),
                    HeatmapData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressAnalytics", x => x.ProgressAnalyticsId);
                    table.ForeignKey(
                        name: "FK_ProgressAnalytics_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    PurchaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.PurchaseId);
                    table.ForeignKey(
                        name: "FK_Purchases_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContentLibraries",
                columns: table => new
                {
                    ContentLibraryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MentorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentLibraries", x => x.ContentLibraryId);
                    table.ForeignKey(
                        name: "FK_ContentLibraries_MentorProfiles_MentorProfileId",
                        column: x => x.MentorProfileId,
                        principalTable: "MentorProfiles",
                        principalColumn: "MentorProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MentorSchedules",
                columns: table => new
                {
                    MentorScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MentorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorSchedules", x => x.MentorScheduleId);
                    table.ForeignKey(
                        name: "FK_MentorSchedules_MentorProfiles_MentorProfileId",
                        column: x => x.MentorProfileId,
                        principalTable: "MentorProfiles",
                        principalColumn: "MentorProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeachingCertificate",
                columns: table => new
                {
                    TeachingCertificateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MentorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingCertificate", x => x.TeachingCertificateId);
                    table.ForeignKey(
                        name: "FK_TeachingCertificate_MentorProfiles_MentorProfileId",
                        column: x => x.MentorProfileId,
                        principalTable: "MentorProfiles",
                        principalColumn: "MentorProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceSummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Periods",
                        principalColumn: "PeriodId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfModule = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.ChapterId);
                    table.ForeignKey(
                        name: "FK_Chapters_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Types_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Types",
                        principalColumn: "TypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupSessions",
                columns: table => new
                {
                    GroupSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConversationTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupSessions", x => x.GroupSessionId);
                    table.ForeignKey(
                        name: "FK_GroupSessions_ConversationTopics_ConversationTopicId",
                        column: x => x.ConversationTopicId,
                        principalTable: "ConversationTopics",
                        principalColumn: "ConversationTopicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PracticeSessions",
                columns: table => new
                {
                    PracticeSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConversationTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeSessions", x => x.PracticeSessionId);
                    table.ForeignKey(
                        name: "FK_PracticeSessions_ConversationTopics_ConversationTopicId",
                        column: x => x.ConversationTopicId,
                        principalTable: "ConversationTopics",
                        principalColumn: "ConversationTopicId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PracticeSessions_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PronunciationResults",
                columns: table => new
                {
                    PronunciationResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordOrPhoneme = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedSound = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearnerSound = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccuracyScore = table.Column<double>(type: "float", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PronunciationResults", x => x.PronunciationResultId);
                    table.ForeignKey(
                        name: "FK_PronunciationResults_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearnerChallenges",
                columns: table => new
                {
                    LearnerChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletionStatus = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerChallenges", x => x.LearnerChallengeId);
                    table.ForeignKey(
                        name: "FK_LearnerChallenges_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "ChallengeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearnerChallenges_GoalSettings_GoalId",
                        column: x => x.GoalId,
                        principalTable: "GoalSettings",
                        principalColumn: "GoalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearnerChallenges_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearningPathTopics",
                columns: table => new
                {
                    LearningPathTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Progress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfChapter = table.Column<int>(type: "int", nullable: false),
                    PathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPathTopics", x => x.LearningPathTopicId);
                    table.ForeignKey(
                        name: "FK_LearningPathTopics_LearningPaths_PathId",
                        column: x => x.PathId,
                        principalTable: "LearningPaths",
                        principalColumn: "PathId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningPathTopics_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicePackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "PurchaseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "ServicePackageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SendToLearners",
                columns: table => new
                {
                    SendToLearnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentLibraryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendToLearners", x => x.SendToLearnerId);
                    table.ForeignKey(
                        name: "FK_SendToLearners_ContentLibraries_ContentLibraryId",
                        column: x => x.ContentLibraryId,
                        principalTable: "ContentLibraries",
                        principalColumn: "ContentLibraryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SendToLearners_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChapterModules",
                columns: table => new
                {
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterModules", x => x.ModuleId);
                    table.ForeignKey(
                        name: "FK_ChapterModules_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupPracticeDetails",
                columns: table => new
                {
                    GroupPracticeDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PronunciationScore = table.Column<double>(type: "float", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrammarCorrection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VocabularySuggest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPracticeDetails", x => x.GroupPracticeDetailId);
                    table.ForeignKey(
                        name: "FK_GroupPracticeDetails_GroupSessions_GroupSessionId",
                        column: x => x.GroupSessionId,
                        principalTable: "GroupSessions",
                        principalColumn: "GroupSessionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupSessionMembers",
                columns: table => new
                {
                    GroupSessionMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RoleInSession = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeaveTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GroupSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupSessionMembers", x => x.GroupSessionMemberId);
                    table.ForeignKey(
                        name: "FK_GroupSessionMembers_GroupSessions_GroupSessionId",
                        column: x => x.GroupSessionId,
                        principalTable: "GroupSessions",
                        principalColumn: "GroupSessionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupSessionMembers_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PracticeSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_MentorSchedules_MentorScheduleId",
                        column: x => x.MentorScheduleId,
                        principalTable: "MentorSchedules",
                        principalColumn: "MentorScheduleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_PracticeSessions_PracticeSessionId",
                        column: x => x.PracticeSessionId,
                        principalTable: "PracticeSessions",
                        principalColumn: "PracticeSessionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PracticeDetails",
                columns: table => new
                {
                    PracticeDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PracticeSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sentence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Transcript = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PronunciationScore = table.Column<double>(type: "float", nullable: false),
                    GrammarCorrection = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeDetails", x => x.PracticeDetailId);
                    table.ForeignKey(
                        name: "FK_PracticeDetails_PracticeSessions_PracticeSessionId",
                        column: x => x.PracticeSessionId,
                        principalTable: "PracticeSessions",
                        principalColumn: "PracticeSessionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearningPathChapters",
                columns: table => new
                {
                    LearningPathChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Progress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfModule = table.Column<int>(type: "int", nullable: false),
                    PathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningPathTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPathChapters", x => x.LearningPathChapterId);
                    table.ForeignKey(
                        name: "FK_LearningPathChapters_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningPathChapters_LearningPathTopics_LearningPathTopicId",
                        column: x => x.LearningPathTopicId,
                        principalTable: "LearningPathTopics",
                        principalColumn: "LearningPathTopicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AudioUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_Questions_ChapterModules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "ChapterModules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Questions_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearningPathModules",
                columns: table => new
                {
                    LearningPathModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    ScoreAchieved = table.Column<double>(type: "float", nullable: false),
                    LearningPathChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPathModules", x => x.LearningPathModuleId);
                    table.ForeignKey(
                        name: "FK_LearningPathModules_ChapterModules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "ChapterModules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningPathModules_LearningPathChapters_LearningPathChapterId",
                        column: x => x.LearningPathChapterId,
                        principalTable: "LearningPathChapters",
                        principalColumn: "LearningPathChapterId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnswerOptions",
                columns: table => new
                {
                    AnswerOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerOptions", x => x.AnswerOptionId);
                    table.ForeignKey(
                        name: "FK_AnswerOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentDetails",
                columns: table => new
                {
                    AssessmentDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Skill = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    AI_Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MentorFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentDetails", x => x.AssessmentDetailId);
                    table.ForeignKey(
                        name: "FK_AssessmentDetails_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssessmentDetails_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChoiceOptions",
                columns: table => new
                {
                    ChoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    Explain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoiceOptions", x => x.ChoiceId);
                    table.ForeignKey(
                        name: "FK_ChoiceOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImageOptions",
                columns: table => new
                {
                    ImageOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageOptions", x => x.ImageOptionId);
                    table.ForeignKey(
                        name: "FK_ImageOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CorrectMatches",
                columns: table => new
                {
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Explain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectMatches", x => x.MatchId);
                    table.ForeignKey(
                        name: "FK_CorrectMatches_AnswerOptions_AnswerOptionId",
                        column: x => x.AnswerOptionId,
                        principalTable: "AnswerOptions",
                        principalColumn: "AnswerOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrectMatches_ImageOptions_ImageOptionId",
                        column: x => x.ImageOptionId,
                        principalTable: "ImageOptions",
                        principalColumn: "ImageOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrectMatches_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentAnswers",
                columns: table => new
                {
                    StudentAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AudioRecordingUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TranscriptText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScoreForVoice = table.Column<double>(type: "float", nullable: false),
                    ExplainTheWrongForVoice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImageOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PathModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAnswers", x => x.StudentAnswerId);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_AnswerOptions_AnswerOptionId",
                        column: x => x.AnswerOptionId,
                        principalTable: "AnswerOptions",
                        principalColumn: "AnswerOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_ChoiceOptions_ChoiceId",
                        column: x => x.ChoiceId,
                        principalTable: "ChoiceOptions",
                        principalColumn: "ChoiceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_ImageOptions_ImageOptionId",
                        column: x => x.ImageOptionId,
                        principalTable: "ImageOptions",
                        principalColumn: "ImageOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_LearningPathModules_PathModuleId",
                        column: x => x.PathModuleId,
                        principalTable: "LearningPathModules",
                        principalColumn: "LearningPathModuleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChoiceStudent",
                columns: table => new
                {
                    ChoiceStudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsChosen = table.Column<bool>(type: "bit", nullable: false),
                    Explain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoiceStudent", x => x.ChoiceStudentId);
                    table.ForeignKey(
                        name: "FK_ChoiceStudent_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChoiceStudent_StudentAnswers_StudentAnswerId",
                        column: x => x.StudentAnswerId,
                        principalTable: "StudentAnswers",
                        principalColumn: "StudentAnswerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationDetails",
                columns: table => new
                {
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    Pronunciation = table.Column<double>(type: "float", nullable: false),
                    StudentAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationDetails", x => x.EvaluationId);
                    table.ForeignKey(
                        name: "FK_EvaluationDetails_StudentAnswers_StudentAnswerId",
                        column: x => x.StudentAnswerId,
                        principalTable: "StudentAnswers",
                        principalColumn: "StudentAnswerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionId",
                table: "AnswerOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentDetails_AssessmentId",
                table: "AssessmentDetails",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentDetails_QuestionId",
                table: "AssessmentDetails",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_LearnerProfileId",
                table: "Assessments",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_LearnerProfileId",
                table: "Bookings",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_MentorScheduleId",
                table: "Bookings",
                column: "MentorScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PracticeSessionId",
                table: "Bookings",
                column: "PracticeSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterModules_ChapterId",
                table: "ChapterModules",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_TopicId",
                table: "Chapters",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceOptions_QuestionId",
                table: "ChoiceOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceStudent_QuestionId",
                table: "ChoiceStudent",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceStudent_StudentAnswerId",
                table: "ChoiceStudent",
                column: "StudentAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentLibraries_MentorProfileId",
                table: "ContentLibraries",
                column: "MentorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationTopics_ConversationTopicCategoryId",
                table: "ConversationTopics",
                column: "ConversationTopicCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectMatches_AnswerOptionId",
                table: "CorrectMatches",
                column: "AnswerOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectMatches_ImageOptionId",
                table: "CorrectMatches",
                column: "ImageOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectMatches_QuestionId",
                table: "CorrectMatches",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationDetails_StudentAnswerId",
                table: "EvaluationDetails",
                column: "StudentAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_TypeId",
                table: "Feedbacks",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserId",
                table: "Feedbacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalSettings_LearnerProfileId",
                table: "GoalSettings",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPracticeDetails_GroupSessionId",
                table: "GroupPracticeDetails",
                column: "GroupSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSessionMembers_GroupSessionId",
                table: "GroupSessionMembers",
                column: "GroupSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSessionMembers_LearnerProfileId",
                table: "GroupSessionMembers",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSessions_ConversationTopicId",
                table: "GroupSessions",
                column: "ConversationTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageOptions_QuestionId",
                table: "ImageOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_LearnerProfileId",
                table: "Leaderboards",
                column: "LearnerProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAchievements_AchievementId",
                table: "LearnerAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAchievements_LearnerProfileId",
                table: "LearnerAchievements",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerChallenges_ChallengeId",
                table: "LearnerChallenges",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerChallenges_GoalId",
                table: "LearnerChallenges",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerChallenges_LearnerProfileId",
                table: "LearnerChallenges",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerProfiles_UserId",
                table: "LearnerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathChapters_ChapterId",
                table: "LearningPathChapters",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathChapters_LearningPathTopicId",
                table: "LearningPathChapters",
                column: "LearningPathTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathModules_LearningPathChapterId",
                table: "LearningPathModules",
                column: "LearningPathChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathModules_ModuleId",
                table: "LearningPathModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_LearnerProfileId",
                table: "LearningPaths",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathTopics_PathId",
                table: "LearningPathTopics",
                column: "PathId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathTopics_TopicId",
                table: "LearningPathTopics",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorProfiles_UserId",
                table: "MentorProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MentorSchedules_MentorProfileId",
                table: "MentorSchedules",
                column: "MentorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeDetails_PracticeSessionId",
                table: "PracticeDetails",
                column: "PracticeSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessions_ConversationTopicId",
                table: "PracticeSessions",
                column: "ConversationTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessions_LearnerProfileId",
                table: "PracticeSessions",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressAnalytics_LearnerProfileId",
                table: "ProgressAnalytics",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationResults_AssessmentId",
                table: "PronunciationResults",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_LearnerProfileId",
                table: "Purchases",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ChapterId",
                table: "Questions",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ModuleId",
                table: "Questions",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_LearnerProfileId",
                table: "Reports",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_PeriodId",
                table: "Reports",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_SendToLearners_ContentLibraryId",
                table: "SendToLearners",
                column: "ContentLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_SendToLearners_LearnerProfileId",
                table: "SendToLearners",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_AnswerOptionId",
                table: "StudentAnswers",
                column: "AnswerOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_ChoiceId",
                table: "StudentAnswers",
                column: "ChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_ImageOptionId",
                table: "StudentAnswers",
                column: "ImageOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_LearnerProfileId",
                table: "StudentAnswers",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_PathModuleId",
                table: "StudentAnswers",
                column: "PathModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_QuestionId",
                table: "StudentAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_LearnerProfileId",
                table: "Subscriptions",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PurchaseId",
                table: "Subscriptions",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ServicePackageId",
                table: "Subscriptions",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemPolicies_UserId",
                table: "SystemPolicies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingCertificate_MentorProfileId",
                table: "TeachingCertificate",
                column: "MentorProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "AssessmentDetails");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "ChoiceStudent");

            migrationBuilder.DropTable(
                name: "CorrectMatches");

            migrationBuilder.DropTable(
                name: "EvaluationDetails");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "GroupPracticeDetails");

            migrationBuilder.DropTable(
                name: "GroupSessionMembers");

            migrationBuilder.DropTable(
                name: "Leaderboards");

            migrationBuilder.DropTable(
                name: "LearnerAchievements");

            migrationBuilder.DropTable(
                name: "LearnerChallenges");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PracticeDetails");

            migrationBuilder.DropTable(
                name: "ProgressAnalytics");

            migrationBuilder.DropTable(
                name: "PronunciationResults");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "SendToLearners");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "SystemPolicies");

            migrationBuilder.DropTable(
                name: "TeachingCertificate");

            migrationBuilder.DropTable(
                name: "MentorSchedules");

            migrationBuilder.DropTable(
                name: "StudentAnswers");

            migrationBuilder.DropTable(
                name: "Types");

            migrationBuilder.DropTable(
                name: "GroupSessions");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "Challenges");

            migrationBuilder.DropTable(
                name: "GoalSettings");

            migrationBuilder.DropTable(
                name: "PracticeSessions");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "Periods");

            migrationBuilder.DropTable(
                name: "ContentLibraries");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "ServicePackages");

            migrationBuilder.DropTable(
                name: "AnswerOptions");

            migrationBuilder.DropTable(
                name: "ChoiceOptions");

            migrationBuilder.DropTable(
                name: "ImageOptions");

            migrationBuilder.DropTable(
                name: "LearningPathModules");

            migrationBuilder.DropTable(
                name: "ConversationTopics");

            migrationBuilder.DropTable(
                name: "MentorProfiles");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "LearningPathChapters");

            migrationBuilder.DropTable(
                name: "ConversationTopicCategories");

            migrationBuilder.DropTable(
                name: "ChapterModules");

            migrationBuilder.DropTable(
                name: "LearningPathTopics");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "LearningPaths");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "LearnerProfiles");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
