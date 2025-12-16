using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixModelTimeVn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 17, 3, 4, 17, 55, DateTimeKind.Unspecified).AddTicks(9982), new DateTime(2025, 12, 17, 3, 4, 17, 56, DateTimeKind.Unspecified).AddTicks(33) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 17, 3, 4, 17, 56, DateTimeKind.Unspecified).AddTicks(114), new DateTime(2025, 12, 17, 3, 4, 17, 56, DateTimeKind.Unspecified).AddTicks(114) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 16, 9, 49, 19, 415, DateTimeKind.Utc).AddTicks(1561), new DateTime(2025, 12, 16, 9, 49, 19, 415, DateTimeKind.Utc).AddTicks(1563) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 16, 9, 49, 19, 415, DateTimeKind.Utc).AddTicks(1609), new DateTime(2025, 12, 16, 9, 49, 19, 415, DateTimeKind.Utc).AddTicks(1609) });
        }
    }
}
