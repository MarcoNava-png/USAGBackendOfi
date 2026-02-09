using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class relation_estudiante_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Estudiante",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estudiante_UsuarioId",
                table: "Estudiante",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Estudiante_AspNetUsers_UsuarioId",
                table: "Estudiante",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estudiante_AspNetUsers_UsuarioId",
                table: "Estudiante");

            migrationBuilder.DropIndex(
                name: "IX_Estudiante_UsuarioId",
                table: "Estudiante");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Estudiante");
        }
    }
}
