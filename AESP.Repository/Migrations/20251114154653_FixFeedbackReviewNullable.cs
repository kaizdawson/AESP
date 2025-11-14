using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixFeedbackReviewNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Records_LearnerRecordCategories_LearnerRecordCategoryId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ReviewFees");

            migrationBuilder.RenameColumn(
                name: "LearnerRecordCategoryId",
                table: "Records",
                newName: "LearnerRecordId");

            migrationBuilder.RenameIndex(
                name: "IX_Records_LearnerRecordCategoryId",
                table: "Records",
                newName: "IX_Records_LearnerRecordId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Purchases",
                newName: "AIConversationChargeId");

            migrationBuilder.RenameColumn(
                name: "LearnerRecordCategoryId",
                table: "LearnerRecordCategories",
                newName: "LearnerRecordId");

            migrationBuilder.AlterColumn<Guid>(
                name: "RecordId",
                table: "Reviews",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "LearnerAnswerId",
                table: "Reviews",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfReview",
                table: "ReviewFees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsNeedReviewed",
                table: "Records",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfReview",
                table: "Records",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReviewId",
                table: "Feedbacks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

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
                name: "ReviewFeeDetail",
                columns: table => new
                {
                    ReviewFeeDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PricePerReviewFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PriceSystem = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayReviewer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReviewFeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewFeeDetail", x => x.ReviewFeeDetailId);
                    table.ForeignKey(
                        name: "FK_ReviewFeeDetail_ReviewFees_ReviewFeeId",
                        column: x => x.ReviewFeeId,
                        principalTable: "ReviewFees",
                        principalColumn: "ReviewFeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 15, 46, 52, 444, DateTimeKind.Utc).AddTicks(9259), new DateTime(2025, 11, 14, 15, 46, 52, 444, DateTimeKind.Utc).AddTicks(9261) });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_AIConversationChargeId",
                table: "Purchases",
                column: "AIConversationChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewFeeDetail_ReviewFeeId",
                table: "ReviewFeeDetail",
                column: "ReviewFeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_AIConversationCharge_AIConversationChargeId",
                table: "Purchases",
                column: "AIConversationChargeId",
                principalTable: "AIConversationCharge",
                principalColumn: "AIConversationChargeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Records_LearnerRecordCategories_LearnerRecordId",
                table: "Records",
                column: "LearnerRecordId",
                principalTable: "LearnerRecordCategories",
                principalColumn: "LearnerRecordId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_AIConversationCharge_AIConversationChargeId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Records_LearnerRecordCategories_LearnerRecordId",
                table: "Records");

            migrationBuilder.DropTable(
                name: "ReviewFeeDetail");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_AIConversationChargeId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "NumberOfReview",
                table: "ReviewFees");

            migrationBuilder.DropColumn(
                name: "IsNeedReviewed",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "NumberOfReview",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "AIConversationCharge");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "AIConversationCharge");

            migrationBuilder.RenameColumn(
                name: "LearnerRecordId",
                table: "Records",
                newName: "LearnerRecordCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Records_LearnerRecordId",
                table: "Records",
                newName: "IX_Records_LearnerRecordCategoryId");

            migrationBuilder.RenameColumn(
                name: "AIConversationChargeId",
                table: "Purchases",
                newName: "ItemId");

            migrationBuilder.RenameColumn(
                name: "LearnerRecordId",
                table: "LearnerRecordCategories",
                newName: "LearnerRecordCategoryId");

            migrationBuilder.AlterColumn<Guid>(
                name: "RecordId",
                table: "Reviews",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LearnerAnswerId",
                table: "Reviews",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "ReviewFees",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReviewId",
                table: "Feedbacks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

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
                values: new object[] { new DateTime(2025, 11, 14, 8, 5, 5, 365, DateTimeKind.Utc).AddTicks(2156), new DateTime(2025, 11, 14, 8, 5, 5, 365, DateTimeKind.Utc).AddTicks(2162) });

            migrationBuilder.AddForeignKey(
                name: "FK_Records_LearnerRecordCategories_LearnerRecordCategoryId",
                table: "Records",
                column: "LearnerRecordCategoryId",
                principalTable: "LearnerRecordCategories",
                principalColumn: "LearnerRecordCategoryId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
