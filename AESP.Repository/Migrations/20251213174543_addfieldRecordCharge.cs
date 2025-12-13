using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addfieldRecordCharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 13, 17, 45, 41, 926, DateTimeKind.Utc).AddTicks(5001), new DateTime(2025, 12, 13, 17, 45, 41, 926, DateTimeKind.Utc).AddTicks(5006) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 13, 17, 45, 41, 926, DateTimeKind.Utc).AddTicks(5096), new DateTime(2025, 12, 13, 17, 45, 41, 926, DateTimeKind.Utc).AddTicks(5097) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 13, 17, 26, 55, 250, DateTimeKind.Utc).AddTicks(9084), new DateTime(2025, 12, 13, 17, 26, 55, 250, DateTimeKind.Utc).AddTicks(9087) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 13, 17, 26, 55, 250, DateTimeKind.Utc).AddTicks(9290), new DateTime(2025, 12, 13, 17, 26, 55, 250, DateTimeKind.Utc).AddTicks(9291) });
        }
    }
}
