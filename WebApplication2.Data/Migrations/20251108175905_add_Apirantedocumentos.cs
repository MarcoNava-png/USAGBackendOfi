using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_Apirantedocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentoRequisito",
                columns: table => new
                {
                    IdDocumentoRequisito = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EsObligatorio = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoRequisito", x => x.IdDocumentoRequisito);
                });

            migrationBuilder.CreateTable(
                name: "AspiranteDocumento",
                columns: table => new
                {
                    IdAspiranteDocumento = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAspirante = table.Column<int>(type: "int", nullable: false),
                    IdDocumentoRequisito = table.Column<int>(type: "int", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    FechaSubidoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UrlArchivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AspiranteIdAspirante = table.Column<int>(type: "int", nullable: true),
                    RequisitoIdDocumentoRequisito = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspiranteDocumento", x => x.IdAspiranteDocumento);
                    table.ForeignKey(
                        name: "FK_AspiranteDocumento_Aspirante_AspiranteIdAspirante",
                        column: x => x.AspiranteIdAspirante,
                        principalTable: "Aspirante",
                        principalColumn: "IdAspirante");
                    table.ForeignKey(
                        name: "FK_AspiranteDocumento_DocumentoRequisito_RequisitoIdDocumentoRequisito",
                        column: x => x.RequisitoIdDocumentoRequisito,
                        principalTable: "DocumentoRequisito",
                        principalColumn: "IdDocumentoRequisito");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteDocumento_AspiranteIdAspirante",
                table: "AspiranteDocumento",
                column: "AspiranteIdAspirante");

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteDocumento_RequisitoIdDocumentoRequisito",
                table: "AspiranteDocumento",
                column: "RequisitoIdDocumentoRequisito");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspiranteDocumento");

            migrationBuilder.DropTable(
                name: "DocumentoRequisito");
        }
    }
}
