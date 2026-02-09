using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class alter_table_direccion_num_interior_exterior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Numero",
                table: "Direccion",
                newName: "NumeroInterior");

            migrationBuilder.AddColumn<string>(
                name: "NumeroExterior",
                table: "Direccion",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroExterior",
                table: "Direccion");

            migrationBuilder.RenameColumn(
                name: "NumeroInterior",
                table: "Direccion",
                newName: "Numero");
        }
    }
}
