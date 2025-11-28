using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddTranscribedText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TranscribedText",
                table: "Records",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 28, 7, 16, 19, 911, DateTimeKind.Utc).AddTicks(3337), new DateTime(2025, 11, 28, 7, 16, 19, 911, DateTimeKind.Utc).AddTicks(3341) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 28, 7, 16, 19, 911, DateTimeKind.Utc).AddTicks(3391), new DateTime(2025, 11, 28, 7, 16, 19, 911, DateTimeKind.Utc).AddTicks(3391) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TranscribedText",
                table: "Records");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 24, 17, 24, 0, 62, DateTimeKind.Utc).AddTicks(354), new DateTime(2025, 11, 24, 17, 24, 0, 62, DateTimeKind.Utc).AddTicks(357) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 24, 17, 24, 0, 62, DateTimeKind.Utc).AddTicks(425), new DateTime(2025, 11, 24, 17, 24, 0, 62, DateTimeKind.Utc).AddTicks(425) });
        }
    }
}
