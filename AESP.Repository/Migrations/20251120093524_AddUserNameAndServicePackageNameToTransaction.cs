using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNameAndServicePackageNameToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServicePackageName",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServicePackageName",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Transactions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 19, 17, 7, 54, 863, DateTimeKind.Utc).AddTicks(3915), new DateTime(2025, 11, 19, 17, 7, 54, 863, DateTimeKind.Utc).AddTicks(3919) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 19, 17, 7, 54, 863, DateTimeKind.Utc).AddTicks(3985), new DateTime(2025, 11, 19, 17, 7, 54, 863, DateTimeKind.Utc).AddTicks(3985) });
        }
    }
}
