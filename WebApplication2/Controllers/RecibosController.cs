using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Recibo;
using WebApplication2.Core.Requests.Pagos;
using WebApplication2.Core.Requests.Recibos;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/recibos")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.FINANZAS},{Rol.ADMISIONES}")]
    public class RecibosController : ControllerBase
    {
        private readonly IReciboService _svc;
        private readonly IPdfService _pdfService;

        public RecibosController(IReciboService svc, IPdfService pdfService)
        {
            _svc = svc;
            _pdfService = pdfService;
        }

        [HttpPost("generar")]
        public async Task<ActionResult<IReadOnlyList<ReciboDto>>> Generar([FromBody] GenerarRecibosDto dto, CancellationToken ct)
            => Ok(await _svc.GenerarRecibosAsync(dto, ct));

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ReciboDto>> Obtener(long id, CancellationToken ct)
        {
            var r = await _svc.ObtenerAsync(id, ct);
            return r is null ? NotFound() : Ok(r);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ReciboDto>>> Listar(
            [FromQuery] int idPeriodoAcademico,
            [FromQuery] int? idEstudiante,
            CancellationToken ct)
            => Ok(await _svc.ListarPorPeriodoAsync(idPeriodoAcademico, idEstudiante, ct));

        [HttpGet("{id:long}/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerarPdf(long id, CancellationToken ct)
        {
            try
            {
                var reciboPdf = await _svc.ObtenerParaPdfAsync(id, ct);

                if (reciboPdf == null)
                    return NotFound(new { message = "Recibo no encontrado" });

                var pdfBytes = _pdfService.GenerarReciboPdf(reciboPdf);

                var nombreArchivo = $"Recibo_{reciboPdf.Folio ?? id.ToString()}_{DateTime.Now:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar el PDF del recibo", error = ex.Message });
            }
        }

        [HttpGet("admin")]
        public async Task<ActionResult<ReciboBusquedaResultadoDto>> ListarAdmin(
            [FromQuery] string? matricula,
            [FromQuery] string? folio,
            [FromQuery] int? idPeriodoAcademico,
            [FromQuery] string? estatus,
            [FromQuery] bool soloVencidos = false,
            [FromQuery] bool soloPagados = false,
            [FromQuery] bool soloPendientes = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 50,
            CancellationToken ct = default)
        {
            var filtros = new ReciboBusquedaFiltrosDto
            {
                Folio = folio,
                Matricula = matricula,
                IdPeriodoAcademico = idPeriodoAcademico,
                SoloVencidos = soloVencidos,
                SoloPagados = soloPagados,
                SoloPendientes = soloPendientes,
                Pagina = pagina,
                TamanioPagina = tamanioPagina
            };

            if (!string.IsNullOrEmpty(estatus) && Enum.TryParse<Core.Enums.EstatusRecibo>(estatus, true, out var estatusEnum))
            {
                filtros.Estatus = estatusEnum;
            }

            return Ok(await _svc.BuscarRecibosAsync(filtros, ct));
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<ReciboBusquedaResultadoDto>> BuscarRecibos(
            [FromQuery] string? folio,
            [FromQuery] string? matricula,
            [FromQuery] int? idPeriodoAcademico,
            [FromQuery] string? estatus,
            [FromQuery] bool soloVencidos = false,
            [FromQuery] bool soloPagados = false,
            [FromQuery] bool soloPendientes = false,
            [FromQuery] DateOnly? fechaEmisionDesde = null,
            [FromQuery] DateOnly? fechaEmisionHasta = null,
            [FromQuery] DateOnly? fechaVencimientoDesde = null,
            [FromQuery] DateOnly? fechaVencimientoHasta = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 50,
            CancellationToken ct = default)
        {
            var filtros = new ReciboBusquedaFiltrosDto
            {
                Folio = folio,
                Matricula = matricula,
                IdPeriodoAcademico = idPeriodoAcademico,
                SoloVencidos = soloVencidos,
                SoloPagados = soloPagados,
                SoloPendientes = soloPendientes,
                FechaEmisionDesde = fechaEmisionDesde,
                FechaEmisionHasta = fechaEmisionHasta,
                FechaVencimientoDesde = fechaVencimientoDesde,
                FechaVencimientoHasta = fechaVencimientoHasta,
                Pagina = pagina,
                TamanioPagina = tamanioPagina
            };

            if (!string.IsNullOrEmpty(estatus) && Enum.TryParse<Core.Enums.EstatusRecibo>(estatus, true, out var estatusEnum))
            {
                filtros.Estatus = estatusEnum;
            }

            return Ok(await _svc.BuscarRecibosAsync(filtros, ct));
        }

        [HttpGet("estadisticas")]
        public async Task<ActionResult<ReciboEstadisticasDto>> ObtenerEstadisticas(
            [FromQuery] int? idPeriodoAcademico,
            CancellationToken ct)
        {
            return Ok(await _svc.ObtenerEstadisticasAsync(idPeriodoAcademico, ct));
        }

        [HttpGet("por-matricula/{matricula}")]
        public async Task<ActionResult<ReciboBusquedaResultadoDto>> BuscarPorMatricula(string matricula, CancellationToken ct)
        {
            return Ok(await _svc.BuscarPorMatriculaAsync(matricula, ct));
        }

        [HttpGet("aspirante/{idAspirante:int}")]
        public async Task<ActionResult<IReadOnlyList<ReciboDto>>> ListarPorAspirante(int idAspirante, CancellationToken ct)
        {
            return Ok(await _svc.ListarPorAspiranteAsync(idAspirante, ct));
        }

        [HttpGet("folio/{folio}")]
        public async Task<ActionResult<ReciboDto>> BuscarPorFolio(string folio, CancellationToken ct)
        {
            var recibo = await _svc.BuscarPorFolioAsync(folio, ct);
            return recibo == null ? NotFound() : Ok(recibo);
        }

        [HttpGet("reportes/cartera-vencida")]
        public async Task<ActionResult<CarteraVencidaReporteDto>> ObtenerCarteraVencida(
            [FromQuery] int? idPeriodoAcademico,
            [FromQuery] int? diasVencidoMinimo,
            CancellationToken ct)
        {
            return Ok(await _svc.ObtenerCarteraVencidaAsync(idPeriodoAcademico, diasVencidoMinimo, ct));
        }

        [HttpGet("reportes/cartera-vencida/excel")]
        public async Task<IActionResult> ExportarCarteraVencidaExcel(
            [FromQuery] int? idPeriodoAcademico,
            [FromQuery] int? diasVencidoMinimo,
            CancellationToken ct)
        {
            try
            {
                var excelBytes = await _svc.ExportarCarteraVencidaExcelAsync(idPeriodoAcademico, diasVencidoMinimo, ct);
                var nombreArchivo = $"CarteraVencida_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al exportar cartera vencida", error = ex.Message });
            }
        }

        [HttpGet("reportes/ingresos")]
        public async Task<ActionResult<IngresosPeriodoReporteDto>> ObtenerIngresos(
            [FromQuery] int idPeriodoAcademico,
            [FromQuery] DateOnly? fechaInicio,
            [FromQuery] DateOnly? fechaFin,
            CancellationToken ct)
        {
            try
            {
                return Ok(await _svc.ObtenerIngresosAsync(idPeriodoAcademico, fechaInicio, fechaFin, ct));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("reportes/ingresos/{idPeriodoAcademico:int}/excel")]
        public async Task<IActionResult> ExportarIngresosExcel(
            int idPeriodoAcademico,
            [FromQuery] DateOnly? fechaInicio,
            [FromQuery] DateOnly? fechaFin,
            CancellationToken ct)
        {
            try
            {
                var excelBytes = await _svc.ExportarIngresosExcelAsync(idPeriodoAcademico, fechaInicio, fechaFin, ct);
                var nombreArchivo = $"Ingresos_Periodo_{idPeriodoAcademico}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al exportar ingresos", error = ex.Message });
            }
        }

        [HttpGet("exportar-excel")]
        public async Task<IActionResult> ExportarExcel(
            [FromQuery] string? folio,
            [FromQuery] string? matricula,
            [FromQuery] int? idPeriodoAcademico,
            [FromQuery] string? estatus,
            [FromQuery] bool soloVencidos = false,
            [FromQuery] bool soloPagados = false,
            [FromQuery] bool soloPendientes = false,
            CancellationToken ct = default)
        {
            try
            {
                var filtros = new ReciboBusquedaFiltrosDto
                {
                    Folio = folio,
                    Matricula = matricula,
                    IdPeriodoAcademico = idPeriodoAcademico,
                    SoloVencidos = soloVencidos,
                    SoloPagados = soloPagados,
                    SoloPendientes = soloPendientes,
                    Pagina = 1,
                    TamanioPagina = 10000
                };

                if (!string.IsNullOrEmpty(estatus) && Enum.TryParse<Core.Enums.EstatusRecibo>(estatus, true, out var estatusEnum))
                {
                    filtros.Estatus = estatusEnum;
                }

                var excelBytes = await _svc.ExportarExcelAsync(filtros, ct);

                var fechaActual = DateTime.Now.ToString("yyyyMMdd_HHmm");
                var nombreArchivo = $"ReporteRecibos_{fechaActual}.xlsx";

                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    nombreArchivo
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar el archivo Excel", error = ex.Message });
            }
        }

        [HttpPut("{id:long}/cancelar")]
        public async Task<ActionResult<ReciboDto>> CancelarRecibo(
            long id,
            [FromBody] CancelarReciboRequest request,
            CancellationToken ct)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";
                var resultado = await _svc.CancelarReciboAsync(id, usuario, request.Motivo, ct);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cancelar el recibo", error = ex.Message });
            }
        }

        [HttpPut("{id:long}/reversar")]
        public async Task<ActionResult<ReciboDto>> ReversarRecibo(
            long id,
            [FromBody] ReversarReciboRequest request,
            CancellationToken ct)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";
                var resultado = await _svc.ReversarReciboAsync(id, usuario, request.Motivo, ct);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al reversar el recibo", error = ex.Message });
            }
        }
    }
}
