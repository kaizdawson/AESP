using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldAccentAudioSourceInQuestionMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Accent",
                table: "QuestionMedias");

            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "QuestionMedias");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "QuestionMedias");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 20, 21, 42, 27, 183, DateTimeKind.Unspecified).AddTicks(9836), new DateTime(2025, 12, 20, 21, 42, 27, 183, DateTimeKind.Unspecified).AddTicks(9867) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 20, 21, 42, 27, 183, DateTimeKind.Unspecified).AddTicks(9947), new DateTime(2025, 12, 20, 21, 42, 27, 183, DateTimeKind.Unspecified).AddTicks(9949) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Accent",
                table: "QuestionMedias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "QuestionMedias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "QuestionMedias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 18, 19, 27, 34, 241, DateTimeKind.Unspecified).AddTicks(3461), new DateTime(2025, 12, 18, 19, 27, 34, 241, DateTimeKind.Unspecified).AddTicks(3494) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 18, 19, 27, 34, 241, DateTimeKind.Unspecified).AddTicks(3603), new DateTime(2025, 12, 18, 19, 27, 34, 241, DateTimeKind.Unspecified).AddTicks(3606) });
        }
    }
}
