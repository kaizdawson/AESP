using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addDescriptionAndRelearncount_fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelearnCount",
                table: "LearningPathExercises",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Courses",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 13, 9, 43, 8, 162, DateTimeKind.Utc).AddTicks(4754), new DateTime(2025, 11, 13, 9, 43, 8, 162, DateTimeKind.Utc).AddTicks(4757) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelearnCount",
                table: "LearningPathExercises");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Courses");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 8, 37, 47, 697, DateTimeKind.Utc).AddTicks(3047), new DateTime(2025, 11, 10, 8, 37, 47, 697, DateTimeKind.Utc).AddTicks(3049) });
        }
    }
}
