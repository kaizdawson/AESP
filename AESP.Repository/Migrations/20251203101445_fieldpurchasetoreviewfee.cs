using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class fieldpurchasetoreviewfee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PercentOfReviewerAtPurchase",
                table: "Purchases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerReviewAtPurchase",
                table: "Purchases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 3, 10, 14, 44, 387, DateTimeKind.Utc).AddTicks(2855), new DateTime(2025, 12, 3, 10, 14, 44, 387, DateTimeKind.Utc).AddTicks(2858) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 3, 10, 14, 44, 387, DateTimeKind.Utc).AddTicks(2984), new DateTime(2025, 12, 3, 10, 14, 44, 387, DateTimeKind.Utc).AddTicks(2984) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PercentOfReviewerAtPurchase",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "PricePerReviewAtPurchase",
                table: "Purchases");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 29, 11, 8, 53, 403, DateTimeKind.Utc).AddTicks(3017), new DateTime(2025, 11, 29, 11, 8, 53, 403, DateTimeKind.Utc).AddTicks(3020) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 29, 11, 8, 53, 403, DateTimeKind.Utc).AddTicks(3151), new DateTime(2025, 11, 29, 11, 8, 53, 403, DateTimeKind.Utc).AddTicks(3151) });
        }
    }
}
