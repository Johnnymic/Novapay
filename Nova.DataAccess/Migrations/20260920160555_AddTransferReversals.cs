using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nova.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferReversals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_Reference",
                table: "WalletTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_Reference",
                table: "WalletTransactions",
                column: "Reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_Reference",
                table: "WalletTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_Reference",
                table: "WalletTransactions",
                column: "Reference",
                unique: true);
        }
    }
}
