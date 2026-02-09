using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_aspirante_bitacora_seguimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspiranteBitacoraSeguimiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AspiranteId = table.Column<int>(type: "int", nullable: false),
                    UsuarioAtiendeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MedioContacto = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    Resumen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProximaAccion = table.Column<string>(type: "nvarchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspiranteBitacoraSeguimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspiranteBitacoraSeguimiento_AspNetUsers_UsuarioAtiendeId",
                        column: x => x.UsuarioAtiendeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AspiranteBitacoraSeguimiento_Aspirante_AspiranteId",
                        column: x => x.AspiranteId,
                        principalTable: "Aspirante",
                        principalColumn: "IdAspirante",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteBitacoraSeguimiento_AspiranteId",
                table: "AspiranteBitacoraSeguimiento",
                column: "AspiranteId");

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteBitacoraSeguimiento_UsuarioAtiendeId",
                table: "AspiranteBitacoraSeguimiento",
                column: "UsuarioAtiendeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspiranteBitacoraSeguimiento");
        }
    }
}
