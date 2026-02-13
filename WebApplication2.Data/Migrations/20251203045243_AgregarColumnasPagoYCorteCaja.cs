using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarColumnasPagoYCorteCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tablas y columnas ya creadas por migración 20251202215442_AgregarColumnasPago
            // Esta migración se deja vacía para evitar conflictos de duplicación
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorteCaja");

            migrationBuilder.DropColumn(
                name: "FolioPago",
                table: "Pago");

            migrationBuilder.DropColumn(
                name: "IdCaja",
                table: "Pago");

            migrationBuilder.DropColumn(
                name: "IdCorteCaja",
                table: "Pago");

            migrationBuilder.DropColumn(
                name: "IdUsuarioCaja",
                table: "Pago");
        }
    }
}
