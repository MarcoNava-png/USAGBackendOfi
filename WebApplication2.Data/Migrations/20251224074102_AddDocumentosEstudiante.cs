using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentosEstudiante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposDocumentoEstudiante",
                columns: table => new
                {
                    IdTipoDocumento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    DiasVigencia = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    RequierePago = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDocumentoEstudiante", x => x.IdTipoDocumento);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesDocumento",
                columns: table => new
                {
                    IdSolicitud = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FolioSolicitud = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    IdTipoDocumento = table.Column<int>(type: "int", nullable: false),
                    IdRecibo = table.Column<long>(type: "bigint", nullable: true),
                    Variante = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CodigoVerificacion = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VecesImpreso = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UsuarioSolicita = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UsuarioGenera = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesDocumento", x => x.IdSolicitud);
                    table.ForeignKey(
                        name: "FK_SolicitudesDocumento_AspNetUsers_UsuarioGenera",
                        column: x => x.UsuarioGenera,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesDocumento_AspNetUsers_UsuarioSolicita",
                        column: x => x.UsuarioSolicita,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesDocumento_Estudiante_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesDocumento_Recibo_IdRecibo",
                        column: x => x.IdRecibo,
                        principalTable: "Recibo",
                        principalColumn: "IdRecibo",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SolicitudesDocumento_TiposDocumentoEstudiante_IdTipoDocumento",
                        column: x => x.IdTipoDocumento,
                        principalTable: "TiposDocumentoEstudiante",
                        principalColumn: "IdTipoDocumento",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesDocumento_CodigoVerificacion",
                table: "SolicitudesDocumento",
                column: "CodigoVerificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesDocumento_Estatus",
                table: "SolicitudesDocumento",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesDocumento_FolioSolicitud",
                table: "SolicitudesDocumento",
                column: "FolioSolicitud",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesDocumento_IdEstudiante",
                table: "SolicitudesDocumento",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesDocumento_IdRecibo",
                table: "SolicitudesDocumento",
                column: "IdRecibo");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesDocumento_IdTipoDocumento",
                table: "SolicitudesDocumento",
                column: "IdTipoDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesDocumento_UsuarioGenera",
                table: "SolicitudesDocumento",
                column: "UsuarioGenera");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesDocumento_UsuarioSolicita",
                table: "SolicitudesDocumento",
                column: "UsuarioSolicita");

            migrationBuilder.CreateIndex(
                name: "IX_TiposDocumentoEstudiante_Clave",
                table: "TiposDocumentoEstudiante",
                column: "Clave",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesDocumento");

            migrationBuilder.DropTable(
                name: "TiposDocumentoEstudiante");
        }
    }
}
