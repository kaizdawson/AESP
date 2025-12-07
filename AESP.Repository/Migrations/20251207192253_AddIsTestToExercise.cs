using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    public partial class AddIsTestToExercise : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ 1. THÊM CỘT IsTest
            migrationBuilder.AddColumn<bool>(
                name: "IsTest",
                table: "Exercises",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // ✅ 2. GIỮ NGUYÊN UPDATE DATA USERS (nếu anh cần seed)
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2025, 12, 7, 19, 22, 52, 403, DateTimeKind.Utc).AddTicks(652),
                    new DateTime(2025, 12, 7, 19, 22, 52, 403, DateTimeKind.Utc).AddTicks(654)
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2025, 12, 7, 19, 22, 52, 403, DateTimeKind.Utc).AddTicks(708),
                    new DateTime(2025, 12, 7, 19, 22, 52, 403, DateTimeKind.Utc).AddTicks(709)
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ 1. XÓA CỘT IsTest KHI ROLLBACK
            migrationBuilder.DropColumn(
                name: "IsTest",
                table: "Exercises");

            // ✅ 2. ROLLBACK UPDATE DATA USERS
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2025, 12, 3, 17, 0, 52, 931, DateTimeKind.Utc).AddTicks(2180),
                    new DateTime(2025, 12, 3, 17, 0, 52, 931, DateTimeKind.Utc).AddTicks(2183)
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2025, 12, 3, 17, 0, 52, 931, DateTimeKind.Utc).AddTicks(2235),
                    new DateTime(2025, 12, 3, 17, 0, 52, 931, DateTimeKind.Utc).AddTicks(2235)
                });
        }
    }
}
