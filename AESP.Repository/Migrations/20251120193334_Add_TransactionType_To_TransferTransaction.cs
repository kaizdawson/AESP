using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_TransactionType_To_TransferTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "TransferTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "TransferTransactions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 9, 35, 23, 514, DateTimeKind.Utc).AddTicks(7862), new DateTime(2025, 11, 20, 9, 35, 23, 514, DateTimeKind.Utc).AddTicks(7864) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 9, 35, 23, 514, DateTimeKind.Utc).AddTicks(7914), new DateTime(2025, 11, 20, 9, 35, 23, 514, DateTimeKind.Utc).AddTicks(7915) });
        }
    }
}
