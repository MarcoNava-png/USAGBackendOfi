using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEntregadoAspiranteDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Entregado",
                table: "AspiranteDocumento",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntrega",
                table: "AspiranteDocumento",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioEntrega",
                table: "AspiranteDocumento",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Entregado",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "FechaEntrega",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "UsuarioEntrega",
                table: "AspiranteDocumento");
        }
    }
}
