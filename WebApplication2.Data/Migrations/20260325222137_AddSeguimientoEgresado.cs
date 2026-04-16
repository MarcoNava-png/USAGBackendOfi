using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeguimientoEgresado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeguimientoEgresados",
                columns: table => new
                {
                    IdSeguimientoEgresado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEstudiante = table.Column<int>(type: "int", nullable: true),
                    Matricula = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgramaAcademico = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expediente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PagoTitulacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LiberacionServicioSocial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaSolicitudTitulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstatusTitulacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstatusCertificado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstatusTituloElectronico = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstatusTituloFisico = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PagoCedula = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TramiteCedula = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroProgramaAcademico = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeguimientoEgresados", x => x.IdSeguimientoEgresado);
                    table.ForeignKey(
                        name: "FK_SeguimientoEgresados_Estudiante_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeguimientoEgresados_IdEstudiante",
                table: "SeguimientoEgresados",
                column: "IdEstudiante");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeguimientoEgresados");
        }
    }
}
