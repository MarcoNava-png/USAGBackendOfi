using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_CambiosIndice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_PlanEstudios",
                table: "PlanEstudios");

            migrationBuilder.CreateIndex(
                name: "UQ_PlanEstudios_Campus",
                table: "PlanEstudios",
                columns: new[] { "ClavePlanEstudios", "IdCampus" },
                unique: true,
                filter: "[ClavePlanEstudios] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_PlanEstudios_Campus",
                table: "PlanEstudios");

            migrationBuilder.CreateIndex(
                name: "UQ_PlanEstudios",
                table: "PlanEstudios",
                column: "ClavePlanEstudios",
                unique: true,
                filter: "[ClavePlanEstudios] IS NOT NULL");
        }
    }
}
