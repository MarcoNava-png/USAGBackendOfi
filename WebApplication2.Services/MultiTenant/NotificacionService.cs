using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication2.Core.DTOs.MultiTenant;
using WebApplication2.Core.Models.MultiTenant;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Services.MultiTenant;

public interface INotificacionService
{
    Task<List<NotificacionDto>> ObtenerNotificacionesAsync(bool soloNoLeidas = false, int limite = 50, CancellationToken ct = default);
    Task<int> ObtenerContadorNoLeidasAsync(CancellationToken ct = default);
    Task MarcarComoLeidaAsync(int idNotificacion, CancellationToken ct = default);
    Task MarcarTodasComoLeidasAsync(CancellationToken ct = default);
    Task<ResultadoEnvioNotificacionesDto> VerificarVencimientosAsync(CancellationToken ct = default);
    Task CrearNotificacionAsync(string tipo, string titulo, string mensaje, int? idTenant = null, string prioridad = "Normal", CancellationToken ct = default);
}

public class NotificacionService : INotificacionService
{
    private readonly MasterDbContext _masterDb;
    private readonly ILogger<NotificacionService> _logger;

    public NotificacionService(
        MasterDbContext masterDb,
        ILogger<NotificacionService> logger)
    {
        _masterDb = masterDb;
        _logger = logger;
    }

    public async Task<List<NotificacionDto>> ObtenerNotificacionesAsync(
        bool soloNoLeidas = false,
        int limite = 50,
        CancellationToken ct = default)
    {
        var query = _masterDb.Notificaciones.AsQueryable();

        if (soloNoLeidas)
        {
            query = query.Where(n => !n.Leida);
        }

        return await query
            .OrderByDescending(n => n.FechaCreacion)
            .Take(limite)
            .Select(n => new NotificacionDto
            {
                Id = n.IdNotificacion,
                Tipo = n.Tipo,
                Titulo = n.Titulo,
                Mensaje = n.Mensaje,
                TenantCodigo = n.TenantCodigo,
                TenantNombre = n.TenantNombre,
                IdTenant = n.IdTenant,
                FechaCreacion = n.FechaCreacion,
                Leida = n.Leida,
                FechaLectura = n.FechaLectura,
                Prioridad = n.Prioridad,
                AccionUrl = n.AccionUrl
            })
            .ToListAsync(ct);
    }

    public async Task<int> ObtenerContadorNoLeidasAsync(CancellationToken ct = default)
    {
        return await _masterDb.Notificaciones.CountAsync(n => !n.Leida, ct);
    }

    public async Task MarcarComoLeidaAsync(int idNotificacion, CancellationToken ct = default)
    {
        var notificacion = await _masterDb.Notificaciones.FindAsync(new object[] { idNotificacion }, ct);
        if (notificacion != null && !notificacion.Leida)
        {
            notificacion.Leida = true;
            notificacion.FechaLectura = DateTime.UtcNow;
            await _masterDb.SaveChangesAsync(ct);
        }
    }

    public async Task MarcarTodasComoLeidasAsync(CancellationToken ct = default)
    {
        var noLeidas = await _masterDb.Notificaciones
            .Where(n => !n.Leida)
            .ToListAsync(ct);

        foreach (var n in noLeidas)
        {
            n.Leida = true;
            n.FechaLectura = DateTime.UtcNow;
        }

        await _masterDb.SaveChangesAsync(ct);
    }

    public async Task<ResultadoEnvioNotificacionesDto> VerificarVencimientosAsync(CancellationToken ct = default)
    {
        var resultado = new ResultadoEnvioNotificacionesDto();
        var hoy = DateTime.UtcNow.Date;

        var tenants = await _masterDb.Tenants
            .Where(t => t.Status == TenantStatus.Active && t.FechaVencimiento.HasValue)
            .ToListAsync(ct);

        foreach (var tenant in tenants)
        {
            var diasRestantes = (tenant.FechaVencimiento!.Value.Date - hoy).Days;

            var notificacionReciente = await _masterDb.Notificaciones
                .AnyAsync(n =>
                    n.IdTenant == tenant.IdTenant &&
                    n.Tipo == TipoNotificacion.Vencimiento &&
                    n.FechaCreacion >= hoy.AddDays(-3), ct);

            if (notificacionReciente) continue;

            string? prioridad = null;
            string? titulo = null;
            string? mensaje = null;

            if (diasRestantes <= 0)
            {
                prioridad = PrioridadNotificacion.Critica;
                titulo = $"Licencia VENCIDA: {tenant.NombreCorto}";
                mensaje = $"La licencia de {tenant.Nombre} ({tenant.Codigo}) ha vencido hace {Math.Abs(diasRestantes)} dias. " +
                          $"Email de contacto: {tenant.EmailContacto ?? "No especificado"}";
            }
            else if (diasRestantes <= 7)
            {
                prioridad = PrioridadNotificacion.Critica;
                titulo = $"Vencimiento CRITICO: {tenant.NombreCorto}";
                mensaje = $"La licencia de {tenant.Nombre} vence en {diasRestantes} dias " +
                          $"({tenant.FechaVencimiento:dd/MM/yyyy}). Contactar urgentemente.";
            }
            else if (diasRestantes <= 15)
            {
                prioridad = PrioridadNotificacion.Alta;
                titulo = $"Proximo vencimiento: {tenant.NombreCorto}";
                mensaje = $"La licencia de {tenant.Nombre} vence en {diasRestantes} dias " +
                          $"({tenant.FechaVencimiento:dd/MM/yyyy}).";
            }
            else if (diasRestantes <= 30)
            {
                prioridad = PrioridadNotificacion.Normal;
                titulo = $"Recordatorio de vencimiento: {tenant.NombreCorto}";
                mensaje = $"La licencia de {tenant.Nombre} vence en {diasRestantes} dias " +
                          $"({tenant.FechaVencimiento:dd/MM/yyyy}).";
            }

            if (prioridad != null && titulo != null && mensaje != null)
            {
                try
                {
                    await CrearNotificacionInterna(
                        TipoNotificacion.Vencimiento,
                        titulo,
                        mensaje,
                        tenant.IdTenant,
                        tenant.Codigo,
                        tenant.NombreCorto,
                        prioridad,
                        $"/dashboard/super-admin/tenants/{tenant.IdTenant}/edit",
                        ct);

                    resultado.NotificacionesCreadas++;
                    resultado.Detalles.Add($"Notificacion creada para {tenant.Codigo}: {diasRestantes} dias restantes");

                    _logger.LogInformation("Notificacion de vencimiento creada para {Codigo}: {DiasRestantes} dias",
                        tenant.Codigo, diasRestantes);
                }
                catch (Exception ex)
                {
                    resultado.Errores++;
                    resultado.Detalles.Add($"Error al crear notificacion para {tenant.Codigo}: {ex.Message}");
                    _logger.LogError(ex, "Error al crear notificacion de vencimiento para {Codigo}", tenant.Codigo);
                }
            }
        }

        return resultado;
    }

    public async Task CrearNotificacionAsync(
        string tipo,
        string titulo,
        string mensaje,
        int? idTenant = null,
        string prioridad = "Normal",
        CancellationToken ct = default)
    {
        string? tenantCodigo = null;
        string? tenantNombre = null;

        if (idTenant.HasValue)
        {
            var tenant = await _masterDb.Tenants.FindAsync(new object[] { idTenant.Value }, ct);
            if (tenant != null)
            {
                tenantCodigo = tenant.Codigo;
                tenantNombre = tenant.NombreCorto;
            }
        }

        await CrearNotificacionInterna(tipo, titulo, mensaje, idTenant, tenantCodigo, tenantNombre, prioridad, null, ct);
    }

    private async Task CrearNotificacionInterna(
        string tipo,
        string titulo,
        string mensaje,
        int? idTenant,
        string? tenantCodigo,
        string? tenantNombre,
        string prioridad,
        string? accionUrl,
        CancellationToken ct)
    {
        var notificacion = new Notificacion
        {
            Tipo = tipo,
            Titulo = titulo,
            Mensaje = mensaje,
            IdTenant = idTenant,
            TenantCodigo = tenantCodigo,
            TenantNombre = tenantNombre,
            FechaCreacion = DateTime.UtcNow,
            Leida = false,
            Prioridad = prioridad,
            AccionUrl = accionUrl
        };

        _masterDb.Notificaciones.Add(notificacion);
        await _masterDb.SaveChangesAsync(ct);
    }
}
