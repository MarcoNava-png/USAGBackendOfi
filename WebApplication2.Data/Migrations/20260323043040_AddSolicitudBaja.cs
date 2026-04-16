using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitudBaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesBaja",
                columns: table => new
                {
                    IdSolicitudBaja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    Matricula = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreEstudiante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Carrera = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoBaja = table.Column<int>(type: "int", nullable: true),
                    EstadoBaja = table.Column<int>(type: "int", nullable: true),
                    MotivoBaja = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MontoAdeudo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecibosVencidos = table.Column<int>(type: "int", nullable: false),
                    RecibosPendientes = table.Column<int>(type: "int", nullable: false),
                    EstatusSolicitud = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SolicitadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutorizadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComentarioFinanzas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaAutorizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesBaja", x => x.IdSolicitudBaja);
                    table.ForeignKey(
                        name: "FK_SolicitudesBaja_Estudiante_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesBaja_IdEstudiante",
                table: "SolicitudesBaja",
                column: "IdEstudiante");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesBaja");
        }
    }
}
