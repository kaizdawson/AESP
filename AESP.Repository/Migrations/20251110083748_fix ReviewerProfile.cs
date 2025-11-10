using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class fixReviewerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Levels",
                table: "ReviewerProfiles",
                newName: "Level");

            migrationBuilder.AlterColumn<int>(
                name: "Experience",
                table: "ReviewerProfiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 8, 37, 47, 697, DateTimeKind.Utc).AddTicks(3047), new DateTime(2025, 11, 10, 8, 37, 47, 697, DateTimeKind.Utc).AddTicks(3049) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Level",
                table: "ReviewerProfiles",
                newName: "Levels");

            migrationBuilder.AlterColumn<string>(
                name: "Experience",
                table: "ReviewerProfiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 11, 37, 21, 768, DateTimeKind.Utc).AddTicks(4609), new DateTime(2025, 11, 9, 11, 37, 21, 768, DateTimeKind.Utc).AddTicks(4611) });
        }
    }
}
