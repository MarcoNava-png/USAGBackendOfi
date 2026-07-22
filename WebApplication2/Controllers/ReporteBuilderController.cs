using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.ReporteBuilder;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/reportes-builder")]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.ACADEMICO},{Rol.FINANZAS}")]
    public class ReporteBuilderController : ControllerBase
    {
        private readonly IReporteBuilderService _reporteBuilderService;

        public ReporteBuilderController(IReporteBuilderService reporteBuilderService)
        {
            _reporteBuilderService = reporteBuilderService;
        }

        [HttpGet("fuentes")]
        public ActionResult<List<ReporteFuenteDto>> Fuentes()
        {
            return Ok(_reporteBuilderService.ObtenerFuentes());
        }

        [HttpPost("ejecutar")]
        public async Task<ActionResult<ReporteResultadoDto>> Ejecutar([FromBody] EjecutarReporteRequest request, CancellationToken ct)
        {
            try
            {
                var resultado = await _reporteBuilderService.EjecutarAsync(request, ct);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("exportar-excel")]
        public async Task<IActionResult> ExportarExcel([FromBody] EjecutarReporteRequest request, CancellationToken ct)
        {
            try
            {
                var bytes = await _reporteBuilderService.ExportarExcelAsync(request, ct);
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"reporte_{request.Fuente}.xlsx");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("exportar-pdf")]
        public async Task<IActionResult> ExportarPdf([FromBody] EjecutarReporteRequest request, CancellationToken ct)
        {
            try
            {
                var bytes = await _reporteBuilderService.ExportarPdfAsync(request, ct);
                return File(bytes, "application/pdf", $"reporte_{request.Fuente}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("definiciones")]
        public async Task<ActionResult<List<ReporteDefinicionDto>>> ListarDefiniciones(CancellationToken ct)
        {
            return Ok(await _reporteBuilderService.ListarDefinicionesAsync(ct));
        }

        [HttpPost("definiciones")]
        public async Task<ActionResult<ReporteDefinicionDto>> GuardarDefinicion([FromBody] ReporteDefinicionDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { message = "El nombre del reporte es requerido" });
            try
            {
                return Ok(await _reporteBuilderService.GuardarDefinicionAsync(dto, ct));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("definiciones/{id:int}")]
        public async Task<IActionResult> EliminarDefinicion(int id, CancellationToken ct)
        {
            var ok = await _reporteBuilderService.EliminarDefinicionAsync(id, ct);
            return ok ? Ok(new { message = "Reporte eliminado" }) : NotFound();
        }
    }
}
