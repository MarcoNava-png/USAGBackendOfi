using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaYConvenioEmpresarial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsConvenioEmpresarial",
                table: "TarifasAdmision",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IdEmpresa",
                table: "Recibo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdEmpresa",
                table: "Aspirante",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    IdEmpresa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.IdEmpresa);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdEmpresa",
                table: "Aspirante",
                column: "IdEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Nombre",
                table: "Empresas",
                column: "Nombre");

            migrationBuilder.AddForeignKey(
                name: "FK_Aspirante_Empresa",
                table: "Aspirante",
                column: "IdEmpresa",
                principalTable: "Empresas",
                principalColumn: "IdEmpresa",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aspirante_Empresa",
                table: "Aspirante");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropIndex(
                name: "IX_Aspirante_IdEmpresa",
                table: "Aspirante");

            migrationBuilder.DropColumn(
                name: "EsConvenioEmpresarial",
                table: "TarifasAdmision");

            migrationBuilder.DropColumn(
                name: "IdEmpresa",
                table: "Recibo");

            migrationBuilder.DropColumn(
                name: "IdEmpresa",
                table: "Aspirante");
        }
    }
}
