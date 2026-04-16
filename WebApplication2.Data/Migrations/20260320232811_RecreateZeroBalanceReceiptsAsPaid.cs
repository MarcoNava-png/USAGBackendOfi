using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class RecreateZeroBalanceReceiptsAsPaid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Recibo SET Estatus = 2
                WHERE Estatus = 0 AND Total = 0 AND Saldo = 0 AND Subtotal > 0 AND Descuento > 0;

                DECLARE @maxNum INT = (SELECT ISNULL(MAX(CAST(RIGHT(Folio, 6) AS INT)),0) FROM Recibo WHERE Folio LIKE 'REC-2026-%');
                SET @maxNum = @maxNum + 1;

                INSERT INTO Recibo (Folio, IdAspirante, FechaEmision, FechaVencimiento, Estatus, Subtotal, Descuento, Recargos, Saldo, Notas, CreatedAt, Status)
                VALUES ('REC-2026-' + RIGHT('000000' + CAST(@maxNum AS VARCHAR), 6), 36, GETDATE(), DATEADD(DAY, 30, GETDATE()), 2, 500, 500, 0, 0, 'Recibo de FICHA DE ADMISIÓN - Descuento por FICHA: $500.00', GETDATE(), 1);

                INSERT INTO ReciboDetalle (IdRecibo, IdConceptoPago, Cantidad, PrecioUnitario, CreatedAt, Status)
                VALUES (SCOPE_IDENTITY(), 4, 1, 500, GETDATE(), 1);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
