using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVentanaCaptura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesProrrogaCaptura",
                columns: table => new
                {
                    IdSolicitudProrroga = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdProfesor = table.Column<int>(type: "integer", nullable: false),
                    IdGrupoMateria = table.Column<int>(type: "integer", nullable: false),
                    NumeroParcial = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "text", nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: true),
                    FechaLimiteProrroga = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResueltaPor = table.Column<string>(type: "text", nullable: true),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NotaResolucion = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesProrrogaCaptura", x => x.IdSolicitudProrroga);
                    table.ForeignKey(
                        name: "FK_SolicitudesProrrogaCaptura_GrupoMateria_IdGrupoMateria",
                        column: x => x.IdGrupoMateria,
                        principalTable: "GrupoMateria",
                        principalColumn: "IdGrupoMateria");
                    table.ForeignKey(
                        name: "FK_SolicitudesProrrogaCaptura_Profesor_IdProfesor",
                        column: x => x.IdProfesor,
                        principalTable: "Profesor",
                        principalColumn: "IdProfesor");
                });

            migrationBuilder.CreateTable(
                name: "VentanasCaptura",
                columns: table => new
                {
                    IdVentanaCaptura = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPeriodoAcademico = table.Column<int>(type: "integer", nullable: false),
                    NumeroParcial = table.Column<int>(type: "integer", nullable: false),
                    Abierta = table.Column<bool>(type: "boolean", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaLimite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentanasCaptura", x => x.IdVentanaCaptura);
                    table.ForeignKey(
                        name: "FK_VentanasCaptura_PeriodoAcademico_IdPeriodoAcademico",
                        column: x => x.IdPeriodoAcademico,
                        principalTable: "PeriodoAcademico",
                        principalColumn: "IdPeriodoAcademico");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesProrrogaCaptura_IdGrupoMateria",
                table: "SolicitudesProrrogaCaptura",
                column: "IdGrupoMateria");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesProrrogaCaptura_IdProfesor",
                table: "SolicitudesProrrogaCaptura",
                column: "IdProfesor");

            migrationBuilder.CreateIndex(
                name: "IX_VentanasCaptura_IdPeriodoAcademico_NumeroParcial",
                table: "VentanasCaptura",
                columns: new[] { "IdPeriodoAcademico", "NumeroParcial" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesProrrogaCaptura");

            migrationBuilder.DropTable(
                name: "VentanasCaptura");
        }
    }
}
