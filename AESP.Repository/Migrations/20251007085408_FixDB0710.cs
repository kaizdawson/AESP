using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixDB0710 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentDetails_Assessments_AssessmentId",
                table: "AssessmentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_LearnerProfiles_LearnerProfileId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Topics_TopicId",
                table: "Chapters");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Types_TypeId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningPathChapters_LearningPathTopics_LearningPathTopicId",
                table: "LearningPathChapters");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressAnalytics_LearnerProfiles_LearnerProfileId",
                table: "ProgressAnalytics");

            migrationBuilder.DropForeignKey(
                name: "FK_PronunciationResults_Assessments_AssessmentId",
                table: "PronunciationResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_ChapterModules_ModuleId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Chapters_ChapterId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Purchases_PurchaseId",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "ChoiceStudent");

            migrationBuilder.DropTable(
                name: "CorrectMatches");

            migrationBuilder.DropTable(
                name: "EvaluationDetails");

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
                name: "LearningPathTopics");

            migrationBuilder.DropTable(
                name: "PracticeDetails");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "SendToLearners");

            migrationBuilder.DropTable(
                name: "SkillMentors");

            migrationBuilder.DropTable(
                name: "SystemPolicies");

            migrationBuilder.DropTable(
                name: "TeachingCertificate");

            migrationBuilder.DropTable(
                name: "Types");

            migrationBuilder.DropTable(
                name: "MentorSchedules");

            migrationBuilder.DropTable(
                name: "StudentAnswers");

            migrationBuilder.DropTable(
                name: "GroupSessions");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "Challenges");

            migrationBuilder.DropTable(
                name: "GoalSettings");

            migrationBuilder.DropTable(
                name: "LearningPaths");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "PracticeSessions");

            migrationBuilder.DropTable(
                name: "Periods");

            migrationBuilder.DropTable(
                name: "ContentLibraries");

            migrationBuilder.DropTable(
                name: "Skills");

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
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "MentorProfiles");

            migrationBuilder.DropTable(
                name: "ChapterModules");

            migrationBuilder.DropTable(
                name: "ConversationTopicCategories");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PurchaseId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ChapterId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_LearningPathChapters_LearningPathTopicId",
                table: "LearningPathChapters");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_TypeId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "PurchaseId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "WithMentor",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "ChapterId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "GrammarAccuracy",
                table: "ProgressAnalytics");

            migrationBuilder.DropColumn(
                name: "VocabularyUsage",
                table: "ProgressAnalytics");

            migrationBuilder.DropColumn(
                name: "LearningPathTopicId",
                table: "LearningPathChapters");

            migrationBuilder.DropColumn(
                name: "Favorite",
                table: "LearnerProfiles");

            migrationBuilder.DropColumn(
                name: "Goal",
                table: "LearnerProfiles");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "Answer",
                table: "AssessmentDetails");

            migrationBuilder.RenameColumn(
                name: "NumberOfMentorMeeting",
                table: "ServicePackages",
                newName: "NumberOfReview");

            migrationBuilder.RenameColumn(
                name: "ModuleId",
                table: "Questions",
                newName: "ExerciseId");

            migrationBuilder.RenameColumn(
                name: "AudioUrl",
                table: "Questions",
                newName: "PhonemeJson");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_ModuleId",
                table: "Questions",
                newName: "IX_Questions_ExerciseId");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Purchases",
                newName: "PriceServicePackage");

            migrationBuilder.RenameColumn(
                name: "PathId",
                table: "LearningPathChapters",
                newName: "LearningPathCourseId");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "Feedbacks",
                newName: "TargetId");

            migrationBuilder.RenameColumn(
                name: "TopicId",
                table: "Chapters",
                newName: "CourseId");

            migrationBuilder.RenameColumn(
                name: "NumberOfModule",
                table: "Chapters",
                newName: "NumberOfExercise");

            migrationBuilder.RenameIndex(
                name: "IX_Chapters_TopicId",
                table: "Chapters",
                newName: "IX_Chapters_CourseId");

            migrationBuilder.RenameColumn(
                name: "Skill",
                table: "AssessmentDetails",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "MentorFeedback",
                table: "AssessmentDetails",
                newName: "AnswerAudio");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "ServicePackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ServicePackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IPA",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "PriceReviewFee",
                table: "Purchases",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewFeeId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<double>(
                name: "Progress",
                table: "LearningPathChapters",
                type: "float",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "DailyMinutes",
                table: "LearnerProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewId",
                table: "Feedbacks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionId",
                table: "AssessmentDetails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionAssessmentId",
                table: "AssessmentDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfChapter = table.Column<int>(type: "int", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseId);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    NumberOfQuestion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.ExerciseId);
                    table.ForeignKey(
                        name: "FK_Exercises_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearnerCourses",
                columns: table => new
                {
                    LearnerCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfCourse = table.Column<int>(type: "int", nullable: false),
                    Progress = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerCourses", x => x.LearnerCourseId);
                    table.ForeignKey(
                        name: "FK_LearnerCourses_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearnerRecordCategories",
                columns: table => new
                {
                    LearnerRecordCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerRecordCategories", x => x.LearnerRecordCategoryId);
                    table.ForeignKey(
                        name: "FK_LearnerRecordCategories_LearnerProfiles_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhonemeTemplates",
                columns: table => new
                {
                    PhonemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhonemeTemplates", x => x.PhonemeId);
                    table.ForeignKey(
                        name: "FK_PhonemeTemplates_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionAssessments",
                columns: table => new
                {
                    QuestionAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAssessments", x => x.QuestionAssessmentId);
                });

            migrationBuilder.CreateTable(
                name: "QuestionMedias",
                columns: table => new
                {
                    QuestionMediaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Accent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AudioUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionMedias", x => x.QuestionMediaId);
                    table.ForeignKey(
                        name: "FK_QuestionMedias_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewFees",
                columns: table => new
                {
                    ReviewFeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewFees", x => x.ReviewFeeId);
                });

            migrationBuilder.CreateTable(
                name: "SubServicePackages",
                columns: table => new
                {
                    SubServicePackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicePackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubServicePackages", x => x.SubServicePackageId);
                    table.ForeignKey(
                        name: "FK_SubServicePackages_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "ServicePackageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.WalletId);
                });

            migrationBuilder.CreateTable(
                name: "LearningPathExercises",
                columns: table => new
                {
                    LearningPathExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningPathChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScoreAchieved = table.Column<double>(type: "float", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfQuestion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPathExercises", x => x.LearningPathExerciseId);
                    table.ForeignKey(
                        name: "FK_LearningPathExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "ExerciseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningPathExercises_LearningPathChapters_LearningPathChapterId",
                        column: x => x.LearningPathChapterId,
                        principalTable: "LearningPathChapters",
                        principalColumn: "LearningPathChapterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningPathCourses",
                columns: table => new
                {
                    LearningPathCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Progress = table.Column<double>(type: "float", nullable: false),
                    NumberOfChapter = table.Column<int>(type: "int", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPathCourses", x => x.LearningPathCourseId);
                    table.ForeignKey(
                        name: "FK_LearningPathCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningPathCourses_LearnerCourses_LearnerCourseId",
                        column: x => x.LearnerCourseId,
                        principalTable: "LearnerCourses",
                        principalColumn: "LearnerCourseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Records",
                columns: table => new
                {
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerRecordCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AudioRecordingURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AIFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Records", x => x.RecordId);
                    table.ForeignKey(
                        name: "FK_Records_LearnerRecordCategories_LearnerRecordCategoryId",
                        column: x => x.LearnerRecordCategoryId,
                        principalTable: "LearnerRecordCategories",
                        principalColumn: "LearnerRecordCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stresses",
                columns: table => new
                {
                    StressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhonemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StressType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SyllableIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stresses", x => x.StressId);
                    table.ForeignKey(
                        name: "FK_Stresses_PhonemeTemplates_PhonemeId",
                        column: x => x.PhonemeId,
                        principalTable: "PhonemeTemplates",
                        principalColumn: "PhonemeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewerProfiles",
                columns: table => new
                {
                    ReviewerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Experience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Levels = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewerProfiles", x => x.ReviewerProfileId);
                    table.ForeignKey(
                        name: "FK_ReviewerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewerProfiles_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    CreatedTransaction = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasonWithdrawReject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionEnum = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearnerAnswers",
                columns: table => new
                {
                    LearnerAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AudioRecordingUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TranscribedText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScoreForVoice = table.Column<int>(type: "int", nullable: false),
                    ExplainTheWrongForVoiceAI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsNeededReviewed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberofReview = table.Column<int>(type: "int", nullable: false),
                    LearningPathExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerAnswers", x => x.LearnerAnswerId);
                    table.ForeignKey(
                        name: "FK_LearnerAnswers_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearnerAnswers_LearningPathExercises_LearningPathExerciseId",
                        column: x => x.LearningPathExerciseId,
                        principalTable: "LearningPathExercises",
                        principalColumn: "LearningPathExerciseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearnerAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    CertificateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.CertificateId);
                    table.ForeignKey(
                        name: "FK_Certificates_ReviewerProfiles_ReviewerProfileId",
                        column: x => x.ReviewerProfileId,
                        principalTable: "ReviewerProfiles",
                        principalColumn: "ReviewerProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhonemeResults",
                columns: table => new
                {
                    PhonemeResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhonemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedSymbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhonemeResults", x => x.PhonemeResultId);
                    table.ForeignKey(
                        name: "FK_PhonemeResults_LearnerAnswers_LearnerAnswerId",
                        column: x => x.LearnerAnswerId,
                        principalTable: "LearnerAnswers",
                        principalColumn: "LearnerAnswerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhonemeResults_PhonemeTemplates_PhonemeId",
                        column: x => x.PhonemeId,
                        principalTable: "PhonemeTemplates",
                        principalColumn: "PhonemeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhonemeResults_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    ReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_Reviews_LearnerAnswers_LearnerAnswerId",
                        column: x => x.LearnerAnswerId,
                        principalTable: "LearnerAnswers",
                        principalColumn: "LearnerAnswerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Records_RecordId",
                        column: x => x.RecordId,
                        principalTable: "Records",
                        principalColumn: "RecordId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_ReviewerProfiles_ReviewerProfileId",
                        column: x => x.ReviewerProfileId,
                        principalTable: "ReviewerProfiles",
                        principalColumn: "ReviewerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StressResults",
                columns: table => new
                {
                    StressResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhonemeResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpectedType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActualType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StressResults", x => x.StressResultId);
                    table.ForeignKey(
                        name: "FK_StressResults_PhonemeResults_PhonemeResultId",
                        column: x => x.PhonemeResultId,
                        principalTable: "PhonemeResults",
                        principalColumn: "PhonemeResultId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ReviewFeeId",
                table: "Purchases",
                column: "ReviewFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_SubscriptionId",
                table: "Purchases",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathChapters_LearningPathCourseId",
                table: "LearningPathChapters",
                column: "LearningPathCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ReviewId",
                table: "Feedbacks",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentDetails_QuestionAssessmentId",
                table: "AssessmentDetails",
                column: "QuestionAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_ReviewerProfileId",
                table: "Certificates",
                column: "ReviewerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_ChapterId",
                table: "Exercises",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAnswers_LearnerProfileId",
                table: "LearnerAnswers",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAnswers_LearningPathExerciseId",
                table: "LearnerAnswers",
                column: "LearningPathExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAnswers_QuestionId",
                table: "LearnerAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerCourses_LearnerProfileId",
                table: "LearnerCourses",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerRecordCategories_LearnerId",
                table: "LearnerRecordCategories",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathCourses_CourseId",
                table: "LearningPathCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathCourses_LearnerCourseId",
                table: "LearningPathCourses",
                column: "LearnerCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathExercises_ExerciseId",
                table: "LearningPathExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathExercises_LearningPathChapterId",
                table: "LearningPathExercises",
                column: "LearningPathChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_PhonemeResults_LearnerAnswerId",
                table: "PhonemeResults",
                column: "LearnerAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_PhonemeResults_PhonemeId",
                table: "PhonemeResults",
                column: "PhonemeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhonemeResults_QuestionId",
                table: "PhonemeResults",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PhonemeTemplates_QuestionId",
                table: "PhonemeTemplates",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionMedias_QuestionId",
                table: "QuestionMedias",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Records_LearnerRecordCategoryId",
                table: "Records",
                column: "LearnerRecordCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerProfiles_UserId",
                table: "ReviewerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerProfiles_WalletId",
                table: "ReviewerProfiles",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_LearnerAnswerId",
                table: "Reviews",
                column: "LearnerAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RecordId",
                table: "Reviews",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerProfileId",
                table: "Reviews",
                column: "ReviewerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Stresses_PhonemeId",
                table: "Stresses",
                column: "PhonemeId");

            migrationBuilder.CreateIndex(
                name: "IX_StressResults_PhonemeResultId",
                table: "StressResults",
                column: "PhonemeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_SubServicePackages_ServicePackageId",
                table: "SubServicePackages",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletId",
                table: "Transactions",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentDetails_Assessments_AssessmentId",
                table: "AssessmentDetails",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentDetails_QuestionAssessments_QuestionAssessmentId",
                table: "AssessmentDetails",
                column: "QuestionAssessmentId",
                principalTable: "QuestionAssessments",
                principalColumn: "QuestionAssessmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_LearnerProfiles_LearnerProfileId",
                table: "Assessments",
                column: "LearnerProfileId",
                principalTable: "LearnerProfiles",
                principalColumn: "LearnerProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Courses_CourseId",
                table: "Chapters",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Reviews_ReviewId",
                table: "Feedbacks",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "ReviewId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPathChapters_LearningPathCourses_LearningPathCourseId",
                table: "LearningPathChapters",
                column: "LearningPathCourseId",
                principalTable: "LearningPathCourses",
                principalColumn: "LearningPathCourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressAnalytics_LearnerProfiles_LearnerProfileId",
                table: "ProgressAnalytics",
                column: "LearnerProfileId",
                principalTable: "LearnerProfiles",
                principalColumn: "LearnerProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PronunciationResults_Assessments_AssessmentId",
                table: "PronunciationResults",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_ReviewFees_ReviewFeeId",
                table: "Purchases",
                column: "ReviewFeeId",
                principalTable: "ReviewFees",
                principalColumn: "ReviewFeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Subscriptions_SubscriptionId",
                table: "Purchases",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Exercises_ExerciseId",
                table: "Questions",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "ExerciseId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentDetails_Assessments_AssessmentId",
                table: "AssessmentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentDetails_QuestionAssessments_QuestionAssessmentId",
                table: "AssessmentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_LearnerProfiles_LearnerProfileId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Courses_CourseId",
                table: "Chapters");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Reviews_ReviewId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningPathChapters_LearningPathCourses_LearningPathCourseId",
                table: "LearningPathChapters");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressAnalytics_LearnerProfiles_LearnerProfileId",
                table: "ProgressAnalytics");

            migrationBuilder.DropForeignKey(
                name: "FK_PronunciationResults_Assessments_AssessmentId",
                table: "PronunciationResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_ReviewFees_ReviewFeeId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Subscriptions_SubscriptionId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Exercises_ExerciseId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "LearningPathCourses");

            migrationBuilder.DropTable(
                name: "QuestionAssessments");

            migrationBuilder.DropTable(
                name: "QuestionMedias");

            migrationBuilder.DropTable(
                name: "ReviewFees");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Stresses");

            migrationBuilder.DropTable(
                name: "StressResults");

            migrationBuilder.DropTable(
                name: "SubServicePackages");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "LearnerCourses");

            migrationBuilder.DropTable(
                name: "Records");

            migrationBuilder.DropTable(
                name: "ReviewerProfiles");

            migrationBuilder.DropTable(
                name: "PhonemeResults");

            migrationBuilder.DropTable(
                name: "LearnerRecordCategories");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "LearnerAnswers");

            migrationBuilder.DropTable(
                name: "PhonemeTemplates");

            migrationBuilder.DropTable(
                name: "LearningPathExercises");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_ReviewFeeId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_SubscriptionId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_LearningPathChapters_LearningPathCourseId",
                table: "LearningPathChapters");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_ReviewId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentDetails_QuestionAssessmentId",
                table: "AssessmentDetails");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "IPA",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "PriceReviewFee",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "ReviewFeeId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "DailyMinutes",
                table: "LearnerProfiles");

            migrationBuilder.DropColumn(
                name: "ReviewId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "QuestionAssessmentId",
                table: "AssessmentDetails");

            migrationBuilder.RenameColumn(
                name: "NumberOfReview",
                table: "ServicePackages",
                newName: "NumberOfMentorMeeting");

            migrationBuilder.RenameColumn(
                name: "PhonemeJson",
                table: "Questions",
                newName: "AudioUrl");

            migrationBuilder.RenameColumn(
                name: "ExerciseId",
                table: "Questions",
                newName: "ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_ExerciseId",
                table: "Questions",
                newName: "IX_Questions_ModuleId");

            migrationBuilder.RenameColumn(
                name: "PriceServicePackage",
                table: "Purchases",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "LearningPathCourseId",
                table: "LearningPathChapters",
                newName: "PathId");

            migrationBuilder.RenameColumn(
                name: "TargetId",
                table: "Feedbacks",
                newName: "TypeId");

            migrationBuilder.RenameColumn(
                name: "NumberOfExercise",
                table: "Chapters",
                newName: "NumberOfModule");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Chapters",
                newName: "TopicId");

            migrationBuilder.RenameIndex(
                name: "IX_Chapters_CourseId",
                table: "Chapters",
                newName: "IX_Chapters_TopicId");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "AssessmentDetails",
                newName: "Skill");

            migrationBuilder.RenameColumn(
                name: "AnswerAudio",
                table: "AssessmentDetails",
                newName: "MentorFeedback");

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseId",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WithMentor",
                table: "ServicePackages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterId",
                table: "Questions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "Purchases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "GrammarAccuracy",
                table: "ProgressAnalytics",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VocabularyUsage",
                table: "ProgressAnalytics",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "Progress",
                table: "LearningPathChapters",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<Guid>(
                name: "LearningPathTopicId",
                table: "LearningPathChapters",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Favorite",
                table: "LearnerProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Goal",
                table: "LearnerProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Chapters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionId",
                table: "AssessmentDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "AssessmentDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PointsReward = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementId);
                });

            migrationBuilder.CreateTable(
                name: "AnswerOptions",
                columns: table => new
                {
                    AnswerOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "Challenges",
                columns: table => new
                {
                    ChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PointsReward = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.ChallengeId);
                });

            migrationBuilder.CreateTable(
                name: "ChapterModules",
                columns: table => new
                {
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "ChoiceOptions",
                columns: table => new
                {
                    ChoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Explain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "GoalSettings",
                columns: table => new
                {
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyMinutes = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetLevel = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "ImageOptions",
                columns: table => new
                {
                    ImageOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "Leaderboards",
                columns: table => new
                {
                    LeaderboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Streak = table.Column<int>(type: "int", nullable: false)
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
                name: "LearningPaths",
                columns: table => new
                {
                    PathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfTopic = table.Column<double>(type: "float", nullable: false),
                    Progress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetLevel = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "MentorProfiles",
                columns: table => new
                {
                    MentorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    About = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    Skills = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeachingMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VoiceStyle = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.SkillId);
                });

            migrationBuilder.CreateTable(
                name: "SystemPolicies",
                columns: table => new
                {
                    SystemPolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    NumberOfChapter = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "LearnerAchievements",
                columns: table => new
                {
                    LearnerAchievementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EarnedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "LearningPathModules",
                columns: table => new
                {
                    LearningPathModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningPathChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    ScoreAchieved = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "ConversationTopics",
                columns: table => new
                {
                    ConversationTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationTopicCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "LearnerChallenges",
                columns: table => new
                {
                    LearnerChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                name: "CorrectMatches",
                columns: table => new
                {
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Explain = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "ContentLibraries",
                columns: table => new
                {
                    ContentLibraryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    MentorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    MentorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceSummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "SkillMentors",
                columns: table => new
                {
                    SkillMentorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsExpertised = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillMentors", x => x.SkillMentorId);
                    table.ForeignKey(
                        name: "FK_SkillMentors_MentorProfiles_MentorProfileId",
                        column: x => x.MentorProfileId,
                        principalTable: "MentorProfiles",
                        principalColumn: "MentorProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillMentors_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearningPathTopics",
                columns: table => new
                {
                    LearningPathTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberOfChapter = table.Column<int>(type: "int", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Progress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "StudentAnswers",
                columns: table => new
                {
                    StudentAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImageOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PathModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AudioRecordingUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExplainTheWrongForVoice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScoreForVoice = table.Column<double>(type: "float", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TranscriptText = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "GroupSessions",
                columns: table => new
                {
                    GroupSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    table.ForeignKey(
                        name: "FK_GroupSessions_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PracticeSessions",
                columns: table => new
                {
                    PracticeSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    table.ForeignKey(
                        name: "FK_PracticeSessions_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
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
                name: "ChoiceStudent",
                columns: table => new
                {
                    ChoiceStudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Explain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsChosen = table.Column<bool>(type: "bit", nullable: false)
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
                    StudentAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    Pronunciation = table.Column<double>(type: "float", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "GroupPracticeDetails",
                columns: table => new
                {
                    GroupPracticeDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrammarCorrection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PronunciationScore = table.Column<double>(type: "float", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VocabularySuggest = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    GroupSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RoleInSession = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PracticeSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    GrammarCorrection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PronunciationScore = table.Column<double>(type: "float", nullable: false),
                    Sentence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Transcript = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PurchaseId",
                table: "Subscriptions",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ChapterId",
                table: "Questions",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathChapters_LearningPathTopicId",
                table: "LearningPathChapters",
                column: "LearningPathTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_TypeId",
                table: "Feedbacks",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionId",
                table: "AnswerOptions",
                column: "QuestionId");

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
                name: "IX_GroupSessions_RoomId",
                table: "GroupSessions",
                column: "RoomId");

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
                name: "IX_PracticeSessions_RoomId",
                table: "PracticeSessions",
                column: "RoomId");

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
                name: "IX_SkillMentors_MentorProfileId",
                table: "SkillMentors",
                column: "MentorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillMentors_SkillId",
                table: "SkillMentors",
                column: "SkillId");

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
                name: "IX_SystemPolicies_UserId",
                table: "SystemPolicies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingCertificate_MentorProfileId",
                table: "TeachingCertificate",
                column: "MentorProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentDetails_Assessments_AssessmentId",
                table: "AssessmentDetails",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_LearnerProfiles_LearnerProfileId",
                table: "Assessments",
                column: "LearnerProfileId",
                principalTable: "LearnerProfiles",
                principalColumn: "LearnerProfileId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Topics_TopicId",
                table: "Chapters",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "TopicId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Types_TypeId",
                table: "Feedbacks",
                column: "TypeId",
                principalTable: "Types",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPathChapters_LearningPathTopics_LearningPathTopicId",
                table: "LearningPathChapters",
                column: "LearningPathTopicId",
                principalTable: "LearningPathTopics",
                principalColumn: "LearningPathTopicId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressAnalytics_LearnerProfiles_LearnerProfileId",
                table: "ProgressAnalytics",
                column: "LearnerProfileId",
                principalTable: "LearnerProfiles",
                principalColumn: "LearnerProfileId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PronunciationResults_Assessments_AssessmentId",
                table: "PronunciationResults",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_ChapterModules_ModuleId",
                table: "Questions",
                column: "ModuleId",
                principalTable: "ChapterModules",
                principalColumn: "ModuleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Chapters_ChapterId",
                table: "Questions",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Purchases_PurchaseId",
                table: "Subscriptions",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "PurchaseId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
