using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaAsistencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Asistencia",
                columns: table => new
                {
                    IdAsistencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InscripcionId = table.Column<int>(type: "int", nullable: false),
                    GrupoMateriaId = table.Column<int>(type: "int", nullable: false),
                    FechaSesion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoAsistencia = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProfesorRegistroId = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencia", x => x.IdAsistencia);
                    table.ForeignKey(
                        name: "FK_Asistencia_GrupoMateria",
                        column: x => x.GrupoMateriaId,
                        principalTable: "GrupoMateria",
                        principalColumn: "IdGrupoMateria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistencia_Inscripcion",
                        column: x => x.InscripcionId,
                        principalTable: "Inscripcion",
                        principalColumn: "IdInscripcion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistencia_Profesor",
                        column: x => x.ProfesorRegistroId,
                        principalTable: "Profesor",
                        principalColumn: "IdProfesor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_GrupoMateria_Fecha",
                table: "Asistencia",
                columns: new[] { "GrupoMateriaId", "FechaSesion" });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_Inscripcion_Fecha",
                table: "Asistencia",
                columns: new[] { "InscripcionId", "FechaSesion" });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_ProfesorRegistroId",
                table: "Asistencia",
                column: "ProfesorRegistroId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asistencia");
        }
    }
}
