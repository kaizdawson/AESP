using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddNumberOfRecordToLearnerRecordCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfRecord",
                table: "LearnerRecordCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 16, 9, 49, 19, 415, DateTimeKind.Utc).AddTicks(1561), new DateTime(2025, 12, 16, 9, 49, 19, 415, DateTimeKind.Utc).AddTicks(1563) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 16, 9, 49, 19, 415, DateTimeKind.Utc).AddTicks(1609), new DateTime(2025, 12, 16, 9, 49, 19, 415, DateTimeKind.Utc).AddTicks(1609) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfRecord",
                table: "LearnerRecordCategories");

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
    }
}
