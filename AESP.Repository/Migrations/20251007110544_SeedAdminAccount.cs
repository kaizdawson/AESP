using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "AvatarUrl", "Email", "FirebaseUid", "FullName", "PasswordHash", "PhoneNumber", "Role", "Status" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "", "admin@aesp.com", "", "Super Admin", "6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc=", "0909000000", "Admin", "Active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
