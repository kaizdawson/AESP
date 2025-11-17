using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class DelHeatMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeatmapData",
                table: "ProgressAnalytics");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 16, 44, 40, 56, DateTimeKind.Utc).AddTicks(4663), new DateTime(2025, 11, 17, 16, 44, 40, 56, DateTimeKind.Utc).AddTicks(4665) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeatmapData",
                table: "ProgressAnalytics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 9, 16, 44, 312, DateTimeKind.Utc).AddTicks(8014), new DateTime(2025, 11, 17, 9, 16, 44, 312, DateTimeKind.Utc).AddTicks(8016) });
        }
    }
}
