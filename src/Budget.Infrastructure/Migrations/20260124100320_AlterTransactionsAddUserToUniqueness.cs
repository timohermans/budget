using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Budget.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterTransactionsAddUserToUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_FollowNumber_Iban",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FollowNumber_Iban_User",
                table: "Transactions",
                columns: new[] { "FollowNumber", "Iban", "User" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_FollowNumber_Iban_User",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FollowNumber_Iban",
                table: "Transactions",
                columns: new[] { "FollowNumber", "Iban" },
                unique: true);
        }
    }
}
