using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MarkIsTestAsApplied : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        IF NOT EXISTS (
            SELECT 1 FROM [__EFMigrationsHistory] 
            WHERE MigrationId = '20251207192253_AddIsTestToExercise'
        )
        BEGIN
            INSERT INTO [__EFMigrationsHistory] (MigrationId, ProductVersion)
            VALUES ('20251207192253_AddIsTestToExercise', '8.0.16');
        END");

            // ===== DÀN CODE CŨ (tạo bảng RecordCharge và các thay đổi khác) - GIỮ NGUYÊN =====
            migrationBuilder.AddColumn<Guid>(
                name: "RecordChargeId",
                table: "Records",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecordChargeId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecordCharge",
                columns: table => new
                {
                    RecordChargeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AmountCoin = table.Column<int>(type: "int", nullable: false),
                    AllowedRecordCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordCharge", x => x.RecordChargeId);
                });

            // ... (giữ nguyên hết phần UpdateData Users, CreateIndex, AddForeignKey như bạn đã có)
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 13, 17, 26, 55, 250, DateTimeKind.Utc).AddTicks(9084), new DateTime(2025, 12, 13, 17, 26, 55, 250, DateTimeKind.Utc).AddTicks(9087) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 13, 17, 26, 55, 250, DateTimeKind.Utc).AddTicks(9290), new DateTime(2025, 12, 13, 17, 26, 55, 250, DateTimeKind.Utc).AddTicks(9291) });

            migrationBuilder.CreateIndex(
                name: "IX_Records_RecordChargeId",
                table: "Records",
                column: "RecordChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_RecordChargeId",
                table: "Purchases",
                column: "RecordChargeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_RecordCharge_RecordChargeId",
                table: "Purchases",
                column: "RecordChargeId",
                principalTable: "RecordCharge",
                principalColumn: "RecordChargeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Records_RecordCharge_RecordChargeId",
                table: "Records",
                column: "RecordChargeId",
                principalTable: "RecordCharge",
                principalColumn: "RecordChargeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_RecordCharge_RecordChargeId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Records_RecordCharge_RecordChargeId",
                table: "Records");

            migrationBuilder.DropTable(
                name: "RecordCharge");

            migrationBuilder.DropIndex(
                name: "IX_Records_RecordChargeId",
                table: "Records");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_RecordChargeId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "RecordChargeId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "RecordChargeId",
                table: "Purchases");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 7, 19, 22, 52, 403, DateTimeKind.Utc).AddTicks(652), new DateTime(2025, 12, 7, 19, 22, 52, 403, DateTimeKind.Utc).AddTicks(654) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 7, 19, 22, 52, 403, DateTimeKind.Utc).AddTicks(708), new DateTime(2025, 12, 7, 19, 22, 52, 403, DateTimeKind.Utc).AddTicks(709) });
        }
    }
}
