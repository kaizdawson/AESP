using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AESP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReviewFeeDetailNewField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceSystem",
                table: "ReviewFeeDetail",
                newName: "PercentOfSystem");

            migrationBuilder.RenameColumn(
                name: "PayReviewer",
                table: "ReviewFeeDetail",
                newName: "PercentOfReviewer");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 11, 13, 2, 948, DateTimeKind.Utc).AddTicks(7164), new DateTime(2025, 11, 16, 11, 13, 2, 948, DateTimeKind.Utc).AddTicks(7167) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PercentOfSystem",
                table: "ReviewFeeDetail",
                newName: "PriceSystem");

            migrationBuilder.RenameColumn(
                name: "PercentOfReviewer",
                table: "ReviewFeeDetail",
                newName: "PayReviewer");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 9, 34, 30, 308, DateTimeKind.Utc).AddTicks(4441), new DateTime(2025, 11, 16, 9, 34, 30, 308, DateTimeKind.Utc).AddTicks(4444) });
        }
    }
}
