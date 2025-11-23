using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Streak_Confidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfidenceLevel",
                table: "ProgressAnalytics");

            migrationBuilder.DropColumn(
                name: "StreakDays",
                table: "ProgressAnalytics");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 23, 7, 5, 8, 409, DateTimeKind.Utc).AddTicks(9048), new DateTime(2025, 11, 23, 7, 5, 8, 409, DateTimeKind.Utc).AddTicks(9050) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 23, 7, 5, 8, 409, DateTimeKind.Utc).AddTicks(9097), new DateTime(2025, 11, 23, 7, 5, 8, 409, DateTimeKind.Utc).AddTicks(9097) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ConfidenceLevel",
                table: "ProgressAnalytics",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "StreakDays",
                table: "ProgressAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 19, 33, 33, 550, DateTimeKind.Utc).AddTicks(4691), new DateTime(2025, 11, 20, 19, 33, 33, 550, DateTimeKind.Utc).AddTicks(4692) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 19, 33, 33, 550, DateTimeKind.Utc).AddTicks(4752), new DateTime(2025, 11, 20, 19, 33, 33, 550, DateTimeKind.Utc).AddTicks(4752) });
        }
    }
}
