using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.EstudiantePanel;
using WebApplication2.Core.Requests.Estudiante;
using WebApplication2.Core.Requests.EstudiantePanel;
using WebApplication2.Core.Responses.EstudiantePanel;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/estudiante-panel")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class EstudiantePanelController : ControllerBase
    {
        private readonly IEstudiantePanelService _panelService;

        public EstudiantePanelController(IEstudiantePanelService panelService)
        {
            _panelService = panelService;
        }

        [HttpGet("{idEstudiante:int}")]
        [ProducesResponseType(typeof(EstudiantePanelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EstudiantePanelDto>> ObtenerPanel(
            [FromRoute] int idEstudiante,
            CancellationToken ct = default)
        {
            try
            {
                var panel = await _panelService.ObtenerPanelEstudianteAsync(idEstudiante, ct);

                if (panel == null)
                {
                    return NotFound(new { Error = $"Estudiante con ID {idEstudiante} no encontrado" });
                }

                return Ok(panel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("matricula/{matricula}")]
        [ProducesResponseType(typeof(EstudiantePanelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EstudiantePanelDto>> ObtenerPanelPorMatricula(
            [FromRoute] string matricula,
            CancellationToken ct = default)
        {
            try
            {
                var panel = await _panelService.ObtenerPanelPorMatriculaAsync(matricula, ct);

                if (panel == null)
                {
                    return NotFound(new { Error = $"Estudiante con matrícula {matricula} no encontrado" });
                }

                return Ok(panel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("buscar")]
        [ProducesResponseType(typeof(BuscarEstudiantesPanelResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<BuscarEstudiantesPanelResponse>> BuscarEstudiantes(
            [FromBody] BuscarEstudiantesPanelRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _panelService.BuscarEstudiantesAsync(request, ct);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("estadisticas")]
        [ProducesResponseType(typeof(EstadisticasEstudiantesDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<EstadisticasEstudiantesDto>> ObtenerEstadisticas(
            [FromQuery] int? idPlanEstudios = null,
            [FromQuery] int? idPeriodoAcademico = null,
            CancellationToken ct = default)
        {
            try
            {
                var estadisticas = await _panelService.ObtenerEstadisticasAsync(idPlanEstudios, idPeriodoAcademico, ct);
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idEstudiante:int}/informacion-academica")]
        [ProducesResponseType(typeof(InformacionAcademicaPanelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InformacionAcademicaPanelDto>> ObtenerInformacionAcademica(
            [FromRoute] int idEstudiante,
            CancellationToken ct = default)
        {
            try
            {
                var info = await _panelService.ObtenerInformacionAcademicaAsync(idEstudiante, ct);

                if (info == null)
                {
                    return NotFound(new { Error = "Estudiante no encontrado" });
                }

                return Ok(info);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idEstudiante:int}/resumen-kardex")]
        [ProducesResponseType(typeof(ResumenKardexDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResumenKardexDto>> ObtenerResumenKardex(
            [FromRoute] int idEstudiante,
            CancellationToken ct = default)
        {
            try
            {
                var resumen = await _panelService.ObtenerResumenKardexAsync(idEstudiante, ct);
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idEstudiante:int}/seguimiento-academico")]
        [ProducesResponseType(typeof(SeguimientoAcademicoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SeguimientoAcademicoDto>> ObtenerSeguimientoAcademico(
            [FromRoute] int idEstudiante,
            CancellationToken ct = default)
        {
            try
            {
                var seguimiento = await _panelService.ObtenerSeguimientoAcademicoAsync(idEstudiante, ct);
                return Ok(seguimiento);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPut("{idEstudiante:int}/datos")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR}")]
        [ProducesResponseType(typeof(AccionPanelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AccionPanelResponse>> ActualizarDatos(
            [FromRoute] int idEstudiante,
            [FromBody] ActualizarDatosEstudianteRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _panelService.ActualizarDatosEstudianteAsync(idEstudiante, request, ct);

                if (!resultado.Exitoso)
                {
                    return BadRequest(resultado);
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idEstudiante:int}/becas")]
        [ProducesResponseType(typeof(List<BecaAsignadaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BecaAsignadaDto>>> ObtenerBecas(
            [FromRoute] int idEstudiante,
            [FromQuery] bool? soloActivas = true,
            CancellationToken ct = default)
        {
            try
            {
                var becas = await _panelService.ObtenerBecasEstudianteAsync(idEstudiante, soloActivas, ct);
                return Ok(becas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idEstudiante:int}/resumen-recibos")]
        [ProducesResponseType(typeof(ResumenRecibosDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResumenRecibosDto>> ObtenerResumenRecibos(
            [FromRoute] int idEstudiante,
            CancellationToken ct = default)
        {
            try
            {
                var resumen = await _panelService.ObtenerResumenRecibosAsync(idEstudiante, ct);
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idEstudiante:int}/recibos")]
        [ProducesResponseType(typeof(List<ReciboPanelResumenDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ReciboPanelResumenDto>>> ObtenerRecibos(
            [FromRoute] int idEstudiante,
            [FromQuery] string? estatus = null,
            [FromQuery] int limite = 50,
            CancellationToken ct = default)
        {
            try
            {
                var recibos = await _panelService.ObtenerRecibosEstudianteAsync(idEstudiante, estatus, limite, ct);
                return Ok(recibos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idEstudiante:int}/documentos-personales")]
        [ProducesResponseType(typeof(DocumentosPersonalesEstudianteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentosPersonalesEstudianteDto>> ObtenerDocumentosPersonales(
            [FromRoute] int idEstudiante,
            CancellationToken ct = default)
        {
            try
            {
                var documentos = await _panelService.ObtenerDocumentosPersonalesAsync(idEstudiante, ct);

                if (documentos == null)
                {
                    return NotFound(new { Error = "No se encontraron documentos personales para este estudiante" });
                }

                return Ok(documentos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("{idEstudiante:int}/subir-documento")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.ADMISIONES}")]
        [ProducesResponseType(typeof(AccionPanelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AccionPanelResponse>> SubirDocumentoPersonal(
            [FromRoute] int idEstudiante,
            [FromForm] int idDocumentoRequisito,
            IFormFile archivo,
            [FromForm] string? notas = null,
            CancellationToken ct = default)
        {
            try
            {
                if (archivo == null || archivo.Length == 0)
                    return BadRequest(new AccionPanelResponse { Exitoso = false, Mensaje = "Debe proporcionar un archivo" });

                var resultado = await _panelService.SubirDocumentoPersonalAsync(idEstudiante, idDocumentoRequisito, archivo, notas, ct);

                if (!resultado.Exitoso)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPatch("{idEstudiante:int}/documentos/{idAspiranteDocumento:long}/validar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR}")]
        [ProducesResponseType(typeof(AccionPanelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AccionPanelResponse>> ValidarDocumentoPersonal(
            [FromRoute] int idEstudiante,
            [FromRoute] long idAspiranteDocumento,
            [FromBody] ValidarDocumentoPersonalRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var usuarioId = User.FindFirst("userId")?.Value;
                var resultado = await _panelService.ValidarDocumentoPersonalAsync(
                    idEstudiante, idAspiranteDocumento, request.Aprobar, request.Notas, usuarioId, ct);

                if (!resultado.Exitoso)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idEstudiante:int}/documentos")]
        [ProducesResponseType(typeof(DocumentosDisponiblesDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<DocumentosDisponiblesDto>> ObtenerDocumentosDisponibles(
            [FromRoute] int idEstudiante,
            CancellationToken ct = default)
        {
            try
            {
                var documentos = await _panelService.ObtenerDocumentosDisponiblesAsync(idEstudiante, ct);
                return Ok(documentos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("generar-documento")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        [ProducesResponseType(typeof(AccionPanelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AccionPanelResponse>> GenerarDocumento(
            [FromBody] GenerarDocumentoPanelRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var usuarioId = User.FindFirst("userId")?.Value ?? "sistema";
                var resultado = await _panelService.GenerarDocumentoAsync(request, usuarioId, ct);

                if (!resultado.Exitoso)
                {
                    return BadRequest(resultado);
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idEstudiante:int}/kardex/pdf")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DescargarKardexPdf(
            [FromRoute] int idEstudiante,
            [FromQuery] bool soloPeriodoActual = false,
            CancellationToken ct = default)
        {
            try
            {
                var pdf = await _panelService.GenerarKardexPdfDirectoAsync(idEstudiante, soloPeriodoActual, ct);
                return File(pdf, "application/pdf", $"Kardex_{idEstudiante}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idEstudiante:int}/constancia/pdf")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DescargarConstanciaPdf(
            [FromRoute] int idEstudiante,
            CancellationToken ct = default)
        {
            try
            {
                var pdf = await _panelService.GenerarConstanciaPdfDirectaAsync(idEstudiante, ct);
                return File(pdf, "application/pdf", $"Constancia_{idEstudiante}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("{idEstudiante:int}/enviar-recordatorio")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.FINANZAS},{Rol.CONTROL_ESCOLAR}")]
        [ProducesResponseType(typeof(AccionPanelResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<AccionPanelResponse>> EnviarRecordatorioPago(
            [FromRoute] int idEstudiante,
            [FromQuery] long? idRecibo = null,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _panelService.EnviarRecordatorioPagoAsync(idEstudiante, idRecibo, ct);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPatch("{idEstudiante:int}/estatus")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR}")]
        [ProducesResponseType(typeof(AccionPanelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccionPanelResponse>> ActualizarEstatus(
            [FromRoute] int idEstudiante,
            [FromQuery] bool activo,
            [FromQuery] string? motivo = null,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _panelService.ActualizarEstatusEstudianteAsync(idEstudiante, activo, motivo, ct);

                if (!resultado.Exitoso)
                {
                    return NotFound(resultado);
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("exportar/excel")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportarExcel(
            [FromBody] BuscarEstudiantesPanelRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var excel = await _panelService.ExportarEstudiantesExcelAsync(request, ct);
                return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Estudiantes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idEstudiante:int}/expediente/pdf")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportarExpedientePdf(
            [FromRoute] int idEstudiante,
            CancellationToken ct = default)
        {
            try
            {
                var pdf = await _panelService.ExportarExpedienteEstudianteAsync(idEstudiante, ct);
                return File(pdf, "application/pdf", $"Expediente_{idEstudiante}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
