using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using WebApplication2;
using WebApplication2.Configuration.Mapping;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Data.Seed;
using WebApplication2.Services;
using WebApplication2.Services.Interfaces;
using WebApplication2.Services.MultiTenant;
using WebApplication2.Data.DbContexts;
using WebApplication2.Middleware;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL: aceptar DateTime con Kind=Unspecified en columnas 'timestamp with time zone'.
// Necesario porque el código construye fechas con new DateTime(...)/.Date (Kind=Unspecified)
// para filtrar por columnas FechaXxx. SQL Server no distingue Kind; Npgsql sí lo exige.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var questPdfTempPath = Environment.GetEnvironmentVariable("QUESTPDF_TEMP_PATH") ?? "/app/tmp";
Directory.CreateDirectory(questPdfTempPath);
QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
QuestPDF.Settings.TemporaryStoragePath = questPdfTempPath;

var jwtKey = builder.Configuration.GetValue<string>("Jwt:Key");
var jwtIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer");
var jwtAudience = builder.Configuration.GetValue<string>("Jwt:Audience");


if (builder.Environment.IsProduction())
{
    if (string.IsNullOrWhiteSpace(jwtKey))
        throw new InvalidOperationException("PRODUCCIÓN: Jwt:Key es requerido. Configure Jwt__Key como variable de entorno.");
    if (jwtKey!.Length < 32)
        throw new InvalidOperationException("PRODUCCIÓN: Jwt:Key debe tener al menos 32 caracteres (256 bits) para HMAC-SHA256.");
    if (string.IsNullOrWhiteSpace(jwtIssuer))
        throw new InvalidOperationException("PRODUCCIÓN: Jwt:Issuer es requerido. Configure Jwt__Issuer como variable de entorno.");
    if (string.IsNullOrWhiteSpace(jwtAudience))
        throw new InvalidOperationException("PRODUCCIÓN: Jwt:Audience es requerido. Configure Jwt__Audience como variable de entorno.");
}

var corsOrigins = (builder.Configuration["CORS_ORIGINS"] ?? "http://localhost:3000")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);



builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(cfg => cfg.AddProfiles(AutoMapperProfiles.GetProfiles()));

builder.Services.AddControllers(options =>
    {
        options.Filters.Add<WebApplication2.Filters.BitacoraActionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "USAG API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "Autenticación JWT con esquema Bearer.\r\n\r\n" +
            "Escribe **Bearer** seguido de un espacio y tu token.\r\n\r\n" +
            "Ejemplo: `Bearer eyJhbGciOiJI...`",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "bearer",
                Name = "Authorization",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});


builder.Services.AddCors(o =>
{
    o.AddPolicy("frontend", p => p
        .WithOrigins(corsOrigins)
        .AllowAnyHeader()
        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
        .AllowCredentials()
    );
});


var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(conn))
{
    Console.WriteLine("WARNING: ConnectionStrings:DefaultConnection está vacío (ConnectionStrings__DefaultConnection).");
}

var dbProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var tenantContext = sp.GetService<ITenantContextAccessor>()?.TenantContext;
    var effectiveConn = !string.IsNullOrWhiteSpace(tenantContext?.ConnectionString)
        ? tenantContext!.ConnectionString
        : conn;

    if (dbProvider == "PostgreSQL")
        options.UseNpgsql(effectiveConn, npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
    else
        options.UseSqlServer(effectiveConn, sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));

    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/dpkeys"))
    .SetApplicationName("usag-saci");

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
    options.Password.RequiredLength = 12;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});


builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{

    x.RequireHttpsMetadata = true;
    x.SaveToken = true;

    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? throw new InvalidOperationException("Jwt:Key no está configurado. Configure Jwt__Key como variable de entorno."))),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
        NameClaimType = "userId",
        RoleClaimType = ClaimTypes.Role,
    };
});


builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEstudioSocioeconomicoService, EstudioSocioeconomicoService>();
builder.Services.AddScoped<IDiagnosticoInscripcionService, DiagnosticoInscripcionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccesoAlumnoDocenteService, AccesoAlumnoDocenteService>();
builder.Services.AddScoped<IComprobanteInscripcionService, ComprobanteInscripcionService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IVentanaCapturaService, VentanaCapturaService>();
builder.Services.AddScoped<IDirectorService, DirectorService>();
builder.Services.AddScoped<ICoordinadorService, CoordinadorService>();
builder.Services.AddScoped<IUbicacionService, UbicacionService>();
builder.Services.AddScoped<IMatriculaService, MatriculaService>();

builder.Services.AddScoped<IAspiranteService, AspiranteService>(sp =>
{
    var dbContext = sp.GetRequiredService<ApplicationDbContext>();
    var matriculaService = sp.GetRequiredService<IMatriculaService>();
    var estudianteService = sp.GetRequiredService<IEstudianteService>();
    var authService = sp.GetRequiredService<IAuthService>();
    var plantillaCobroService = sp.GetRequiredService<IPlantillaCobroService>();
    var convenioService = sp.GetRequiredService<IConvenioService>();
    var reciboService = sp.GetRequiredService<IReciboService>();
    var graphService = sp.GetRequiredService<IMicrosoftGraphService>();
    return new AspiranteService(dbContext, matriculaService, estudianteService, authService, plantillaCobroService, convenioService, reciboService, graphService);
});

builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<IEstudiantePanelService, EstudiantePanelService>();
builder.Services.AddScoped<IPlanEstudioService, PlanEstudiosService>();
builder.Services.AddScoped<IDepartamentoService, DepartamentoService>();
builder.Services.AddScoped<IInscripcionService, InscripcionService>();
builder.Services.AddScoped<IPreInscripcionService, PreInscripcionService>();
builder.Services.AddScoped<IPeriodoAcademicoService, PeriodoAcademicoService>();

builder.Services.AddScoped<IGrupoService, GrupoService>(sp =>
{
    var dbContext = sp.GetRequiredService<ApplicationDbContext>();
    var inscripcionService = sp.GetRequiredService<IInscripcionService>();
    var estudianteService = sp.GetRequiredService<IEstudianteService>();
    var periodoAcademicoService = sp.GetRequiredService<IPeriodoAcademicoService>();
    var matriculaService = sp.GetRequiredService<IMatriculaService>();
    var accesoService = sp.GetRequiredService<IAccesoAlumnoDocenteService>();
    var graphService = sp.GetRequiredService<IMicrosoftGraphService>();
    return new GrupoService(dbContext, inscripcionService, estudianteService, periodoAcademicoService, matriculaService, accesoService, graphService);
});

builder.Services.AddScoped<ICampusService, CampusService>();
builder.Services.AddScoped<IDocumentoRequisitoService, DocumentoRequisitoService>();
builder.Services.AddScoped<IBlobStorageService, LocalStorageService>();
builder.Services.AddScoped<IMateriaPlanService, MateriaPlanService>();
builder.Services.AddScoped<IParcialesService, ParcialesService>();
builder.Services.AddScoped<ICalificacionesService, CalificacionesService>();
builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IReciboService, ReciboService>();
builder.Services.AddScoped<IInstitucionProvider, InstitucionProvider>();
builder.Services.AddScoped<IPlanLimiteService, PlanLimiteService>();
builder.Services.AddScoped<IReporteBuilderService, ReporteBuilderService>();
builder.Services.AddScoped<IConfiguracionCalificacionesService, ConfiguracionCalificacionesService>();
builder.Services.AddScoped<IAspiranteDocumentoService, AspiranteDocumentoService>();
builder.Services.AddScoped<IConceptoService, ConceptoService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IBecaService, BecaService>();
builder.Services.AddScoped<IBecaCatalogoService, BecaCatalogoService>();
builder.Services.AddScoped<IPlantillaCobroService, PlantillaCobroService>();
builder.Services.AddScoped<ICajaService, CajaService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IDocumentoEstudianteService, DocumentoEstudianteService>();
builder.Services.AddScoped<IImportacionService, ImportacionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPortalAlumnoService, PortalAlumnoService>();
builder.Services.AddScoped<IConvenioService, ConvenioService>();
builder.Services.AddScoped<IBitacoraAccionService, BitacoraAccionService>();
builder.Services.AddScoped<INotificacionInternalService, NotificacionInternalService>();
builder.Services.AddScoped<IReporteAcademicoService, ReporteAcademicoService>();
builder.Services.AddScoped<IPlaneacionDocenteService, PlaneacionDocenteService>();
builder.Services.AddScoped<ITareaDocenteService, TareaDocenteService>();
builder.Services.AddScoped<ITarifaAdmisionService, TarifaAdmisionService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<ITicketSoporteService, TicketSoporteService>();
builder.Services.AddScoped<IConfiguracionTitulacionService, ConfiguracionTitulacionService>();
builder.Services.AddScoped<ICertificadoElectronicoService, CertificadoElectronicoService>();
builder.Services.AddScoped<ICertificadoXmlService, CertificadoXmlService>();
builder.Services.AddScoped<ITituloElectronicoXmlService, TituloElectronicoXmlService>();
builder.Services.AddScoped<ITitulosElectronicosSepService, TitulosElectronicosSepService>();
builder.Services.AddScoped<IPlantillaReporteService, PlantillaReporteService>();
builder.Services.AddScoped<IFormatoDatosService, FormatoDatosService>();
builder.Services.AddHostedService<TareasAutomaticasService>();
builder.Services.AddHostedService<WebApplication2.Services.MultiTenant.SuscripcionTenantMonitor>();


var masterConn = builder.Configuration.GetConnectionString("MasterConnection") ?? conn;


builder.Services.AddDbContext<MasterDbContext>(options =>
{
    if (dbProvider == "PostgreSQL")
        options.UseNpgsql(masterConn);
    else
        options.UseSqlServer(masterConn);

    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddMemoryCache();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddSingleton<ITenantContextAccessor, TenantContextAccessor>();

builder.Services.AddScoped<ITenantService, TenantService>();

builder.Services.AddScoped<INotificacionService, NotificacionService>();

builder.Services.AddScoped<IMicrosoftGraphService, MicrosoftGraphService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database", HealthStatus.Unhealthy, tags: new[] { "db", "ready" })
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), tags: new[] { "live" });

var app = builder.Build();

await app.Services.InsertInitialDataAsync();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors("frontend");

app.UseRateLimiter();

app.UseTenantMiddleware();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        var tokenTenant = context.User.FindFirst("tenant")?.Value;
        var currentTenant = context.RequestServices
            .GetService<WebApplication2.Services.MultiTenant.ITenantContextAccessor>()?.TenantContext;

        if (!string.IsNullOrEmpty(tokenTenant) && currentTenant != null &&
            !string.Equals(tokenTenant, currentTenant.Codigo, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Sesión de otra institución",
                message = "Tu sesión pertenece a otra institución. Vuelve a iniciar sesión.",
                code = "TENANT_MISMATCH"
            });
            return;
        }
    }

    await next();
});

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// Ejecutar seeders automáticos
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Seed de tipos de documento
    TipoDocumentoSeed.Seed(context);
}

app.Run();
