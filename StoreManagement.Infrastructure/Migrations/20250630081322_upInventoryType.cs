using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class upInventoryType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_InventoryTransactionTypes_TransactionTypeId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_ProductVariants_ProductVariantId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_TransactionTypeId",
                table: "InventoryTransactions");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_ProductVariants_ProductVariantId",
                table: "InventoryTransactions",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_ProductVariants_ProductVariantId",
                table: "InventoryTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TransactionTypeId",
                table: "InventoryTransactions",
                column: "TransactionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_InventoryTransactionTypes_TransactionTypeId",
                table: "InventoryTransactions",
                column: "TransactionTypeId",
                principalTable: "InventoryTransactionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_ProductVariants_ProductVariantId",
                table: "InventoryTransactions",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
