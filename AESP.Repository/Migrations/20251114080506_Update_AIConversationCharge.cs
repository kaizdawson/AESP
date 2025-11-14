using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    public partial class Update_AIConversationCharge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIConversationCharge_Users_UserId",
                table: "AIConversationCharge");

            migrationBuilder.DropIndex(
                name: "IX_AIConversationCharge_UserId",
                table: "AIConversationCharge");

            migrationBuilder.DropColumn("ContentJson", "AIConversationCharge");
            migrationBuilder.DropColumn("EndTime", "AIConversationCharge");
            migrationBuilder.DropColumn("RoomId", "AIConversationCharge");
            migrationBuilder.DropColumn("UserId", "AIConversationCharge");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "AIConversationCharge",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<int>(
                name: "AmountCoin",
                table: "AIConversationCharge",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AIConversationCharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] {
            new DateTime(2025, 11, 14, 8, 5, 5, 365, DateTimeKind.Utc).AddTicks(2156),
            new DateTime(2025, 11, 14, 8, 5, 5, 365, DateTimeKind.Utc).AddTicks(2162)
                });
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 🔥 REMOVE BaseEntity fields in DOWN()
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AIConversationCharge");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AIConversationCharge");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AIConversationCharge");

            migrationBuilder.AlterColumn<decimal>(
                name: "AmountCoin",
                table: "AIConversationCharge",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ContentJson",
                table: "AIConversationCharge",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "AIConversationCharge",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomId",
                table: "AIConversationCharge",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AIConversationCharge",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] {
                    new DateTime(2025, 11, 14, 3, 42, 36, 47, DateTimeKind.Utc).AddTicks(9200),
                    new DateTime(2025, 11, 14, 3, 42, 36, 47, DateTimeKind.Utc).AddTicks(9203)
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIConversationCharge_UserId",
                table: "AIConversationCharge",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIConversationCharge_Users_UserId",
                table: "AIConversationCharge",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
