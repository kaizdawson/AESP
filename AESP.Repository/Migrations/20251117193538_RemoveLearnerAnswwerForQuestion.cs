using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLearnerAnswwerForQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearnerAnswers_Questions_QuestionId",
                table: "LearnerAnswers");

            migrationBuilder.DropIndex(
                name: "IX_LearnerAnswers_QuestionId",
                table: "LearnerAnswers");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "LearnerAnswers");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 19, 35, 37, 944, DateTimeKind.Utc).AddTicks(5225), new DateTime(2025, 11, 17, 19, 35, 37, 944, DateTimeKind.Utc).AddTicks(5227) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "QuestionId",
                table: "LearnerAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 19, 9, 42, 337, DateTimeKind.Utc).AddTicks(2317), new DateTime(2025, 11, 17, 19, 9, 42, 337, DateTimeKind.Utc).AddTicks(2322) });

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAnswers_QuestionId",
                table: "LearnerAnswers",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerAnswers_Questions_QuestionId",
                table: "LearnerAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
