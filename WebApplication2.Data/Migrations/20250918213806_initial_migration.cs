using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class initial_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspiranteEstatus",
                columns: table => new
                {
                    IdAspiranteEstatus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescEstatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Aspirant__7B8DBE92EB8DEDF7", x => x.IdAspiranteEstatus);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Convenio",
                columns: table => new
                {
                    IdConvenio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaveConvenio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TipoBeneficio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DescuentoPct = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: true),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Convenio__51CFFF2B890D4417", x => x.IdConvenio);
                });

            migrationBuilder.CreateTable(
                name: "DiaSemana",
                columns: table => new
                {
                    IdDiaSemana = table.Column<byte>(type: "tinyint", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DiaSeman__7A209B4EBAF307F1", x => x.IdDiaSemana);
                });

            migrationBuilder.CreateTable(
                name: "Direccion",
                columns: table => new
                {
                    IdDireccion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Calle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Numero = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Direccio__1F8E0C76513A158C", x => x.IdDireccion);
                });

            migrationBuilder.CreateTable(
                name: "EstadoCivil",
                columns: table => new
                {
                    IdEstadoCivil = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescEstadoCivil = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EstadoCi__889DE1B24D585C92", x => x.IdEstadoCivil);
                });

            migrationBuilder.CreateTable(
                name: "Estados",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Abreviatura = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genero",
                columns: table => new
                {
                    IdGenero = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescGenero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Genero__0F8349880F3BC981", x => x.IdGenero);
                });

            migrationBuilder.CreateTable(
                name: "Materia",
                columns: table => new
                {
                    IdMateria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Creditos = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    HorasTeoria = table.Column<byte>(type: "tinyint", nullable: false),
                    HorasPractica = table.Column<byte>(type: "tinyint", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Materia__EC17467041102790", x => x.IdMateria);
                });

            migrationBuilder.CreateTable(
                name: "MedioContacto",
                columns: table => new
                {
                    IdMedioContacto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescMedio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MedioCon__3E86CE3C31C937DB", x => x.IdMedioContacto);
                });

            migrationBuilder.CreateTable(
                name: "NivelEducativo",
                columns: table => new
                {
                    IdNivelEducativo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescNivelEducativo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NivelEdu__5035CA164D3A42BB", x => x.IdNivelEducativo);
                });

            migrationBuilder.CreateTable(
                name: "Periodicidad",
                columns: table => new
                {
                    IdPeriodicidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescPeriodicidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PeriodosPorAnio = table.Column<byte>(type: "tinyint", nullable: false),
                    MesesPorPeriodo = table.Column<byte>(type: "tinyint", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Periodic__DA476CCD8B84E741", x => x.IdPeriodicidad);
                });

            migrationBuilder.CreateTable(
                name: "Turno",
                columns: table => new
                {
                    IdTurno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Turno__C1ECF79ACE66190F", x => x.IdTurno);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Campus",
                columns: table => new
                {
                    IdCampus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaveCampus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IdDireccion = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Campus__DA49C2DE1E9DB12C", x => x.IdCampus);
                    table.ForeignKey(
                        name: "FK_Campus_Direccion",
                        column: x => x.IdDireccion,
                        principalTable: "Direccion",
                        principalColumn: "IdDireccion");
                });

            migrationBuilder.CreateTable(
                name: "Municipios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Municipios_Estados_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "Estados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Persona",
                columns: table => new
                {
                    IdPersona = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApellidoPaterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApellidoMaterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    Curp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Rfc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IdDireccion = table.Column<int>(type: "int", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Celular = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdGenero = table.Column<int>(type: "int", nullable: true),
                    IdEstadoCivil = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Persona__2EC8D2AC48F4B00B", x => x.IdPersona);
                    table.ForeignKey(
                        name: "FK_Persona_Direccion",
                        column: x => x.IdDireccion,
                        principalTable: "Direccion",
                        principalColumn: "IdDireccion");
                    table.ForeignKey(
                        name: "FK_Persona_EstadoCivil",
                        column: x => x.IdEstadoCivil,
                        principalTable: "EstadoCivil",
                        principalColumn: "IdEstadoCivil");
                    table.ForeignKey(
                        name: "FK_Persona_Genero",
                        column: x => x.IdGenero,
                        principalTable: "Genero",
                        principalColumn: "IdGenero");
                });

            migrationBuilder.CreateTable(
                name: "PeriodoAcademico",
                columns: table => new
                {
                    IdPeriodoAcademico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdPeriodicidad = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PeriodoA__E57AB387D551DE0A", x => x.IdPeriodoAcademico);
                    table.ForeignKey(
                        name: "FK_Periodo_Periodicidad",
                        column: x => x.IdPeriodicidad,
                        principalTable: "Periodicidad",
                        principalColumn: "IdPeriodicidad");
                });

            migrationBuilder.CreateTable(
                name: "PlanEstudios",
                columns: table => new
                {
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClavePlanEstudios = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NombrePlanEstudios = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RVOE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PermiteAdelantar = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    Version = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IdProgramaEstudios = table.Column<int>(type: "int", nullable: true),
                    DuracionMeses = table.Column<int>(type: "int", nullable: true, defaultValue: 48),
                    MinimaAprobatoriaParcial = table.Column<int>(type: "int", nullable: false, defaultValue: 60),
                    MinimaAprobatoriaFinal = table.Column<int>(type: "int", nullable: false, defaultValue: 70),
                    IdPeriodicidad = table.Column<int>(type: "int", nullable: false),
                    IdNivelEducativo = table.Column<int>(type: "int", nullable: false),
                    IdCampus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PlanEstu__C60618471021EFD8", x => x.IdPlanEstudios);
                    table.ForeignKey(
                        name: "FK_Plan_Campus",
                        column: x => x.IdCampus,
                        principalTable: "Campus",
                        principalColumn: "IdCampus");
                    table.ForeignKey(
                        name: "FK_Plan_NivelEducativo",
                        column: x => x.IdNivelEducativo,
                        principalTable: "NivelEducativo",
                        principalColumn: "IdNivelEducativo");
                    table.ForeignKey(
                        name: "FK_Plan_Periodicidad",
                        column: x => x.IdPeriodicidad,
                        principalTable: "Periodicidad",
                        principalColumn: "IdPeriodicidad");
                });

            migrationBuilder.CreateTable(
                name: "CodigosPostales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Asentamiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MunicipioId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigosPostales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodigosPostales_Municipios_MunicipioId",
                        column: x => x.MunicipioId,
                        principalTable: "Municipios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Profesor",
                columns: table => new
                {
                    IdProfesor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoEmpleado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IdPersona = table.Column<int>(type: "int", nullable: false),
                    EmailInstitucional = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Profesor__C377C3A119E36880", x => x.IdProfesor);
                    table.ForeignKey(
                        name: "FK_Profesor_Persona",
                        column: x => x.IdPersona,
                        principalTable: "Persona",
                        principalColumn: "IdPersona");
                });

            migrationBuilder.CreateTable(
                name: "Aspirante",
                columns: table => new
                {
                    IdAspirante = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPersona = table.Column<int>(type: "int", nullable: true),
                    IdAspiranteEstatus = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    IdPlan = table.Column<int>(type: "int", nullable: false),
                    IdMedioContacto = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Aspirant__09EE6349C82C95C4", x => x.IdAspirante);
                    table.ForeignKey(
                        name: "FK_Aspirante_Estatus",
                        column: x => x.IdAspiranteEstatus,
                        principalTable: "AspiranteEstatus",
                        principalColumn: "IdAspiranteEstatus");
                    table.ForeignKey(
                        name: "FK_Aspirante_Medio",
                        column: x => x.IdMedioContacto,
                        principalTable: "MedioContacto",
                        principalColumn: "IdMedioContacto");
                    table.ForeignKey(
                        name: "FK_Aspirante_Persona",
                        column: x => x.IdPersona,
                        principalTable: "Persona",
                        principalColumn: "IdPersona");
                    table.ForeignKey(
                        name: "FK_Aspirante_Plan",
                        column: x => x.IdPlan,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios");
                });

            migrationBuilder.CreateTable(
                name: "ConvenioAlcance",
                columns: table => new
                {
                    IdConvenioAlcance = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdConvenio = table.Column<int>(type: "int", nullable: false),
                    IdCampus = table.Column<int>(type: "int", nullable: true),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: true),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: true),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Convenio__2A4E02C0B88720E9", x => x.IdConvenioAlcance);
                    table.ForeignKey(
                        name: "FK_ConvAlc_Campus",
                        column: x => x.IdCampus,
                        principalTable: "Campus",
                        principalColumn: "IdCampus");
                    table.ForeignKey(
                        name: "FK_ConvAlc_Convenio",
                        column: x => x.IdConvenio,
                        principalTable: "Convenio",
                        principalColumn: "IdConvenio");
                    table.ForeignKey(
                        name: "FK_ConvAlc_Plan",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios");
                });

            migrationBuilder.CreateTable(
                name: "Estudiante",
                columns: table => new
                {
                    IdEstudiante = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Matricula = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IdPersona = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],getdate()))"),
                    IdPlanActual = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Estudian__B5007C24138D11BB", x => x.IdEstudiante);
                    table.ForeignKey(
                        name: "FK_Estudiante_Persona",
                        column: x => x.IdPersona,
                        principalTable: "Persona",
                        principalColumn: "IdPersona");
                    table.ForeignKey(
                        name: "FK_Estudiante_Plan",
                        column: x => x.IdPlanActual,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios");
                });

            migrationBuilder.CreateTable(
                name: "Grupo",
                columns: table => new
                {
                    IdGrupo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: false),
                    IdPeriodoAcademico = table.Column<int>(type: "int", nullable: false),
                    NumeroCuatrimestre = table.Column<byte>(type: "tinyint", nullable: false),
                    NumeroGrupo = table.Column<byte>(type: "tinyint", nullable: false),
                    IdTurno = table.Column<int>(type: "int", nullable: false),
                    CapacidadMaxima = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)40)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Grupo__303F6FD92351A792", x => x.IdGrupo);
                    table.ForeignKey(
                        name: "FK_Grupo_Periodo",
                        column: x => x.IdPeriodoAcademico,
                        principalTable: "PeriodoAcademico",
                        principalColumn: "IdPeriodoAcademico");
                    table.ForeignKey(
                        name: "FK_Grupo_Plan",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios");
                    table.ForeignKey(
                        name: "FK_Grupo_Turno",
                        column: x => x.IdTurno,
                        principalTable: "Turno",
                        principalColumn: "IdTurno");
                });

            migrationBuilder.CreateTable(
                name: "MateriaPlan",
                columns: table => new
                {
                    IdMateriaPlan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: false),
                    IdMateria = table.Column<int>(type: "int", nullable: false),
                    Cuatrimestre = table.Column<byte>(type: "tinyint", nullable: false),
                    EsOptativa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MateriaP__216FB17FE2CA7B4E", x => x.IdMateriaPlan);
                    table.ForeignKey(
                        name: "FK_MateriaPlan_Materia",
                        column: x => x.IdMateria,
                        principalTable: "Materia",
                        principalColumn: "IdMateria");
                    table.ForeignKey(
                        name: "FK_MateriaPlan_Plan",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios");
                });

            migrationBuilder.CreateTable(
                name: "AspiranteConvenio",
                columns: table => new
                {
                    IdAspiranteConvenio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAspirante = table.Column<int>(type: "int", nullable: false),
                    IdConvenio = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Estatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente"),
                    Evidencia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Aspirant__F372F05FD82F39BA", x => x.IdAspiranteConvenio);
                    table.ForeignKey(
                        name: "FK_AspConv_Aspirante",
                        column: x => x.IdAspirante,
                        principalTable: "Aspirante",
                        principalColumn: "IdAspirante");
                    table.ForeignKey(
                        name: "FK_AspConv_Convenio",
                        column: x => x.IdConvenio,
                        principalTable: "Convenio",
                        principalColumn: "IdConvenio");
                });

            migrationBuilder.CreateTable(
                name: "EstudiantePlan",
                columns: table => new
                {
                    IdEstudiantePlan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    IdPlanEstudios = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],getdate()))"),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Estudian__1CF83B276A8D49B7", x => x.IdEstudiantePlan);
                    table.ForeignKey(
                        name: "FK_EstudiantePlan_Estudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante");
                    table.ForeignKey(
                        name: "FK_EstudiantePlan_Plan",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios");
                });

            migrationBuilder.CreateTable(
                name: "GrupoMateria",
                columns: table => new
                {
                    IdGrupoMateria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdGrupo = table.Column<int>(type: "int", nullable: false),
                    IdMateriaPlan = table.Column<int>(type: "int", nullable: false),
                    IdProfesor = table.Column<int>(type: "int", nullable: true),
                    Aula = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Cupo = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)40)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GrupoMat__9D026FCD2F0EA6B3", x => x.IdGrupoMateria);
                    table.ForeignKey(
                        name: "FK_GrupoMateria_Grupo",
                        column: x => x.IdGrupo,
                        principalTable: "Grupo",
                        principalColumn: "IdGrupo");
                    table.ForeignKey(
                        name: "FK_GrupoMateria_MatPlan",
                        column: x => x.IdMateriaPlan,
                        principalTable: "MateriaPlan",
                        principalColumn: "IdMateriaPlan");
                    table.ForeignKey(
                        name: "FK_GrupoMateria_Profesor",
                        column: x => x.IdProfesor,
                        principalTable: "Profesor",
                        principalColumn: "IdProfesor");
                });

            migrationBuilder.CreateTable(
                name: "Horario",
                columns: table => new
                {
                    IdHorario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdGrupoMateria = table.Column<int>(type: "int", nullable: false),
                    IdDiaSemana = table.Column<byte>(type: "tinyint", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: false),
                    Aula = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Horario__1539229BCC12B082", x => x.IdHorario);
                    table.ForeignKey(
                        name: "FK_Horario_Dia",
                        column: x => x.IdDiaSemana,
                        principalTable: "DiaSemana",
                        principalColumn: "IdDiaSemana");
                    table.ForeignKey(
                        name: "FK_Horario_GrupoMat",
                        column: x => x.IdGrupoMateria,
                        principalTable: "GrupoMateria",
                        principalColumn: "IdGrupoMateria");
                });

            migrationBuilder.CreateTable(
                name: "Inscripcion",
                columns: table => new
                {
                    IdInscripcion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    IdGrupoMateria = table.Column<int>(type: "int", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Inscrito"),
                    CalificacionFinal = table.Column<decimal>(type: "decimal(4,1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Inscripc__A122F2BF81A0DA45", x => x.IdInscripcion);
                    table.ForeignKey(
                        name: "FK_Inscripcion_Estudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante");
                    table.ForeignKey(
                        name: "FK_Inscripcion_GrupoMateria",
                        column: x => x.IdGrupoMateria,
                        principalTable: "GrupoMateria",
                        principalColumn: "IdGrupoMateria");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdAspiranteEstatus",
                table: "Aspirante",
                column: "IdAspiranteEstatus");

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdMedioContacto",
                table: "Aspirante",
                column: "IdMedioContacto");

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdPersona",
                table: "Aspirante",
                column: "IdPersona");

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdPlan",
                table: "Aspirante",
                column: "IdPlan");

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteConvenio_IdConvenio",
                table: "AspiranteConvenio",
                column: "IdConvenio");

            migrationBuilder.CreateIndex(
                name: "UQ_Aspirante_Convenio",
                table: "AspiranteConvenio",
                columns: new[] { "IdAspirante", "IdConvenio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_AspiranteEstatus",
                table: "AspiranteEstatus",
                column: "DescEstatus",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Campus_IdDireccion",
                table: "Campus",
                column: "IdDireccion");

            migrationBuilder.CreateIndex(
                name: "UQ_Campus_Clave",
                table: "Campus",
                column: "ClaveCampus",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Campus_Nombre",
                table: "Campus",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodigosPostales_MunicipioId",
                table: "CodigosPostales",
                column: "MunicipioId");

            migrationBuilder.CreateIndex(
                name: "UQ__Convenio__A6197EE9ADAF6A0B",
                table: "Convenio",
                column: "ClaveConvenio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConvenioAlcance_IdCampus",
                table: "ConvenioAlcance",
                column: "IdCampus");

            migrationBuilder.CreateIndex(
                name: "IX_ConvenioAlcance_IdConvenio",
                table: "ConvenioAlcance",
                column: "IdConvenio");

            migrationBuilder.CreateIndex(
                name: "IX_ConvenioAlcance_IdPlanEstudios",
                table: "ConvenioAlcance",
                column: "IdPlanEstudios");

            migrationBuilder.CreateIndex(
                name: "UQ__DiaSeman__75E3EFCF34622C9D",
                table: "DiaSemana",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EstadoCivil",
                table: "EstadoCivil",
                column: "DescEstadoCivil",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estudiante_IdPersona",
                table: "Estudiante",
                column: "IdPersona");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiante_IdPlanActual",
                table: "Estudiante",
                column: "IdPlanActual");

            migrationBuilder.CreateIndex(
                name: "UQ__Estudian__0FB9FB4F890AE66D",
                table: "Estudiante",
                column: "Matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstudiantePlan_IdPlanEstudios",
                table: "EstudiantePlan",
                column: "IdPlanEstudios");

            migrationBuilder.CreateIndex(
                name: "UQ_EstudiantePlan",
                table: "EstudiantePlan",
                columns: new[] { "IdEstudiante", "IdPlanEstudios", "FechaInicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Genero",
                table: "Genero",
                column: "DescGenero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grupo_IdPeriodoAcademico",
                table: "Grupo",
                column: "IdPeriodoAcademico");

            migrationBuilder.CreateIndex(
                name: "IX_Grupo_IdTurno",
                table: "Grupo",
                column: "IdTurno");

            migrationBuilder.CreateIndex(
                name: "UQ_Grupo_Num",
                table: "Grupo",
                columns: new[] { "IdPlanEstudios", "IdPeriodoAcademico", "NumeroCuatrimestre", "NumeroGrupo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrupoMateria_IdMateriaPlan",
                table: "GrupoMateria",
                column: "IdMateriaPlan");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoMateria_IdProfesor",
                table: "GrupoMateria",
                column: "IdProfesor");

            migrationBuilder.CreateIndex(
                name: "UQ_GrupoMateria",
                table: "GrupoMateria",
                columns: new[] { "IdGrupo", "IdMateriaPlan" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Horario_IdDiaSemana",
                table: "Horario",
                column: "IdDiaSemana");

            migrationBuilder.CreateIndex(
                name: "UQ_Horario",
                table: "Horario",
                columns: new[] { "IdGrupoMateria", "IdDiaSemana", "HoraInicio", "HoraFin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inscripcion_IdGrupoMateria",
                table: "Inscripcion",
                column: "IdGrupoMateria");

            migrationBuilder.CreateIndex(
                name: "UQ_Inscripcion",
                table: "Inscripcion",
                columns: new[] { "IdEstudiante", "IdGrupoMateria" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Materia__E8181E1169244C5A",
                table: "Materia",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MateriaPlan_IdMateria",
                table: "MateriaPlan",
                column: "IdMateria");

            migrationBuilder.CreateIndex(
                name: "UQ_MateriaPlan",
                table: "MateriaPlan",
                columns: new[] { "IdPlanEstudios", "IdMateria" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Municipios_EstadoId",
                table: "Municipios",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "UQ_NivelEducativo",
                table: "NivelEducativo",
                column: "DescNivelEducativo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Periodicidad",
                table: "Periodicidad",
                column: "DescPeriodicidad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodoAcademico_IdPeriodicidad",
                table: "PeriodoAcademico",
                column: "IdPeriodicidad");

            migrationBuilder.CreateIndex(
                name: "UQ__PeriodoA__E8181E117466779A",
                table: "PeriodoAcademico",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persona_IdDireccion",
                table: "Persona",
                column: "IdDireccion");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_IdEstadoCivil",
                table: "Persona",
                column: "IdEstadoCivil");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_IdGenero",
                table: "Persona",
                column: "IdGenero");

            migrationBuilder.CreateIndex(
                name: "UQ_Persona_CURP",
                table: "Persona",
                column: "Curp",
                unique: true,
                filter: "[Curp] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Persona_Email",
                table: "Persona",
                column: "Correo",
                unique: true,
                filter: "[Correo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Persona_RFC",
                table: "Persona",
                column: "Rfc",
                unique: true,
                filter: "[Rfc] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlanEstudios_IdCampus",
                table: "PlanEstudios",
                column: "IdCampus");

            migrationBuilder.CreateIndex(
                name: "IX_PlanEstudios_IdNivelEducativo",
                table: "PlanEstudios",
                column: "IdNivelEducativo");

            migrationBuilder.CreateIndex(
                name: "IX_PlanEstudios_IdPeriodicidad",
                table: "PlanEstudios",
                column: "IdPeriodicidad");

            migrationBuilder.CreateIndex(
                name: "UQ_PlanEstudios",
                table: "PlanEstudios",
                column: "ClavePlanEstudios",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profesor_IdPersona",
                table: "Profesor",
                column: "IdPersona");

            migrationBuilder.CreateIndex(
                name: "UQ__Profesor__82F7575B30F93488",
                table: "Profesor",
                column: "NoEmpleado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Turno__E8181E11A9929118",
                table: "Turno",
                column: "Clave",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspiranteConvenio");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CodigosPostales");

            migrationBuilder.DropTable(
                name: "ConvenioAlcance");

            migrationBuilder.DropTable(
                name: "EstudiantePlan");

            migrationBuilder.DropTable(
                name: "Horario");

            migrationBuilder.DropTable(
                name: "Inscripcion");

            migrationBuilder.DropTable(
                name: "Aspirante");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Municipios");

            migrationBuilder.DropTable(
                name: "Convenio");

            migrationBuilder.DropTable(
                name: "DiaSemana");

            migrationBuilder.DropTable(
                name: "Estudiante");

            migrationBuilder.DropTable(
                name: "GrupoMateria");

            migrationBuilder.DropTable(
                name: "AspiranteEstatus");

            migrationBuilder.DropTable(
                name: "MedioContacto");

            migrationBuilder.DropTable(
                name: "Estados");

            migrationBuilder.DropTable(
                name: "Grupo");

            migrationBuilder.DropTable(
                name: "MateriaPlan");

            migrationBuilder.DropTable(
                name: "Profesor");

            migrationBuilder.DropTable(
                name: "PeriodoAcademico");

            migrationBuilder.DropTable(
                name: "Turno");

            migrationBuilder.DropTable(
                name: "Materia");

            migrationBuilder.DropTable(
                name: "PlanEstudios");

            migrationBuilder.DropTable(
                name: "Persona");

            migrationBuilder.DropTable(
                name: "Campus");

            migrationBuilder.DropTable(
                name: "NivelEducativo");

            migrationBuilder.DropTable(
                name: "Periodicidad");

            migrationBuilder.DropTable(
                name: "EstadoCivil");

            migrationBuilder.DropTable(
                name: "Genero");

            migrationBuilder.DropTable(
                name: "Direccion");
        }
    }
}
