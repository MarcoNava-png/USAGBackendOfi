using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropUniqueCorreoPersona : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Persona_Email",
                table: "Persona");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_Correo",
                table: "Persona",
                column: "Correo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persona_Correo",
                table: "Persona");

            migrationBuilder.CreateIndex(
                name: "UQ_Persona_Email",
                table: "Persona",
                column: "Correo",
                unique: true);
        }
    }
}
