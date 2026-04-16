using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    public partial class AddCreditosTitulacion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Creditos",
                table: "CatalogoAsignaturaSEP",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTipoAsignatura",
                table: "CatalogoAsignaturaSEP",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoAsignatura",
                table: "CatalogoAsignaturaSEP",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Creditos",
                table: "CertificadoAsignatura",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTipoAsignatura",
                table: "CertificadoAsignatura",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoAsignatura",
                table: "CertificadoAsignatura",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditosObtenidos",
                table: "CertificadoElectronico",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCreditos",
                table: "CertificadoElectronico",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroCiclos",
                table: "CertificadoElectronico",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Creditos", table: "CatalogoAsignaturaSEP");
            migrationBuilder.DropColumn(name: "IdTipoAsignatura", table: "CatalogoAsignaturaSEP");
            migrationBuilder.DropColumn(name: "TipoAsignatura", table: "CatalogoAsignaturaSEP");
            migrationBuilder.DropColumn(name: "Creditos", table: "CertificadoAsignatura");
            migrationBuilder.DropColumn(name: "IdTipoAsignatura", table: "CertificadoAsignatura");
            migrationBuilder.DropColumn(name: "TipoAsignatura", table: "CertificadoAsignatura");
            migrationBuilder.DropColumn(name: "CreditosObtenidos", table: "CertificadoElectronico");
            migrationBuilder.DropColumn(name: "TotalCreditos", table: "CertificadoElectronico");
            migrationBuilder.DropColumn(name: "NumeroCiclos", table: "CertificadoElectronico");
        }
    }
}
