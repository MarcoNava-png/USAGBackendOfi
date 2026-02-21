using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodoAspiranteYPlanModalidadDia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdPeriodoAcademico",
                table: "Aspirante",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlanModalidadDia",
                columns: table => new
                {
                    IdPlanModalidadDia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: false),
                    IdModalidad = table.Column<int>(type: "int", nullable: false),
                    IdDiaSemana = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanModalidadDia", x => x.IdPlanModalidadDia);
                    table.ForeignKey(
                        name: "FK_PlanModalidadDia_DiaSemana",
                        column: x => x.IdDiaSemana,
                        principalTable: "DiaSemana",
                        principalColumn: "IdDiaSemana");
                    table.ForeignKey(
                        name: "FK_PlanModalidadDia_Modalidad",
                        column: x => x.IdModalidad,
                        principalTable: "Modalidad",
                        principalColumn: "IdModalidad");
                    table.ForeignKey(
                        name: "FK_PlanModalidadDia_Plan",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdPeriodoAcademico",
                table: "Aspirante",
                column: "IdPeriodoAcademico");

            migrationBuilder.CreateIndex(
                name: "IX_PlanModalidadDia_IdDiaSemana",
                table: "PlanModalidadDia",
                column: "IdDiaSemana");

            migrationBuilder.CreateIndex(
                name: "IX_PlanModalidadDia_IdModalidad",
                table: "PlanModalidadDia",
                column: "IdModalidad");

            migrationBuilder.CreateIndex(
                name: "UQ_PlanModalidadDia",
                table: "PlanModalidadDia",
                columns: new[] { "IdPlanEstudios", "IdModalidad", "IdDiaSemana" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Aspirante_PeriodoAcademico",
                table: "Aspirante",
                column: "IdPeriodoAcademico",
                principalTable: "PeriodoAcademico",
                principalColumn: "IdPeriodoAcademico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aspirante_PeriodoAcademico",
                table: "Aspirante");

            migrationBuilder.DropTable(
                name: "PlanModalidadDia");

            migrationBuilder.DropIndex(
                name: "IX_Aspirante_IdPeriodoAcademico",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "IdPeriodoAcademico",
                table: "Aspirante");
        }
    }
}
