using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class DeleteAspirantesHistoricos10a36 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM BitacoraRecibo WHERE IdRecibo IN (SELECT IdRecibo FROM Recibo WHERE IdAspirante BETWEEN 10 AND 36);
                DELETE FROM PagoAplicacion WHERE IdReciboDetalle IN (SELECT IdReciboDetalle FROM ReciboDetalle WHERE IdRecibo IN (SELECT IdRecibo FROM Recibo WHERE IdAspirante BETWEEN 10 AND 36));
                DELETE FROM ReciboDetalle WHERE IdRecibo IN (SELECT IdRecibo FROM Recibo WHERE IdAspirante BETWEEN 10 AND 36);
                DELETE FROM Recibo WHERE IdAspirante BETWEEN 10 AND 36;
                DELETE FROM AspiranteDocumento WHERE IdAspirante BETWEEN 10 AND 36;
                DELETE FROM AspiranteConvenio WHERE IdAspirante BETWEEN 10 AND 36;
                DELETE FROM AspiranteBitacoraSeguimiento WHERE AspiranteId BETWEEN 10 AND 36;
                DELETE FROM Aspirante WHERE IdAspirante BETWEEN 10 AND 36;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
