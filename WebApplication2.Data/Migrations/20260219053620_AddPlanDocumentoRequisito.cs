using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanDocumentoRequisito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanDocumentoRequisito",
                columns: table => new
                {
                    IdPlanDocumentoRequisito = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: false),
                    IdDocumentoRequisito = table.Column<int>(type: "int", nullable: false),
                    EsObligatorio = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanDocumentoRequisito", x => x.IdPlanDocumentoRequisito);
                    table.ForeignKey(
                        name: "FK_PlanDocumentoRequisito_DocumentoRequisito_IdDocumentoRequisito",
                        column: x => x.IdDocumentoRequisito,
                        principalTable: "DocumentoRequisito",
                        principalColumn: "IdDocumentoRequisito",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanDocumentoRequisito_PlanEstudios_IdPlanEstudios",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanDocumentoRequisito_IdDocumentoRequisito",
                table: "PlanDocumentoRequisito",
                column: "IdDocumentoRequisito");

            migrationBuilder.CreateIndex(
                name: "IX_PlanDocumentoRequisito_IdPlanEstudios_IdDocumentoRequisito",
                table: "PlanDocumentoRequisito",
                columns: new[] { "IdPlanEstudios", "IdDocumentoRequisito" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanDocumentoRequisito");
        }
    }
}
