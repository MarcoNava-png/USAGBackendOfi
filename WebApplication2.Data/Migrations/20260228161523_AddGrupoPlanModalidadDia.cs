using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGrupoPlanModalidadDia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_PlanModalidadDia",
                table: "PlanModalidadDia");

            migrationBuilder.AddColumn<int>(
                name: "Grupo",
                table: "PlanModalidadDia",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "UQ_PlanModalidadDia",
                table: "PlanModalidadDia",
                columns: new[] { "IdPlanEstudios", "IdModalidad", "Grupo", "IdDiaSemana" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_PlanModalidadDia",
                table: "PlanModalidadDia");

            migrationBuilder.DropColumn(
                name: "Grupo",
                table: "PlanModalidadDia");

            migrationBuilder.CreateIndex(
                name: "UQ_PlanModalidadDia",
                table: "PlanModalidadDia",
                columns: new[] { "IdPlanEstudios", "IdModalidad", "IdDiaSemana" },
                unique: true);
        }
    }
}
