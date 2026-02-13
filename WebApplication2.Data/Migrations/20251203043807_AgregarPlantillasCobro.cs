using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPlantillasCobro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tablas e índices ya creados por migración 20251202215442_AgregarColumnasPago
            // Esta migración se deja vacía para evitar conflictos de duplicación
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantillasCobroDetalles");

            migrationBuilder.DropTable(
                name: "PlantillasCobro");
        }
    }
}
