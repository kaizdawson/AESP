using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldRelearnAndRemovePhonemeJsonInQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhonemeJson",
                table: "Questions");

            migrationBuilder.AddColumn<int>(
                name: "RelearnScore",
                table: "LearningPathQuestions",
                type: "int",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelearnScore",
                table: "LearningPathQuestions");

            migrationBuilder.AddColumn<string>(
                name: "PhonemeJson",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 17, 23, 3, 16, 700, DateTimeKind.Unspecified).AddTicks(9201), new DateTime(2025, 12, 17, 23, 3, 16, 700, DateTimeKind.Unspecified).AddTicks(9230) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 17, 23, 3, 16, 700, DateTimeKind.Unspecified).AddTicks(9277), new DateTime(2025, 12, 17, 23, 3, 16, 700, DateTimeKind.Unspecified).AddTicks(9277) });
        }
    }
}
