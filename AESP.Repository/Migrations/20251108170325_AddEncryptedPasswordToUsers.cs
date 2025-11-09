using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptedPasswordToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptedPassword",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "EncryptedPassword" },
                values: new object[] { new DateTime(2025, 11, 8, 17, 3, 24, 657, DateTimeKind.Utc).AddTicks(2257), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedPassword",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 6, 15, 53, 48, 288, DateTimeKind.Utc).AddTicks(4784));
        }
    }
}
