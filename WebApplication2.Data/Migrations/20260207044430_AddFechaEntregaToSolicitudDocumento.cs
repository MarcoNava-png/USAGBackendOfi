using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaEntregaToSolicitudDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntrega",
                table: "SolicitudesDocumento",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioEntrega",
                table: "SolicitudesDocumento",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaProrroga",
                table: "AspiranteDocumento",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaProrrogaAsignada",
                table: "AspiranteDocumento",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoProrroga",
                table: "AspiranteDocumento",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioProrroga",
                table: "AspiranteDocumento",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaEntrega",
                table: "SolicitudesDocumento");

            migrationBuilder.DropColumn(
                name: "UsuarioEntrega",
                table: "SolicitudesDocumento");

            migrationBuilder.DropColumn(
                name: "FechaProrroga",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "FechaProrrogaAsignada",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "MotivoProrroga",
                table: "AspiranteDocumento");

            migrationBuilder.DropColumn(
                name: "UsuarioProrroga",
                table: "AspiranteDocumento");
        }
    }
}
