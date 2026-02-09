using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBitacoraAccionAndNotificacionUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BitacoraAcciones",
                columns: table => new
                {
                    IdBitacora = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NombreUsuario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntidadId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DatosAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatosNuevos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacoraAcciones", x => x.IdBitacora);
                });

            migrationBuilder.CreateTable(
                name: "NotificacionesUsuario",
                columns: table => new
                {
                    IdNotificacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioDestinoId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "info"),
                    Modulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UrlAccion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Leida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    FechaLectura = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacionesUsuario", x => x.IdNotificacion);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraAcciones_FechaUtc",
                table: "BitacoraAcciones",
                column: "FechaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraAcciones_Modulo",
                table: "BitacoraAcciones",
                column: "Modulo");

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraAcciones_UsuarioId",
                table: "BitacoraAcciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionesUsuario_FechaCreacion",
                table: "NotificacionesUsuario",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionesUsuario_UsuarioDestinoId",
                table: "NotificacionesUsuario",
                column: "UsuarioDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionesUsuario_UsuarioDestinoId_Leida",
                table: "NotificacionesUsuario",
                columns: new[] { "UsuarioDestinoId", "Leida" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BitacoraAcciones");

            migrationBuilder.DropTable(
                name: "NotificacionesUsuario");
        }
    }
}
