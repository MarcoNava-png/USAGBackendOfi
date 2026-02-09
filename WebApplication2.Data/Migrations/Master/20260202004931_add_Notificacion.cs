using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations.Master
{
    /// <inheritdoc />
    public partial class add_Notificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificacion",
                columns: table => new
                {
                    IdNotificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdTenant = table.Column<int>(type: "int", nullable: true),
                    TenantCodigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TenantNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Leida = table.Column<bool>(type: "bit", nullable: false),
                    FechaLectura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Prioridad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailEnviado = table.Column<bool>(type: "bit", nullable: false),
                    FechaEmailEnviado = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacion", x => x.IdNotificacion);
                    table.ForeignKey(
                        name: "FK_Notificacion_Tenant_IdTenant",
                        column: x => x.IdTenant,
                        principalTable: "Tenant",
                        principalColumn: "IdTenant",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_FechaCreacion",
                table: "Notificacion",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_IdTenant",
                table: "Notificacion",
                column: "IdTenant");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_Leida",
                table: "Notificacion",
                column: "Leida");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_Tipo",
                table: "Notificacion",
                column: "Tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificacion");
        }
    }
}
