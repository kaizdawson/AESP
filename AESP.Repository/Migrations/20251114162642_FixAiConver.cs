using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixAiConver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoinTransaction");

            migrationBuilder.DropTable(
                name: "TransferTransaction");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "NumberOfReview",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "AIConversationCharge");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "AIConversationCharge");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReviewFeeId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AIConversationChargeId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AmountCoin",
                table: "AIConversationCharge",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 16, 26, 41, 936, DateTimeKind.Utc).AddTicks(7531), new DateTime(2025, 11, 14, 16, 26, 41, 936, DateTimeKind.Utc).AddTicks(7534) });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CourseId",
                table: "Purchases",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Courses_CourseId",
                table: "Purchases",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Courses_CourseId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_CourseId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Purchases");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReviewFeeId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "AIConversationChargeId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "Purchases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfReview",
                table: "Purchases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "AmountCoin",
                table: "AIConversationCharge",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "AIConversationCharge",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "AIConversationCharge",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TransferTransaction",
                columns: table => new
                {
                    TransferTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AmountCoin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferTransaction", x => x.TransferTransactionId);
                    table.ForeignKey(
                        name: "FK_TransferTransaction_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferTransaction_ReviewerProfiles_ReviewerProfileId",
                        column: x => x.ReviewerProfileId,
                        principalTable: "ReviewerProfiles",
                        principalColumn: "ReviewerProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferTransaction_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "ReviewId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoinTransaction",
                columns: table => new
                {
                    CoinTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AIConversationChargeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurchaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransferTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinTransaction", x => x.CoinTransactionId);
                    table.ForeignKey(
                        name: "FK_CoinTransaction_AIConversationCharge_AIConversationChargeId",
                        column: x => x.AIConversationChargeId,
                        principalTable: "AIConversationCharge",
                        principalColumn: "AIConversationChargeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoinTransaction_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "PurchaseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoinTransaction_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoinTransaction_TransferTransaction_TransferTransactionId",
                        column: x => x.TransferTransactionId,
                        principalTable: "TransferTransaction",
                        principalColumn: "TransferTransactionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoinTransaction_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 15, 46, 52, 444, DateTimeKind.Utc).AddTicks(9259), new DateTime(2025, 11, 14, 15, 46, 52, 444, DateTimeKind.Utc).AddTicks(9261) });

            migrationBuilder.CreateIndex(
                name: "IX_CoinTransaction_AIConversationChargeId",
                table: "CoinTransaction",
                column: "AIConversationChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinTransaction_PurchaseId",
                table: "CoinTransaction",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinTransaction_TransactionId",
                table: "CoinTransaction",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinTransaction_TransferTransactionId",
                table: "CoinTransaction",
                column: "TransferTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinTransaction_UserId",
                table: "CoinTransaction",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTransaction_LearnerProfileId",
                table: "TransferTransaction",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTransaction_ReviewerProfileId",
                table: "TransferTransaction",
                column: "ReviewerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTransaction_ReviewId",
                table: "TransferTransaction",
                column: "ReviewId");
        }
    }
}
