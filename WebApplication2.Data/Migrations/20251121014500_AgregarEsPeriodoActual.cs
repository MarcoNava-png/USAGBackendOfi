using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEsPeriodoActual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReciboDetalle_Recibo_ReciboIdRecibo",
                table: "ReciboDetalle");

            migrationBuilder.DropIndex(
                name: "IX_ReciboDetalle_ReciboIdRecibo",
                table: "ReciboDetalle");

            migrationBuilder.DropColumn(
                name: "ReciboIdRecibo",
                table: "ReciboDetalle");

            migrationBuilder.AddColumn<bool>(
                name: "EsPeriodoActual",
                table: "PeriodoAcademico",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ReciboDetalle_IdRecibo",
                table: "ReciboDetalle",
                column: "IdRecibo");

            migrationBuilder.AddForeignKey(
                name: "FK_ReciboDetalle_Recibo_IdRecibo",
                table: "ReciboDetalle",
                column: "IdRecibo",
                principalTable: "Recibo",
                principalColumn: "IdRecibo",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReciboDetalle_Recibo_IdRecibo",
                table: "ReciboDetalle");

            migrationBuilder.DropIndex(
                name: "IX_ReciboDetalle_IdRecibo",
                table: "ReciboDetalle");

            migrationBuilder.DropColumn(
                name: "EsPeriodoActual",
                table: "PeriodoAcademico");

            migrationBuilder.AddColumn<long>(
                name: "ReciboIdRecibo",
                table: "ReciboDetalle",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReciboDetalle_ReciboIdRecibo",
                table: "ReciboDetalle",
                column: "ReciboIdRecibo");

            migrationBuilder.AddForeignKey(
                name: "FK_ReciboDetalle_Recibo_ReciboIdRecibo",
                table: "ReciboDetalle",
                column: "ReciboIdRecibo",
                principalTable: "Recibo",
                principalColumn: "IdRecibo");
        }
    }
}
