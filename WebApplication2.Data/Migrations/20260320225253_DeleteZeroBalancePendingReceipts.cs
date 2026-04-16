using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class DeleteZeroBalancePendingReceipts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ReciboDetalle WHERE IdRecibo IN (SELECT IdRecibo FROM Recibo WHERE Estatus = 'PENDIENTE' AND Saldo = 0 AND Total = 0);
                DELETE FROM Recibo WHERE Estatus = 'PENDIENTE' AND Saldo = 0 AND Total = 0;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
