using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.DTOs.MultiTenant;
using WebApplication2.Services.MultiTenant;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/admin/reportes")]
[Authorize(Roles = "SuperAdmin")]
public class ReportesGlobalesController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<ReportesGlobalesController> _logger;

    public ReportesGlobalesController(
        ITenantService tenantService,
        ILogger<ReportesGlobalesController> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    [HttpGet("ingresos")]
    [ProducesResponseType(typeof(ReporteIngresosGlobalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReporteIngresosGlobalDto>> GetReporteIngresos(
        [FromQuery] int? anio,
        CancellationToken ct)
    {
        try
        {
            var reporte = await _tenantService.ObtenerReporteIngresosAsync(anio, ct);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte de ingresos");
            return StatusCode(500, new { error = "Error al generar el reporte de ingresos" });
        }
    }

    [HttpGet("estudiantes")]
    [ProducesResponseType(typeof(ReporteEstudiantesGlobalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReporteEstudiantesGlobalDto>> GetReporteEstudiantes(CancellationToken ct)
    {
        try
        {
            var reporte = await _tenantService.ObtenerReporteEstudiantesAsync(ct);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte de estudiantes");
            return StatusCode(500, new { error = "Error al generar el reporte de estudiantes" });
        }
    }

    [HttpGet("uso")]
    [ProducesResponseType(typeof(ReporteUsoSistemaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReporteUsoSistemaDto>> GetReporteUso(CancellationToken ct)
    {
        try
        {
            var reporte = await _tenantService.ObtenerReporteUsoSistemaAsync(ct);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte de uso del sistema");
            return StatusCode(500, new { error = "Error al generar el reporte de uso" });
        }
    }

    [HttpGet("licencias")]
    [ProducesResponseType(typeof(ReporteLicenciasDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReporteLicenciasDto>> GetReporteLicencias(CancellationToken ct)
    {
        try
        {
            var reporte = await _tenantService.ObtenerReporteLicenciasAsync(ct);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte de licencias");
            return StatusCode(500, new { error = "Error al generar el reporte de licencias" });
        }
    }

    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ResumenEjecutivoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenEjecutivoDto>> GetResumenEjecutivo(CancellationToken ct)
    {
        try
        {
            var ingresos = await _tenantService.ObtenerReporteIngresosAsync(null, ct);
            var estudiantes = await _tenantService.ObtenerReporteEstudiantesAsync(ct);
            var licencias = await _tenantService.ObtenerReporteLicenciasAsync(ct);

            return Ok(new ResumenEjecutivoDto
            {
                TotalEscuelas = licencias.TotalTenants,
                EscuelasActivas = licencias.TenantsActivos,
                TotalEstudiantes = estudiantes.TotalEstudiantes,
                EstudiantesActivos = estudiantes.EstudiantesActivos,
                IngresosMesActual = ingresos.IngresosTotalMes,
                IngresosAnioActual = ingresos.IngresosTotalAnio,
                AdeudoTotal = ingresos.AdeudoTotalGlobal,
                IngresosRecurrentes = licencias.IngresosRecurrentesMensual,
                LicenciasPorVencer = licencias.TenantsPorVencer,
                TopEscuelasIngresos = ingresos.IngresosPorTenant
                    .OrderByDescending(t => t.IngresosMes)
                    .Take(5)
                    .ToList(),
                TopEscuelasEstudiantes = estudiantes.EstudiantesPorTenant
                    .OrderByDescending(t => t.TotalEstudiantes)
                    .Take(5)
                    .ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen ejecutivo");
            return StatusCode(500, new { error = "Error al generar el resumen ejecutivo" });
        }
    }
}
