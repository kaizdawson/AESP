using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RecordContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Records_LearnerRecordCategories_LearnerRecordId",
                table: "Records");

            migrationBuilder.RenameColumn(
                name: "LearnerRecordId",
                table: "Records",
                newName: "RecordContentId");

            migrationBuilder.RenameIndex(
                name: "IX_Records_LearnerRecordId",
                table: "Records",
                newName: "IX_Records_RecordContentId");

            migrationBuilder.CreateTable(
                name: "RecordContents",
                columns: table => new
                {
                    RecordContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordContents", x => x.RecordContentId);
                    table.ForeignKey(
                        name: "FK_RecordContents_LearnerRecordCategories_LearnerRecordId",
                        column: x => x.LearnerRecordId,
                        principalTable: "LearnerRecordCategories",
                        principalColumn: "LearnerRecordId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 20, 15, 55, 12, 854, DateTimeKind.Unspecified).AddTicks(8004), new DateTime(2025, 12, 20, 15, 55, 12, 854, DateTimeKind.Unspecified).AddTicks(8032) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 20, 15, 55, 12, 854, DateTimeKind.Unspecified).AddTicks(8080), new DateTime(2025, 12, 20, 15, 55, 12, 854, DateTimeKind.Unspecified).AddTicks(8081) });

            migrationBuilder.CreateIndex(
                name: "IX_RecordContents_LearnerRecordId",
                table: "RecordContents",
                column: "LearnerRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Records_RecordContents_RecordContentId",
                table: "Records",
                column: "RecordContentId",
                principalTable: "RecordContents",
                principalColumn: "RecordContentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Records_RecordContents_RecordContentId",
                table: "Records");

            migrationBuilder.DropTable(
                name: "RecordContents");

            migrationBuilder.RenameColumn(
                name: "RecordContentId",
                table: "Records",
                newName: "LearnerRecordId");

            migrationBuilder.RenameIndex(
                name: "IX_Records_RecordContentId",
                table: "Records",
                newName: "IX_Records_LearnerRecordId");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 19, 22, 46, 34, 10, DateTimeKind.Unspecified).AddTicks(1678), new DateTime(2025, 12, 19, 22, 46, 34, 10, DateTimeKind.Unspecified).AddTicks(1728) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 19, 22, 46, 34, 10, DateTimeKind.Unspecified).AddTicks(1798), new DateTime(2025, 12, 19, 22, 46, 34, 10, DateTimeKind.Unspecified).AddTicks(1799) });

            migrationBuilder.AddForeignKey(
                name: "FK_Records_LearnerRecordCategories_LearnerRecordId",
                table: "Records",
                column: "LearnerRecordId",
                principalTable: "LearnerRecordCategories",
                principalColumn: "LearnerRecordId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
