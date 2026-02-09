using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class aspirante_horario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HorarioId",
                table: "Aspirante",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_HorarioId",
                table: "Aspirante",
                column: "HorarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Aspirante_Horario_HorarioId",
                table: "Aspirante",
                column: "HorarioId",
                principalTable: "Horario",
                principalColumn: "IdHorario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aspirante_Horario_HorarioId",
                table: "Aspirante");

            migrationBuilder.DropIndex(
                name: "IX_Aspirante_HorarioId",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "HorarioId",
                table: "Aspirante");
        }
    }
}
