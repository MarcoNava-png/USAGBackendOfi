using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class a : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aspirante_Horario_HorarioId",
                table: "Aspirante");

            migrationBuilder.AlterColumn<int>(
                name: "HorarioId",
                table: "Aspirante",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Aspirante_Horario_HorarioId",
                table: "Aspirante",
                column: "HorarioId",
                principalTable: "Horario",
                principalColumn: "IdHorario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aspirante_Horario_HorarioId",
                table: "Aspirante");

            migrationBuilder.AlterColumn<int>(
                name: "HorarioId",
                table: "Aspirante",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Aspirante_Horario_HorarioId",
                table: "Aspirante",
                column: "HorarioId",
                principalTable: "Horario",
                principalColumn: "IdHorario",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
