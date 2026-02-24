using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailToventa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailCliente",
                table: "Ventas",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailCliente",
                table: "Ventas");
        }
    }
}
