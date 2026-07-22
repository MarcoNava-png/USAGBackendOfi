using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProfesorOpcionalEnParcial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalificacionesParciales_Profesor_ProfesorId",
                table: "CalificacionesParciales");

            migrationBuilder.AlterColumn<int>(
                name: "ProfesorId",
                table: "CalificacionesParciales",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_CalificacionesParciales_Profesor_ProfesorId",
                table: "CalificacionesParciales",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "IdProfesor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalificacionesParciales_Profesor_ProfesorId",
                table: "CalificacionesParciales");

            migrationBuilder.AlterColumn<int>(
                name: "ProfesorId",
                table: "CalificacionesParciales",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CalificacionesParciales_Profesor_ProfesorId",
                table: "CalificacionesParciales",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "IdProfesor",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
