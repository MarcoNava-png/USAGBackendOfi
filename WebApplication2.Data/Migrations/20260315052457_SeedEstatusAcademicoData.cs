using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class SeedEstatusAcademicoData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Estudiante SET EstatusAcademico = 7 WHERE Activo = 0 AND TipoBaja = 2");
            migrationBuilder.Sql("UPDATE Estudiante SET EstatusAcademico = 6 WHERE Activo = 0 AND (TipoBaja IS NULL OR TipoBaja = 1)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Estudiante SET EstatusAcademico = 2");
        }
    }
}
