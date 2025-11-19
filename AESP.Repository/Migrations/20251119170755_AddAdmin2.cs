using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddAdmin2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 19, 17, 7, 54, 863, DateTimeKind.Utc).AddTicks(3915), new DateTime(2025, 11, 19, 17, 7, 54, 863, DateTimeKind.Utc).AddTicks(3919) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "AvatarUrl", "CoinBalance", "CreatedAt", "Email", "EncryptedPassword", "FirebaseUid", "FullName", "IsDeleted", "LastActiveAt", "PasswordHash", "PhoneNumber", "Role", "Status", "UpdatedAt" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), "", 0, new DateTime(2025, 11, 19, 17, 7, 54, 863, DateTimeKind.Utc).AddTicks(3985), "admin2@aesp.com", null, "", "Second Admin", false, null, "6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc=", "0912345678", "ADMIN", "Active", new DateTime(2025, 11, 19, 17, 7, 54, 863, DateTimeKind.Utc).AddTicks(3985) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

          

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 19, 35, 37, 944, DateTimeKind.Utc).AddTicks(5225), new DateTime(2025, 11, 17, 19, 35, 37, 944, DateTimeKind.Utc).AddTicks(5227) });
        }
    }
}
