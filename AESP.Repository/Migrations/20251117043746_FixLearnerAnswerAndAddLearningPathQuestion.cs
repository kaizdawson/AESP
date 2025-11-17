using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixLearnerAnswerAndAddLearningPathQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearnerAnswers_LearningPathExercises_LearningPathExerciseId",
                table: "LearnerAnswers");

            migrationBuilder.RenameColumn(
                name: "RelearnCount",
                table: "LearningPathExercises",
                newName: "NumberOfRetake");

            migrationBuilder.RenameColumn(
                name: "LearningPathExerciseId",
                table: "LearnerAnswers",
                newName: "LearningPathQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_LearnerAnswers_LearningPathExerciseId",
                table: "LearnerAnswers",
                newName: "IX_LearnerAnswers_LearningPathQuestionId");

            migrationBuilder.CreateTable(
                name: "LearningPathQuestion",
                columns: table => new
                {
                    LearningPathQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningPathExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberOfRetake = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPathQuestion", x => x.LearningPathQuestionId);
                    table.ForeignKey(
                        name: "FK_LearningPathQuestion_LearningPathExercises_LearningPathExerciseId",
                        column: x => x.LearningPathExerciseId,
                        principalTable: "LearningPathExercises",
                        principalColumn: "LearningPathExerciseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningPathQuestion_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 37, 43, 267, DateTimeKind.Utc).AddTicks(4194), new DateTime(2025, 11, 17, 4, 37, 43, 267, DateTimeKind.Utc).AddTicks(4198) });

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathQuestion_LearningPathExerciseId",
                table: "LearningPathQuestion",
                column: "LearningPathExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathQuestion_QuestionId",
                table: "LearningPathQuestion",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerAnswers_LearningPathQuestion_LearningPathQuestionId",
                table: "LearnerAnswers",
                column: "LearningPathQuestionId",
                principalTable: "LearningPathQuestion",
                principalColumn: "LearningPathQuestionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearnerAnswers_LearningPathQuestion_LearningPathQuestionId",
                table: "LearnerAnswers");

            migrationBuilder.DropTable(
                name: "LearningPathQuestion");

            migrationBuilder.RenameColumn(
                name: "NumberOfRetake",
                table: "LearningPathExercises",
                newName: "RelearnCount");

            migrationBuilder.RenameColumn(
                name: "LearningPathQuestionId",
                table: "LearnerAnswers",
                newName: "LearningPathExerciseId");

            migrationBuilder.RenameIndex(
                name: "IX_LearnerAnswers_LearningPathQuestionId",
                table: "LearnerAnswers",
                newName: "IX_LearnerAnswers_LearningPathExerciseId");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 14, 19, 14, 220, DateTimeKind.Utc).AddTicks(8853), new DateTime(2025, 11, 16, 14, 19, 14, 220, DateTimeKind.Utc).AddTicks(8855) });

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerAnswers_LearningPathExercises_LearningPathExerciseId",
                table: "LearnerAnswers",
                column: "LearningPathExerciseId",
                principalTable: "LearningPathExercises",
                principalColumn: "LearningPathExerciseId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
