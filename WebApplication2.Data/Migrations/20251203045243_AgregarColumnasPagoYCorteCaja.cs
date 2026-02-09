using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarColumnasPagoYCorteCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar columnas a tabla Pago
            migrationBuilder.AddColumn<string>(
                name: "FolioPago",
                table: "Pago",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCaja",
                table: "Pago",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCorteCaja",
                table: "Pago",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdUsuarioCaja",
                table: "Pago",
                type: "nvarchar(max)",
                nullable: true);

            // Crear tabla CorteCaja
            migrationBuilder.CreateTable(
                name: "CorteCaja",
                columns: table => new
                {
                    IdCorteCaja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FolioCorteCaja = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuarioCaja = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    MontoInicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEfectivo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTransferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTarjeta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalGeneral = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Cerrado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CerradoPor = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorteCaja", x => x.IdCorteCaja);
                    table.ForeignKey(
                        name: "FK_CorteCaja_AspNetUsers_CerradoPor",
                        column: x => x.CerradoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CorteCaja_AspNetUsers_IdUsuarioCaja",
                        column: x => x.IdUsuarioCaja,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorteCaja_CerradoPor",
                table: "CorteCaja",
                column: "CerradoPor");

            migrationBuilder.CreateIndex(
                name: "IX_CorteCaja_IdUsuarioCaja",
                table: "CorteCaja",
                column: "IdUsuarioCaja");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorteCaja");

            migrationBuilder.DropColumn(
                name: "FolioPago",
                table: "Pago");

            migrationBuilder.DropColumn(
                name: "IdCaja",
                table: "Pago");

            migrationBuilder.DropColumn(
                name: "IdCorteCaja",
                table: "Pago");

            migrationBuilder.DropColumn(
                name: "IdUsuarioCaja",
                table: "Pago");
        }
    }
}
