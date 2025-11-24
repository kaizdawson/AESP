using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordAudioUrlToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecordAudioUrl",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordAudioUrl",
                table: "Reviews");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 23, 7, 5, 8, 409, DateTimeKind.Utc).AddTicks(9048), new DateTime(2025, 11, 23, 7, 5, 8, 409, DateTimeKind.Utc).AddTicks(9050) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 23, 7, 5, 8, 409, DateTimeKind.Utc).AddTicks(9097), new DateTime(2025, 11, 23, 7, 5, 8, 409, DateTimeKind.Utc).AddTicks(9097) });
        }
    }
}
