using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class RemoveIMEAEFromLicIMEA : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM MateriaPlan
                WHERE IdPlanEstudios = 53
                AND IdMateria IN (SELECT IdMateria FROM Materia WHERE Clave LIKE 'IMEAE%')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
