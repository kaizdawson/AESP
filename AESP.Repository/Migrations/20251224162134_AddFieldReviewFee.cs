using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldReviewFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ReviewFees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ReviewFees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReviewFees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ReviewFeeDetail",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ReviewFeeDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReviewFeeDetail",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 24, 23, 21, 33, 318, DateTimeKind.Unspecified).AddTicks(8602), new DateTime(2025, 12, 24, 23, 21, 33, 318, DateTimeKind.Unspecified).AddTicks(8643) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 24, 23, 21, 33, 318, DateTimeKind.Unspecified).AddTicks(8726), new DateTime(2025, 12, 24, 23, 21, 33, 318, DateTimeKind.Unspecified).AddTicks(8727) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ReviewFees");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ReviewFees");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ReviewFees");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ReviewFeeDetail");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ReviewFeeDetail");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ReviewFeeDetail");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 20, 15, 55, 12, 854, DateTimeKind.Unspecified).AddTicks(8004), new DateTime(2025, 12, 20, 15, 55, 12, 854, DateTimeKind.Unspecified).AddTicks(8032) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 20, 15, 55, 12, 854, DateTimeKind.Unspecified).AddTicks(8080), new DateTime(2025, 12, 20, 15, 55, 12, 854, DateTimeKind.Unspecified).AddTicks(8081) });
        }
    }
}
