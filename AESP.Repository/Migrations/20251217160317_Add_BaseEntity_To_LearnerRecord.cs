using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_BaseEntity_To_LearnerRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LearnerRecordCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LearnerRecordCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 17, 23, 3, 16, 700, DateTimeKind.Unspecified).AddTicks(9201), new DateTime(2025, 12, 17, 23, 3, 16, 700, DateTimeKind.Unspecified).AddTicks(9230) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 17, 23, 3, 16, 700, DateTimeKind.Unspecified).AddTicks(9277), new DateTime(2025, 12, 17, 23, 3, 16, 700, DateTimeKind.Unspecified).AddTicks(9277) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LearnerRecordCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LearnerRecordCategories");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 17, 3, 4, 17, 55, DateTimeKind.Unspecified).AddTicks(9982), new DateTime(2025, 12, 17, 3, 4, 17, 56, DateTimeKind.Unspecified).AddTicks(33) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 17, 3, 4, 17, 56, DateTimeKind.Unspecified).AddTicks(114), new DateTime(2025, 12, 17, 3, 4, 17, 56, DateTimeKind.Unspecified).AddTicks(114) });
        }
    }
}
