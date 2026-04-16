using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecargoPoliticaRangos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CargoDiarioFijo",
                table: "RecargoPolitica",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte>(
                name: "DiaFinRecargoPorcentaje",
                table: "RecargoPolitica",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "DiaInicioCargoDiario",
                table: "RecargoPolitica",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "DiaInicioRecargoPorcentaje",
                table: "RecargoPolitica",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeRecargo",
                table: "RecargoPolitica",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CargoDiarioFijo",
                table: "RecargoPolitica");

            migrationBuilder.DropColumn(
                name: "DiaFinRecargoPorcentaje",
                table: "RecargoPolitica");

            migrationBuilder.DropColumn(
                name: "DiaInicioCargoDiario",
                table: "RecargoPolitica");

            migrationBuilder.DropColumn(
                name: "DiaInicioRecargoPorcentaje",
                table: "RecargoPolitica");

            migrationBuilder.DropColumn(
                name: "PorcentajeRecargo",
                table: "RecargoPolitica");
        }
    }
}
