using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class UpperCaseNombresPlanEstudios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE PlanEstudios SET NombrePlanEstudios = UPPER(NombrePlanEstudios) WHERE NombrePlanEstudios IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
