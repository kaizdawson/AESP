using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixDBWithMainFL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhonemeResults_PhonemeTemplates_PhonemeId",
                table: "PhonemeResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Subscriptions_SubscriptionId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewerProfiles_Wallets_WalletId",
                table: "ReviewerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_WalletId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Stresses");

            migrationBuilder.DropTable(
                name: "StressResults");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "SubServicePackages");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "PhonemeTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_WalletId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_ReviewerProfiles_WalletId",
                table: "ReviewerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_SubscriptionId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_PhonemeResults_PhonemeId",
                table: "PhonemeResults");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "ReviewerProfiles");

            migrationBuilder.DropColumn(
                name: "PriceReviewFee",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "PriceServicePackage",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "ExpectedSymbol",
                table: "PhonemeResults");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "PhonemeResults");

            migrationBuilder.DropColumn(
                name: "PhonemeId",
                table: "PhonemeResults");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Assessments");

            migrationBuilder.RenameColumn(
                name: "NumberOfReview",
                table: "ServicePackages",
                newName: "NumberOfCoin");

            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "Purchases",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "PurchaseDate",
                table: "Purchases",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "PaymentStatus",
                table: "Purchases",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "PhonemeResults",
                newName: "PhonemeJson");

            migrationBuilder.AddColumn<decimal>(
                name: "CoinBalance",
                table: "Users",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountCoin",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountMoney",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ServicePackageId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "ServicePackages",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<double>(
                name: "BonusPercent",
                table: "ServicePackages",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReviewFeeId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "LearnerProfileId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountCoin",
                table: "Purchases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ItemId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: true);

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

            migrationBuilder.CreateTable(
                name: "AIConversationCharge",
                columns: table => new
                {
                    AIConversationChargeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AmountCoin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllowedMinutes = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIConversationCharge", x => x.AIConversationChargeId);
                    table.ForeignKey(
                        name: "FK_AIConversationCharge_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

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
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurchaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AIConversationChargeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransferTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                columns: new[] { "CoinBalance", "CreatedAt" },
                values: new object[] { 0m, new DateTime(2025, 11, 1, 9, 42, 18, 700, DateTimeKind.Utc).AddTicks(9391) });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ServicePackageId",
                table: "Transactions",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_UserId",
                table: "Purchases",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversationCharge_UserId",
                table: "AIConversationCharge",
                column: "UserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Users_UserId",
                table: "Purchases",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ServicePackages_ServicePackageId",
                table: "Transactions",
                column: "ServicePackageId",
                principalTable: "ServicePackages",
                principalColumn: "ServicePackageId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Users_UserId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ServicePackages_ServicePackageId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "CoinTransaction");

            migrationBuilder.DropTable(
                name: "AIConversationCharge");

            migrationBuilder.DropTable(
                name: "TransferTransaction");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ServicePackageId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_UserId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "CoinBalance",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AmountCoin",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "AmountMoney",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ServicePackageId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BonusPercent",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "AmountCoin",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "NumberOfReview",
                table: "Purchases");

            migrationBuilder.RenameColumn(
                name: "NumberOfCoin",
                table: "ServicePackages",
                newName: "NumberOfReview");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Purchases",
                newName: "SubscriptionId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Purchases",
                newName: "PaymentStatus");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Purchases",
                newName: "PurchaseDate");

            migrationBuilder.RenameColumn(
                name: "PhonemeJson",
                table: "PhonemeResults",
                newName: "Status");

            migrationBuilder.AddColumn<double>(
                name: "Amount",
                table: "Transactions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "WalletId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "ServicePackages",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "ServicePackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "ServicePackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "WalletId",
                table: "ReviewerProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
                name: "LearnerProfileId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PriceReviewFee",
                table: "Purchases",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PriceServicePackage",
                table: "Purchases",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedSymbol",
                table: "PhonemeResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrderIndex",
                table: "PhonemeResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PhonemeId",
                table: "PhonemeResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Assessments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PhonemeTemplates",
                columns: table => new
                {
                    PhonemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhonemeTemplates", x => x.PhonemeId);
                    table.ForeignKey(
                        name: "FK_PhonemeTemplates_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StressResults",
                columns: table => new
                {
                    StressResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhonemeResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActualType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StressResults", x => x.StressResultId);
                    table.ForeignKey(
                        name: "FK_StressResults_PhonemeResults_PhonemeResultId",
                        column: x => x.PhonemeResultId,
                        principalTable: "PhonemeResults",
                        principalColumn: "PhonemeResultId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicePackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_LearnerProfiles_LearnerProfileId",
                        column: x => x.LearnerProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "LearnerProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "ServicePackageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubServicePackages",
                columns: table => new
                {
                    SubServicePackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicePackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubServicePackages", x => x.SubServicePackageId);
                    table.ForeignKey(
                        name: "FK_SubServicePackages_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "ServicePackageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.WalletId);
                });

            migrationBuilder.CreateTable(
                name: "Stresses",
                columns: table => new
                {
                    StressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhonemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StressType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SyllableIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stresses", x => x.StressId);
                    table.ForeignKey(
                        name: "FK_Stresses_PhonemeTemplates_PhonemeId",
                        column: x => x.PhonemeId,
                        principalTable: "PhonemeTemplates",
                        principalColumn: "PhonemeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 6, 17, 46, 198, DateTimeKind.Utc).AddTicks(7891));

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletId",
                table: "Transactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerProfiles_WalletId",
                table: "ReviewerProfiles",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_SubscriptionId",
                table: "Purchases",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhonemeResults_PhonemeId",
                table: "PhonemeResults",
                column: "PhonemeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhonemeTemplates_QuestionId",
                table: "PhonemeTemplates",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Stresses_PhonemeId",
                table: "Stresses",
                column: "PhonemeId");

            migrationBuilder.CreateIndex(
                name: "IX_StressResults_PhonemeResultId",
                table: "StressResults",
                column: "PhonemeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_LearnerProfileId",
                table: "Subscriptions",
                column: "LearnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ServicePackageId",
                table: "Subscriptions",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_SubServicePackages_ServicePackageId",
                table: "SubServicePackages",
                column: "ServicePackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhonemeResults_PhonemeTemplates_PhonemeId",
                table: "PhonemeResults",
                column: "PhonemeId",
                principalTable: "PhonemeTemplates",
                principalColumn: "PhonemeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Subscriptions_SubscriptionId",
                table: "Purchases",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewerProfiles_Wallets_WalletId",
                table: "ReviewerProfiles",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_WalletId",
                table: "Transactions",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
