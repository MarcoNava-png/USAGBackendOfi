using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContactoEmergenciaToPersona : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreContactoEmergencia",
                table: "Persona",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentescoContactoEmergencia",
                table: "Persona",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoContactoEmergencia",
                table: "Persona",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreContactoEmergencia",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "ParentescoContactoEmergencia",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "TelefonoContactoEmergencia",
                table: "Persona");
        }
    }
}
