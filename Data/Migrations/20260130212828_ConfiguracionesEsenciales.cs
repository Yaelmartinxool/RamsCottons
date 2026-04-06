using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RamsCottons.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguracionesEsenciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TokenUnico",
                table: "Promociones",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_TokenUnico",
                table: "Promociones",
                column: "TokenUnico",
                unique: true,
                filter: "[TokenUnico] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Telefono",
                table: "Clientes",
                column: "Telefono",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Promociones_TokenUnico",
                table: "Promociones");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Telefono",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "TokenUnico",
                table: "Promociones",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
