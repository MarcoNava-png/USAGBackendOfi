using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposSocioeconomicosAspirante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nacionalidad",
                table: "Persona",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DomicilioEmpresa",
                table: "Aspirante",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modalidad",
                table: "Aspirante",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreEmpresa",
                table: "Aspirante",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Preparatoria",
                table: "Aspirante",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PuestoEmpresa",
                table: "Aspirante",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuienCubreGastos",
                table: "Aspirante",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RecorridoPlantel",
                table: "Aspirante",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Trabaja",
                table: "Aspirante",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nacionalidad",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "DomicilioEmpresa",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "Modalidad",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "NombreEmpresa",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "Preparatoria",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "PuestoEmpresa",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "QuienCubreGastos",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "RecorridoPlantel",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "Trabaja",
                table: "Aspirante");
        }
    }
}
