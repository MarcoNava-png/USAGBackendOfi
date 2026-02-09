using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Documentos;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Documentos;
using WebApplication2.Core.Responses.Documentos;
using WebApplication2.Data.DbContexts;
using WebApplication2.Data.Seed;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoEstudianteController : ControllerBase
    {
        private readonly IDocumentoEstudianteService _documentoService;
        private readonly ApplicationDbContext _context;

        public DocumentoEstudianteController(
            IDocumentoEstudianteService documentoService,
            ApplicationDbContext context)
        {
            _documentoService = documentoService;
            _context = context;
        }

        [HttpGet("tipos")]
        [Authorize]
        public async Task<ActionResult<List<TipoDocumentoDto>>> GetTiposDocumento()
        {
            var tipos = await _documentoService.GetTiposDocumentoAsync();
            return Ok(tipos);
        }

        [HttpGet("tipos/{id}")]
        [Authorize]
        public async Task<ActionResult<TipoDocumentoDto>> GetTipoDocumento(int id)
        {
            var tipo = await _documentoService.GetTipoDocumentoByIdAsync(id);
            if (tipo == null)
                return NotFound(new { message = "Tipo de documento no encontrado" });
            return Ok(tipo);
        }

        [HttpPost("tipos")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<ActionResult<TipoDocumentoEstudiante>> CreateTipoDocumento([FromBody] TipoDocumentoEstudiante tipo)
        {
            var created = await _documentoService.CreateTipoDocumentoAsync(tipo);
            return CreatedAtAction(nameof(GetTipoDocumento), new { id = created.IdTipoDocumento }, created);
        }

        [HttpPut("tipos/{id}")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<ActionResult<TipoDocumentoEstudiante>> UpdateTipoDocumento(int id, [FromBody] TipoDocumentoEstudiante tipo)
        {
            if (id != tipo.IdTipoDocumento)
                return BadRequest(new { message = "El ID no coincide" });

            var updated = await _documentoService.UpdateTipoDocumentoAsync(tipo);
            return Ok(updated);
        }

        [HttpDelete("tipos/{id}")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<ActionResult> DeleteTipoDocumento(int id)
        {
            await _documentoService.DeleteTipoDocumentoAsync(id);
            return NoContent();
        }

        [HttpPost("solicitar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR}")]
        public async Task<ActionResult<SolicitudDocumentoDto>> CrearSolicitud([FromBody] CrearSolicitudDocumentoRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value ?? "System";
                var solicitud = await _documentoService.CrearSolicitudAsync(request, userId);
                return Ok(solicitud);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("solicitudes/{id}")]
        [Authorize]
        public async Task<ActionResult<SolicitudDocumentoDto>> GetSolicitud(long id)
        {
            var solicitud = await _documentoService.GetSolicitudByIdAsync(id);
            if (solicitud == null)
                return NotFound(new { message = "Solicitud no encontrada" });
            return Ok(solicitud);
        }

        [HttpGet("solicitudes")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.FINANZAS}")]
        public async Task<ActionResult<SolicitudesListResponse>> GetSolicitudes([FromQuery] SolicitudesFiltro filtro)
        {
            var result = await _documentoService.GetSolicitudesAsync(filtro);
            return Ok(result);
        }

        [HttpGet("estudiante/{idEstudiante}/solicitudes")]
        [Authorize]
        public async Task<ActionResult<List<SolicitudDocumentoDto>>> GetSolicitudesByEstudiante(int idEstudiante)
        {
            var solicitudes = await _documentoService.GetSolicitudesByEstudianteAsync(idEstudiante);
            return Ok(solicitudes);
        }

        [HttpPost("solicitudes/{id}/generar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR}")]
        public async Task<ActionResult<SolicitudDocumentoDto>> MarcarComoGenerada(long id)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value ?? "System";
                var solicitud = await _documentoService.MarcarComoGeneradaAsync(id, userId);
                return Ok(solicitud);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("solicitudes/{id}/entregar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR}")]
        public async Task<ActionResult<SolicitudDocumentoDto>> MarcarComoEntregado(long id)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value ?? "System";
                var solicitud = await _documentoService.MarcarComoEntregadoAsync(id, userId);
                return Ok(solicitud);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("solicitudes/{id}/cancelar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR}")]
        public async Task<ActionResult> CancelarSolicitud(long id, [FromBody] CancelarSolicitudRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value ?? "System";
                await _documentoService.CancelarSolicitudAsync(id, request.Motivo, userId);
                return Ok(new { message = "Solicitud cancelada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("solicitudes/{id}/kardex/pdf")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        public async Task<ActionResult> DescargarKardexPdf(long id)
        {
            try
            {
                var puedeGenerar = await _documentoService.PuedeGenerarAsync(id);
                if (!puedeGenerar)
                    return BadRequest(new { message = "La solicitud debe estar pagada para generar el documento" });

                var userId = User.FindFirst("userId")?.Value ?? "System";
                await _documentoService.MarcarComoGeneradaAsync(id, userId);

                var pdfBytes = await _documentoService.GenerarKardexPdfAsync(id);
                return File(pdfBytes, "application/pdf", $"Kardex_{id}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("solicitudes/{id}/constancia/pdf")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        public async Task<ActionResult> DescargarConstanciaPdf(long id)
        {
            try
            {
                var puedeGenerar = await _documentoService.PuedeGenerarAsync(id);
                if (!puedeGenerar)
                    return BadRequest(new { message = "La solicitud debe estar pagada para generar el documento" });

                var userId = User.FindFirst("userId")?.Value ?? "System";
                await _documentoService.MarcarComoGeneradaAsync(id, userId);

                var pdfBytes = await _documentoService.GenerarConstanciaPdfAsync(id);
                return File(pdfBytes, "application/pdf", $"Constancia_{id}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("estudiante/{idEstudiante}/kardex")]
        [Authorize]
        public async Task<ActionResult<KardexEstudianteDto>> GetKardex(int idEstudiante, [FromQuery] bool soloPeriodoActual = false)
        {
            try
            {
                var kardex = await _documentoService.GenerarKardexAsync(idEstudiante, soloPeriodoActual);
                return Ok(kardex);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("verificar/{codigoVerificacion}")]
        [AllowAnonymous]
        public async Task<ActionResult<VerificacionDocumentoDto>> VerificarDocumento(Guid codigoVerificacion)
        {
            var verificacion = await _documentoService.VerificarDocumentoAsync(codigoVerificacion);
            return Ok(verificacion);
        }

        [HttpPost("notificar-pago/{idRecibo}")]
        [Authorize(Roles = Rol.ROLES_CAJA)]
        public async Task<ActionResult> NotificarPago(long idRecibo)
        {
            await _documentoService.ActualizarEstatusPagoAsync(idRecibo);
            return Ok(new { message = "Estatus actualizado" });
        }

        [HttpPost("actualizar-vencidos")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<ActionResult> ActualizarDocumentosVencidos()
        {
            await _documentoService.ActualizarDocumentosVencidosAsync();
            return Ok(new { message = "Documentos vencidos actualizados" });
        }

        /// <summary>
        /// Ejecuta el seed de tipos de documento si la tabla está vacía
        /// </summary>
        [HttpPost("seed-tipos")]
        [Authorize(Roles = Rol.ADMIN)]
        public ActionResult SeedTiposDocumento()
        {
            try
            {
                var countBefore = _context.TiposDocumentoEstudiante.Count();

                TipoDocumentoSeed.Seed(_context);

                var countAfter = _context.TiposDocumentoEstudiante.Count();

                return Ok(new
                {
                    message = "Seed ejecutado exitosamente",
                    tiposAntes = countBefore,
                    tiposDespues = countAfter,
                    nuevosInsertados = countAfter - countBefore
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al ejecutar seed: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene todas las solicitudes para el panel de Control Escolar con estadísticas
        /// </summary>
        [HttpGet("panel-control-escolar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS},{Rol.ADMISIONES},{Rol.DIRECTOR}")]
        public async Task<ActionResult<SolicitudesPendientesDto>> GetSolicitudesControlEscolar(
            [FromQuery] string? estatus = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string? busqueda = null,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _documentoService.GetSolicitudesParaControlEscolarAsync(
                    estatus, fechaDesde, fechaHasta, busqueda, ct);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el contador de solicitudes listas para generar (pagadas)
        /// </summary>
        [HttpGet("contador-pendientes")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS},{Rol.ADMISIONES},{Rol.DIRECTOR}")]
        public async Task<ActionResult<int>> GetContadorPendientes(CancellationToken ct = default)
        {
            try
            {
                var contador = await _documentoService.GetContadorSolicitudesListasAsync(ct);
                return Ok(contador);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
