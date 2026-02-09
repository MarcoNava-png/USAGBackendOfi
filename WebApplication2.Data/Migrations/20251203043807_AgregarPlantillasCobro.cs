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
            migrationBuilder.CreateTable(
                name: "PlantillasCobro",
                columns: table => new
                {
                    IdPlantillaCobro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombrePlantilla = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: false),
                    NumeroCuatrimestre = table.Column<int>(type: "int", nullable: false),
                    IdPeriodoAcademico = table.Column<int>(type: "int", nullable: true),
                    IdTurno = table.Column<int>(type: "int", nullable: true),
                    IdModalidad = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    EsActiva = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaVigenciaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVigenciaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstrategiaEmision = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    NumeroRecibos = table.Column<int>(type: "int", nullable: false, defaultValue: 4),
                    DiaVencimiento = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasCobro", x => x.IdPlantillaCobro);
                    table.ForeignKey(
                        name: "FK_PlantillasCobro_PlanEstudios_IdPlanEstudios",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlantillasCobroDetalles",
                columns: table => new
                {
                    IdPlantillaDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPlantillaCobro = table.Column<int>(type: "int", nullable: false),
                    IdConceptoPago = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false, defaultValue: 1m),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    AplicaEnRecibo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasCobroDetalles", x => x.IdPlantillaDetalle);
                    table.ForeignKey(
                        name: "FK_PlantillasCobroDetalles_PlantillasCobro_IdPlantillaCobro",
                        column: x => x.IdPlantillaCobro,
                        principalTable: "PlantillasCobro",
                        principalColumn: "IdPlantillaCobro",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlantillasCobroDetalles_ConceptoPago_IdConceptoPago",
                        column: x => x.IdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago",
                        onDelete: ReferentialAction.Restrict);
                });

            // Índices para PlantillasCobro
            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobro_IdPlanEstudios",
                table: "PlantillasCobro",
                column: "IdPlanEstudios");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobro_EsActiva",
                table: "PlantillasCobro",
                column: "EsActiva");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobro_PlanCuatrimestre",
                table: "PlantillasCobro",
                columns: new[] { "IdPlanEstudios", "NumeroCuatrimestre", "EsActiva" });

            // Índices para PlantillasCobroDetalles
            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobroDetalles_IdPlantillaCobro",
                table: "PlantillasCobroDetalles",
                column: "IdPlantillaCobro");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobroDetalles_IdConceptoPago",
                table: "PlantillasCobroDetalles",
                column: "IdConceptoPago");
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
