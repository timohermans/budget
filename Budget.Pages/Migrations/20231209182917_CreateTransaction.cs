using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Budget.Pages.Migrations
{
    /// <inheritdoc />
    public partial class CreateTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FollowNumber = table.Column<int>(type: "int", nullable: false),
                    Iban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    DateTransaction = table.Column<DateOnly>(type: "date", nullable: false),
                    BalanceAfterTransaction = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    NameOtherParty = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IbanOtherParty = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    AuthorizationCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FollowNumber_Iban",
                table: "Transactions",
                columns: new[] { "FollowNumber", "Iban" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
