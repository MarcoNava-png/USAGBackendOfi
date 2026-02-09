using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBecaCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdBeca",
                table: "BecaAsignacion",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Beca",
                columns: table => new
                {
                    IdBeca = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "PORCENTAJE"),
                    Valor = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TopeMensual = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    IdConceptoPago = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beca", x => x.IdBeca);
                    table.ForeignKey(
                        name: "FK_Beca_ConceptoPago_IdConceptoPago",
                        column: x => x.IdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BecaAsignacion_IdBeca",
                table: "BecaAsignacion",
                column: "IdBeca");

            migrationBuilder.CreateIndex(
                name: "IX_BecaAsignacion_IdEstudiante",
                table: "BecaAsignacion",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_Beca_Clave",
                table: "Beca",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beca_IdConceptoPago",
                table: "Beca",
                column: "IdConceptoPago");

            migrationBuilder.AddForeignKey(
                name: "FK_BecaAsignacion_Beca_IdBeca",
                table: "BecaAsignacion",
                column: "IdBeca",
                principalTable: "Beca",
                principalColumn: "IdBeca",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BecaAsignacion_Estudiante_IdEstudiante",
                table: "BecaAsignacion",
                column: "IdEstudiante",
                principalTable: "Estudiante",
                principalColumn: "IdEstudiante",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BecaAsignacion_Beca_IdBeca",
                table: "BecaAsignacion");

            migrationBuilder.DropForeignKey(
                name: "FK_BecaAsignacion_Estudiante_IdEstudiante",
                table: "BecaAsignacion");

            migrationBuilder.DropTable(
                name: "Beca");

            migrationBuilder.DropIndex(
                name: "IX_BecaAsignacion_IdBeca",
                table: "BecaAsignacion");

            migrationBuilder.DropIndex(
                name: "IX_BecaAsignacion_IdEstudiante",
                table: "BecaAsignacion");

            migrationBuilder.DropColumn(
                name: "IdBeca",
                table: "BecaAsignacion");
        }
    }
}
