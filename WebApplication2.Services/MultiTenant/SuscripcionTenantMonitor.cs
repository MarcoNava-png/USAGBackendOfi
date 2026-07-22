using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApplication2.Core.Models.MultiTenant;
using WebApplication2.Data.DbContexts;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services.MultiTenant
{
    public class SuscripcionTenantMonitor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SuscripcionTenantMonitor> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _intervalo = TimeSpan.FromHours(12);
        private const int DiasAvisoPrevio = 7;

        public SuscripcionTenantMonitor(IServiceScopeFactory scopeFactory, ILogger<SuscripcionTenantMonitor> logger, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        private ApplicationDbContext CrearContextoTenant(string connectionString)
        {
            var ob = new DbContextOptionsBuilder<ApplicationDbContext>();
            if ((_configuration["DatabaseProvider"] ?? "SqlServer") == "PostgreSQL")
                ob.UseNpgsql(connectionString);
            else
                ob.UseSqlServer(connectionString);
            ob.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            return new ApplicationDbContext(ob.Options, forProvisioning: true);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SuscripcionTenantMonitor iniciado. Intervalo: {Horas}h", _intervalo.TotalHours);

            await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RevisarSuscripcionesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en SuscripcionTenantMonitor");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }
        }

        private async Task RevisarSuscripcionesAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var master = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();

            var ahora = DateTime.UtcNow;

            var vencidos = await master.Tenants
                .Where(t => t.Status == TenantStatus.Active
                         && t.FechaVencimiento != null
                         && t.FechaVencimiento < ahora)
                .ToListAsync(ct);

            foreach (var t in vencidos)
            {
                await tenantService.CambiarStatusTenantAsync(t.IdTenant, TenantStatus.Suspended, "Suscripción vencida (suspensión automática)", ct);

                master.Notificaciones.Add(new Notificacion
                {
                    Tipo = TipoNotificacion.Vencimiento,
                    Titulo = "Suscripción vencida",
                    Mensaje = $"La escuela {t.Nombre} ({t.Codigo}) fue suspendida automáticamente por vencimiento de suscripción ({t.FechaVencimiento:yyyy-MM-dd}).",
                    IdTenant = t.IdTenant,
                    TenantCodigo = t.Codigo,
                    TenantNombre = t.Nombre,
                    Prioridad = "Alta",
                    FechaCreacion = ahora
                });

                _logger.LogWarning("Tenant {Codigo} suspendido automáticamente por vencimiento de suscripción", t.Codigo);
            }

            var limite = ahora.AddDays(DiasAvisoPrevio);
            var porVencer = await master.Tenants
                .Where(t => t.Status == TenantStatus.Active
                         && t.FechaVencimiento != null
                         && t.FechaVencimiento >= ahora
                         && t.FechaVencimiento <= limite)
                .ToListAsync(ct);

            foreach (var t in porVencer)
            {
                var yaAvisado = await master.Notificaciones.AnyAsync(n =>
                    n.IdTenant == t.IdTenant
                    && n.Tipo == TipoNotificacion.Vencimiento
                    && n.Titulo == "Suscripción por vencer"
                    && n.FechaCreacion > ahora.AddDays(-DiasAvisoPrevio), ct);

                if (yaAvisado)
                    continue;

                master.Notificaciones.Add(new Notificacion
                {
                    Tipo = TipoNotificacion.Vencimiento,
                    Titulo = "Suscripción por vencer",
                    Mensaje = $"La suscripción de {t.Nombre} ({t.Codigo}) vence el {t.FechaVencimiento:yyyy-MM-dd}.",
                    IdTenant = t.IdTenant,
                    TenantCodigo = t.Codigo,
                    TenantNombre = t.Nombre,
                    Prioridad = "Normal",
                    FechaCreacion = ahora
                });

                _logger.LogInformation("Aviso de vencimiento próximo generado para tenant {Codigo}", t.Codigo);
            }

            var activos = await master.Tenants
                .Where(t => t.Status == TenantStatus.Active && t.MaximoEstudiantes > 0)
                .ToListAsync(ct);

            foreach (var t in activos)
            {
                try
                {
                    using var ctx = CrearContextoTenant(t.ConnectionString);
                    var alumnos = await ctx.Estudiante.CountAsync(e => e.Activo && e.Status == StatusEnum.Active, ct);

                    if (alumnos >= t.MaximoEstudiantes)
                    {
                        var yaAvisado = await master.Notificaciones.AnyAsync(n =>
                            n.IdTenant == t.IdTenant
                            && n.Titulo == "Sobrecupo de estudiantes"
                            && n.FechaCreacion > ahora.AddDays(-DiasAvisoPrevio), ct);

                        if (!yaAvisado)
                        {
                            master.Notificaciones.Add(new Notificacion
                            {
                                Tipo = TipoNotificacion.Alerta,
                                Titulo = "Sobrecupo de estudiantes",
                                Mensaje = $"La escuela {t.Nombre} ({t.Codigo}) tiene {alumnos} estudiantes y alcanzó/superó el cupo de su plan ({t.MaximoEstudiantes}). Considera ofrecerle un plan mayor.",
                                IdTenant = t.IdTenant,
                                TenantCodigo = t.Codigo,
                                TenantNombre = t.Nombre,
                                Prioridad = "Normal",
                                FechaCreacion = ahora
                            });
                            _logger.LogInformation("Aviso de sobrecupo de estudiantes generado para {Codigo} ({Alumnos}/{Max})", t.Codigo, alumnos, t.MaximoEstudiantes);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error revisando sobrecupo de estudiantes de {Codigo}", t.Codigo);
                }
            }

            if (master.ChangeTracker.HasChanges())
                await master.SaveChangesAsync(ct);
        }
    }
}
