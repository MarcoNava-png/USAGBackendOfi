using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_indices_to_persona : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Persona_ApellidoMaterno",
                table: "Persona",
                column: "ApellidoMaterno");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_ApellidoPaterno",
                table: "Persona",
                column: "ApellidoPaterno");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_Curp",
                table: "Persona",
                column: "Curp");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_Nombre",
                table: "Persona",
                column: "Nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persona_ApellidoMaterno",
                table: "Persona");

            migrationBuilder.DropIndex(
                name: "IX_Persona_ApellidoPaterno",
                table: "Persona");

            migrationBuilder.DropIndex(
                name: "IX_Persona_Curp",
                table: "Persona");

            migrationBuilder.DropIndex(
                name: "IX_Persona_Nombre",
                table: "Persona");
        }
    }
}
