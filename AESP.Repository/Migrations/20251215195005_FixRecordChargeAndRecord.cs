using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixRecordChargeAndRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Records_RecordCharge_RecordChargeId",
                table: "Records");

            migrationBuilder.DropIndex(
                name: "IX_Records_RecordChargeId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "RecordChargeId",
                table: "Records");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 15, 19, 50, 4, 757, DateTimeKind.Utc).AddTicks(5375), new DateTime(2025, 12, 15, 19, 50, 4, 757, DateTimeKind.Utc).AddTicks(5378) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 15, 19, 50, 4, 757, DateTimeKind.Utc).AddTicks(5439), new DateTime(2025, 12, 15, 19, 50, 4, 757, DateTimeKind.Utc).AddTicks(5439) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecordChargeId",
                table: "Records",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 13, 17, 45, 41, 926, DateTimeKind.Utc).AddTicks(5001), new DateTime(2025, 12, 13, 17, 45, 41, 926, DateTimeKind.Utc).AddTicks(5006) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 13, 17, 45, 41, 926, DateTimeKind.Utc).AddTicks(5096), new DateTime(2025, 12, 13, 17, 45, 41, 926, DateTimeKind.Utc).AddTicks(5097) });

            migrationBuilder.CreateIndex(
                name: "IX_Records_RecordChargeId",
                table: "Records",
                column: "RecordChargeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Records_RecordCharge_RecordChargeId",
                table: "Records",
                column: "RecordChargeId",
                principalTable: "RecordCharge",
                principalColumn: "RecordChargeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
