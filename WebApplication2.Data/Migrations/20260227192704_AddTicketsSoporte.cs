using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketsSoporte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketsSoporte",
                columns: table => new
                {
                    IdTicket = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Folio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Prioridad = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Estatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Categoria = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UsuarioCreadorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NombreCreador = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UsuarioAsignadoId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    NombreAsignado = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ArchivoAdjuntoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ArchivoAdjuntoNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketsSoporte", x => x.IdTicket);
                });

            migrationBuilder.CreateTable(
                name: "TicketComentarios",
                columns: table => new
                {
                    IdComentario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTicket = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NombreUsuario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    ArchivoAdjuntoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ArchivoAdjuntoNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketComentarios", x => x.IdComentario);
                    table.ForeignKey(
                        name: "FK_TicketComentarios_TicketsSoporte_IdTicket",
                        column: x => x.IdTicket,
                        principalTable: "TicketsSoporte",
                        principalColumn: "IdTicket",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketComentarios_IdTicket",
                table: "TicketComentarios",
                column: "IdTicket");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsSoporte_Estatus",
                table: "TicketsSoporte",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsSoporte_Folio",
                table: "TicketsSoporte",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketsSoporte_Prioridad",
                table: "TicketsSoporte",
                column: "Prioridad");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsSoporte_UsuarioAsignadoId",
                table: "TicketsSoporte",
                column: "UsuarioAsignadoId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsSoporte_UsuarioCreadorId",
                table: "TicketsSoporte",
                column: "UsuarioCreadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketComentarios");

            migrationBuilder.DropTable(
                name: "TicketsSoporte");
        }
    }
}
