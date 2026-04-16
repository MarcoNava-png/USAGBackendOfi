using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class SeedCatalogosSEP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO CatalogoCargoSEP (IdCargo, Descripcion, Activo) VALUES
                ('0', 'Secretario de Educación Pública', 1),
                ('1', 'Director', 1),
                ('2', 'Subdirector', 1),
                ('3', 'Rector', 1),
                ('4', 'Vicerrector', 1),
                ('5', 'Responsable de Expedición', 1),
                ('6', 'Secretario General', 1),
                ('7', 'Autoridad Local', 1),
                ('8', 'Autoridad Federal', 1),
                ('9', 'Director General', 1),
                ('10', 'Rector General', 1),
                ('11', 'Titular de la Autoridad Educativa Federal en la Ciudad de México', 1);
            ");

            migrationBuilder.Sql(@"
                INSERT INTO CatalogoTipoCertificacionSEP (IdTipoCertificacion, Descripcion, Activo) VALUES
                ('1', 'Total', 1),
                ('2', 'Parcial', 1);
            ");

            migrationBuilder.Sql(@"
                INSERT INTO CatalogoTipoPeriodoSEP (IdTipoPeriodo, Descripcion, Activo) VALUES
                ('1', 'Semestral', 1),
                ('2', 'Cuatrimestral', 1),
                ('3', 'Trimestral', 1),
                ('4', 'Bimestral', 1),
                ('5', 'Mensual', 1),
                ('6', 'Anual', 1);
            ");

            migrationBuilder.Sql(@"
                INSERT INTO CatalogoObservacionSEP (IdObservacion, Descripcion, Activo) VALUES
                ('1', 'Sin observaciones', 1),
                ('2', 'Extraordinario', 1),
                ('3', 'Recursamiento', 1),
                ('4', 'Equivalencia', 1),
                ('5', 'Revalidación', 1);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM CatalogoCargoSEP");
            migrationBuilder.Sql("DELETE FROM CatalogoTipoCertificacionSEP");
            migrationBuilder.Sql("DELETE FROM CatalogoTipoPeriodoSEP");
            migrationBuilder.Sql("DELETE FROM CatalogoObservacionSEP");
        }
    }
}
