using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixGrupoUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar el índice único existente que no incluye IdTurno
            migrationBuilder.DropIndex(
                name: "UQ_Grupo_Num",
                table: "Grupo");

            // Crear el nuevo índice único que incluye IdTurno
            // Esto permite tener grupos con el mismo número en diferentes turnos
            migrationBuilder.CreateIndex(
                name: "UQ_Grupo_Num",
                table: "Grupo",
                columns: new[] { "IdPlanEstudios", "IdPeriodoAcademico", "NumeroCuatrimestre", "NumeroGrupo", "IdTurno" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir al índice original sin IdTurno
            migrationBuilder.DropIndex(
                name: "UQ_Grupo_Num",
                table: "Grupo");

            migrationBuilder.CreateIndex(
                name: "UQ_Grupo_Num",
                table: "Grupo",
                columns: new[] { "IdPlanEstudios", "IdPeriodoAcademico", "NumeroCuatrimestre", "NumeroGrupo" },
                unique: true);
        }
    }
}
