using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class turno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aspirante_Horario_HorarioId",
                table: "Aspirante");

            migrationBuilder.RenameColumn(
                name: "HorarioId",
                table: "Aspirante",
                newName: "TurnoId");

            migrationBuilder.RenameIndex(
                name: "IX_Aspirante_HorarioId",
                table: "Aspirante",
                newName: "IX_Aspirante_TurnoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Aspirante_Turno_TurnoId",
                table: "Aspirante",
                column: "TurnoId",
                principalTable: "Turno",
                principalColumn: "IdTurno");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aspirante_Turno_TurnoId",
                table: "Aspirante");

            migrationBuilder.RenameColumn(
                name: "TurnoId",
                table: "Aspirante",
                newName: "HorarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Aspirante_TurnoId",
                table: "Aspirante",
                newName: "IX_Aspirante_HorarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Aspirante_Horario_HorarioId",
                table: "Aspirante",
                column: "HorarioId",
                principalTable: "Horario",
                principalColumn: "IdHorario");
        }
    }
}
