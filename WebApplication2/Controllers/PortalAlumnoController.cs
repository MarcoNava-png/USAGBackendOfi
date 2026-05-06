using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.PortalAlumno;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/portal-alumno")]
    [ApiController]
    [Authorize(Roles = Rol.ALUMNO)]
    public class PortalAlumnoController : ControllerBase
    {
        private readonly IPortalAlumnoService _portalAlumnoService;

        public PortalAlumnoController(IPortalAlumnoService portalAlumnoService)
        {
            _portalAlumnoService = portalAlumnoService;
        }

        private string? GetUserId() => User.FindFirst("userId")?.Value;

        [HttpGet("mi-perfil")]
        public async Task<ActionResult<MiPerfilDto>> ObtenerMiPerfil(CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var perfil = await _portalAlumnoService.ObtenerMiPerfilAsync(userId, ct);
            if (perfil == null) return NotFound(new { error = "No se encontro el perfil del estudiante" });

            return Ok(perfil);
        }

        [HttpPatch("mi-perfil")]
        public async Task<ActionResult> ActualizarMiPerfil([FromBody] ActualizarMiPerfilRequest request, CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var actualizado = await _portalAlumnoService.ActualizarMiPerfilAsync(userId, request, ct);
            if (!actualizado) return NotFound(new { error = "No se encontro el perfil del estudiante" });

            return Ok(new { success = true, message = "Perfil actualizado exitosamente" });
        }

        [HttpGet("mis-materias")]
        public async Task<ActionResult<MisMateriasDto>> ObtenerMisMaterias([FromQuery] int? idPeriodoAcademico, CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var materias = await _portalAlumnoService.ObtenerMisMateriasAsync(userId, idPeriodoAcademico, ct);
            return Ok(materias);
        }

        [HttpGet("mis-calificaciones")]
        public async Task<ActionResult<MisCalificacionesDto>> ObtenerMisCalificaciones([FromQuery] int? idPeriodoAcademico, CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var calificaciones = await _portalAlumnoService.ObtenerMisCalificacionesAsync(userId, idPeriodoAcademico, ct);
            return Ok(calificaciones);
        }

        [HttpGet("mi-asistencia")]
        public async Task<ActionResult<MiAsistenciaDto>> ObtenerMiAsistencia([FromQuery] int? idPeriodoAcademico, CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var asistencia = await _portalAlumnoService.ObtenerMiAsistenciaAsync(userId, idPeriodoAcademico, ct);
            return Ok(asistencia);
        }

        [HttpGet("mis-pagos")]
        public async Task<ActionResult<MisPagosDto>> ObtenerMisPagos(CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var pagos = await _portalAlumnoService.ObtenerMisPagosAsync(userId, ct);
            return Ok(pagos);
        }

        [HttpGet("mis-pagos/{idRecibo:long}")]
        public async Task<ActionResult<MiReciboDto>> ObtenerMiReciboDetalle([FromRoute] long idRecibo, CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var recibo = await _portalAlumnoService.ObtenerMiReciboDetalleAsync(userId, idRecibo, ct);
            if (recibo == null) return NotFound(new { error = "No se encontro el recibo" });

            return Ok(recibo);
        }

        [HttpGet("mi-expediente")]
        public async Task<ActionResult<IReadOnlyList<Core.DTOs.AspiranteDocumentoDto>>> ObtenerMiExpediente(CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var docs = await _portalAlumnoService.ObtenerMiExpedienteAsync(userId, ct);
            return Ok(docs);
        }

        [HttpGet("mis-documentos-pendientes")]
        public async Task<ActionResult<MisDocumentosPendientesDto>> ObtenerMisDocumentosPendientes(CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var documentos = await _portalAlumnoService.ObtenerMisDocumentosPendientesAsync(userId, ct);
            return Ok(documentos);
        }

        [HttpGet("mis-documentos-oficiales")]
        public async Task<ActionResult<MisDocumentosOficialesDto>> ObtenerMisDocumentosOficiales(CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var documentos = await _portalAlumnoService.ObtenerMisDocumentosOficialesAsync(userId, ct);
            return Ok(documentos);
        }
    }
}
