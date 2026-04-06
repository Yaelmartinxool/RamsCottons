using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RamsCottons.Migrations
{
    /// <inheritdoc />
    public partial class FixApplicationUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_AspNetUsers_ApplicationUserId",
                table: "Clientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Telefono",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Etiquetas",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "VendedorAsignadoId",
                table: "Clientes");

            migrationBuilder.RenameTable(
                name: "Clientes",
                newName: "clientes");

            migrationBuilder.RenameColumn(
                name: "FechaInicio",
                table: "Promociones",
                newName: "Fechalnicio");

            migrationBuilder.RenameColumn(
                name: "Telefono",
                table: "clientes",
                newName: "telefono");

            migrationBuilder.RenameColumn(
                name: "FechaRegistro",
                table: "clientes",
                newName: "regtimestamp");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "clientes",
                newName: "id_folio");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_ApplicationUserId",
                table: "clientes",
                newName: "IX_clientes_ApplicationUserId");

            migrationBuilder.AddColumn<string>(
                name: "pdf_nombre",
                table: "Promociones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdf_ruta",
                table: "Promociones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdf_url",
                table: "Promociones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "telefono",
                table: "clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "correo",
                table: "clientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "estado",
                table: "clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_categoriaProducto",
                table: "clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_generador_qr",
                table: "clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_sucursal",
                table: "clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "id_usuario",
                table: "clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "nombre_completo",
                table: "clientes",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_clientes",
                table: "clientes",
                column: "id_folio");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_telefono",
                table: "clientes",
                column: "telefono",
                unique: true,
                filter: "[telefono] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_clientes_AspNetUsers_ApplicationUserId",
                table: "clientes",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_clientes_AspNetUsers_ApplicationUserId",
                table: "clientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_clientes",
                table: "clientes");

            migrationBuilder.DropIndex(
                name: "IX_clientes_telefono",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "pdf_nombre",
                table: "Promociones");

            migrationBuilder.DropColumn(
                name: "pdf_ruta",
                table: "Promociones");

            migrationBuilder.DropColumn(
                name: "pdf_url",
                table: "Promociones");

            migrationBuilder.DropColumn(
                name: "correo",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "id_categoriaProducto",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "id_generador_qr",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "id_sucursal",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "id_usuario",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "nombre_completo",
                table: "clientes");

            migrationBuilder.RenameTable(
                name: "clientes",
                newName: "Clientes");

            migrationBuilder.RenameColumn(
                name: "Fechalnicio",
                table: "Promociones",
                newName: "FechaInicio");

            migrationBuilder.RenameColumn(
                name: "telefono",
                table: "Clientes",
                newName: "Telefono");

            migrationBuilder.RenameColumn(
                name: "regtimestamp",
                table: "Clientes",
                newName: "FechaRegistro");

            migrationBuilder.RenameColumn(
                name: "id_folio",
                table: "Clientes",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_clientes_ApplicationUserId",
                table: "Clientes",
                newName: "IX_Clientes_ApplicationUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Etiquetas",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Clientes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendedorAsignadoId",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Telefono",
                table: "Clientes",
                column: "Telefono",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_AspNetUsers_ApplicationUserId",
                table: "Clientes",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
