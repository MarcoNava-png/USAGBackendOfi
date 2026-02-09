using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class relation_aspirante_user_and_direccion_codigo_postal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CodigoPostalId",
                table: "Direccion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdAtendidoPorUsuario",
                table: "Aspirante",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Direccion_CodigoPostalId",
                table: "Direccion",
                column: "CodigoPostalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Direccion_CodigosPostales_CodigoPostalId",
                table: "Direccion",
                column: "CodigoPostalId",
                principalTable: "CodigosPostales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Direccion_CodigosPostales_CodigoPostalId",
                table: "Direccion");

            migrationBuilder.DropIndex(
                name: "IX_Direccion_CodigoPostalId",
                table: "Direccion");

            migrationBuilder.DropColumn(
                name: "CodigoPostalId",
                table: "Direccion");

            migrationBuilder.DropColumn(
                name: "IdAtendidoPorUsuario",
                table: "Aspirante");
        }
    }
}
