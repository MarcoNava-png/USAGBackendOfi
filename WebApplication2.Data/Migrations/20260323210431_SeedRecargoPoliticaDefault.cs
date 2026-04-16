using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class SeedRecargoPoliticaDefault : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO RecargoPolitica (TasaDiaria, DiaInicioGracia, DiaFinGracia, RecargoMinimo, RecargoMaximo, TopeDiasMora, Activo, DiaInicioRecargoPorcentaje, DiaFinRecargoPorcentaje, PorcentajeRecargo, DiaInicioCargoDiario, CargoDiarioFijo)
                VALUES (0, 1, 5, NULL, NULL, NULL, 1, 6, 15, 0.05, 16, 20)
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM RecargoPolitica");
        }
    }
}
