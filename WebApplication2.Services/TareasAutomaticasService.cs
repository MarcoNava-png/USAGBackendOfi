using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class TareasAutomaticasService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TareasAutomaticasService> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(30);

        public TareasAutomaticasService(IServiceScopeFactory scopeFactory, ILogger<TareasAutomaticasService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TareasAutomaticasService iniciado. Intervalo: {Intervalo} minutos", _intervalo.TotalMinutes);

            // Wait 2 minutes before first run to let the app fully start
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Ejecutando tareas automaticas...");
                    await EjecutarTareasAsync(stoppingToken);
                    _logger.LogInformation("Tareas automaticas completadas.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en tareas automaticas");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }
        }

        private async Task EjecutarTareasAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notifService = scope.ServiceProvider.GetRequiredService<INotificacionInternalService>();

            await VerificarSolicitudesVencidasAsync(db, notifService, ct);
            await VerificarProrrogasVencidasAsync(db, notifService, ct);
        }

        private async Task VerificarSolicitudesVencidasAsync(ApplicationDbContext db, INotificacionInternalService notifService, CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            // Find generated documents that have expired
            var solicitudesVencidas = await db.SolicitudesDocumento
                .Include(s => s.TipoDocumento)
                .Include(s => s.Estudiante)
                .Where(s => s.Estatus == EstatusSolicitudDocumento.GENERADO
                    && s.FechaVencimiento.HasValue
                    && s.FechaVencimiento.Value < ahora)
                .ToListAsync(ct);

            foreach (var sol in solicitudesVencidas)
            {
                sol.Estatus = EstatusSolicitudDocumento.VENCIDO;
                sol.FechaModificacion = ahora;

                // Notify the user who requested it
                if (!string.IsNullOrEmpty(sol.UsuarioSolicita))
                {
                    await notifService.CrearAsync(
                        sol.UsuarioSolicita,
                        "Documento vencido",
                        $"La solicitud {sol.FolioSolicitud} ({sol.TipoDocumento?.Nombre ?? "Documento"}) ha vencido y necesita regenerarse.",
                        "warning",
                        "Documentos",
                        "/dashboard/documentos-solicitudes"
                    );
                }
            }

            if (solicitudesVencidas.Count > 0)
            {
                await db.SaveChangesAsync(ct);
                _logger.LogInformation("Marcadas {Count} solicitudes como vencidas", solicitudesVencidas.Count);
            }
        }

        private async Task VerificarProrrogasVencidasAsync(ApplicationDbContext db, INotificacionInternalService notifService, CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;
            var enTresDias = ahora.AddDays(3);
            var hoy = DateOnly.FromDateTime(ahora);
            var limiteVencimiento = DateOnly.FromDateTime(enTresDias);

            // Find receipts about to expire (within 3 days) that haven't been paid
            var recibosProximos = await db.Recibo
                .Where(r => r.Estatus != EstatusRecibo.PAGADO
                    && r.Estatus != EstatusRecibo.CANCELADO
                    && r.IdEstudiante.HasValue
                    && r.FechaVencimiento >= hoy
                    && r.FechaVencimiento <= limiteVencimiento)
                .ToListAsync(ct);

            foreach (var recibo in recibosProximos)
            {
                var estudianteId = recibo.IdEstudiante!.Value;

                // Find the student and their email
                var estudiante = await db.Estudiante.FirstOrDefaultAsync(e => e.IdEstudiante == estudianteId, ct);
                if (estudiante == null || string.IsNullOrEmpty(estudiante.Email)) continue;

                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == estudiante.Email, ct);
                if (user == null) continue;

                var folio = recibo.Folio ?? $"REC-{recibo.IdRecibo}";

                // Check if we already sent a notification for this receipt recently
                var yaNotificado = await db.NotificacionesUsuario
                    .AnyAsync(n => n.UsuarioDestinoId == user.Id
                        && n.Modulo == "Pagos"
                        && n.Mensaje.Contains(folio)
                        && n.FechaCreacion > ahora.AddDays(-3), ct);

                if (yaNotificado) continue;

                await notifService.CrearAsync(
                    user.Id,
                    "Recibo proximo a vencer",
                    $"El recibo {folio} vence el {recibo.FechaVencimiento:dd/MM/yyyy}. Realiza tu pago para evitar recargos.",
                    "warning",
                    "Pagos",
                    "/dashboard/pagos"
                );
            }

            if (recibosProximos.Count > 0)
                _logger.LogInformation("Revisados {Count} recibos proximos a vencer", recibosProximos.Count);
        }
    }
}
