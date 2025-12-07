using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class SyncPurchaseAndExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ 1. DROP FOREIGN KEY TRƯỚC
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_LearnerProfiles_LearnerProfileId",
                table: "Purchases");

            // ✅ 2. DROP INDEX
            migrationBuilder.DropIndex(
                name: "IX_Purchases_LearnerProfileId",
                table: "Purchases");

            // ✅ 3. DROP COLUMN
            migrationBuilder.DropColumn(
                name: "LearnerProfileId",
                table: "Purchases");

            // ✅ 4. ADD CỘT IsTest VÀO EXERCISES
            migrationBuilder.AddColumn<bool>(
                name: "IsTest",
                table: "Exercises",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // ✅ 5. GIỮ NGUYÊN UPDATE DATA
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 7, 14, 8, 34, 852, DateTimeKind.Utc).AddTicks(4038), new DateTime(2025, 12, 7, 14, 8, 34, 852, DateTimeKind.Utc).AddTicks(4039) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 7, 14, 8, 34, 852, DateTimeKind.Utc).AddTicks(4087), new DateTime(2025, 12, 7, 14, 8, 34, 852, DateTimeKind.Utc).AddTicks(4087) });
        }




        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ DROP IsTest
            migrationBuilder.DropColumn(
                name: "IsTest",
                table: "Exercises");

            // ✅ ADD LẠI LearnerProfileId
            migrationBuilder.AddColumn<Guid>(
                name: "LearnerProfileId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: true);

            // ✅ TẠO LẠI INDEX
            migrationBuilder.CreateIndex(
                name: "IX_Purchases_LearnerProfileId",
                table: "Purchases",
                column: "LearnerProfileId");

            // ✅ TẠO LẠI FOREIGN KEY
            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_LearnerProfiles_LearnerProfileId",
                table: "Purchases",
                column: "LearnerProfileId",
                principalTable: "LearnerProfiles",
                principalColumn: "LearnerProfileId",
                onDelete: ReferentialAction.Restrict);

            // ✅ ROLLBACK UPDATE DATA
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 3, 17, 0, 52, 931, DateTimeKind.Utc).AddTicks(2180), new DateTime(2025, 12, 3, 17, 0, 52, 931, DateTimeKind.Utc).AddTicks(2183) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 3, 17, 0, 52, 931, DateTimeKind.Utc).AddTicks(2235), new DateTime(2025, 12, 3, 17, 0, 52, 931, DateTimeKind.Utc).AddTicks(2235) });
        }



    }
}
