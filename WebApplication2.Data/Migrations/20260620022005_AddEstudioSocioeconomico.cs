using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEstudioSocioeconomico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatParentescoVivienda",
                columns: table => new
                {
                    IdParentescoVivienda = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatParentescoVivienda", x => x.IdParentescoVivienda);
                });

            migrationBuilder.CreateTable(
                name: "CatRecursoTecnologico",
                columns: table => new
                {
                    IdRecursoTecnologico = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatRecursoTecnologico", x => x.IdRecursoTecnologico);
                });

            migrationBuilder.CreateTable(
                name: "CatServicioMedico",
                columns: table => new
                {
                    IdServicioMedico = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatServicioMedico", x => x.IdServicioMedico);
                });

            migrationBuilder.CreateTable(
                name: "CatServicioVivienda",
                columns: table => new
                {
                    IdServicioVivienda = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatServicioVivienda", x => x.IdServicioVivienda);
                });

            migrationBuilder.CreateTable(
                name: "EstudioSocioeconomico",
                columns: table => new
                {
                    IdEstudioSocioeconomico = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAspirante = table.Column<int>(type: "integer", nullable: false),
                    IdParentescoVivienda = table.Column<int>(type: "integer", nullable: true),
                    ConQuienViveOtro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    NumeroPersonasHogar = table.Column<int>(type: "integer", nullable: true),
                    PrincipalSostenEconomico = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PersonasAportanIngresos = table.Column<int>(type: "integer", nullable: true),
                    Trabaja = table.Column<bool>(type: "boolean", nullable: true),
                    EmpresaActividad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HorarioLaboral = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    QuienCubreGastos = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DificultadesEconomicas = table.Column<bool>(type: "boolean", nullable: true),
                    IdServicioMedico = table.Column<int>(type: "integer", nullable: true),
                    PadeceEnfermedad = table.Column<bool>(type: "boolean", nullable: true),
                    PadeceEnfermedadDetalle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TieneDiscapacidad = table.Column<bool>(type: "boolean", nullable: true),
                    TieneDiscapacidadDetalle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    EscuelaProcedencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PromedioNivelAnterior = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    AnalistaId = table.Column<string>(type: "text", nullable: true),
                    FechaLlenado = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LlenadoPorAspirante = table.Column<bool>(type: "boolean", nullable: false),
                    Token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstudioSocioeconomico", x => x.IdEstudioSocioeconomico);
                    table.ForeignKey(
                        name: "FK_EstudioSocioeconomico_Aspirante",
                        column: x => x.IdAspirante,
                        principalTable: "Aspirante",
                        principalColumn: "IdAspirante",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstudioSocioeconomico_Parentesco",
                        column: x => x.IdParentescoVivienda,
                        principalTable: "CatParentescoVivienda",
                        principalColumn: "IdParentescoVivienda",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EstudioSocioeconomico_ServicioMedico",
                        column: x => x.IdServicioMedico,
                        principalTable: "CatServicioMedico",
                        principalColumn: "IdServicioMedico",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EstudioRecursoTecnologico",
                columns: table => new
                {
                    IdEstudioSocioeconomico = table.Column<int>(type: "integer", nullable: false),
                    IdRecursoTecnologico = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstudioRecursoTecnologico", x => new { x.IdEstudioSocioeconomico, x.IdRecursoTecnologico });
                    table.ForeignKey(
                        name: "FK_EstudioRecursoTecnologico_Estudio",
                        column: x => x.IdEstudioSocioeconomico,
                        principalTable: "EstudioSocioeconomico",
                        principalColumn: "IdEstudioSocioeconomico",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstudioRecursoTecnologico_Recurso",
                        column: x => x.IdRecursoTecnologico,
                        principalTable: "CatRecursoTecnologico",
                        principalColumn: "IdRecursoTecnologico",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstudioServicioVivienda",
                columns: table => new
                {
                    IdEstudioSocioeconomico = table.Column<int>(type: "integer", nullable: false),
                    IdServicioVivienda = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstudioServicioVivienda", x => new { x.IdEstudioSocioeconomico, x.IdServicioVivienda });
                    table.ForeignKey(
                        name: "FK_EstudioServicioVivienda_Estudio",
                        column: x => x.IdEstudioSocioeconomico,
                        principalTable: "EstudioSocioeconomico",
                        principalColumn: "IdEstudioSocioeconomico",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstudioServicioVivienda_Servicio",
                        column: x => x.IdServicioVivienda,
                        principalTable: "CatServicioVivienda",
                        principalColumn: "IdServicioVivienda",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstudioRecursoTecnologico_IdRecursoTecnologico",
                table: "EstudioRecursoTecnologico",
                column: "IdRecursoTecnologico");

            migrationBuilder.CreateIndex(
                name: "IX_EstudioServicioVivienda_IdServicioVivienda",
                table: "EstudioServicioVivienda",
                column: "IdServicioVivienda");

            migrationBuilder.CreateIndex(
                name: "IX_EstudioSocioeconomico_IdParentescoVivienda",
                table: "EstudioSocioeconomico",
                column: "IdParentescoVivienda");

            migrationBuilder.CreateIndex(
                name: "IX_EstudioSocioeconomico_IdServicioMedico",
                table: "EstudioSocioeconomico",
                column: "IdServicioMedico");

            migrationBuilder.CreateIndex(
                name: "IX_EstudioSocioeconomico_Token",
                table: "EstudioSocioeconomico",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "UQ_EstudioSocioeconomico_Aspirante",
                table: "EstudioSocioeconomico",
                column: "IdAspirante",
                unique: true);

            var seedFecha = new DateTime(2026, 6, 20, 0, 0, 0, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                table: "CatParentescoVivienda",
                columns: new[] { "IdParentescoVivienda", "Nombre", "Orden", "Status", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { 1, "Padres", 1, 1, seedFecha, "seed" },
                    { 2, "Madre", 2, 1, seedFecha, "seed" },
                    { 3, "Padre", 3, 1, seedFecha, "seed" },
                    { 4, "Familiares", 4, 1, seedFecha, "seed" },
                    { 5, "Solo(a)", 5, 1, seedFecha, "seed" },
                    { 6, "Otro", 6, 1, seedFecha, "seed" }
                });

            migrationBuilder.InsertData(
                table: "CatServicioVivienda",
                columns: new[] { "IdServicioVivienda", "Nombre", "Orden", "Status", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { 1, "Agua potable", 1, 1, seedFecha, "seed" },
                    { 2, "Electricidad", 2, 1, seedFecha, "seed" },
                    { 3, "Drenaje", 3, 1, seedFecha, "seed" },
                    { 4, "Internet", 4, 1, seedFecha, "seed" },
                    { 5, "Gas", 5, 1, seedFecha, "seed" }
                });

            migrationBuilder.InsertData(
                table: "CatServicioMedico",
                columns: new[] { "IdServicioMedico", "Nombre", "Orden", "Status", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { 1, "IMSS", 1, 1, seedFecha, "seed" },
                    { 2, "ISSSTE", 2, 1, seedFecha, "seed" },
                    { 3, "Seguro privado", 3, 1, seedFecha, "seed" },
                    { 4, "INSABI/Servicios Estatales", 4, 1, seedFecha, "seed" },
                    { 5, "Ninguno", 5, 1, seedFecha, "seed" }
                });

            migrationBuilder.InsertData(
                table: "CatRecursoTecnologico",
                columns: new[] { "IdRecursoTecnologico", "Nombre", "Orden", "Status", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { 1, "Computadora personal", 1, 1, seedFecha, "seed" },
                    { 2, "Acceso a internet en casa", 2, 1, seedFecha, "seed" },
                    { 3, "Teléfono con acceso a internet", 3, 1, seedFecha, "seed" },
                    { 4, "Herramientas digitales para actividades académicas", 4, 1, seedFecha, "seed" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstudioRecursoTecnologico");

            migrationBuilder.DropTable(
                name: "EstudioServicioVivienda");

            migrationBuilder.DropTable(
                name: "CatRecursoTecnologico");

            migrationBuilder.DropTable(
                name: "EstudioSocioeconomico");

            migrationBuilder.DropTable(
                name: "CatServicioVivienda");

            migrationBuilder.DropTable(
                name: "CatParentescoVivienda");

            migrationBuilder.DropTable(
                name: "CatServicioMedico");
        }
    }
}
