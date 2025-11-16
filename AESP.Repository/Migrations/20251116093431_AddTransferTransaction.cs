using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransferTransactions",
                columns: table => new
                {
                    TransferTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AmountCoin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferTransactions", x => x.TransferTransactionId);
                    table.ForeignKey(
                        name: "FK_TransferTransactions_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferTransactions_ReviewerProfiles_ReviewerProfileId",
                        column: x => x.ReviewerProfileId,
                        principalTable: "ReviewerProfiles",
                        principalColumn: "ReviewerProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferTransactions_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "ReviewId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 9, 34, 30, 308, DateTimeKind.Utc).AddTicks(4441), new DateTime(2025, 11, 16, 9, 34, 30, 308, DateTimeKind.Utc).AddTicks(4444) });

            migrationBuilder.CreateIndex(
                name: "IX_TransferTransactions_LearnerProfileId",
                table: "TransferTransactions",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTransactions_ReviewerProfileId",
                table: "TransferTransactions",
                column: "ReviewerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTransactions_ReviewId",
                table: "TransferTransactions",
                column: "ReviewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferTransactions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 16, 26, 41, 936, DateTimeKind.Utc).AddTicks(7531), new DateTime(2025, 11, 14, 16, 26, 41, 936, DateTimeKind.Utc).AddTicks(7534) });
        }
    }
}
