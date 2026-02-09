using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class alter_table_profesor_add_campus_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampusId",
                table: "Profesor",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profesor_CampusId",
                table: "Profesor",
                column: "CampusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profesor_Campus_CampusId",
                table: "Profesor",
                column: "CampusId",
                principalTable: "Campus",
                principalColumn: "IdCampus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profesor_Campus_CampusId",
                table: "Profesor");

            migrationBuilder.DropIndex(
                name: "IX_Profesor_CampusId",
                table: "Profesor");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "Profesor");
        }
    }
}
