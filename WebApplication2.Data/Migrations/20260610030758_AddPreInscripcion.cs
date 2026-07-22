using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPreInscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreInscripcion",
                columns: table => new
                {
                    IdPreInscripcion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: false),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: false),
                    IdPeriodoAcademicoDestino = table.Column<int>(type: "integer", nullable: false),
                    NumeroCuatrimestreObjetivo = table.Column<byte>(type: "smallint", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "Pendiente"),
                    Nota = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaApartado = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreInscripcion", x => x.IdPreInscripcion);
                    table.ForeignKey(
                        name: "FK_PreInscripcion_Estudiante_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreInscripcion_PeriodoAcademico_IdPeriodoAcademicoDestino",
                        column: x => x.IdPeriodoAcademicoDestino,
                        principalTable: "PeriodoAcademico",
                        principalColumn: "IdPeriodoAcademico",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreInscripcion_PlanEstudios_IdPlanEstudios",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreInscripcion_IdEstudiante_IdPeriodoAcademicoDestino",
                table: "PreInscripcion",
                columns: new[] { "IdEstudiante", "IdPeriodoAcademicoDestino" },
                unique: true,
                filter: "\"Status\" <> 0 AND \"Estado\" = 'Pendiente'");

            migrationBuilder.CreateIndex(
                name: "IX_PreInscripcion_IdPeriodoAcademicoDestino",
                table: "PreInscripcion",
                column: "IdPeriodoAcademicoDestino");

            migrationBuilder.CreateIndex(
                name: "IX_PreInscripcion_IdPlanEstudios",
                table: "PreInscripcion",
                column: "IdPlanEstudios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreInscripcion");
        }
    }
}
