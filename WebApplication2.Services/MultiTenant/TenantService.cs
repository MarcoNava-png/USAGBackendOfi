using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApplication2.Core.DTOs.MultiTenant;
using WebApplication2.Core.Models;
using WebApplication2.Core.Models.MultiTenant;
using WebApplication2.Core.Requests.MultiTenant;
using WebApplication2.Core.Responses.MultiTenant;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Services.MultiTenant;

public class TenantService : ITenantService
{
    private readonly MasterDbContext _masterDb;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TenantService> _logger;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public TenantService(
        MasterDbContext masterDb,
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<TenantService> logger,
        ITenantContextAccessor tenantContextAccessor)
    {
        _masterDb = masterDb;
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public TenantContext? GetCurrentTenant() => _tenantContextAccessor.TenantContext;

    public void SetCurrentTenant(TenantContext tenant)
    {
        _tenantContextAccessor.TenantContext = tenant;
    }

    public async Task<Tenant?> GetTenantBySubdomainAsync(string subdomain, CancellationToken ct = default)
    {
        var cacheKey = $"tenant_subdomain_{subdomain.ToLower()}";

        if (!_cache.TryGetValue(cacheKey, out Tenant? tenant))
        {
            tenant = await _masterDb.Tenants
                .Include(t => t.PlanLicencia)
                .FirstOrDefaultAsync(t =>
                    t.Subdominio.ToLower() == subdomain.ToLower() ||
                    t.Codigo.ToLower() == subdomain.ToLower() ||
                    t.DominioPersonalizado == subdomain, ct);

            if (tenant != null)
            {
                _cache.Set(cacheKey, tenant, CacheDuration);
            }
        }

        return tenant;
    }

    public async Task<Tenant?> GetTenantByIdAsync(int idTenant, CancellationToken ct = default)
    {
        var cacheKey = $"tenant_id_{idTenant}";

        if (!_cache.TryGetValue(cacheKey, out Tenant? tenant))
        {
            tenant = await _masterDb.Tenants
                .Include(t => t.PlanLicencia)
                .FirstOrDefaultAsync(t => t.IdTenant == idTenant, ct);

            if (tenant != null)
            {
                _cache.Set(cacheKey, tenant, CacheDuration);
            }
        }

        return tenant;
    }

    public async Task<List<TenantListDto>> ListarTenantsAsync(CancellationToken ct = default)
    {
        return await _masterDb.Tenants
            .Include(t => t.PlanLicencia)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TenantListDto
            {
                IdTenant = t.IdTenant,
                Codigo = t.Codigo,
                Nombre = t.Nombre,
                NombreCorto = t.NombreCorto,
                Subdominio = t.Subdominio,
                LogoUrl = t.LogoUrl,
                ColorPrimario = t.ColorPrimario,
                Status = t.Status.ToString(),
                Plan = t.PlanLicencia != null ? t.PlanLicencia.Nombre : "Sin plan",
                FechaContratacion = t.FechaContratacion,
                FechaVencimiento = t.FechaVencimiento,
                LastAccessAt = t.LastAccessAt
            })
            .ToListAsync(ct);
    }

    public async Task<TenantDetalleDto?> ObtenerTenantDetalleAsync(int idTenant, CancellationToken ct = default)
    {
        var tenant = await _masterDb.Tenants
            .Include(t => t.PlanLicencia)
            .FirstOrDefaultAsync(t => t.IdTenant == idTenant, ct);

        if (tenant == null) return null;

        var detalle = new TenantDetalleDto
        {
            IdTenant = tenant.IdTenant,
            Codigo = tenant.Codigo,
            Nombre = tenant.Nombre,
            NombreCorto = tenant.NombreCorto,
            Subdominio = tenant.Subdominio,
            DominioPersonalizado = tenant.DominioPersonalizado,
            LogoUrl = tenant.LogoUrl,
            ColorPrimario = tenant.ColorPrimario,
            ColorSecundario = tenant.ColorSecundario,
            Timezone = tenant.Timezone,
            EmailContacto = tenant.EmailContacto,
            TelefonoContacto = tenant.TelefonoContacto,
            DireccionFiscal = tenant.DireccionFiscal,
            RFC = tenant.RFC,
            IdPlanLicencia = tenant.IdPlanLicencia,
            NombrePlan = tenant.PlanLicencia?.Nombre ?? "Sin plan",
            MaximoEstudiantes = tenant.MaximoEstudiantes,
            MaximoUsuarios = tenant.MaximoUsuarios,
            FechaContratacion = tenant.FechaContratacion,
            FechaVencimiento = tenant.FechaVencimiento,
            Status = tenant.Status,
            CreatedAt = tenant.CreatedAt,
            LastAccessAt = tenant.LastAccessAt
        };

        try
        {
            detalle.Estadisticas = await ObtenerEstadisticasTenantAsync(idTenant, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron obtener estadísticas del tenant {IdTenant}", idTenant);
        }

        return detalle;
    }

    public async Task<TenantCreadoResponse> CrearTenantAsync(CrearTenantRequest request, string creadoPor, CancellationToken ct = default)
    {
        _logger.LogInformation("Iniciando creación de tenant: {Codigo}", request.Codigo);

        try
        {
            if (await _masterDb.Tenants.AnyAsync(t => t.Codigo == request.Codigo, ct))
            {
                return new TenantCreadoResponse
                {
                    Exitoso = false,
                    Mensaje = $"Ya existe un tenant con el código '{request.Codigo}'"
                };
            }

            if (await _masterDb.Tenants.AnyAsync(t => t.Subdominio == request.Subdominio, ct))
            {
                return new TenantCreadoResponse
                {
                    Exitoso = false,
                    Mensaje = $"Ya existe un tenant con el subdominio '{request.Subdominio}'"
                };
            }

            var plan = await _masterDb.PlanesLicencia.FindAsync(new object[] { request.IdPlanLicencia }, ct);
            if (plan == null)
            {
                return new TenantCreadoResponse
                {
                    Exitoso = false,
                    Mensaje = "El plan de licencia seleccionado no existe"
                };
            }

            var dbName = $"GestionEscolar_{request.Codigo}";
            var serverConnectionString = _configuration.GetConnectionString("TenantServerConnection")
                ?? _configuration.GetConnectionString("DefaultConnection");

            var builder = new SqlConnectionStringBuilder(serverConnectionString);
            builder.InitialCatalog = dbName;
            var tenantConnectionString = builder.ConnectionString;

            var tenant = new Tenant
            {
                Codigo = request.Codigo.ToUpper(),
                Nombre = request.Nombre,
                NombreCorto = request.NombreCorto,
                Subdominio = request.Subdominio.ToLower(),
                DatabaseName = dbName,
                ConnectionString = tenantConnectionString,
                LogoUrl = request.LogoUrl,
                ColorPrimario = request.ColorPrimario ?? "#14356F",
                ColorSecundario = request.ColorSecundario,
                EmailContacto = request.EmailContacto,
                TelefonoContacto = request.TelefonoContacto,
                DireccionFiscal = request.DireccionFiscal,
                RFC = request.RFC,
                IdPlanLicencia = request.IdPlanLicencia,
                MaximoEstudiantes = plan.MaxEstudiantes,
                MaximoUsuarios = plan.MaxUsuarios,
                Status = TenantStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creadoPor
            };

            _masterDb.Tenants.Add(tenant);
            await _masterDb.SaveChangesAsync(ct);

            _logger.LogInformation("Tenant registrado en master DB: {IdTenant}", tenant.IdTenant);

            await CrearBaseDatosTenantAsync(serverConnectionString!, dbName, ct);
            _logger.LogInformation("Base de datos creada: {DbName}", dbName);

            await AplicarMigracionesTenantAsync(tenantConnectionString, ct);
            _logger.LogInformation("Migraciones aplicadas a: {DbName}", dbName);

            await CrearAdminInicialAsync(tenantConnectionString, request, ct);
            _logger.LogInformation("Admin inicial creado para tenant: {Codigo}", request.Codigo);

            await SembrarDatosInicialesAsync(tenantConnectionString, ct);
            _logger.LogInformation("Datos iniciales sembrados para tenant: {Codigo}", request.Codigo);

            tenant.Status = TenantStatus.Active;
            await _masterDb.SaveChangesAsync(ct);

            _masterDb.AuditLogs.Add(new TenantAuditLog
            {
                IdTenant = tenant.IdTenant,
                TenantCodigo = tenant.Codigo,
                Accion = "TENANT_CREATED",
                Descripcion = $"Tenant '{tenant.Nombre}' creado exitosamente",
                Timestamp = DateTime.UtcNow
            });
            await _masterDb.SaveChangesAsync(ct);

            _logger.LogInformation("Tenant creado exitosamente: {Codigo} - {Url}",
                tenant.Codigo, $"https://{tenant.Subdominio}.saciusag.com.mx");

            return new TenantCreadoResponse
            {
                Exitoso = true,
                Mensaje = "Escuela creada exitosamente",
                IdTenant = tenant.IdTenant,
                Codigo = tenant.Codigo,
                Url = $"https://{tenant.Subdominio}.saciusag.com.mx",
                AdminEmail = request.AdminEmail,
                PasswordTemporal = request.AdminPassword
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tenant: {Codigo}", request.Codigo);

            return new TenantCreadoResponse
            {
                Exitoso = false,
                Mensaje = $"Error al crear la escuela: {ex.Message}"
            };
        }
    }

    public async Task<bool> ActualizarTenantAsync(int idTenant, ActualizarTenantRequest request, CancellationToken ct = default)
    {
        var tenant = await _masterDb.Tenants.FindAsync(new object[] { idTenant }, ct);
        if (tenant == null) return false;

        if (request.Nombre != null) tenant.Nombre = request.Nombre;
        if (request.NombreCorto != null) tenant.NombreCorto = request.NombreCorto;
        if (request.DominioPersonalizado != null) tenant.DominioPersonalizado = request.DominioPersonalizado;
        if (request.LogoUrl != null) tenant.LogoUrl = request.LogoUrl;
        if (request.ColorPrimario != null) tenant.ColorPrimario = request.ColorPrimario;
        if (request.ColorSecundario != null) tenant.ColorSecundario = request.ColorSecundario;
        if (request.EmailContacto != null) tenant.EmailContacto = request.EmailContacto;
        if (request.TelefonoContacto != null) tenant.TelefonoContacto = request.TelefonoContacto;
        if (request.DireccionFiscal != null) tenant.DireccionFiscal = request.DireccionFiscal;
        if (request.RFC != null) tenant.RFC = request.RFC;
        if (request.IdPlanLicencia.HasValue) tenant.IdPlanLicencia = request.IdPlanLicencia.Value;
        if (request.FechaVencimiento.HasValue) tenant.FechaVencimiento = request.FechaVencimiento;

        tenant.UpdatedAt = DateTime.UtcNow;

        await _masterDb.SaveChangesAsync(ct);

        _cache.Remove($"tenant_id_{idTenant}");
        _cache.Remove($"tenant_subdomain_{tenant.Subdominio}");

        return true;
    }

    public async Task<bool> CambiarStatusTenantAsync(int idTenant, TenantStatus nuevoStatus, string? motivo, CancellationToken ct = default)
    {
        var tenant = await _masterDb.Tenants.FindAsync(new object[] { idTenant }, ct);
        if (tenant == null) return false;

        var statusAnterior = tenant.Status;
        tenant.Status = nuevoStatus;
        tenant.UpdatedAt = DateTime.UtcNow;

        _masterDb.AuditLogs.Add(new TenantAuditLog
        {
            IdTenant = idTenant,
            TenantCodigo = tenant.Codigo,
            Accion = $"STATUS_CHANGED_{nuevoStatus}",
            Descripcion = $"Status cambiado de {statusAnterior} a {nuevoStatus}. Motivo: {motivo ?? "No especificado"}",
            Timestamp = DateTime.UtcNow
        });

        await _masterDb.SaveChangesAsync(ct);

        _cache.Remove($"tenant_id_{idTenant}");
        _cache.Remove($"tenant_subdomain_{tenant.Subdominio}");

        return true;
    }

    public async Task<DashboardGlobalDto> ObtenerDashboardGlobalAsync(CancellationToken ct = default)
    {
        var tenants = await _masterDb.Tenants.ToListAsync(ct);

        var dashboard = new DashboardGlobalDto
        {
            TotalTenants = tenants.Count,
            TenantsActivos = tenants.Count(t => t.Status == TenantStatus.Active),
            TenantsPendientes = tenants.Count(t => t.Status == TenantStatus.Pending),
            TenantsSuspendidos = tenants.Count(t => t.Status == TenantStatus.Suspended)
        };

        dashboard.UltimosTenantsCreados = await _masterDb.Tenants
            .Include(t => t.PlanLicencia)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new TenantListDto
            {
                IdTenant = t.IdTenant,
                Codigo = t.Codigo,
                Nombre = t.Nombre,
                NombreCorto = t.NombreCorto,
                Subdominio = t.Subdominio,
                Status = t.Status.ToString(),
                Plan = t.PlanLicencia != null ? t.PlanLicencia.Nombre : "Sin plan",
                FechaContratacion = t.FechaContratacion
            })
            .ToListAsync(ct);

        var fechaLimite = DateTime.UtcNow.AddDays(30);
        dashboard.TenantsConProblemas = await _masterDb.Tenants
            .Include(t => t.PlanLicencia)
            .Where(t => t.Status == TenantStatus.Suspended ||
                       (t.FechaVencimiento != null && t.FechaVencimiento < fechaLimite))
            .Select(t => new TenantListDto
            {
                IdTenant = t.IdTenant,
                Codigo = t.Codigo,
                Nombre = t.Nombre,
                NombreCorto = t.NombreCorto,
                Subdominio = t.Subdominio,
                Status = t.Status.ToString(),
                Plan = t.PlanLicencia != null ? t.PlanLicencia.Nombre : "Sin plan",
                FechaVencimiento = t.FechaVencimiento
            })
            .ToListAsync(ct);

        foreach (var tenant in tenants.Where(t => t.Status == TenantStatus.Active))
        {
            try
            {
                var stats = await ObtenerEstadisticasTenantAsync(tenant.IdTenant, ct);
                dashboard.TotalEstudiantesGlobal += stats.TotalEstudiantes;
                dashboard.TotalUsuariosGlobal += stats.TotalUsuarios;
                dashboard.IngresosMesGlobal += stats.IngresosMes;
            }
            catch
            {
            }
        }

        return dashboard;
    }

    public async Task<TenantStatsDto> ObtenerEstadisticasTenantAsync(int idTenant, CancellationToken ct = default)
    {
        var tenant = await GetTenantByIdAsync(idTenant, ct);
        if (tenant == null)
        {
            return new TenantStatsDto();
        }

        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(tenant.ConnectionString);

            using var context = new ApplicationDbContext(optionsBuilder.Options, forProvisioning: true);

            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            return new TenantStatsDto
            {
                TotalEstudiantes = await context.Estudiante.CountAsync(ct),
                EstudiantesActivos = await context.Estudiante.CountAsync(e => e.Activo, ct),
                TotalUsuarios = await context.Users.CountAsync(ct),
                TotalProfesores = await context.Profesor.CountAsync(ct),
                TotalRecibos = await context.Recibo.CountAsync(ct),
                IngresosMes = await context.Pago
                    .Where(p => p.FechaPagoUtc >= inicioMes)
                    .SumAsync(p => p.Monto, ct),
                AdeudoTotal = await context.Recibo
                    .Where(r => r.Saldo > 0)
                    .SumAsync(r => r.Saldo, ct),
                AspirantesActivos = await context.Aspirante
                    .CountAsync(a => a.IdAspiranteEstatus == 1, ct)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener estadísticas del tenant {IdTenant}", idTenant);
            return new TenantStatsDto();
        }
    }

    public async Task<List<PlanLicenciaDto>> ListarPlanesAsync(CancellationToken ct = default)
    {
        return await _masterDb.PlanesLicencia
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .Select(p => new PlanLicenciaDto
            {
                IdPlanLicencia = p.IdPlanLicencia,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioMensual = p.PrecioMensual,
                PrecioAnual = p.PrecioAnual,
                MaxEstudiantes = p.MaxEstudiantes,
                MaxUsuarios = p.MaxUsuarios,
                MaxCampus = p.MaxCampus,
                IncluyeSoporte = p.IncluyeSoporte,
                IncluyeReportes = p.IncluyeReportes,
                IncluyeAPI = p.IncluyeAPI,
                IncluyeFacturacion = p.IncluyeFacturacion,
                Activo = p.Activo
            })
            .ToListAsync(ct);
    }

    private async Task CrearBaseDatosTenantAsync(string serverConnection, string dbName, CancellationToken ct)
    {
        var builder = new SqlConnectionStringBuilder(serverConnection);
        builder.InitialCatalog = "master";

        using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(ct);

        var sql = $@"
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{dbName}')
            BEGIN
                CREATE DATABASE [{dbName}]
            END";

        using var command = new SqlCommand(sql, connection);
        command.CommandTimeout = 60;
        await command.ExecuteNonQueryAsync(ct);

        await Task.Delay(2000, ct);
    }

    private async Task AplicarMigracionesTenantAsync(string connectionString, CancellationToken ct)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new ApplicationDbContext(optionsBuilder.Options, forProvisioning: true);

        await context.Database.MigrateAsync(ct);
    }

    private async Task CrearAdminInicialAsync(string connectionString, CrearTenantRequest request, CancellationToken ct)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new ApplicationDbContext(optionsBuilder.Options, forProvisioning: true);

        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin", ct);
        if (adminRole == null)
        {
            adminRole = new Microsoft.AspNetCore.Identity.IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            context.Roles.Add(adminRole);
            await context.SaveChangesAsync(ct);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = request.AdminEmail,
            Email = request.AdminEmail,
            NormalizedUserName = request.AdminEmail.ToUpper(),
            NormalizedEmail = request.AdminEmail.ToUpper(),
            EmailConfirmed = true,
            Nombres = request.AdminNombre.Split(' ').FirstOrDefault() ?? request.AdminNombre,
            Apellidos = string.Join(' ', request.AdminNombre.Split(' ').Skip(1)),
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        var hasher = new PasswordHasher<ApplicationUser>();
        user.PasswordHash = hasher.HashPassword(user, request.AdminPassword);

        context.Users.Add(user);
        await context.SaveChangesAsync(ct);

        context.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string>
        {
            UserId = user.Id,
            RoleId = adminRole.Id
        });

        await context.SaveChangesAsync(ct);
    }

    private async Task SembrarDatosInicialesAsync(string connectionString, CancellationToken ct)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new ApplicationDbContext(optionsBuilder.Options, forProvisioning: true);

        if (await context.NivelEducativo.AnyAsync(ct))
        {
            return;
        }

        context.NivelEducativo.AddRange(
            new NivelEducativo { DescNivelEducativo = "Licenciatura", Activo = true },
            new NivelEducativo { DescNivelEducativo = "Preparatoria", Activo = true },
            new NivelEducativo { DescNivelEducativo = "Secundaria", Activo = true },
            new NivelEducativo { DescNivelEducativo = "Primaria", Activo = true },
            new NivelEducativo { DescNivelEducativo = "Maestría", Activo = true },
            new NivelEducativo { DescNivelEducativo = "Doctorado", Activo = true }
        );

        context.Periodicidad.AddRange(
            new Periodicidad { DescPeriodicidad = "Semestral", PeriodosPorAnio = 2, MesesPorPeriodo = 6, Activo = true },
            new Periodicidad { DescPeriodicidad = "Cuatrimestral", PeriodosPorAnio = 3, MesesPorPeriodo = 4, Activo = true },
            new Periodicidad { DescPeriodicidad = "Trimestral", PeriodosPorAnio = 4, MesesPorPeriodo = 3, Activo = true },
            new Periodicidad { DescPeriodicidad = "Anual", PeriodosPorAnio = 1, MesesPorPeriodo = 12, Activo = true }
        );

        context.Genero.AddRange(
            new Genero { DescGenero = "Masculino" },
            new Genero { DescGenero = "Femenino" },
            new Genero { DescGenero = "Otro" }
        );

        context.EstadoCivil.AddRange(
            new EstadoCivil { DescEstadoCivil = "Soltero(a)" },
            new EstadoCivil { DescEstadoCivil = "Casado(a)" },
            new EstadoCivil { DescEstadoCivil = "Divorciado(a)" },
            new EstadoCivil { DescEstadoCivil = "Viudo(a)" },
            new EstadoCivil { DescEstadoCivil = "Unión Libre" }
        );

        context.MedioContacto.AddRange(
            new MedioContacto { DescMedio = "Facebook", Activo = true },
            new MedioContacto { DescMedio = "Instagram", Activo = true },
            new MedioContacto { DescMedio = "Referido", Activo = true },
            new MedioContacto { DescMedio = "Google", Activo = true },
            new MedioContacto { DescMedio = "Espectacular", Activo = true },
            new MedioContacto { DescMedio = "Otro", Activo = true }
        );

        context.AspiranteEstatus.AddRange(
            new AspiranteEstatus { DescEstatus = "Nuevo" },
            new AspiranteEstatus { DescEstatus = "En seguimiento" },
            new AspiranteEstatus { DescEstatus = "Inscrito" },
            new AspiranteEstatus { DescEstatus = "Descartado" }
        );

        await context.SaveChangesAsync(ct);

        _logger.LogInformation("Datos iniciales sembrados correctamente");
    }

    public async Task<ImportarTenantsResultado> ImportarTenantsAsync(
        List<ImportarTenantFila> filas,
        string creadoPor,
        CancellationToken ct = default)
    {
        var resultado = new ImportarTenantsResultado
        {
            TotalFilas = filas.Count,
            Exitosos = 0,
            Fallidos = 0,
            Resultados = new List<ImportarTenantResultadoFila>()
        };

        foreach (var fila in filas)
        {
            var resultadoFila = new ImportarTenantResultadoFila
            {
                Fila = fila.Fila,
                Codigo = fila.Codigo,
                Nombre = fila.Nombre
            };

            try
            {
                var password = GenerarPasswordAleatorio();

                var request = new CrearTenantRequest
                {
                    Codigo = fila.Codigo,
                    Nombre = fila.Nombre,
                    NombreCorto = fila.NombreCorto,
                    Subdominio = fila.Subdominio,
                    ColorPrimario = fila.ColorPrimario ?? "#14356F",
                    EmailContacto = fila.EmailContacto,
                    TelefonoContacto = fila.TelefonoContacto,
                    IdPlanLicencia = fila.IdPlanLicencia,
                    AdminEmail = fila.AdminEmail,
                    AdminNombre = fila.AdminNombre,
                    AdminPassword = password
                };

                var respuesta = await CrearTenantAsync(request, creadoPor, ct);

                resultadoFila.Exitoso = respuesta.Exitoso;
                resultadoFila.Mensaje = respuesta.Mensaje;
                resultadoFila.Url = respuesta.Url;
                resultadoFila.AdminEmail = respuesta.AdminEmail;
                resultadoFila.PasswordTemporal = password;

                if (respuesta.Exitoso)
                    resultado.Exitosos++;
                else
                    resultado.Fallidos++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al importar fila {Fila}: {Codigo}", fila.Fila, fila.Codigo);
                resultadoFila.Exitoso = false;
                resultadoFila.Mensaje = $"Error: {ex.Message}";
                resultado.Fallidos++;
            }

            resultado.Resultados.Add(resultadoFila);
        }

        return resultado;
    }

    public byte[] GenerarPlantillaExcel()
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Escuelas");

        var headers = new[]
        {
            "Codigo", "Nombre", "NombreCorto", "Subdominio", "ColorPrimario",
            "EmailContacto", "TelefonoContacto", "IdPlanLicencia", "AdminEmail", "AdminNombre"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        worksheet.Cell(2, 1).Value = "(Único, sin espacios)";
        worksheet.Cell(2, 2).Value = "(Nombre completo)";
        worksheet.Cell(2, 3).Value = "(Abreviación)";
        worksheet.Cell(2, 4).Value = "(URL: xxx.saciusag.com.mx)";
        worksheet.Cell(2, 5).Value = "(Ej: #14356F)";
        worksheet.Cell(2, 6).Value = "(Opcional)";
        worksheet.Cell(2, 7).Value = "(Opcional)";
        worksheet.Cell(2, 8).Value = "(Ver hoja PLANES)";
        worksheet.Cell(2, 9).Value = "(Email del admin)";
        worksheet.Cell(2, 10).Value = "(Nombre del admin)";
        worksheet.Row(2).Style.Font.Italic = true;
        worksheet.Row(2).Style.Font.FontColor = XLColor.Gray;

        worksheet.Cell(3, 1).Value = "ESCUELA1";
        worksheet.Cell(3, 2).Value = "Universidad Ejemplo";
        worksheet.Cell(3, 3).Value = "UNI-EJEMPLO";
        worksheet.Cell(3, 4).Value = "ejemplo";
        worksheet.Cell(3, 5).Value = "#14356F";
        worksheet.Cell(3, 6).Value = "contacto@ejemplo.edu.mx";
        worksheet.Cell(3, 7).Value = "33-1234-5678";
        worksheet.Cell(3, 8).Value = 2;
        worksheet.Cell(3, 9).Value = "admin@ejemplo.edu.mx";
        worksheet.Cell(3, 10).Value = "Juan Pérez García";
        worksheet.Row(3).Style.Fill.BackgroundColor = XLColor.LightGreen;

        worksheet.Cell(4, 1).Value = "COLEGIO2";
        worksheet.Cell(4, 2).Value = "Colegio San Pedro";
        worksheet.Cell(4, 3).Value = "CSP";
        worksheet.Cell(4, 4).Value = "sanpedro";
        worksheet.Cell(4, 5).Value = "#2E7D32";
        worksheet.Cell(4, 6).Value = "info@sanpedro.edu.mx";
        worksheet.Cell(4, 7).Value = "33-9876-5432";
        worksheet.Cell(4, 8).Value = 1;
        worksheet.Cell(4, 9).Value = "director@sanpedro.edu.mx";
        worksheet.Cell(4, 10).Value = "María López Hernández";
        worksheet.Row(4).Style.Fill.BackgroundColor = XLColor.LightGreen;

        worksheet.Cell(5, 1).Value = "← ELIMINA ESTAS FILAS DE EJEMPLO Y AGREGA TUS ESCUELAS ↑";
        worksheet.Range("A5:J5").Merge();
        worksheet.Cell(5, 1).Style.Font.Bold = true;
        worksheet.Cell(5, 1).Style.Font.FontColor = XLColor.Red;
        worksheet.Cell(5, 1).Style.Fill.BackgroundColor = XLColor.LightYellow;

        var planes = workbook.Worksheets.Add("PLANES");

        planes.Cell(1, 1).Value = "PLANES DE LICENCIA DISPONIBLES";
        planes.Cell(1, 1).Style.Font.Bold = true;
        planes.Cell(1, 1).Style.Font.FontSize = 16;
        planes.Range("A1:F1").Merge();

        planes.Cell(3, 1).Value = "Usa el número de la columna 'ID' en la hoja Escuelas";
        planes.Cell(3, 1).Style.Font.Italic = true;
        planes.Range("A3:F3").Merge();

        var planHeaders = new[] { "ID", "Código", "Nombre", "Precio/Mes", "Max Estudiantes", "Max Usuarios" };
        for (int i = 0; i < planHeaders.Length; i++)
        {
            var cell = planes.Cell(5, i + 1);
            cell.Value = planHeaders[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        planes.Cell(6, 1).Value = 1;
        planes.Cell(6, 2).Value = "BASIC";
        planes.Cell(6, 3).Value = "Básico";
        planes.Cell(6, 4).Value = "$2,500 MXN";
        planes.Cell(6, 5).Value = "200";
        planes.Cell(6, 6).Value = "5";
        planes.Row(6).Style.Fill.BackgroundColor = XLColor.LightGray;

        planes.Cell(7, 1).Value = 2;
        planes.Cell(7, 2).Value = "PRO";
        planes.Cell(7, 3).Value = "Profesional";
        planes.Cell(7, 4).Value = "$5,000 MXN";
        planes.Cell(7, 5).Value = "1,000";
        planes.Cell(7, 6).Value = "20";
        planes.Row(7).Style.Fill.BackgroundColor = XLColor.LightBlue;

        planes.Cell(8, 1).Value = 3;
        planes.Cell(8, 2).Value = "ENTERPRISE";
        planes.Cell(8, 3).Value = "Enterprise";
        planes.Cell(8, 4).Value = "$15,000 MXN";
        planes.Cell(8, 5).Value = "50,000";
        planes.Cell(8, 6).Value = "100";
        planes.Row(8).Style.Fill.BackgroundColor = XLColor.LightGreen;

        planes.Cell(10, 1).Value = "EJEMPLO: Si quieres el plan Profesional, pon 2 en la columna IdPlanLicencia";
        planes.Cell(10, 1).Style.Font.Bold = true;
        planes.Range("A10:F10").Merge();

        var instrucciones = workbook.Worksheets.Add("Instrucciones");

        instrucciones.Cell(1, 1).Value = "INSTRUCCIONES PARA IMPORTAR ESCUELAS";
        instrucciones.Cell(1, 1).Style.Font.Bold = true;
        instrucciones.Cell(1, 1).Style.Font.FontSize = 16;

        instrucciones.Cell(3, 1).Value = "PASO 1: Ve a la hoja 'Escuelas'";
        instrucciones.Cell(3, 1).Style.Font.Bold = true;

        instrucciones.Cell(4, 1).Value = "PASO 2: Elimina las filas de ejemplo (filas 3 y 4)";
        instrucciones.Cell(4, 1).Style.Font.Bold = true;

        instrucciones.Cell(5, 1).Value = "PASO 3: Agrega los datos de tus escuelas a partir de la fila 3";
        instrucciones.Cell(5, 1).Style.Font.Bold = true;

        instrucciones.Cell(6, 1).Value = "PASO 4: Consulta la hoja 'PLANES' para saber qué número poner en IdPlanLicencia";
        instrucciones.Cell(6, 1).Style.Font.Bold = true;

        instrucciones.Cell(8, 1).Value = "COLUMNAS REQUERIDAS:";
        instrucciones.Cell(8, 1).Style.Font.Bold = true;
        instrucciones.Cell(8, 1).Style.Fill.BackgroundColor = XLColor.LightCoral;

        instrucciones.Cell(9, 1).Value = "• Codigo → Identificador único (ej: USAG, COLEGIO1). Sin espacios, en mayúsculas.";
        instrucciones.Cell(10, 1).Value = "• Nombre → Nombre completo de la institución";
        instrucciones.Cell(11, 1).Value = "• NombreCorto → Nombre corto o siglas (máx 50 caracteres)";
        instrucciones.Cell(12, 1).Value = "• Subdominio → URL de la escuela. Solo letras minúsculas y números.";
        instrucciones.Cell(13, 1).Value = "    Ejemplo: Si pones 'miescuela', la URL será: miescuela.saciusag.com.mx";
        instrucciones.Cell(13, 1).Style.Font.Italic = true;
        instrucciones.Cell(14, 1).Value = "• IdPlanLicencia → Número del plan (1, 2 o 3). Ver hoja PLANES.";
        instrucciones.Cell(15, 1).Value = "• AdminEmail → Email del administrador. Será su usuario para iniciar sesión.";
        instrucciones.Cell(16, 1).Value = "• AdminNombre → Nombre completo del administrador";

        instrucciones.Cell(18, 1).Value = "COLUMNAS OPCIONALES:";
        instrucciones.Cell(18, 1).Style.Font.Bold = true;
        instrucciones.Cell(18, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;

        instrucciones.Cell(19, 1).Value = "• ColorPrimario → Color de la marca en formato hexadecimal (ej: #14356F)";
        instrucciones.Cell(20, 1).Value = "• EmailContacto → Email de contacto de la escuela";
        instrucciones.Cell(21, 1).Value = "• TelefonoContacto → Teléfono de contacto";

        instrucciones.Cell(23, 1).Value = "NOTAS IMPORTANTES:";
        instrucciones.Cell(23, 1).Style.Font.Bold = true;
        instrucciones.Cell(23, 1).Style.Fill.BackgroundColor = XLColor.LightYellow;

        instrucciones.Cell(24, 1).Value = "• Se generará una CONTRASEÑA ALEATORIA para cada administrador";
        instrucciones.Cell(25, 1).Value = "• Las contraseñas aparecerán en los RESULTADOS de la importación";
        instrucciones.Cell(26, 1).Value = "• GUARDA los resultados para poder entregar las credenciales a cada escuela";
        instrucciones.Cell(27, 1).Value = "• Se recomienda que cada admin cambie su contraseña en el primer inicio de sesión";

        worksheet.Columns().AdjustToContents();
        planes.Columns().AdjustToContents();
        instrucciones.Column(1).Width = 100;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string GenerarPasswordAleatorio()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
        var random = new Random();
        return new string(Enumerable.Range(0, 14).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }

    public async Task<ReporteIngresosGlobalDto> ObtenerReporteIngresosAsync(int? anio = null, CancellationToken ct = default)
    {
        var targetYear = anio ?? DateTime.Now.Year;
        var inicioAnio = new DateTime(targetYear, 1, 1);
        var finAnio = new DateTime(targetYear, 12, 31, 23, 59, 59);
        var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var reporte = new ReporteIngresosGlobalDto
        {
            IngresosPorTenant = new List<IngresosPorTenantDto>(),
            TendenciaAnual = new List<IngresosMensualesDto>()
        };

        var tenants = await _masterDb.Tenants
            .Where(t => t.Status == TenantStatus.Active)
            .ToListAsync(ct);

        foreach (var tenant in tenants)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(tenant.ConnectionString);

                using var context = new ApplicationDbContext(optionsBuilder.Options, forProvisioning: true);

                var ingresosMes = await context.Pago
                    .Where(p => p.FechaPagoUtc >= inicioMes)
                    .SumAsync(p => p.Monto, ct);

                var ingresosAnio = await context.Pago
                    .Where(p => p.FechaPagoUtc >= inicioAnio && p.FechaPagoUtc <= finAnio)
                    .SumAsync(p => p.Monto, ct);

                var adeudo = await context.Recibo
                    .Where(r => r.Saldo > 0)
                    .SumAsync(r => r.Saldo, ct);

                var inicioMesDate = DateOnly.FromDateTime(inicioMes);
                var recibosEmitidos = await context.Recibo
                    .Where(r => r.FechaEmision >= inicioMesDate)
                    .CountAsync(ct);

                var recibosPagados = await context.Recibo
                    .Where(r => r.Saldo == 0 && r.FechaEmision >= inicioMesDate)
                    .CountAsync(ct);

                reporte.IngresosPorTenant.Add(new IngresosPorTenantDto
                {
                    IdTenant = tenant.IdTenant,
                    Codigo = tenant.Codigo,
                    NombreCorto = tenant.NombreCorto,
                    ColorPrimario = tenant.ColorPrimario,
                    IngresosMes = ingresosMes,
                    IngresosAnio = ingresosAnio,
                    Adeudo = adeudo,
                    RecibosEmitidos = recibosEmitidos,
                    RecibosPagados = recibosPagados
                });

                reporte.IngresosTotalMes += ingresosMes;
                reporte.IngresosTotalAnio += ingresosAnio;
                reporte.AdeudoTotalGlobal += adeudo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener ingresos del tenant {Codigo}", tenant.Codigo);
            }
        }

        var primerTenant = tenants.FirstOrDefault();
        if (primerTenant != null)
        {
            var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            for (int mes = 1; mes <= 12; mes++)
            {
                reporte.TendenciaAnual.Add(new IngresosMensualesDto
                {
                    Anio = targetYear,
                    Mes = mes,
                    NombreMes = meses[mes - 1],
                    Total = reporte.IngresosPorTenant.Sum(t => t.IngresosAnio) / 12,
                    CantidadRecibos = 0
                });
            }
        }

        return reporte;
    }

    public async Task<ReporteEstudiantesGlobalDto> ObtenerReporteEstudiantesAsync(CancellationToken ct = default)
    {
        var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var inicioAnio = new DateTime(DateTime.Now.Year, 1, 1);

        var reporte = new ReporteEstudiantesGlobalDto
        {
            EstudiantesPorTenant = new List<EstudiantesPorTenantDto>(),
            DistribucionNivel = new List<EstudiantesPorNivelDto>()
        };

        var tenants = await _masterDb.Tenants
            .Include(t => t.PlanLicencia)
            .Where(t => t.Status == TenantStatus.Active)
            .ToListAsync(ct);

        var nivelesGlobal = new Dictionary<string, int>();

        foreach (var tenant in tenants)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(tenant.ConnectionString);

                using var context = new ApplicationDbContext(optionsBuilder.Options, forProvisioning: true);

                var total = await context.Estudiante.CountAsync(ct);
                var activos = await context.Estudiante.CountAsync(e => e.Activo, ct);
                var inicioMesDate = DateOnly.FromDateTime(inicioMes);
                var nuevosEsteMes = await context.Estudiante
                    .CountAsync(e => e.FechaIngreso >= inicioMesDate, ct);

                var capacidad = tenant.MaximoEstudiantes;
                var ocupacion = capacidad > 0 ? (decimal)total / capacidad * 100 : 0;

                reporte.EstudiantesPorTenant.Add(new EstudiantesPorTenantDto
                {
                    IdTenant = tenant.IdTenant,
                    Codigo = tenant.Codigo,
                    NombreCorto = tenant.NombreCorto,
                    ColorPrimario = tenant.ColorPrimario,
                    TotalEstudiantes = total,
                    Activos = activos,
                    NuevosEsteMes = nuevosEsteMes,
                    CapacidadMaxima = capacidad,
                    PorcentajeOcupacion = Math.Round(ocupacion, 1)
                });

                reporte.TotalEstudiantes += total;
                reporte.EstudiantesActivos += activos;
                reporte.EstudiantesInactivos += (total - activos);
                reporte.NuevosEsteMes += nuevosEsteMes;

                var niveles = await context.Estudiante
                    .Include(e => e.IdPlanActualNavigation)
                    .ThenInclude(p => p!.IdNivelEducativoNavigation)
                    .Where(e => e.IdPlanActualNavigation != null && e.IdPlanActualNavigation.IdNivelEducativoNavigation != null)
                    .GroupBy(e => e.IdPlanActualNavigation!.IdNivelEducativoNavigation!.DescNivelEducativo)
                    .Select(g => new { Nivel = g.Key, Count = g.Count() })
                    .ToListAsync(ct);

                foreach (var nivel in niveles)
                {
                    if (!nivelesGlobal.ContainsKey(nivel.Nivel))
                        nivelesGlobal[nivel.Nivel] = 0;
                    nivelesGlobal[nivel.Nivel] += nivel.Count;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener estudiantes del tenant {Codigo}", tenant.Codigo);
            }
        }

        var totalNiveles = nivelesGlobal.Values.Sum();
        reporte.DistribucionNivel = nivelesGlobal
            .OrderByDescending(n => n.Value)
            .Select(n => new EstudiantesPorNivelDto
            {
                Nivel = n.Key,
                Total = n.Value,
                Porcentaje = totalNiveles > 0 ? Math.Round((decimal)n.Value / totalNiveles * 100, 1) : 0
            })
            .ToList();

        return reporte;
    }

    public async Task<ReporteUsoSistemaDto> ObtenerReporteUsoSistemaAsync(CancellationToken ct = default)
    {
        var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var hoy = DateTime.Today;

        var reporte = new ReporteUsoSistemaDto
        {
            UsoPorTenant = new List<UsoPorTenantDto>(),
            ActividadReciente = new List<ActividadRecienteDto>()
        };

        var tenants = await _masterDb.Tenants
            .Where(t => t.Status == TenantStatus.Active)
            .ToListAsync(ct);

        foreach (var tenant in tenants)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(tenant.ConnectionString);

                using var context = new ApplicationDbContext(optionsBuilder.Options, forProvisioning: true);

                var totalUsuarios = await context.Users.CountAsync(ct);

                reporte.UsoPorTenant.Add(new UsoPorTenantDto
                {
                    IdTenant = tenant.IdTenant,
                    Codigo = tenant.Codigo,
                    NombreCorto = tenant.NombreCorto,
                    TotalUsuarios = totalUsuarios,
                    UsuariosActivos = totalUsuarios,
                    UltimoAcceso = tenant.LastAccessAt,
                    LoginsMes = 0
                });

                reporte.TotalUsuarios += totalUsuarios;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener uso del tenant {Codigo}", tenant.Codigo);
            }
        }

        var actividadReciente = await _masterDb.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .Select(a => new ActividadRecienteDto
            {
                IdTenant = a.IdTenant ?? 0,
                TenantNombre = a.TenantCodigo ?? "Sistema",
                Usuario = "Sistema",
                Accion = a.Accion,
                Fecha = a.Timestamp
            })
            .ToListAsync(ct);

        reporte.ActividadReciente = actividadReciente;
        reporte.UsuariosActivos = reporte.TotalUsuarios;

        return reporte;
    }

    public async Task<ReporteLicenciasDto> ObtenerReporteLicenciasAsync(CancellationToken ct = default)
    {
        var hoy = DateTime.UtcNow;
        var en30Dias = hoy.AddDays(30);

        var tenants = await _masterDb.Tenants
            .Include(t => t.PlanLicencia)
            .ToListAsync(ct);

        var planes = await _masterDb.PlanesLicencia.ToListAsync(ct);

        var reporte = new ReporteLicenciasDto
        {
            TotalTenants = tenants.Count,
            TenantsActivos = tenants.Count(t => t.Status == TenantStatus.Active),
            TenantsPorVencer = tenants.Count(t => t.FechaVencimiento.HasValue &&
                                                  t.FechaVencimiento.Value > hoy &&
                                                  t.FechaVencimiento.Value <= en30Dias),
            TenantsVencidos = tenants.Count(t => t.FechaVencimiento.HasValue &&
                                                 t.FechaVencimiento.Value < hoy),
            ProximosVencimientos = new List<LicenciaPorVencerDto>(),
            DistribucionPlanes = new List<DistribucionPlanesDto>()
        };

        reporte.ProximosVencimientos = tenants
            .Where(t => t.FechaVencimiento.HasValue && t.FechaVencimiento.Value > hoy)
            .OrderBy(t => t.FechaVencimiento)
            .Take(10)
            .Select(t => new LicenciaPorVencerDto
            {
                IdTenant = t.IdTenant,
                Codigo = t.Codigo,
                NombreCorto = t.NombreCorto,
                Plan = t.PlanLicencia?.Nombre ?? "Sin plan",
                FechaVencimiento = t.FechaVencimiento,
                DiasRestantes = (int)(t.FechaVencimiento!.Value - hoy).TotalDays,
                EmailContacto = t.EmailContacto ?? ""
            })
            .ToList();

        var totalConPlan = tenants.Count(t => t.IdPlanLicencia > 0);
        reporte.DistribucionPlanes = planes
            .Select(p => {
                var count = tenants.Count(t => t.IdPlanLicencia == p.IdPlanLicencia);
                return new DistribucionPlanesDto
                {
                    IdPlan = p.IdPlanLicencia,
                    NombrePlan = p.Nombre,
                    CantidadTenants = count,
                    IngresoMensual = count * p.PrecioMensual,
                    Porcentaje = totalConPlan > 0 ? Math.Round((decimal)count / totalConPlan * 100, 1) : 0
                };
            })
            .Where(d => d.CantidadTenants > 0)
            .OrderByDescending(d => d.CantidadTenants)
            .ToList();

        reporte.IngresosRecurrentesMensual = reporte.DistribucionPlanes.Sum(d => d.IngresoMensual);

        return reporte;
    }
}
