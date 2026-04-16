using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/solicitudes-baja")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS}")]
    public class SolicitudBajaController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IEstudiantePanelService _panelService;
        private readonly INotificacionInternalService _notifService;

        public SolicitudBajaController(ApplicationDbContext db, IEstudiantePanelService panelService, INotificacionInternalService notifService)
        {
            _db = db;
            _panelService = panelService;
            _notifService = notifService;
        }

        [HttpPost("{idEstudiante:int}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR}")]
        public async Task<ActionResult> SolicitarBaja(int idEstudiante, [FromBody] SolicitarBajaRequest request, CancellationToken ct)
        {
            var estudiante = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Include(e => e.IdPlanActualNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante == null)
                return NotFound(new { error = "Estudiante no encontrado" });

            var adeudos = await _db.Recibo
                .Where(r => r.IdEstudiante == idEstudiante
                    && r.Estatus != EstatusRecibo.PAGADO
                    && r.Estatus != EstatusRecibo.CANCELADO
                    && r.Saldo > 0)
                .ToListAsync(ct);

            var montoAdeudo = adeudos.Sum(r => r.Saldo);
            var recibosVencidos = adeudos.Count(r => r.Estatus == EstatusRecibo.VENCIDO);
            var recibosPendientes = adeudos.Count(r => r.Estatus != EstatusRecibo.VENCIDO);
            var persona = estudiante.IdPersonaNavigation;
            var nombreCompleto = persona != null
                ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                : "Sin nombre";

            if (montoAdeudo <= 0)
            {
                var resultado = await _panelService.ActualizarEstatusEstudianteAsync(
                    idEstudiante, false, request.MotivoBaja, request.TipoBaja, request.EstadoBaja, ct);

                return Ok(new
                {
                    procesada = true,
                    mensaje = "Baja procesada correctamente. El estudiante no tiene adeudos.",
                    requiereAutorizacion = false
                });
            }

            var solicitudExistente = await _db.SolicitudesBaja
                .FirstOrDefaultAsync(s => s.IdEstudiante == idEstudiante
                    && s.EstatusSolicitud == "Pendiente"
                    && s.Status == StatusEnum.Active, ct);

            if (solicitudExistente != null)
                return BadRequest(new { error = "Ya existe una solicitud de baja pendiente para este estudiante" });

            var solicitud = new SolicitudBaja
            {
                IdEstudiante = idEstudiante,
                Matricula = estudiante.Matricula,
                NombreEstudiante = nombreCompleto,
                Carrera = estudiante.IdPlanActualNavigation?.NombrePlanEstudios,
                TipoBaja = request.TipoBaja,
                EstadoBaja = request.EstadoBaja,
                MotivoBaja = request.MotivoBaja,
                MontoAdeudo = montoAdeudo,
                RecibosVencidos = recibosVencidos,
                RecibosPendientes = recibosPendientes,
                EstatusSolicitud = "Pendiente",
                SolicitadoPor = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User?.Identity?.Name ?? "Sistema",
                FechaSolicitud = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Status = StatusEnum.Active
            };

            _db.SolicitudesBaja.Add(solicitud);
            await _db.SaveChangesAsync(ct);

            var usuariosFinanzas = await _db.UserRoles
                .Where(ur => ur.RoleId == _db.Roles.Where(r => r.Name == "finanzas").Select(r => r.Id).FirstOrDefault())
                .Select(ur => ur.UserId)
                .ToListAsync(ct);

            var usuariosAdmin = await _db.UserRoles
                .Where(ur => ur.RoleId == _db.Roles.Where(r => r.Name == "admin").Select(r => r.Id).FirstOrDefault())
                .Select(ur => ur.UserId)
                .ToListAsync(ct);

            var destinatarios = usuariosFinanzas.Union(usuariosAdmin).Distinct();

            foreach (var userId in destinatarios)
            {
                await _notifService.CrearAsync(
                    userId,
                    "Solicitud de baja con adeudo",
                    $"El estudiante {estudiante.Matricula} - {nombreCompleto} solicita baja pero tiene un adeudo de ${montoAdeudo:N2} ({recibosVencidos} vencidos, {recibosPendientes} pendientes). Requiere autorización.",
                    "warning",
                    "Bajas",
                    "/dashboard/solicitudes-baja"
                );
            }

            return Ok(new
            {
                procesada = false,
                mensaje = $"El estudiante tiene un adeudo de ${montoAdeudo:N2}. Se envió solicitud a Finanzas para autorización.",
                requiereAutorizacion = true,
                idSolicitud = solicitud.IdSolicitudBaja,
                montoAdeudo,
                recibosVencidos,
                recibosPendientes
            });
        }

        [HttpGet]
        public async Task<ActionResult> Listar([FromQuery] string? estatus, CancellationToken ct)
        {
            var query = _db.SolicitudesBaja
                .Where(s => s.Status == StatusEnum.Active)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estatus))
                query = query.Where(s => s.EstatusSolicitud == estatus);

            var solicitudes = await query
                .OrderByDescending(s => s.FechaSolicitud)
                .Select(s => new
                {
                    s.IdSolicitudBaja,
                    s.IdEstudiante,
                    s.Matricula,
                    s.NombreEstudiante,
                    s.Carrera,
                    TipoBajaTexto = s.TipoBaja == 1 ? "Administrativa" : s.TipoBaja == 2 ? "Académica" : "No especificada",
                    EstadoBajaTexto = s.EstadoBaja == 1 ? "Temporal" : s.EstadoBaja == 2 ? "Definitiva" : "No especificada",
                    s.MotivoBaja,
                    s.MontoAdeudo,
                    s.RecibosVencidos,
                    s.RecibosPendientes,
                    s.EstatusSolicitud,
                    s.SolicitadoPor,
                    s.AutorizadoPor,
                    s.ComentarioFinanzas,
                    s.FechaSolicitud,
                    s.FechaAutorizacion
                })
                .ToListAsync(ct);

            return Ok(solicitudes);
        }

        [HttpGet("pendientes/count")]
        public async Task<ActionResult> ContarPendientes(CancellationToken ct)
        {
            var count = await _db.SolicitudesBaja
                .CountAsync(s => s.EstatusSolicitud == "Pendiente" && s.Status == StatusEnum.Active, ct);
            return Ok(new { pendientes = count });
        }

        [HttpPut("{idSolicitud:int}/autorizar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.FINANZAS},{Rol.DIRECTOR}")]
        public async Task<ActionResult> Autorizar(int idSolicitud, [FromBody] AutorizarBajaRequest request, CancellationToken ct)
        {
            var solicitud = await _db.SolicitudesBaja
                .FirstOrDefaultAsync(s => s.IdSolicitudBaja == idSolicitud && s.Status == StatusEnum.Active, ct);

            if (solicitud == null)
                return NotFound(new { error = "Solicitud no encontrada" });

            if (solicitud.EstatusSolicitud != "Pendiente")
                return BadRequest(new { error = "La solicitud ya fue procesada" });

            solicitud.EstatusSolicitud = "Autorizada";
            solicitud.AutorizadoPor = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User?.Identity?.Name ?? "Sistema";
            solicitud.ComentarioFinanzas = request.Comentario;
            solicitud.FechaAutorizacion = DateTime.UtcNow;
            solicitud.UpdatedAt = DateTime.UtcNow;

            var resultado = await _panelService.ActualizarEstatusEstudianteAsync(
                solicitud.IdEstudiante, false, solicitud.MotivoBaja, solicitud.TipoBaja, solicitud.EstadoBaja, ct);

            await _db.SaveChangesAsync(ct);

            return Ok(new { mensaje = "Baja autorizada y procesada", solicitud.IdSolicitudBaja });
        }

        [HttpPut("{idSolicitud:int}/rechazar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.FINANZAS},{Rol.DIRECTOR}")]
        public async Task<ActionResult> Rechazar(int idSolicitud, [FromBody] AutorizarBajaRequest request, CancellationToken ct)
        {
            var solicitud = await _db.SolicitudesBaja
                .FirstOrDefaultAsync(s => s.IdSolicitudBaja == idSolicitud && s.Status == StatusEnum.Active, ct);

            if (solicitud == null)
                return NotFound(new { error = "Solicitud no encontrada" });

            solicitud.EstatusSolicitud = "Rechazada";
            solicitud.AutorizadoPor = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User?.Identity?.Name ?? "Sistema";
            solicitud.ComentarioFinanzas = request.Comentario;
            solicitud.FechaAutorizacion = DateTime.UtcNow;
            solicitud.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return Ok(new { mensaje = "Solicitud de baja rechazada", solicitud.IdSolicitudBaja });
        }

        [HttpGet("{idEstudiante:int}/verificar-adeudo")]
        public async Task<ActionResult> VerificarAdeudo(int idEstudiante, CancellationToken ct)
        {
            var adeudos = await _db.Recibo
                .Where(r => r.IdEstudiante == idEstudiante
                    && r.Estatus != EstatusRecibo.PAGADO
                    && r.Estatus != EstatusRecibo.CANCELADO
                    && r.Saldo > 0)
                .ToListAsync(ct);

            return Ok(new
            {
                tieneAdeudo = adeudos.Count > 0,
                montoTotal = adeudos.Sum(r => r.Saldo),
                recibosVencidos = adeudos.Count(r => r.Estatus == EstatusRecibo.VENCIDO),
                recibosPendientes = adeudos.Count(r => r.Estatus != EstatusRecibo.VENCIDO),
                recibos = adeudos.Select(r => new
                {
                    r.IdRecibo,
                    r.Folio,
                    Estatus = r.Estatus.ToString(),
                    r.Total,
                    r.Saldo,
                    r.FechaVencimiento
                })
            });
        }
    }

    public class SolicitarBajaRequest
    {
        public int? TipoBaja { get; set; }
        public int? EstadoBaja { get; set; }
        public string? MotivoBaja { get; set; }
    }

    public class AutorizarBajaRequest
    {
        public string? Comentario { get; set; }
    }
}
