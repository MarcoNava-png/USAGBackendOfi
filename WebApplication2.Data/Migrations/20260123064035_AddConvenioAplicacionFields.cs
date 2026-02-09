using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConvenioAplicacionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AplicaA",
                table: "Convenio",
                type: "nvarchar(20)",
                nullable: false,
                defaultValue: "TODOS");

            migrationBuilder.AddColumn<int>(
                name: "MaxAplicaciones",
                table: "Convenio",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VecesAplicado",
                table: "AspiranteConvenio",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AplicaA",
                table: "Convenio");

            migrationBuilder.DropColumn(
                name: "MaxAplicaciones",
                table: "Convenio");

            migrationBuilder.DropColumn(
                name: "VecesAplicado",
                table: "AspiranteConvenio");
        }
    }
}
