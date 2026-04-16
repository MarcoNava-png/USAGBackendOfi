using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitudPlanEstudios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesPlanEstudios",
                columns: table => new
                {
                    IdSolicitudPlanEstudios = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: false),
                    ClavePlanEstudios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombrePlanEstudios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Campus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rvoe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstatusSolicitud = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SolicitadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AprobadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComentarioRevision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesPlanEstudios", x => x.IdSolicitudPlanEstudios);
                    table.ForeignKey(
                        name: "FK_SolicitudesPlanEstudios_PlanEstudios_IdPlanEstudios",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPlanEstudios_IdPlanEstudios",
                table: "SolicitudesPlanEstudios",
                column: "IdPlanEstudios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesPlanEstudios");
        }
    }
}
