using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspiranteEstatus",
                columns: table => new
                {
                    IdAspiranteEstatus = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescEstatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Aspirant__7B8DBE92EB8DEDF7", x => x.IdAspiranteEstatus);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Nombres = table.Column<string>(type: "text", nullable: true),
                    Apellidos = table.Column<string>(type: "text", nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    Biografia = table.Column<string>(type: "text", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BitacoraAcciones",
                columns: table => new
                {
                    IdBitacora = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    NombreUsuario = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Accion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Entidad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntidadId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DatosAnteriores = table.Column<string>(type: "text", nullable: true),
                    DatosNuevos = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacoraAcciones", x => x.IdBitacora);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoAsignaturaSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNombreInstitucion = table.Column<string>(type: "text", nullable: true),
                    IdCarrera = table.Column<int>(type: "integer", nullable: false),
                    IdAsignatura = table.Column<int>(type: "integer", nullable: false),
                    ClaveAsignatura = table.Column<string>(type: "text", nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    Creditos = table.Column<decimal>(type: "numeric", nullable: true),
                    IdTipoAsignatura = table.Column<int>(type: "integer", nullable: true),
                    TipoAsignatura = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoAsignaturaSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoCargoSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCargo = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoCargoSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoCarreraSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNombreInstitucion = table.Column<string>(type: "text", nullable: true),
                    IdNivelEstudios = table.Column<string>(type: "text", nullable: true),
                    IdCarrera = table.Column<string>(type: "text", nullable: true),
                    ClaveCarrera = table.Column<string>(type: "text", nullable: true),
                    NombreCarrera = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoCarreraSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoEntidadFederativaSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEntidadFederativa = table.Column<string>(type: "text", nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoEntidadFederativaSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoGeneroSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdGenero = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoGeneroSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoNivelEstudiosSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNivelEstudios = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoNivelEstudiosSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoObservacionSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdObservacion = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoObservacionSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoTipoAsignaturaSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTipoAsignatura = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoTipoAsignaturaSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoTipoCertificacionSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTipoCertificacion = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoTipoCertificacionSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoTipoPeriodoSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTipoPeriodo = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoTipoPeriodoSEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConceptoPago",
                columns: table => new
                {
                    IdConceptoPago = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Clave = table.Column<string>(type: "text", nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    AplicaA = table.Column<int>(type: "integer", nullable: false),
                    EsObligatorio = table.Column<bool>(type: "boolean", nullable: false),
                    PeriodicidadMeses = table.Column<byte>(type: "smallint", nullable: true),
                    PermiteBeca = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptoPago", x => x.IdConceptoPago);
                });

            migrationBuilder.CreateTable(
                name: "Convenio",
                columns: table => new
                {
                    IdConvenio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClaveConvenio = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    TipoBeneficio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DescuentoPct = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Monto = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: true),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    AplicaA = table.Column<string>(type: "text", nullable: true),
                    MaxAplicaciones = table.Column<int>(type: "integer", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Convenio__51CFFF2B890D4417", x => x.IdConvenio);
                });

            migrationBuilder.CreateTable(
                name: "DiaSemana",
                columns: table => new
                {
                    IdDiaSemana = table.Column<byte>(type: "smallint", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DiaSeman__7A209B4EBAF307F1", x => x.IdDiaSemana);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoRequisito",
                columns: table => new
                {
                    IdDocumentoRequisito = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Clave = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EsObligatorio = table.Column<bool>(type: "boolean", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoRequisito", x => x.IdDocumentoRequisito);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    IdEmpresa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.IdEmpresa);
                });

            migrationBuilder.CreateTable(
                name: "EstadoCivil",
                columns: table => new
                {
                    IdEstadoCivil = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescEstadoCivil = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EstadoCi__889DE1B24D585C92", x => x.IdEstadoCivil);
                });

            migrationBuilder.CreateTable(
                name: "Estados",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    Abreviatura = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genero",
                columns: table => new
                {
                    IdGenero = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescGenero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Genero__0F8349880F3BC981", x => x.IdGenero);
                });

            migrationBuilder.CreateTable(
                name: "Materia",
                columns: table => new
                {
                    IdMateria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Clave = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Creditos = table.Column<decimal>(type: "numeric(4,1)", nullable: false),
                    HorasTeoria = table.Column<byte>(type: "smallint", nullable: false),
                    HorasPractica = table.Column<byte>(type: "smallint", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Materia__EC17467041102790", x => x.IdMateria);
                });

            migrationBuilder.CreateTable(
                name: "MedioContacto",
                columns: table => new
                {
                    IdMedioContacto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescMedio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MedioCon__3E86CE3C31C937DB", x => x.IdMedioContacto);
                });

            migrationBuilder.CreateTable(
                name: "MedioPago",
                columns: table => new
                {
                    IdMedioPago = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Clave = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    RequiereReferencia = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedioPago", x => x.IdMedioPago);
                });

            migrationBuilder.CreateTable(
                name: "Modalidad",
                columns: table => new
                {
                    IdModalidad = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescModalidad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modalidad", x => x.IdModalidad);
                });

            migrationBuilder.CreateTable(
                name: "ModalidadPlan",
                columns: table => new
                {
                    IdModalidadPlan = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescModalidadPlan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModalidadPlan", x => x.IdModalidadPlan);
                });

            migrationBuilder.CreateTable(
                name: "NivelEducativo",
                columns: table => new
                {
                    IdNivelEducativo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescNivelEducativo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NivelEdu__5035CA164D3A42BB", x => x.IdNivelEducativo);
                });

            migrationBuilder.CreateTable(
                name: "NotificacionesUsuario",
                columns: table => new
                {
                    IdNotificacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioDestinoId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "info"),
                    Modulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UrlAccion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Leida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')"),
                    FechaLectura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacionesUsuario", x => x.IdNotificacion);
                });

            migrationBuilder.CreateTable(
                name: "Parciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parciales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Periodicidad",
                columns: table => new
                {
                    IdPeriodicidad = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescPeriodicidad = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PeriodosPorAnio = table.Column<byte>(type: "smallint", nullable: false),
                    MesesPorPeriodo = table.Column<byte>(type: "smallint", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Periodic__DA476CCD8B84E741", x => x.IdPeriodicidad);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    IdPermission = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.IdPermission);
                });

            migrationBuilder.CreateTable(
                name: "PlantillaReportes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    Codigo = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Categoria = table.Column<string>(type: "text", nullable: true),
                    RutaArchivo = table.Column<string>(type: "text", nullable: true),
                    NombreArchivoOriginal = table.Column<string>(type: "text", nullable: true),
                    VariablesDisponibles = table.Column<string>(type: "text", nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillaReportes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecargoPolitica",
                columns: table => new
                {
                    IdRecargoPolitica = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCampus = table.Column<int>(type: "integer", nullable: true),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: true),
                    TasaDiaria = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    DiaInicioGracia = table.Column<byte>(type: "smallint", nullable: false),
                    DiaFinGracia = table.Column<byte>(type: "smallint", nullable: false),
                    RecargoMinimo = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    RecargoMaximo = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    TopeDiasMora = table.Column<int>(type: "integer", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    DiaInicioRecargoPorcentaje = table.Column<byte>(type: "smallint", nullable: false),
                    DiaFinRecargoPorcentaje = table.Column<byte>(type: "smallint", nullable: false),
                    PorcentajeRecargo = table.Column<decimal>(type: "numeric", nullable: false),
                    DiaInicioCargoDiario = table.Column<byte>(type: "smallint", nullable: false),
                    CargoDiarioFijo = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecargoPolitica", x => x.IdRecargoPolitica);
                });

            migrationBuilder.CreateTable(
                name: "Recibo",
                columns: table => new
                {
                    IdRecibo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Folio = table.Column<string>(type: "text", nullable: true),
                    IdAspirante = table.Column<int>(type: "integer", nullable: true),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: true),
                    IdPeriodoAcademico = table.Column<int>(type: "integer", nullable: true),
                    FechaEmision = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Estatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Recargos = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, computedColumnSql: "ROUND(\"Subtotal\"-\"Descuento\"+\"Recargos\",2)", stored: true),
                    Saldo = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    IdEmpresa = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recibo", x => x.IdRecibo);
                });

            migrationBuilder.CreateTable(
                name: "TicketsSoporte",
                columns: table => new
                {
                    IdTicket = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Folio = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Prioridad = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Estatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Categoria = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UsuarioCreadorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    NombreCreador = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AreaDestino = table.Column<string>(type: "text", nullable: true),
                    UsuarioAsignadoId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    NombreAsignado = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ArchivoAdjuntoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ArchivoAdjuntoNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketsSoporte", x => x.IdTicket);
                });

            migrationBuilder.CreateTable(
                name: "TiposDocumentoEstudiante",
                columns: table => new
                {
                    IdTipoDocumento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Clave = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Precio = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    DiasVigencia = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    RequierePago = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Orden = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDocumentoEstudiante", x => x.IdTipoDocumento);
                });

            migrationBuilder.CreateTable(
                name: "Turno",
                columns: table => new
                {
                    IdTurno = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Clave = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Turno__C1ECF79ACE66190F", x => x.IdTurno);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
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
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
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
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
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
                name: "CorteCaja",
                columns: table => new
                {
                    IdCorteCaja = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FolioCorteCaja = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdUsuarioCaja = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IdCaja = table.Column<int>(type: "integer", nullable: true),
                    MontoInicial = table.Column<decimal>(type: "numeric(18,2)", precision: 12, scale: 2, nullable: false),
                    TotalEfectivo = table.Column<decimal>(type: "numeric(18,2)", precision: 12, scale: 2, nullable: false),
                    TotalTransferencia = table.Column<decimal>(type: "numeric(18,2)", precision: 12, scale: 2, nullable: false),
                    TotalTarjeta = table.Column<decimal>(type: "numeric(18,2)", precision: 12, scale: 2, nullable: false),
                    TotalGeneral = table.Column<decimal>(type: "numeric(18,2)", precision: 12, scale: 2, nullable: false),
                    Cerrado = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CerradoPor = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorteCaja", x => x.IdCorteCaja);
                    table.ForeignKey(
                        name: "FK_CorteCaja_AspNetUsers_CerradoPor",
                        column: x => x.CerradoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CorteCaja_AspNetUsers_IdUsuarioCaja",
                        column: x => x.IdUsuarioCaja,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Beca",
                columns: table => new
                {
                    IdBeca = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Clave = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "PORCENTAJE"),
                    Valor = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    TopeMensual = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    IdConceptoPago = table.Column<int>(type: "integer", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beca", x => x.IdBeca);
                    table.ForeignKey(
                        name: "FK_Beca_ConceptoPago_IdConceptoPago",
                        column: x => x.IdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConceptoPrecio",
                columns: table => new
                {
                    IdConceptoPrecio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdConceptoPago = table.Column<int>(type: "integer", nullable: false),
                    IdCampus = table.Column<int>(type: "integer", nullable: true),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: true),
                    Moneda = table.Column<string>(type: "text", nullable: true),
                    Importe = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    VigenciaDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenciaHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptoPrecio", x => x.IdConceptoPrecio);
                    table.ForeignKey(
                        name: "FK_ConceptoPrecio_ConceptoPago_IdConceptoPago",
                        column: x => x.IdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Municipios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    EstadoId = table.Column<string>(type: "text", nullable: false)
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
                name: "Pago",
                columns: table => new
                {
                    IdPago = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FolioPago = table.Column<string>(type: "text", nullable: true),
                    FechaPagoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdMedioPago = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "text", nullable: true),
                    Referencia = table.Column<string>(type: "text", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    Estatus = table.Column<int>(type: "integer", nullable: false),
                    IdUsuarioCaja = table.Column<string>(type: "text", nullable: true),
                    IdCaja = table.Column<int>(type: "integer", nullable: true),
                    IdCorteCaja = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pago", x => x.IdPago);
                    table.ForeignKey(
                        name: "FK_Pago_MedioPago_IdMedioPago",
                        column: x => x.IdMedioPago,
                        principalTable: "MedioPago",
                        principalColumn: "IdMedioPago",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanPago",
                columns: table => new
                {
                    IdPlanPago = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    IdPeriodicidad = table.Column<int>(type: "integer", nullable: false),
                    IdPeriodoAcademico = table.Column<int>(type: "integer", nullable: false),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: true),
                    IdModalidadPlan = table.Column<int>(type: "integer", nullable: false),
                    Moneda = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    VigenciaDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenciaHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanPago", x => x.IdPlanPago);
                    table.ForeignKey(
                        name: "FK_PlanPago_ModalidadPlan",
                        column: x => x.IdModalidadPlan,
                        principalTable: "ModalidadPlan",
                        principalColumn: "IdModalidadPlan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeriodoAcademico",
                columns: table => new
                {
                    IdPeriodoAcademico = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Clave = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdPeriodicidad = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    EsPeriodoActual = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "RolePermissions",
                columns: table => new
                {
                    IdRolePermission = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false),
                    CanView = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CanCreate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CanEdit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CanDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')"),
                    AssignedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.IdRolePermission);
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "IdPermission",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BitacoraRecibo",
                columns: table => new
                {
                    IdBitacora = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdRecibo = table.Column<long>(type: "bigint", nullable: false),
                    TipoRecibo = table.Column<string>(type: "text", nullable: true),
                    Usuario = table.Column<string>(type: "text", nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Accion = table.Column<string>(type: "text", nullable: true),
                    Origen = table.Column<string>(type: "text", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    ReciboIdRecibo = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacoraRecibo", x => x.IdBitacora);
                    table.ForeignKey(
                        name: "FK_BitacoraRecibo_Recibo_IdRecibo",
                        column: x => x.IdRecibo,
                        principalTable: "Recibo",
                        principalColumn: "IdRecibo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BitacoraRecibo_Recibo_ReciboIdRecibo",
                        column: x => x.ReciboIdRecibo,
                        principalTable: "Recibo",
                        principalColumn: "IdRecibo");
                });

            migrationBuilder.CreateTable(
                name: "LigaPago",
                columns: table => new
                {
                    IdLigaPago = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    TipoRecibo = table.Column<string>(type: "text", nullable: true),
                    IdRecibo = table.Column<long>(type: "bigint", nullable: false),
                    Folio = table.Column<string>(type: "text", nullable: true),
                    FechaGeneracionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaPrimeraVistaUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IPPrimeraVista = table.Column<string>(type: "text", nullable: true),
                    ReciboIdRecibo = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LigaPago", x => x.IdLigaPago);
                    table.ForeignKey(
                        name: "FK_LigaPago_Recibo_IdRecibo",
                        column: x => x.IdRecibo,
                        principalTable: "Recibo",
                        principalColumn: "IdRecibo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LigaPago_Recibo_ReciboIdRecibo",
                        column: x => x.ReciboIdRecibo,
                        principalTable: "Recibo",
                        principalColumn: "IdRecibo");
                });

            migrationBuilder.CreateTable(
                name: "ReciboDetalle",
                columns: table => new
                {
                    IdReciboDetalle = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdRecibo = table.Column<long>(type: "bigint", nullable: false),
                    IdConceptoPago = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Cantidad = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, computedColumnSql: "ROUND(\"Cantidad\"*\"PrecioUnitario\",2)", stored: true),
                    RefTabla = table.Column<string>(type: "text", nullable: true),
                    RefId = table.Column<long>(type: "bigint", nullable: true),
                    ConceptoPagoIdConceptoPago = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReciboDetalle", x => x.IdReciboDetalle);
                    table.ForeignKey(
                        name: "FK_ReciboDetalle_ConceptoPago_ConceptoPagoIdConceptoPago",
                        column: x => x.ConceptoPagoIdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago");
                    table.ForeignKey(
                        name: "FK_ReciboDetalle_Recibo_IdRecibo",
                        column: x => x.IdRecibo,
                        principalTable: "Recibo",
                        principalColumn: "IdRecibo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketComentarios",
                columns: table => new
                {
                    IdComentario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTicket = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    NombreUsuario = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Contenido = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivoAdjuntoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ArchivoAdjuntoNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "CodigosPostales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: true),
                    Asentamiento = table.Column<string>(type: "text", nullable: true),
                    MunicipioId = table.Column<string>(type: "text", nullable: false)
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
                name: "PagoMetodo",
                columns: table => new
                {
                    IdPagoMetodo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPago = table.Column<long>(type: "bigint", nullable: false),
                    IdMedioPago = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric", nullable: false),
                    Referencia = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagoMetodo", x => x.IdPagoMetodo);
                    table.ForeignKey(
                        name: "FK_PagoMetodo_MedioPago_IdMedioPago",
                        column: x => x.IdMedioPago,
                        principalTable: "MedioPago",
                        principalColumn: "IdMedioPago",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PagoMetodo_Pago_IdPago",
                        column: x => x.IdPago,
                        principalTable: "Pago",
                        principalColumn: "IdPago",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanPagoAsignacion",
                columns: table => new
                {
                    IdPlanPagoAsignacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPlanPago = table.Column<int>(type: "integer", nullable: false),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: false),
                    FechaAsignacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    PlanPagoIdPlanPago = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanPagoAsignacion", x => x.IdPlanPagoAsignacion);
                    table.ForeignKey(
                        name: "FK_PlanPagoAsignacion_PlanPago_IdPlanPago",
                        column: x => x.IdPlanPago,
                        principalTable: "PlanPago",
                        principalColumn: "IdPlanPago",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanPagoAsignacion_PlanPago_PlanPagoIdPlanPago",
                        column: x => x.PlanPagoIdPlanPago,
                        principalTable: "PlanPago",
                        principalColumn: "IdPlanPago");
                });

            migrationBuilder.CreateTable(
                name: "PlanPagoDetalle",
                columns: table => new
                {
                    IdPlanPagoDetalle = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPlanPago = table.Column<int>(type: "integer", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    IdConceptoPago = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Cantidad = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    EsInscripcion = table.Column<bool>(type: "boolean", nullable: false),
                    EsMensualidad = table.Column<bool>(type: "boolean", nullable: false),
                    MesOffset = table.Column<int>(type: "integer", nullable: false),
                    DiaPago = table.Column<byte>(type: "smallint", nullable: true),
                    PintaInternet = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanPagoDetalle", x => x.IdPlanPagoDetalle);
                    table.ForeignKey(
                        name: "FK_PlanPagoDetalle_ConceptoPago_IdConceptoPago",
                        column: x => x.IdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanPagoDetalle_PlanPago_IdPlanPago",
                        column: x => x.IdPlanPago,
                        principalTable: "PlanPago",
                        principalColumn: "IdPlanPago",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagoAplicacion",
                columns: table => new
                {
                    IdPagoAplicacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPago = table.Column<long>(type: "bigint", nullable: false),
                    IdReciboDetalle = table.Column<long>(type: "bigint", nullable: false),
                    MontoAplicado = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagoAplicacion", x => x.IdPagoAplicacion);
                    table.ForeignKey(
                        name: "FK_PagoAplicacion_Pago_IdPago",
                        column: x => x.IdPago,
                        principalTable: "Pago",
                        principalColumn: "IdPago",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PagoAplicacion_ReciboDetalle_IdReciboDetalle",
                        column: x => x.IdReciboDetalle,
                        principalTable: "ReciboDetalle",
                        principalColumn: "IdReciboDetalle",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Direccion",
                columns: table => new
                {
                    IdDireccion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Calle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumeroExterior = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    NumeroInterior = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CodigoPostalId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Direccio__1F8E0C76513A158C", x => x.IdDireccion);
                    table.ForeignKey(
                        name: "FK_Direccion_CodigosPostales_CodigoPostalId",
                        column: x => x.CodigoPostalId,
                        principalTable: "CodigosPostales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Campus",
                columns: table => new
                {
                    IdCampus = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClaveCampus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    IdDireccion = table.Column<int>(type: "integer", nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "Persona",
                columns: table => new
                {
                    IdPersona = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApellidoPaterno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ApellidoMaterno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    Curp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Rfc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IdDireccion = table.Column<int>(type: "integer", nullable: true),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Celular = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Correo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdGenero = table.Column<int>(type: "integer", nullable: true),
                    IdEstadoCivil = table.Column<int>(type: "integer", nullable: true),
                    NombreContactoEmergencia = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    TelefonoContactoEmergencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ParentescoContactoEmergencia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Nacionalidad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "ConfiguracionIPES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNombreInstitucion = table.Column<string>(type: "text", nullable: true),
                    NombreInstitucion = table.Column<string>(type: "text", nullable: true),
                    IdCampusSEP = table.Column<string>(type: "text", nullable: true),
                    CampusSEP = table.Column<string>(type: "text", nullable: true),
                    IdEntidadFederativa = table.Column<string>(type: "text", nullable: true),
                    EntidadFederativa = table.Column<string>(type: "text", nullable: true),
                    IdCampus = table.Column<int>(type: "integer", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionIPES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionIPES_Campus_IdCampus",
                        column: x => x.IdCampus,
                        principalTable: "Campus",
                        principalColumn: "IdCampus");
                });

            migrationBuilder.CreateTable(
                name: "PlanEstudios",
                columns: table => new
                {
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClavePlanEstudios = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NombrePlanEstudios = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RVOE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FechaExpedicionRvoe = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IdCarreraSEP = table.Column<int>(type: "integer", nullable: true),
                    PermiteAdelantar = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    Version = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IdProgramaEstudios = table.Column<int>(type: "integer", nullable: true),
                    DuracionMeses = table.Column<int>(type: "integer", nullable: true, defaultValue: 48),
                    MinimaAprobatoriaParcial = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    MinimaAprobatoriaFinal = table.Column<int>(type: "integer", nullable: false, defaultValue: 70),
                    IdPeriodicidad = table.Column<int>(type: "integer", nullable: false),
                    IdNivelEducativo = table.Column<int>(type: "integer", nullable: false),
                    IdCampus = table.Column<int>(type: "integer", nullable: false),
                    EsOficial = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "Profesor",
                columns: table => new
                {
                    IdProfesor = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NoEmpleado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IdPersona = table.Column<int>(type: "integer", nullable: false),
                    EmailInstitucional = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UsuarioId = table.Column<string>(type: "text", nullable: true),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Profesor__C377C3A119E36880", x => x.IdProfesor);
                    table.ForeignKey(
                        name: "FK_Profesor_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Profesor_Campus_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campus",
                        principalColumn: "IdCampus");
                    table.ForeignKey(
                        name: "FK_Profesor_Persona",
                        column: x => x.IdPersona,
                        principalTable: "Persona",
                        principalColumn: "IdPersona");
                });

            migrationBuilder.CreateTable(
                name: "CredencialSEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Usuario = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    EndpointUrl = table.Column<string>(type: "text", nullable: true),
                    EsProduccion = table.Column<bool>(type: "boolean", nullable: false),
                    IdConfiguracionIPES = table.Column<int>(type: "integer", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredencialSEP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CredencialSEP_ConfiguracionIPES_IdConfiguracionIPES",
                        column: x => x.IdConfiguracionIPES,
                        principalTable: "ConfiguracionIPES",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ResponsableFirma",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Curp = table.Column<string>(type: "text", nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    PrimerApellido = table.Column<string>(type: "text", nullable: true),
                    SegundoApellido = table.Column<string>(type: "text", nullable: true),
                    IdCargo = table.Column<string>(type: "text", nullable: true),
                    Cargo = table.Column<string>(type: "text", nullable: true),
                    RutaCertificadoCer = table.Column<string>(type: "text", nullable: true),
                    RutaLlavePrivadaKey = table.Column<string>(type: "text", nullable: true),
                    PasswordLlavePrivada = table.Column<string>(type: "text", nullable: true),
                    NoCertificadoResponsable = table.Column<string>(type: "text", nullable: true),
                    IdConfiguracionIPES = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    VigenciaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VigenciaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsableFirma", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponsableFirma_ConfiguracionIPES_IdConfiguracionIPES",
                        column: x => x.IdConfiguracionIPES,
                        principalTable: "ConfiguracionIPES",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Aspirante",
                columns: table => new
                {
                    IdAspirante = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPersona = table.Column<int>(type: "integer", nullable: true),
                    IdAspiranteEstatus = table.Column<int>(type: "integer", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')"),
                    IdPlan = table.Column<int>(type: "integer", nullable: false),
                    IdMedioContacto = table.Column<int>(type: "integer", nullable: false),
                    IdAtendidoPorUsuario = table.Column<string>(type: "text", nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TurnoId = table.Column<int>(type: "integer", nullable: true),
                    CuatrimestreInteres = table.Column<int>(type: "integer", nullable: true),
                    InstitucionProcedencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IdModalidad = table.Column<int>(type: "integer", nullable: true),
                    GrupoDiasImparticion = table.Column<int>(type: "integer", nullable: true),
                    IdPeriodoAcademico = table.Column<int>(type: "integer", nullable: true),
                    RecorridoPlantel = table.Column<bool>(type: "boolean", nullable: true),
                    Trabaja = table.Column<bool>(type: "boolean", nullable: true),
                    NombreEmpresa = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DomicilioEmpresa = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PuestoEmpresa = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    QuienCubreGastos = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IdEmpresa = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Aspirant__09EE6349C82C95C4", x => x.IdAspirante);
                    table.ForeignKey(
                        name: "FK_Aspirante_Empresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresas",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.SetNull);
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
                        name: "FK_Aspirante_Modalidad",
                        column: x => x.IdModalidad,
                        principalTable: "Modalidad",
                        principalColumn: "IdModalidad");
                    table.ForeignKey(
                        name: "FK_Aspirante_PeriodoAcademico",
                        column: x => x.IdPeriodoAcademico,
                        principalTable: "PeriodoAcademico",
                        principalColumn: "IdPeriodoAcademico");
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
                    table.ForeignKey(
                        name: "FK_Aspirante_Turno_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turno",
                        principalColumn: "IdTurno");
                });

            migrationBuilder.CreateTable(
                name: "ConvenioAlcance",
                columns: table => new
                {
                    IdConvenioAlcance = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdConvenio = table.Column<int>(type: "integer", nullable: false),
                    IdCampus = table.Column<int>(type: "integer", nullable: true),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: true),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: true),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                    IdEstudiante = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Matricula = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IdPersona = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CURRENT_DATE)"),
                    IdPlanActual = table.Column<int>(type: "integer", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EstatusAcademico = table.Column<int>(type: "integer", nullable: false),
                    TipoBaja = table.Column<int>(type: "integer", nullable: true),
                    EstadoBaja = table.Column<int>(type: "integer", nullable: true),
                    MotivoBaja = table.Column<string>(type: "text", nullable: true),
                    FechaBaja = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Estudian__B5007C24138D11BB", x => x.IdEstudiante);
                    table.ForeignKey(
                        name: "FK_Estudiante_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
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
                    IdGrupo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreGrupo = table.Column<string>(type: "text", nullable: true),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: false),
                    IdPeriodoAcademico = table.Column<int>(type: "integer", nullable: false),
                    NumeroCuatrimestre = table.Column<byte>(type: "smallint", nullable: false),
                    NumeroGrupo = table.Column<byte>(type: "smallint", nullable: false),
                    IdTurno = table.Column<int>(type: "integer", nullable: false),
                    CapacidadMaxima = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)40),
                    CodigoGrupo = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                    IdMateriaPlan = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: false),
                    IdMateria = table.Column<int>(type: "integer", nullable: false),
                    Cuatrimestre = table.Column<byte>(type: "smallint", nullable: false),
                    EsOptativa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "PlanDocumentoRequisito",
                columns: table => new
                {
                    IdPlanDocumentoRequisito = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: false),
                    IdDocumentoRequisito = table.Column<int>(type: "integer", nullable: false),
                    EsObligatorio = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanDocumentoRequisito", x => x.IdPlanDocumentoRequisito);
                    table.ForeignKey(
                        name: "FK_PlanDocumentoRequisito_DocumentoRequisito_IdDocumentoRequis~",
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

            migrationBuilder.CreateTable(
                name: "PlanModalidadDia",
                columns: table => new
                {
                    IdPlanModalidadDia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: false),
                    IdModalidad = table.Column<int>(type: "integer", nullable: false),
                    Grupo = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IdDiaSemana = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanModalidadDia", x => x.IdPlanModalidadDia);
                    table.ForeignKey(
                        name: "FK_PlanModalidadDia_DiaSemana",
                        column: x => x.IdDiaSemana,
                        principalTable: "DiaSemana",
                        principalColumn: "IdDiaSemana");
                    table.ForeignKey(
                        name: "FK_PlanModalidadDia_Modalidad",
                        column: x => x.IdModalidad,
                        principalTable: "Modalidad",
                        principalColumn: "IdModalidad");
                    table.ForeignKey(
                        name: "FK_PlanModalidadDia_Plan",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios");
                });

            migrationBuilder.CreateTable(
                name: "PlantillasCobro",
                columns: table => new
                {
                    IdPlantillaCobro = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombrePlantilla = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: false),
                    NumeroCuatrimestre = table.Column<int>(type: "integer", nullable: false),
                    IdPeriodoAcademico = table.Column<int>(type: "integer", nullable: true),
                    IdTurno = table.Column<int>(type: "integer", nullable: true),
                    IdModalidad = table.Column<int>(type: "integer", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    EsActiva = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaVigenciaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaVigenciaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstrategiaEmision = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NumeroRecibos = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    DiaVencimiento = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    CreadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')"),
                    ModificadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasCobro", x => x.IdPlantillaCobro);
                    table.ForeignKey(
                        name: "FK_PlantillaCobro_Modalidad",
                        column: x => x.IdModalidad,
                        principalTable: "Modalidad",
                        principalColumn: "IdModalidad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlantillasCobro_PlanEstudios_IdPlanEstudios",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesPlanEstudios",
                columns: table => new
                {
                    IdSolicitudPlanEstudios = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: false),
                    ClavePlanEstudios = table.Column<string>(type: "text", nullable: true),
                    NombrePlanEstudios = table.Column<string>(type: "text", nullable: true),
                    Campus = table.Column<string>(type: "text", nullable: true),
                    Rvoe = table.Column<string>(type: "text", nullable: true),
                    EstatusSolicitud = table.Column<string>(type: "text", nullable: true),
                    SolicitadoPor = table.Column<string>(type: "text", nullable: true),
                    AprobadoPor = table.Column<string>(type: "text", nullable: true),
                    ComentarioRevision = table.Column<string>(type: "text", nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesPlanEstudios", x => x.IdSolicitudPlanEstudios);
                    table.ForeignKey(
                        name: "FK_SolicitudesPlanEstudios_PlanEstudios_IdPlanEstudios",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios");
                });

            migrationBuilder.CreateTable(
                name: "TarifasAdmision",
                columns: table => new
                {
                    IdTarifaAdmision = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AplicaConvenioMensualidad = table.Column<bool>(type: "boolean", nullable: false),
                    EsConvenioEmpresarial = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarifasAdmision", x => x.IdTarifaAdmision);
                    table.ForeignKey(
                        name: "FK_TarifasAdmision_PlanEstudios_IdPlanEstudios",
                        column: x => x.IdPlanEstudios,
                        principalTable: "PlanEstudios",
                        principalColumn: "IdPlanEstudios",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspiranteBitacoraSeguimiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AspiranteId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioAtiendeId = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MedioContacto = table.Column<string>(type: "text", nullable: true),
                    Resumen = table.Column<string>(type: "text", nullable: true),
                    ProximaAccion = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspiranteConvenio",
                columns: table => new
                {
                    IdAspiranteConvenio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAspirante = table.Column<int>(type: "integer", nullable: false),
                    IdConvenio = table.Column<int>(type: "integer", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')"),
                    Estatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "Pendiente"),
                    Evidencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VecesAplicado = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "AspiranteDocumento",
                columns: table => new
                {
                    IdAspiranteDocumento = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAspirante = table.Column<int>(type: "integer", nullable: false),
                    IdDocumentoRequisito = table.Column<int>(type: "integer", nullable: false),
                    Estatus = table.Column<int>(type: "integer", nullable: false),
                    FechaSubidoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UrlArchivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaValidacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioValidacion = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    FechaProrroga = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoProrroga = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UsuarioProrroga = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    FechaProrrogaAsignada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspiranteDocumento", x => x.IdAspiranteDocumento);
                    table.ForeignKey(
                        name: "FK_AspiranteDocumento_Aspirante_IdAspirante",
                        column: x => x.IdAspirante,
                        principalTable: "Aspirante",
                        principalColumn: "IdAspirante",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspiranteDocumento_DocumentoRequisito_IdDocumentoRequisito",
                        column: x => x.IdDocumentoRequisito,
                        principalTable: "DocumentoRequisito",
                        principalColumn: "IdDocumentoRequisito",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BecaAsignacion",
                columns: table => new
                {
                    IdBecaAsignacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: false),
                    IdBeca = table.Column<int>(type: "integer", nullable: true),
                    IdConceptoPago = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<string>(type: "text", nullable: true),
                    Valor = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    TopeMensual = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    IdPeriodoAcademico = table.Column<int>(type: "integer", nullable: true),
                    VigenciaDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenciaHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BecaAsignacion", x => x.IdBecaAsignacion);
                    table.ForeignKey(
                        name: "FK_BecaAsignacion_Beca_IdBeca",
                        column: x => x.IdBeca,
                        principalTable: "Beca",
                        principalColumn: "IdBeca",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BecaAsignacion_ConceptoPago_IdConceptoPago",
                        column: x => x.IdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BecaAsignacion_Estudiante_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BecaAsignacion_PeriodoAcademico_IdPeriodoAcademico",
                        column: x => x.IdPeriodoAcademico,
                        principalTable: "PeriodoAcademico",
                        principalColumn: "IdPeriodoAcademico",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CertificadoElectronico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FolioControl = table.Column<string>(type: "text", nullable: true),
                    TipoTitulacion = table.Column<int>(type: "integer", nullable: false),
                    Estatus = table.Column<int>(type: "integer", nullable: false),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: true),
                    IdPersona = table.Column<int>(type: "integer", nullable: true),
                    NumeroControl = table.Column<string>(type: "text", nullable: true),
                    Curp = table.Column<string>(type: "text", nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    PrimerApellido = table.Column<string>(type: "text", nullable: true),
                    SegundoApellido = table.Column<string>(type: "text", nullable: true),
                    IdGenero = table.Column<int>(type: "integer", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FotoHash = table.Column<string>(type: "text", nullable: true),
                    FirmaAutografaHash = table.Column<string>(type: "text", nullable: true),
                    IdCarreraSEP = table.Column<string>(type: "text", nullable: true),
                    ClaveCarrera = table.Column<string>(type: "text", nullable: true),
                    NombreCarrera = table.Column<string>(type: "text", nullable: true),
                    IdTipoPeriodo = table.Column<string>(type: "text", nullable: true),
                    TipoPeriodo = table.Column<string>(type: "text", nullable: true),
                    ClavePlan = table.Column<string>(type: "text", nullable: true),
                    NumeroRvoe = table.Column<string>(type: "text", nullable: true),
                    FechaExpedicionRvoe = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdTipoCertificacion = table.Column<string>(type: "text", nullable: true),
                    TipoCertificacion = table.Column<string>(type: "text", nullable: true),
                    FechaExpedicion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdLugarExpedicion = table.Column<string>(type: "text", nullable: true),
                    LugarExpedicion = table.Column<string>(type: "text", nullable: true),
                    TotalAsignaturas = table.Column<int>(type: "integer", nullable: false),
                    AsignaturasAsignadas = table.Column<int>(type: "integer", nullable: false),
                    Promedio = table.Column<string>(type: "text", nullable: true),
                    CreditosObtenidos = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalCreditos = table.Column<decimal>(type: "numeric", nullable: true),
                    NumeroCiclos = table.Column<int>(type: "integer", nullable: true),
                    IdConfiguracionIPES = table.Column<int>(type: "integer", nullable: false),
                    IdResponsableFirma = table.Column<int>(type: "integer", nullable: true),
                    SelloDigital = table.Column<string>(type: "text", nullable: true),
                    CadenaOriginal = table.Column<string>(type: "text", nullable: true),
                    XmlGenerado = table.Column<string>(type: "text", nullable: true),
                    NumeroLoteSEP = table.Column<int>(type: "integer", nullable: true),
                    EstatusLoteSEP = table.Column<int>(type: "integer", nullable: true),
                    FolioControlSEP = table.Column<string>(type: "text", nullable: true),
                    MensajeSEP = table.Column<string>(type: "text", nullable: true),
                    FechaEnvioSEP = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaRespuestaSEP = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificadoElectronico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificadoElectronico_ConfiguracionIPES_IdConfiguracionIPES",
                        column: x => x.IdConfiguracionIPES,
                        principalTable: "ConfiguracionIPES",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CertificadoElectronico_Estudiante_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante");
                    table.ForeignKey(
                        name: "FK_CertificadoElectronico_Persona_IdPersona",
                        column: x => x.IdPersona,
                        principalTable: "Persona",
                        principalColumn: "IdPersona");
                    table.ForeignKey(
                        name: "FK_CertificadoElectronico_ResponsableFirma_IdResponsableFirma",
                        column: x => x.IdResponsableFirma,
                        principalTable: "ResponsableFirma",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EstudiantePlan",
                columns: table => new
                {
                    IdEstudiantePlan = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: false),
                    IdPlanEstudios = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CURRENT_DATE)"),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "SeguimientoEgresados",
                columns: table => new
                {
                    IdSeguimientoEgresado = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: true),
                    Matricula = table.Column<string>(type: "text", nullable: true),
                    NombreCompleto = table.Column<string>(type: "text", nullable: true),
                    ProgramaAcademico = table.Column<string>(type: "text", nullable: true),
                    Expediente = table.Column<string>(type: "text", nullable: true),
                    PagoTitulacion = table.Column<string>(type: "text", nullable: true),
                    LiberacionServicioSocial = table.Column<string>(type: "text", nullable: true),
                    FechaSolicitudTitulacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstatusTitulacion = table.Column<string>(type: "text", nullable: true),
                    EstatusCertificado = table.Column<string>(type: "text", nullable: true),
                    EstatusTituloElectronico = table.Column<string>(type: "text", nullable: true),
                    EstatusTituloFisico = table.Column<string>(type: "text", nullable: true),
                    PagoCedula = table.Column<string>(type: "text", nullable: true),
                    TramiteCedula = table.Column<string>(type: "text", nullable: true),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    NumeroProgramaAcademico = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeguimientoEgresados", x => x.IdSeguimientoEgresado);
                    table.ForeignKey(
                        name: "FK_SeguimientoEgresados_Estudiante_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante");
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesBaja",
                columns: table => new
                {
                    IdSolicitudBaja = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: false),
                    Matricula = table.Column<string>(type: "text", nullable: true),
                    NombreEstudiante = table.Column<string>(type: "text", nullable: true),
                    Carrera = table.Column<string>(type: "text", nullable: true),
                    TipoBaja = table.Column<int>(type: "integer", nullable: true),
                    EstadoBaja = table.Column<int>(type: "integer", nullable: true),
                    MotivoBaja = table.Column<string>(type: "text", nullable: true),
                    MontoAdeudo = table.Column<decimal>(type: "numeric", nullable: false),
                    RecibosVencidos = table.Column<int>(type: "integer", nullable: false),
                    RecibosPendientes = table.Column<int>(type: "integer", nullable: false),
                    EstatusSolicitud = table.Column<string>(type: "text", nullable: true),
                    SolicitadoPor = table.Column<string>(type: "text", nullable: true),
                    AutorizadoPor = table.Column<string>(type: "text", nullable: true),
                    ComentarioFinanzas = table.Column<string>(type: "text", nullable: true),
                    FechaAutorizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesBaja", x => x.IdSolicitudBaja);
                    table.ForeignKey(
                        name: "FK_SolicitudesBaja_Estudiante_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante");
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesDocumento",
                columns: table => new
                {
                    IdSolicitud = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FolioSolicitud = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: false),
                    IdTipoDocumento = table.Column<int>(type: "integer", nullable: false),
                    IdRecibo = table.Column<long>(type: "bigint", nullable: true),
                    Variante = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')"),
                    FechaGeneracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Estatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CodigoVerificacion = table.Column<Guid>(type: "uuid", nullable: false),
                    VecesImpreso = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UsuarioSolicita = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    UsuarioGenera = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioEntrega = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
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
                        name: "FK_SolicitudesDocumento_TiposDocumentoEstudiante_IdTipoDocumen~",
                        column: x => x.IdTipoDocumento,
                        principalTable: "TiposDocumentoEstudiante",
                        principalColumn: "IdTipoDocumento",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EstudianteGrupo",
                columns: table => new
                {
                    IdEstudianteGrupo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: false),
                    IdGrupo = table.Column<int>(type: "integer", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')"),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true, defaultValue: "Inscrito"),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstudianteGrupo", x => x.IdEstudianteGrupo);
                    table.ForeignKey(
                        name: "FK_EstudianteGrupo_Estudiante_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstudianteGrupo_Grupo_IdGrupo",
                        column: x => x.IdGrupo,
                        principalTable: "Grupo",
                        principalColumn: "IdGrupo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GrupoMateria",
                columns: table => new
                {
                    IdGrupoMateria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IdGrupo = table.Column<int>(type: "integer", nullable: false),
                    IdMateriaPlan = table.Column<int>(type: "integer", nullable: false),
                    IdProfesor = table.Column<int>(type: "integer", nullable: true),
                    Aula = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Cupo = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)40),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "PlantillasCobroDetalles",
                columns: table => new
                {
                    IdPlantillaDetalle = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPlantillaCobro = table.Column<int>(type: "integer", nullable: false),
                    IdConceptoPago = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false, defaultValue: 1m),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AplicaEnRecibo = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasCobroDetalles", x => x.IdPlantillaDetalle);
                    table.ForeignKey(
                        name: "FK_PlantillasCobroDetalles_ConceptoPago_IdConceptoPago",
                        column: x => x.IdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlantillasCobroDetalles_PlantillasCobro_IdPlantillaCobro",
                        column: x => x.IdPlantillaCobro,
                        principalTable: "PlantillasCobro",
                        principalColumn: "IdPlantillaCobro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TarifasAdmisionDetalles",
                columns: table => new
                {
                    IdTarifaAdmisionDetalle = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTarifaAdmision = table.Column<int>(type: "integer", nullable: false),
                    IdConceptoPago = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    EsAplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarifasAdmisionDetalles", x => x.IdTarifaAdmisionDetalle);
                    table.ForeignKey(
                        name: "FK_TarifasAdmisionDetalles_ConceptoPago_IdConceptoPago",
                        column: x => x.IdConceptoPago,
                        principalTable: "ConceptoPago",
                        principalColumn: "IdConceptoPago",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TarifasAdmisionDetalles_TarifasAdmision_IdTarifaAdmision",
                        column: x => x.IdTarifaAdmision,
                        principalTable: "TarifasAdmision",
                        principalColumn: "IdTarifaAdmision",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CertificadoAsignatura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCertificadoElectronico = table.Column<int>(type: "integer", nullable: false),
                    IdAsignatura = table.Column<int>(type: "integer", nullable: false),
                    ClaveAsignatura = table.Column<string>(type: "text", nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    Ciclo = table.Column<string>(type: "text", nullable: true),
                    Calificacion = table.Column<string>(type: "text", nullable: true),
                    Creditos = table.Column<decimal>(type: "numeric", nullable: true),
                    IdTipoAsignatura = table.Column<int>(type: "integer", nullable: true),
                    TipoAsignatura = table.Column<string>(type: "text", nullable: true),
                    IdObservaciones = table.Column<int>(type: "integer", nullable: true),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificadoAsignatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificadoAsignatura_CertificadoElectronico_IdCertificadoE~",
                        column: x => x.IdCertificadoElectronico,
                        principalTable: "CertificadoElectronico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Horario",
                columns: table => new
                {
                    IdHorario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdGrupoMateria = table.Column<int>(type: "integer", nullable: false),
                    IdDiaSemana = table.Column<byte>(type: "smallint", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time(0) without time zone", precision: 0, nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time(0) without time zone", precision: 0, nullable: false),
                    Aula = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                    IdInscripcion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: false),
                    IdGrupoMateria = table.Column<int>(type: "integer", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')"),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "Inscrito"),
                    CalificacionFinal = table.Column<decimal>(type: "numeric(4,1)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "PlaneacionDocente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdProfesor = table.Column<int>(type: "integer", nullable: false),
                    IdGrupoMateria = table.Column<int>(type: "integer", nullable: false),
                    NombreArchivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UrlArchivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TipoArchivo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaneacionDocente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaneacionDocente_GrupoMateria_IdGrupoMateria",
                        column: x => x.IdGrupoMateria,
                        principalTable: "GrupoMateria",
                        principalColumn: "IdGrupoMateria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlaneacionDocente_Profesor_IdProfesor",
                        column: x => x.IdProfesor,
                        principalTable: "Profesor",
                        principalColumn: "IdProfesor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TareaDocente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdGrupoMateria = table.Column<int>(type: "integer", nullable: false),
                    IdProfesor = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaLimite = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PuntosMaximos = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TareaDocente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TareaDocente_GrupoMateria_IdGrupoMateria",
                        column: x => x.IdGrupoMateria,
                        principalTable: "GrupoMateria",
                        principalColumn: "IdGrupoMateria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TareaDocente_Profesor_IdProfesor",
                        column: x => x.IdProfesor,
                        principalTable: "Profesor",
                        principalColumn: "IdProfesor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Asistencia",
                columns: table => new
                {
                    IdAsistencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InscripcionId = table.Column<int>(type: "integer", nullable: false),
                    GrupoMateriaId = table.Column<int>(type: "integer", nullable: false),
                    FechaSesion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstadoAsistencia = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProfesorRegistroId = table.Column<int>(type: "integer", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencia", x => x.IdAsistencia);
                    table.ForeignKey(
                        name: "FK_Asistencia_GrupoMateria",
                        column: x => x.GrupoMateriaId,
                        principalTable: "GrupoMateria",
                        principalColumn: "IdGrupoMateria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistencia_Inscripcion",
                        column: x => x.InscripcionId,
                        principalTable: "Inscripcion",
                        principalColumn: "IdInscripcion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistencia_Profesor",
                        column: x => x.ProfesorRegistroId,
                        principalTable: "Profesor",
                        principalColumn: "IdProfesor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalificacionesParciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoMateriaId = table.Column<int>(type: "integer", nullable: false),
                    ParcialId = table.Column<int>(type: "integer", nullable: false),
                    InscripcionId = table.Column<int>(type: "integer", nullable: false),
                    ProfesorId = table.Column<int>(type: "integer", nullable: false),
                    StatusParcial = table.Column<int>(type: "integer", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionesParciales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalificacionesParciales_GrupoMateria_GrupoMateriaId",
                        column: x => x.GrupoMateriaId,
                        principalTable: "GrupoMateria",
                        principalColumn: "IdGrupoMateria",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesParciales_Inscripcion_InscripcionId",
                        column: x => x.InscripcionId,
                        principalTable: "Inscripcion",
                        principalColumn: "IdInscripcion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesParciales_Parciales_ParcialId",
                        column: x => x.ParcialId,
                        principalTable: "Parciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesParciales_Profesor_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Profesor",
                        principalColumn: "IdProfesor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntregaTarea",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTarea = table.Column<int>(type: "integer", nullable: false),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: false),
                    NombreArchivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UrlArchivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoArchivo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Calificacion = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Retroalimentacion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaRevision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Revisada = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntregaTarea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntregaTarea_Estudiante_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiante",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntregaTarea_TareaDocente_IdTarea",
                        column: x => x.IdTarea,
                        principalTable: "TareaDocente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalificacionDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CalificacionParcialId = table.Column<int>(type: "integer", nullable: false),
                    GrupoMateriaId = table.Column<int>(type: "integer", nullable: false),
                    TipoEvaluacionEnum = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    PesoEvaluacion = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    MaxPuntos = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    FechaAplicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Puntos = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ApplicationUserName = table.Column<string>(type: "text", nullable: true),
                    FechaCaptura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalificacionDetalle_CalificacionesParciales_CalificacionPar~",
                        column: x => x.CalificacionParcialId,
                        principalTable: "CalificacionesParciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_GrupoMateria_Fecha",
                table: "Asistencia",
                columns: new[] { "GrupoMateriaId", "FechaSesion" });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_Inscripcion_Fecha",
                table: "Asistencia",
                columns: new[] { "InscripcionId", "FechaSesion" });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_ProfesorRegistroId",
                table: "Asistencia",
                column: "ProfesorRegistroId");

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdAspiranteEstatus",
                table: "Aspirante",
                column: "IdAspiranteEstatus");

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdEmpresa",
                table: "Aspirante",
                column: "IdEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdMedioContacto",
                table: "Aspirante",
                column: "IdMedioContacto");

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdModalidad",
                table: "Aspirante",
                column: "IdModalidad");

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdPeriodoAcademico",
                table: "Aspirante",
                column: "IdPeriodoAcademico");

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdPersona",
                table: "Aspirante",
                column: "IdPersona");

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_IdPlan",
                table: "Aspirante",
                column: "IdPlan");

            migrationBuilder.CreateIndex(
                name: "IX_Aspirante_TurnoId",
                table: "Aspirante",
                column: "TurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteBitacoraSeguimiento_AspiranteId",
                table: "AspiranteBitacoraSeguimiento",
                column: "AspiranteId");

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteBitacoraSeguimiento_UsuarioAtiendeId",
                table: "AspiranteBitacoraSeguimiento",
                column: "UsuarioAtiendeId");

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
                name: "IX_AspiranteDocumento_IdAspirante_IdDocumentoRequisito",
                table: "AspiranteDocumento",
                columns: new[] { "IdAspirante", "IdDocumentoRequisito" },
                unique: true,
                filter: "\"Status\" <> 0");

            migrationBuilder.CreateIndex(
                name: "IX_AspiranteDocumento_IdDocumentoRequisito",
                table: "AspiranteDocumento",
                column: "IdDocumentoRequisito");

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
                unique: true);

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
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beca_Clave",
                table: "Beca",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beca_IdConceptoPago",
                table: "Beca",
                column: "IdConceptoPago");

            migrationBuilder.CreateIndex(
                name: "IX_BecaAsignacion_IdBeca",
                table: "BecaAsignacion",
                column: "IdBeca");

            migrationBuilder.CreateIndex(
                name: "IX_BecaAsignacion_IdConceptoPago",
                table: "BecaAsignacion",
                column: "IdConceptoPago");

            migrationBuilder.CreateIndex(
                name: "IX_BecaAsignacion_IdEstudiante",
                table: "BecaAsignacion",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_BecaAsignacion_IdPeriodoAcademico",
                table: "BecaAsignacion",
                column: "IdPeriodoAcademico");

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
                name: "IX_BitacoraRecibo_IdRecibo",
                table: "BitacoraRecibo",
                column: "IdRecibo");

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraRecibo_ReciboIdRecibo",
                table: "BitacoraRecibo",
                column: "ReciboIdRecibo");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionDetalle_CalificacionParcialId",
                table: "CalificacionDetalle",
                column: "CalificacionParcialId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesParciales_GrupoMateriaId",
                table: "CalificacionesParciales",
                column: "GrupoMateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesParciales_InscripcionId",
                table: "CalificacionesParciales",
                column: "InscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesParciales_ParcialId",
                table: "CalificacionesParciales",
                column: "ParcialId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesParciales_ProfesorId",
                table: "CalificacionesParciales",
                column: "ProfesorId");

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
                name: "IX_CertificadoAsignatura_IdCertificadoElectronico",
                table: "CertificadoAsignatura",
                column: "IdCertificadoElectronico");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadoElectronico_IdConfiguracionIPES",
                table: "CertificadoElectronico",
                column: "IdConfiguracionIPES");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadoElectronico_IdEstudiante",
                table: "CertificadoElectronico",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadoElectronico_IdPersona",
                table: "CertificadoElectronico",
                column: "IdPersona");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadoElectronico_IdResponsableFirma",
                table: "CertificadoElectronico",
                column: "IdResponsableFirma");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosPostales_MunicipioId",
                table: "CodigosPostales",
                column: "MunicipioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptoPrecio_IdConceptoPago",
                table: "ConceptoPrecio",
                column: "IdConceptoPago");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionIPES_IdCampus",
                table: "ConfiguracionIPES",
                column: "IdCampus");

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
                name: "IX_CorteCaja_CerradoPor",
                table: "CorteCaja",
                column: "CerradoPor");

            migrationBuilder.CreateIndex(
                name: "IX_CorteCaja_IdUsuarioCaja",
                table: "CorteCaja",
                column: "IdUsuarioCaja");

            migrationBuilder.CreateIndex(
                name: "IX_CredencialSEP_IdConfiguracionIPES",
                table: "CredencialSEP",
                column: "IdConfiguracionIPES");

            migrationBuilder.CreateIndex(
                name: "UQ__DiaSeman__75E3EFCF34622C9D",
                table: "DiaSemana",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Direccion_CodigoPostalId",
                table: "Direccion",
                column: "CodigoPostalId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoRequisito_Clave",
                table: "DocumentoRequisito",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Nombre",
                table: "Empresas",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_EntregaTarea_IdEstudiante",
                table: "EntregaTarea",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_EntregaTarea_IdTarea_IdEstudiante",
                table: "EntregaTarea",
                columns: new[] { "IdTarea", "IdEstudiante" });

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
                name: "IX_Estudiante_UsuarioId",
                table: "Estudiante",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UQ__Estudian__0FB9FB4F890AE66D",
                table: "Estudiante",
                column: "Matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstudianteGrupo_IdEstudiante_IdGrupo",
                table: "EstudianteGrupo",
                columns: new[] { "IdEstudiante", "IdGrupo" },
                unique: true,
                filter: "\"Status\" <> 0");

            migrationBuilder.CreateIndex(
                name: "IX_EstudianteGrupo_IdGrupo",
                table: "EstudianteGrupo",
                column: "IdGrupo");

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
                columns: new[] { "IdPlanEstudios", "IdPeriodoAcademico", "NumeroCuatrimestre", "NumeroGrupo", "IdTurno" },
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
                name: "IX_LigaPago_IdRecibo",
                table: "LigaPago",
                column: "IdRecibo");

            migrationBuilder.CreateIndex(
                name: "IX_LigaPago_ReciboIdRecibo",
                table: "LigaPago",
                column: "ReciboIdRecibo");

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
                name: "UQ_Modalidad",
                table: "Modalidad",
                column: "DescModalidad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ModalidadPlan",
                table: "ModalidadPlan",
                column: "DescModalidadPlan",
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

            migrationBuilder.CreateIndex(
                name: "IX_Pago_IdMedioPago",
                table: "Pago",
                column: "IdMedioPago");

            migrationBuilder.CreateIndex(
                name: "IX_PagoAplicacion_IdPago",
                table: "PagoAplicacion",
                column: "IdPago");

            migrationBuilder.CreateIndex(
                name: "IX_PagoAplicacion_IdReciboDetalle",
                table: "PagoAplicacion",
                column: "IdReciboDetalle");

            migrationBuilder.CreateIndex(
                name: "IX_PagoMetodo_IdMedioPago",
                table: "PagoMetodo",
                column: "IdMedioPago");

            migrationBuilder.CreateIndex(
                name: "IX_PagoMetodo_IdPago",
                table: "PagoMetodo",
                column: "IdPago");

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
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Module",
                table: "Permissions",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_ApellidoMaterno",
                table: "Persona",
                column: "ApellidoMaterno");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_ApellidoPaterno",
                table: "Persona",
                column: "ApellidoPaterno");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_Curp",
                table: "Persona",
                column: "Curp");

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
                name: "IX_Persona_Nombre",
                table: "Persona",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "UQ_Persona_CURP",
                table: "Persona",
                column: "Curp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Persona_Email",
                table: "Persona",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Persona_RFC",
                table: "Persona",
                column: "Rfc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanDocumentoRequisito_IdDocumentoRequisito",
                table: "PlanDocumentoRequisito",
                column: "IdDocumentoRequisito");

            migrationBuilder.CreateIndex(
                name: "IX_PlanDocumentoRequisito_IdPlanEstudios_IdDocumentoRequisito",
                table: "PlanDocumentoRequisito",
                columns: new[] { "IdPlanEstudios", "IdDocumentoRequisito" },
                unique: true,
                filter: "\"Status\" <> 0");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneacionDocente_IdGrupoMateria",
                table: "PlaneacionDocente",
                column: "IdGrupoMateria");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneacionDocente_IdProfesor_IdGrupoMateria",
                table: "PlaneacionDocente",
                columns: new[] { "IdProfesor", "IdGrupoMateria" });

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
                name: "UQ_PlanEstudios_Campus",
                table: "PlanEstudios",
                columns: new[] { "ClavePlanEstudios", "IdCampus" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanModalidadDia_IdDiaSemana",
                table: "PlanModalidadDia",
                column: "IdDiaSemana");

            migrationBuilder.CreateIndex(
                name: "IX_PlanModalidadDia_IdModalidad",
                table: "PlanModalidadDia",
                column: "IdModalidad");

            migrationBuilder.CreateIndex(
                name: "UQ_PlanModalidadDia",
                table: "PlanModalidadDia",
                columns: new[] { "IdPlanEstudios", "IdModalidad", "Grupo", "IdDiaSemana" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanPago_IdModalidadPlan",
                table: "PlanPago",
                column: "IdModalidadPlan");

            migrationBuilder.CreateIndex(
                name: "IX_PlanPagoAsignacion_IdPlanPago",
                table: "PlanPagoAsignacion",
                column: "IdPlanPago");

            migrationBuilder.CreateIndex(
                name: "IX_PlanPagoAsignacion_PlanPagoIdPlanPago",
                table: "PlanPagoAsignacion",
                column: "PlanPagoIdPlanPago");

            migrationBuilder.CreateIndex(
                name: "IX_PlanPagoDetalle_IdConceptoPago",
                table: "PlanPagoDetalle",
                column: "IdConceptoPago");

            migrationBuilder.CreateIndex(
                name: "IX_PlanPagoDetalle_IdPlanPago",
                table: "PlanPagoDetalle",
                column: "IdPlanPago");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobro_EsActiva",
                table: "PlantillasCobro",
                column: "EsActiva");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobro_IdModalidad",
                table: "PlantillasCobro",
                column: "IdModalidad");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobro_IdPlanEstudios",
                table: "PlantillasCobro",
                column: "IdPlanEstudios");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobro_IdPlanEstudios_NumeroCuatrimestre_EsActiva",
                table: "PlantillasCobro",
                columns: new[] { "IdPlanEstudios", "NumeroCuatrimestre", "EsActiva" });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobroDetalles_IdConceptoPago",
                table: "PlantillasCobroDetalles",
                column: "IdConceptoPago");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCobroDetalles_IdPlantillaCobro",
                table: "PlantillasCobroDetalles",
                column: "IdPlantillaCobro");

            migrationBuilder.CreateIndex(
                name: "IX_Profesor_CampusId",
                table: "Profesor",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Profesor_IdPersona",
                table: "Profesor",
                column: "IdPersona");

            migrationBuilder.CreateIndex(
                name: "IX_Profesor_UsuarioId",
                table: "Profesor",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UQ__Profesor__82F7575B30F93488",
                table: "Profesor",
                column: "NoEmpleado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReciboDetalle_ConceptoPagoIdConceptoPago",
                table: "ReciboDetalle",
                column: "ConceptoPagoIdConceptoPago");

            migrationBuilder.CreateIndex(
                name: "IX_ReciboDetalle_IdRecibo",
                table: "ReciboDetalle",
                column: "IdRecibo");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsableFirma_IdConfiguracionIPES",
                table: "ResponsableFirma",
                column: "IdConfiguracionIPES");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeguimientoEgresados_IdEstudiante",
                table: "SeguimientoEgresados",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesBaja_IdEstudiante",
                table: "SolicitudesBaja",
                column: "IdEstudiante");

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
                name: "IX_SolicitudesPlanEstudios_IdPlanEstudios",
                table: "SolicitudesPlanEstudios",
                column: "IdPlanEstudios");

            migrationBuilder.CreateIndex(
                name: "IX_TareaDocente_IdGrupoMateria_Activa",
                table: "TareaDocente",
                columns: new[] { "IdGrupoMateria", "Activa" });

            migrationBuilder.CreateIndex(
                name: "IX_TareaDocente_IdProfesor",
                table: "TareaDocente",
                column: "IdProfesor");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasAdmision_IdPlanEstudios",
                table: "TarifasAdmision",
                column: "IdPlanEstudios");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasAdmision_IdPlanEstudios_Activo",
                table: "TarifasAdmision",
                columns: new[] { "IdPlanEstudios", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_TarifasAdmisionDetalles_IdConceptoPago",
                table: "TarifasAdmisionDetalles",
                column: "IdConceptoPago");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasAdmisionDetalles_IdTarifaAdmision",
                table: "TarifasAdmisionDetalles",
                column: "IdTarifaAdmision");

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

            migrationBuilder.CreateIndex(
                name: "IX_TiposDocumentoEstudiante_Clave",
                table: "TiposDocumentoEstudiante",
                column: "Clave",
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
                name: "Asistencia");

            migrationBuilder.DropTable(
                name: "AspiranteBitacoraSeguimiento");

            migrationBuilder.DropTable(
                name: "AspiranteConvenio");

            migrationBuilder.DropTable(
                name: "AspiranteDocumento");

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
                name: "BecaAsignacion");

            migrationBuilder.DropTable(
                name: "BitacoraAcciones");

            migrationBuilder.DropTable(
                name: "BitacoraRecibo");

            migrationBuilder.DropTable(
                name: "CalificacionDetalle");

            migrationBuilder.DropTable(
                name: "CatalogoAsignaturaSEP");

            migrationBuilder.DropTable(
                name: "CatalogoCargoSEP");

            migrationBuilder.DropTable(
                name: "CatalogoCarreraSEP");

            migrationBuilder.DropTable(
                name: "CatalogoEntidadFederativaSEP");

            migrationBuilder.DropTable(
                name: "CatalogoGeneroSEP");

            migrationBuilder.DropTable(
                name: "CatalogoNivelEstudiosSEP");

            migrationBuilder.DropTable(
                name: "CatalogoObservacionSEP");

            migrationBuilder.DropTable(
                name: "CatalogoTipoAsignaturaSEP");

            migrationBuilder.DropTable(
                name: "CatalogoTipoCertificacionSEP");

            migrationBuilder.DropTable(
                name: "CatalogoTipoPeriodoSEP");

            migrationBuilder.DropTable(
                name: "CertificadoAsignatura");

            migrationBuilder.DropTable(
                name: "ConceptoPrecio");

            migrationBuilder.DropTable(
                name: "ConvenioAlcance");

            migrationBuilder.DropTable(
                name: "CorteCaja");

            migrationBuilder.DropTable(
                name: "CredencialSEP");

            migrationBuilder.DropTable(
                name: "EntregaTarea");

            migrationBuilder.DropTable(
                name: "EstudianteGrupo");

            migrationBuilder.DropTable(
                name: "EstudiantePlan");

            migrationBuilder.DropTable(
                name: "Horario");

            migrationBuilder.DropTable(
                name: "LigaPago");

            migrationBuilder.DropTable(
                name: "NotificacionesUsuario");

            migrationBuilder.DropTable(
                name: "PagoAplicacion");

            migrationBuilder.DropTable(
                name: "PagoMetodo");

            migrationBuilder.DropTable(
                name: "PlanDocumentoRequisito");

            migrationBuilder.DropTable(
                name: "PlaneacionDocente");

            migrationBuilder.DropTable(
                name: "PlanModalidadDia");

            migrationBuilder.DropTable(
                name: "PlanPagoAsignacion");

            migrationBuilder.DropTable(
                name: "PlanPagoDetalle");

            migrationBuilder.DropTable(
                name: "PlantillaReportes");

            migrationBuilder.DropTable(
                name: "PlantillasCobroDetalles");

            migrationBuilder.DropTable(
                name: "RecargoPolitica");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SeguimientoEgresados");

            migrationBuilder.DropTable(
                name: "SolicitudesBaja");

            migrationBuilder.DropTable(
                name: "SolicitudesDocumento");

            migrationBuilder.DropTable(
                name: "SolicitudesPlanEstudios");

            migrationBuilder.DropTable(
                name: "TarifasAdmisionDetalles");

            migrationBuilder.DropTable(
                name: "TicketComentarios");

            migrationBuilder.DropTable(
                name: "Aspirante");

            migrationBuilder.DropTable(
                name: "Beca");

            migrationBuilder.DropTable(
                name: "CalificacionesParciales");

            migrationBuilder.DropTable(
                name: "CertificadoElectronico");

            migrationBuilder.DropTable(
                name: "Convenio");

            migrationBuilder.DropTable(
                name: "TareaDocente");

            migrationBuilder.DropTable(
                name: "ReciboDetalle");

            migrationBuilder.DropTable(
                name: "Pago");

            migrationBuilder.DropTable(
                name: "DocumentoRequisito");

            migrationBuilder.DropTable(
                name: "DiaSemana");

            migrationBuilder.DropTable(
                name: "PlanPago");

            migrationBuilder.DropTable(
                name: "PlantillasCobro");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "TiposDocumentoEstudiante");

            migrationBuilder.DropTable(
                name: "TarifasAdmision");

            migrationBuilder.DropTable(
                name: "TicketsSoporte");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropTable(
                name: "AspiranteEstatus");

            migrationBuilder.DropTable(
                name: "MedioContacto");

            migrationBuilder.DropTable(
                name: "Inscripcion");

            migrationBuilder.DropTable(
                name: "Parciales");

            migrationBuilder.DropTable(
                name: "ResponsableFirma");

            migrationBuilder.DropTable(
                name: "ConceptoPago");

            migrationBuilder.DropTable(
                name: "Recibo");

            migrationBuilder.DropTable(
                name: "MedioPago");

            migrationBuilder.DropTable(
                name: "ModalidadPlan");

            migrationBuilder.DropTable(
                name: "Modalidad");

            migrationBuilder.DropTable(
                name: "Estudiante");

            migrationBuilder.DropTable(
                name: "GrupoMateria");

            migrationBuilder.DropTable(
                name: "ConfiguracionIPES");

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
                name: "AspNetUsers");

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

            migrationBuilder.DropTable(
                name: "CodigosPostales");

            migrationBuilder.DropTable(
                name: "Municipios");

            migrationBuilder.DropTable(
                name: "Estados");
        }
    }
}
