using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
            name: "ProductoId",
            table: "VentaDetalle",
            newName: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalle_ProductId",
                table: "VentaDetalle",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_VentaDetalle_Products_ProductId",
                table: "VentaDetalle",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Primero deshacemos lo que se añadió al final del Up
            migrationBuilder.DropForeignKey(
                name: "FK_VentaDetalle_Products_ProductId",
                table: "VentaDetalle");

            migrationBuilder.DropIndex(
                name: "IX_VentaDetalle_ProductId",
                table: "VentaDetalle");

            // 2. Por último, revertimos el nombre: de "ProductId" a "ProductoId"
            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "VentaDetalle",
                newName: "ProductoId");
        }
    }
}
