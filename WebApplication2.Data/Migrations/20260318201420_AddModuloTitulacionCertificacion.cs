using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloTitulacionCertificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogoCargoSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCargo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoCargoSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoCarreraSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCarrera = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaveCarrera = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreCarrera = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoCarreraSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoObservacionSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdObservacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoObservacionSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoTipoCertificacionSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTipoCertificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoTipoCertificacionSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoTipoPeriodoSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTipoPeriodo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoTipoPeriodoSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionIPES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdNombreInstitucion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreInstitucion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCampusSEP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampusSEP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdEntidadFederativa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntidadFederativa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCampus = table.Column<int>(type: "int", nullable: false),
                    CampusNavigationIdCampus = table.Column<int>(type: "int", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionIPES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionIPES_Campus_CampusNavigationIdCampus",
                        column: x => x.CampusNavigationIdCampus,
                        principalTable: "Campus",
                        principalColumn: "IdCampus");
                });

            migrationBuilder.CreateTable(
                name: "CredencialSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndpointUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EsProduccion = table.Column<bool>(type: "bit", nullable: false),
                    IdConfiguracionIPES = table.Column<int>(type: "int", nullable: false),
                    ConfiguracionIPESNavigationId = table.Column<int>(type: "int", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredencialSEP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CredencialSEP_ConfiguracionIPES_ConfiguracionIPESNavigationId",
                        column: x => x.ConfiguracionIPESNavigationId,
                        principalTable: "ConfiguracionIPES",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ResponsableFirma",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Curp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimerApellido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SegundoApellido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCargo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cargo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutaCertificadoCer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutaLlavePrivadaKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordLlavePrivada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoCertificadoResponsable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdConfiguracionIPES = table.Column<int>(type: "int", nullable: false),
                    ConfiguracionIPESNavigationId = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    VigenciaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VigenciaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsableFirma", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponsableFirma_ConfiguracionIPES_ConfiguracionIPESNavigationId",
                        column: x => x.ConfiguracionIPESNavigationId,
                        principalTable: "ConfiguracionIPES",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CertificadoElectronico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FolioControl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoTitulacion = table.Column<int>(type: "int", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    IdEstudiante = table.Column<int>(type: "int", nullable: true),
                    EstudianteNavigationIdEstudiante = table.Column<int>(type: "int", nullable: true),
                    IdPersona = table.Column<int>(type: "int", nullable: true),
                    PersonaNavigationIdPersona = table.Column<int>(type: "int", nullable: true),
                    NumeroControl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Curp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimerApellido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SegundoApellido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdGenero = table.Column<int>(type: "int", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FotoHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirmaAutografaHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCarreraSEP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaveCarrera = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreCarrera = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdTipoPeriodo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoPeriodo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClavePlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroRvoe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaExpedicionRvoe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdTipoCertificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoCertificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaExpedicion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdLugarExpedicion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LugarExpedicion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAsignaturas = table.Column<int>(type: "int", nullable: false),
                    AsignaturasAsignadas = table.Column<int>(type: "int", nullable: false),
                    Promedio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdConfiguracionIPES = table.Column<int>(type: "int", nullable: false),
                    ConfiguracionIPESNavigationId = table.Column<int>(type: "int", nullable: true),
                    IdResponsableFirma = table.Column<int>(type: "int", nullable: true),
                    ResponsableFirmaNavigationId = table.Column<int>(type: "int", nullable: true),
                    SelloDigital = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CadenaOriginal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XmlGenerado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroLoteSEP = table.Column<int>(type: "int", nullable: true),
                    EstatusLoteSEP = table.Column<int>(type: "int", nullable: true),
                    FolioControlSEP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MensajeSEP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaEnvioSEP = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRespuestaSEP = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificadoElectronico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificadoElectronico_ConfiguracionIPES_ConfiguracionIPESNavigationId",
                        column: x => x.ConfiguracionIPESNavigationId,
                        principalTable: "ConfiguracionIPES",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CertificadoElectronico_Estudiante_EstudianteNavigationIdEstudiante",
                        column: x => x.EstudianteNavigationIdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante");
                    table.ForeignKey(
                        name: "FK_CertificadoElectronico_Persona_PersonaNavigationIdPersona",
                        column: x => x.PersonaNavigationIdPersona,
                        principalTable: "Persona",
                        principalColumn: "IdPersona");
                    table.ForeignKey(
                        name: "FK_CertificadoElectronico_ResponsableFirma_ResponsableFirmaNavigationId",
                        column: x => x.ResponsableFirmaNavigationId,
                        principalTable: "ResponsableFirma",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CertificadoAsignatura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCertificadoElectronico = table.Column<int>(type: "int", nullable: false),
                    CertificadoElectronicoNavigationId = table.Column<int>(type: "int", nullable: true),
                    IdAsignatura = table.Column<int>(type: "int", nullable: false),
                    ClaveAsignatura = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ciclo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Calificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdObservaciones = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificadoAsignatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificadoAsignatura_CertificadoElectronico_CertificadoElectronicoNavigationId",
                        column: x => x.CertificadoElectronicoNavigationId,
                        principalTable: "CertificadoElectronico",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificadoAsignatura_CertificadoElectronicoNavigationId",
                table: "CertificadoAsignatura",
                column: "CertificadoElectronicoNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadoElectronico_ConfiguracionIPESNavigationId",
                table: "CertificadoElectronico",
                column: "ConfiguracionIPESNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadoElectronico_EstudianteNavigationIdEstudiante",
                table: "CertificadoElectronico",
                column: "EstudianteNavigationIdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadoElectronico_PersonaNavigationIdPersona",
                table: "CertificadoElectronico",
                column: "PersonaNavigationIdPersona");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadoElectronico_ResponsableFirmaNavigationId",
                table: "CertificadoElectronico",
                column: "ResponsableFirmaNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionIPES_CampusNavigationIdCampus",
                table: "ConfiguracionIPES",
                column: "CampusNavigationIdCampus");

            migrationBuilder.CreateIndex(
                name: "IX_CredencialSEP_ConfiguracionIPESNavigationId",
                table: "CredencialSEP",
                column: "ConfiguracionIPESNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsableFirma_ConfiguracionIPESNavigationId",
                table: "ResponsableFirma",
                column: "ConfiguracionIPESNavigationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogoCargoSEP");

            migrationBuilder.DropTable(
                name: "CatalogoCarreraSEP");

            migrationBuilder.DropTable(
                name: "CatalogoObservacionSEP");

            migrationBuilder.DropTable(
                name: "CatalogoTipoCertificacionSEP");

            migrationBuilder.DropTable(
                name: "CatalogoTipoPeriodoSEP");

            migrationBuilder.DropTable(
                name: "CertificadoAsignatura");

            migrationBuilder.DropTable(
                name: "CredencialSEP");

            migrationBuilder.DropTable(
                name: "CertificadoElectronico");

            migrationBuilder.DropTable(
                name: "ResponsableFirma");

            migrationBuilder.DropTable(
                name: "ConfiguracionIPES");
        }
    }
}
