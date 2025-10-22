using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEntityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ReviewerProfiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ReviewerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReviewerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "LearnerProfiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LearnerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LearnerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 30, 58, 152, DateTimeKind.Utc).AddTicks(5249), false, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ReviewerProfiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ReviewerProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ReviewerProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LearnerProfiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LearnerProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LearnerProfiles");
        }
    }
}
