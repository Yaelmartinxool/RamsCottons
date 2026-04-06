using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RamsCottons.Migrations
{
    public partial class SincronizacionEstadoActual : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Solo dejamos cambios reales que sí necesitas
            migrationBuilder.AlterColumn<string>(
                name: "id_usuario",
                table: "clientes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "id_usuario",
                table: "clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);
        }
    }
}