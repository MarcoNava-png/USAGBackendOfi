using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTarifaAdmision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TarifasAdmision",
                columns: table => new
                {
                    IdTarifaAdmision = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AplicaConvenioMensualidad = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarifasAdmision", x => x.IdTarifaAdmision);
                    table.ForeignKey(
                        name: "FK_TarifasAdmision_PlanEstudios_IdPlanEstudios",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TarifasAdmisionDetalles",
                columns: table => new
                {
                    IdTarifaAdmisionDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTarifaAdmision = table.Column<int>(type: "int", nullable: false),
                    IdConceptoPago = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    EsAplicable = table.Column<bool>(type: "bit", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarifasAdmisionDetalles", x => x.IdTarifaAdmisionDetalle);
                    table.ForeignKey(
                        name: "FK_TarifasAdmisionDetalles_ConceptoPago_IdConceptoPago",
                        column: x => x.IdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TarifasAdmisionDetalles_TarifasAdmision_IdTarifaAdmision",
                        column: x => x.IdTarifaAdmision,
                        principalTable: "TarifasAdmision",
                        principalColumn: "IdTarifaAdmision",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TarifasAdmision_IdPlanEstudios",
                table: "TarifasAdmision",
                column: "IdPlanEstudios");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasAdmision_IdPlanEstudios_Activo",
                table: "TarifasAdmision",
                columns: new[] { "IdPlanEstudios", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_TarifasAdmisionDetalles_IdConceptoPago",
                table: "TarifasAdmisionDetalles",
                column: "IdConceptoPago");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasAdmisionDetalles_IdTarifaAdmision",
                table: "TarifasAdmisionDetalles",
                column: "IdTarifaAdmision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TarifasAdmisionDetalles");

            migrationBuilder.DropTable(
                name: "TarifasAdmision");
        }
    }
}
