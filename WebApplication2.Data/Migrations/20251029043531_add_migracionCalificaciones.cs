using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_migracionCalificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalificacionesParciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrupoMateriaId = table.Column<int>(type: "int", nullable: false),
                    ParcialId = table.Column<int>(type: "int", nullable: false),
                    InscripcionId = table.Column<int>(type: "int", nullable: false),
                    ProfesorId = table.Column<int>(type: "int", nullable: false),
                    StatusParcial = table.Column<int>(type: "int", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionesParciales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalificacionesParciales_GrupoMateria_GrupoMateriaId",
                        column: x => x.GrupoMateriaId,
                        principalTable: "GrupoMateria",
                        principalColumn: "IdGrupoMateria",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesParciales_Inscripcion_InscripcionId",
                        column: x => x.InscripcionId,
                        principalTable: "Inscripcion",
                        principalColumn: "IdInscripcion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesParciales_Parciales_ParcialId",
                        column: x => x.ParcialId,
                        principalTable: "Parciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesParciales_Profesor_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Profesor",
                        principalColumn: "IdProfesor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalificacionDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalificacionParcialId = table.Column<int>(type: "int", nullable: false),
                    GrupoMateriaId = table.Column<int>(type: "int", nullable: false),
                    TipoEvaluacionEnum = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    PesoEvaluacion = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    MaxPuntos = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    FechaAplicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Puntos = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ApplicationUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCaptura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalificacionDetalle_CalificacionesParciales_CalificacionParcialId",
                        column: x => x.CalificacionParcialId,
                        principalTable: "CalificacionesParciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionDetalle_CalificacionParcialId",
                table: "CalificacionDetalle",
                column: "CalificacionParcialId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesParciales_GrupoMateriaId",
                table: "CalificacionesParciales",
                column: "GrupoMateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesParciales_InscripcionId",
                table: "CalificacionesParciales",
                column: "InscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesParciales_ParcialId",
                table: "CalificacionesParciales",
                column: "ParcialId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesParciales_ProfesorId",
                table: "CalificacionesParciales",
                column: "ProfesorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalificacionDetalle");

            migrationBuilder.DropTable(
                name: "CalificacionesParciales");
        }
    }
}
