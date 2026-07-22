using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.VentanaCaptura;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/ventana-captura")]
    [ApiController]
    [Authorize]
    public class VentanaCapturaController : ControllerBase
    {
        private readonly IVentanaCapturaService _service;
        private readonly IProfesorService _profesorService;
        private readonly IAuthService _authService;

        // Abrir/cerrar ventanas y gestionar prórrogas: solo administración escolar
        private const string ROLES_ADMIN_VENTANAS = $"{Rol.SUPER_ADMIN},{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.ACADEMICO}";
        // Ver estado de ventanas y avance: incluye director/coordinador (supervisión, solo lectura)
        private const string ROLES_SUPERVISION = $"{Rol.SUPER_ADMIN},{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.COORDINADOR},{Rol.DIRECTOR},{Rol.ACADEMICO}";

        public VentanaCapturaController(IVentanaCapturaService service, IProfesorService profesorService, IAuthService authService)
        {
            _service = service;
            _profesorService = profesorService;
            _authService = authService;
        }

        private async Task<int?> CampusRestringidoAsync()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId)) return null;
            if (!User.IsInRole(Rol.DIRECTOR) && !User.IsInRole(Rol.COORDINADOR)) return null;
            var user = await _authService.GetUserById(userId);
            return user?.IdCampusAsignado;
        }

        private async Task<int?> ProfesorActualAsync(CancellationToken ct)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId)) return null;
            var profesor = await _profesorService.GetProfesorByUsuarioId(userId, ct);
            return profesor?.IdProfesor;
        }

        // ─── Ventanas (admin) ───────────────────────────────
        [HttpGet]
        [Authorize(Roles = ROLES_SUPERVISION)]
        public async Task<ActionResult<List<VentanaCapturaDto>>> GetVentanas([FromQuery] int idPeriodoAcademico, CancellationToken ct)
        {
            return Ok(await _service.GetVentanasAsync(idPeriodoAcademico, ct));
        }

        [HttpPost("abrir")]
        [Authorize(Roles = ROLES_ADMIN_VENTANAS)]
        public async Task<ActionResult<VentanaCapturaDto>> Abrir([FromBody] AbrirVentanaRequest req, CancellationToken ct)
        {
            return Ok(await _service.AbrirAsync(req.IdPeriodoAcademico, req.NumeroParcial, req.FechaLimite, ct));
        }

        [HttpPost("cerrar")]
        [Authorize(Roles = ROLES_ADMIN_VENTANAS)]
        public async Task<ActionResult<VentanaCapturaDto>> Cerrar([FromBody] CerrarVentanaRequest req, CancellationToken ct)
        {
            return Ok(await _service.CerrarAsync(req.IdPeriodoAcademico, req.NumeroParcial, ct));
        }

        // ─── Estado / prórroga (docente) ────────────────────
        [HttpGet("estado")]
        public async Task<ActionResult<EstadoCapturaDto>> GetEstado([FromQuery] int idGrupoMateria, [FromQuery] int numeroParcial, CancellationToken ct)
        {
            var idProfesor = await ProfesorActualAsync(ct);
            if (idProfesor == null) return NotFound(new { message = "Profesor no encontrado" });
            return Ok(await _service.GetEstadoAsync(idGrupoMateria, numeroParcial, idProfesor.Value, ct));
        }

        [HttpPost("prorroga")]
        public async Task<ActionResult<SolicitudProrrogaDto>> SolicitarProrroga([FromBody] SolicitarProrrogaRequest req, CancellationToken ct)
        {
            var idProfesor = await ProfesorActualAsync(ct);
            if (idProfesor == null) return NotFound(new { message = "Profesor no encontrado" });
            try
            {
                return Ok(await _service.SolicitarProrrogaAsync(idProfesor.Value, req, ct));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("mis-prorrogas")]
        public async Task<ActionResult<List<SolicitudProrrogaDto>>> MisProrrogas(CancellationToken ct)
        {
            var idProfesor = await ProfesorActualAsync(ct);
            if (idProfesor == null) return NotFound(new { message = "Profesor no encontrado" });
            return Ok(await _service.GetMisProrrogasAsync(idProfesor.Value, ct));
        }

        // ─── Supervisión (coordinador/director/admin) ───────
        [HttpGet("prorrogas")]
        [Authorize(Roles = ROLES_ADMIN_VENTANAS)]
        public async Task<ActionResult<List<SolicitudProrrogaDto>>> Prorrogas([FromQuery] string? estado, CancellationToken ct)
        {
            var campus = await CampusRestringidoAsync();
            return Ok(await _service.GetProrrogasAsync(estado, campus, ct));
        }

        [HttpPost("prorroga/{id:int}/resolver")]
        [Authorize(Roles = ROLES_ADMIN_VENTANAS)]
        public async Task<ActionResult<SolicitudProrrogaDto>> ResolverProrroga(int id, [FromBody] ResolverProrrogaRequest req, CancellationToken ct)
        {
            var userId = User.FindFirst("userId")?.Value ?? "";
            var resultado = await _service.ResolverProrrogaAsync(id, req, userId, ct);
            if (resultado == null) return NotFound(new { message = "Solicitud no encontrada" });
            return Ok(resultado);
        }

        [HttpGet("avance")]
        [Authorize(Roles = ROLES_SUPERVISION)]
        public async Task<ActionResult<AvanceCapturaDto>> Avance([FromQuery] int idPeriodoAcademico, [FromQuery] int numeroParcial, CancellationToken ct)
        {
            var campus = await CampusRestringidoAsync();
            return Ok(await _service.GetAvanceAsync(idPeriodoAcademico, numeroParcial, campus, ct));
        }
    }
}
