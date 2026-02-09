using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class relation_profesor_usuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Profesor",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profesor_UsuarioId",
                table: "Profesor",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profesor_AspNetUsers_UsuarioId",
                table: "Profesor",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profesor_AspNetUsers_UsuarioId",
                table: "Profesor");

            migrationBuilder.DropIndex(
                name: "IX_Profesor_UsuarioId",
                table: "Profesor");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Profesor");
        }
    }
}
