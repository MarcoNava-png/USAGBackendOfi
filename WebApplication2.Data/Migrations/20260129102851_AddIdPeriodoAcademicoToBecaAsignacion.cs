using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdPeriodoAcademicoToBecaAsignacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PagoAplicacion_Pago_PagoIdPago",
                table: "PagoAplicacion");

            migrationBuilder.DropForeignKey(
                name: "FK_PagoAplicacion_ReciboDetalle_ReciboDetalleIdReciboDetalle",
                table: "PagoAplicacion");

            migrationBuilder.DropIndex(
                name: "IX_PagoAplicacion_PagoIdPago",
                table: "PagoAplicacion");

            migrationBuilder.DropIndex(
                name: "IX_PagoAplicacion_ReciboDetalleIdReciboDetalle",
                table: "PagoAplicacion");

            migrationBuilder.DropColumn(
                name: "PagoIdPago",
                table: "PagoAplicacion");

            migrationBuilder.DropColumn(
                name: "ReciboDetalleIdReciboDetalle",
                table: "PagoAplicacion");

            migrationBuilder.AddColumn<int>(
                name: "IdPeriodoAcademico",
                table: "BecaAsignacion",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PagoAplicacion_IdPago",
                table: "PagoAplicacion",
                column: "IdPago");

            migrationBuilder.CreateIndex(
                name: "IX_PagoAplicacion_IdReciboDetalle",
                table: "PagoAplicacion",
                column: "IdReciboDetalle");

            migrationBuilder.CreateIndex(
                name: "IX_BecaAsignacion_IdPeriodoAcademico",
                table: "BecaAsignacion",
                column: "IdPeriodoAcademico");

            migrationBuilder.AddForeignKey(
                name: "FK_BecaAsignacion_PeriodoAcademico_IdPeriodoAcademico",
                table: "BecaAsignacion",
                column: "IdPeriodoAcademico",
                principalTable: "PeriodoAcademico",
                principalColumn: "IdPeriodoAcademico",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PagoAplicacion_Pago_IdPago",
                table: "PagoAplicacion",
                column: "IdPago",
                principalTable: "Pago",
                principalColumn: "IdPago",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PagoAplicacion_ReciboDetalle_IdReciboDetalle",
                table: "PagoAplicacion",
                column: "IdReciboDetalle",
                principalTable: "ReciboDetalle",
                principalColumn: "IdReciboDetalle",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BecaAsignacion_PeriodoAcademico_IdPeriodoAcademico",
                table: "BecaAsignacion");

            migrationBuilder.DropForeignKey(
                name: "FK_PagoAplicacion_Pago_IdPago",
                table: "PagoAplicacion");

            migrationBuilder.DropForeignKey(
                name: "FK_PagoAplicacion_ReciboDetalle_IdReciboDetalle",
                table: "PagoAplicacion");

            migrationBuilder.DropIndex(
                name: "IX_PagoAplicacion_IdPago",
                table: "PagoAplicacion");

            migrationBuilder.DropIndex(
                name: "IX_PagoAplicacion_IdReciboDetalle",
                table: "PagoAplicacion");

            migrationBuilder.DropIndex(
                name: "IX_BecaAsignacion_IdPeriodoAcademico",
                table: "BecaAsignacion");

            migrationBuilder.DropColumn(
                name: "IdPeriodoAcademico",
                table: "BecaAsignacion");

            migrationBuilder.AddColumn<long>(
                name: "PagoIdPago",
                table: "PagoAplicacion",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReciboDetalleIdReciboDetalle",
                table: "PagoAplicacion",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PagoAplicacion_PagoIdPago",
                table: "PagoAplicacion",
                column: "PagoIdPago");

            migrationBuilder.CreateIndex(
                name: "IX_PagoAplicacion_ReciboDetalleIdReciboDetalle",
                table: "PagoAplicacion",
                column: "ReciboDetalleIdReciboDetalle");

            migrationBuilder.AddForeignKey(
                name: "FK_PagoAplicacion_Pago_PagoIdPago",
                table: "PagoAplicacion",
                column: "PagoIdPago",
                principalTable: "Pago",
                principalColumn: "IdPago");

            migrationBuilder.AddForeignKey(
                name: "FK_PagoAplicacion_ReciboDetalle_ReciboDetalleIdReciboDetalle",
                table: "PagoAplicacion",
                column: "ReciboDetalleIdReciboDetalle",
                principalTable: "ReciboDetalle",
                principalColumn: "IdReciboDetalle");
        }
    }
}
