using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposBajaEstudiante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoBaja",
                table: "Estudiante",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaBaja",
                table: "Estudiante",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoBaja",
                table: "Estudiante",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoBaja",
                table: "Estudiante",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoBaja",
                table: "Estudiante");

            migrationBuilder.DropColumn(
                name: "FechaBaja",
                table: "Estudiante");

            migrationBuilder.DropColumn(
                name: "MotivoBaja",
                table: "Estudiante");

            migrationBuilder.DropColumn(
                name: "TipoBaja",
                table: "Estudiante");
        }
    }
}
