using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixAddLearningPathQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearnerAnswers_LearningPathQuestion_LearningPathQuestionId",
                table: "LearnerAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningPathQuestion_LearningPathExercises_LearningPathExerciseId",
                table: "LearningPathQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningPathQuestion_Questions_QuestionId",
                table: "LearningPathQuestion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningPathQuestion",
                table: "LearningPathQuestion");

            migrationBuilder.RenameTable(
                name: "LearningPathQuestion",
                newName: "LearningPathQuestions");

            migrationBuilder.RenameIndex(
                name: "IX_LearningPathQuestion_QuestionId",
                table: "LearningPathQuestions",
                newName: "IX_LearningPathQuestions_QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningPathQuestion_LearningPathExerciseId",
                table: "LearningPathQuestions",
                newName: "IX_LearningPathQuestions_LearningPathExerciseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningPathQuestions",
                table: "LearningPathQuestions",
                column: "LearningPathQuestionId");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 9, 16, 44, 312, DateTimeKind.Utc).AddTicks(8014), new DateTime(2025, 11, 17, 9, 16, 44, 312, DateTimeKind.Utc).AddTicks(8016) });

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerAnswers_LearningPathQuestions_LearningPathQuestionId",
                table: "LearnerAnswers",
                column: "LearningPathQuestionId",
                principalTable: "LearningPathQuestions",
                principalColumn: "LearningPathQuestionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPathQuestions_LearningPathExercises_LearningPathExerciseId",
                table: "LearningPathQuestions",
                column: "LearningPathExerciseId",
                principalTable: "LearningPathExercises",
                principalColumn: "LearningPathExerciseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPathQuestions_Questions_QuestionId",
                table: "LearningPathQuestions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearnerAnswers_LearningPathQuestions_LearningPathQuestionId",
                table: "LearnerAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningPathQuestions_LearningPathExercises_LearningPathExerciseId",
                table: "LearningPathQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningPathQuestions_Questions_QuestionId",
                table: "LearningPathQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningPathQuestions",
                table: "LearningPathQuestions");

            migrationBuilder.RenameTable(
                name: "LearningPathQuestions",
                newName: "LearningPathQuestion");

            migrationBuilder.RenameIndex(
                name: "IX_LearningPathQuestions_QuestionId",
                table: "LearningPathQuestion",
                newName: "IX_LearningPathQuestion_QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningPathQuestions_LearningPathExerciseId",
                table: "LearningPathQuestion",
                newName: "IX_LearningPathQuestion_LearningPathExerciseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningPathQuestion",
                table: "LearningPathQuestion",
                column: "LearningPathQuestionId");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 37, 43, 267, DateTimeKind.Utc).AddTicks(4194), new DateTime(2025, 11, 17, 4, 37, 43, 267, DateTimeKind.Utc).AddTicks(4198) });

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerAnswers_LearningPathQuestion_LearningPathQuestionId",
                table: "LearnerAnswers",
                column: "LearningPathQuestionId",
                principalTable: "LearningPathQuestion",
                principalColumn: "LearningPathQuestionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPathQuestion_LearningPathExercises_LearningPathExerciseId",
                table: "LearningPathQuestion",
                column: "LearningPathExerciseId",
                principalTable: "LearningPathExercises",
                principalColumn: "LearningPathExerciseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPathQuestion_Questions_QuestionId",
                table: "LearningPathQuestion",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
