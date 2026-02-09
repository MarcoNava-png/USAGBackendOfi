using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.DTOs.MultiTenant;
using WebApplication2.Core.Models.MultiTenant;
using WebApplication2.Core.Requests.MultiTenant;
using WebApplication2.Core.Responses.MultiTenant;
using WebApplication2.Services.Interfaces;
using WebApplication2.Services.MultiTenant;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/admin/tenants")]
public class TenantAdminController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly IBlobStorageService _storageService;
    private readonly ILogger<TenantAdminController> _logger;

    public TenantAdminController(
        ITenantService tenantService,
        IBlobStorageService storageService,
        ILogger<TenantAdminController> logger)
    {
        _tenantService = tenantService;
        _storageService = storageService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardGlobalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardGlobalDto>> GetDashboard(CancellationToken ct)
    {
        var dashboard = await _tenantService.ObtenerDashboardGlobalAsync(ct);
        return Ok(dashboard);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TenantListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TenantListDto>>> GetTenants(CancellationToken ct)
    {
        var tenants = await _tenantService.ListarTenantsAsync(ct);
        return Ok(tenants);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TenantDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDetalleDto>> GetTenant(int id, CancellationToken ct)
    {
        var tenant = await _tenantService.ObtenerTenantDetalleAsync(id, ct);
        if (tenant == null)
        {
            return NotFound(new { error = "Escuela no encontrada" });
        }
        return Ok(tenant);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TenantCreadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TenantCreadoResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TenantCreadoResponse>> CreateTenant(
        [FromBody] CrearTenantRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Solicitud de creación de tenant: {Codigo}", request.Codigo);

        if (string.IsNullOrWhiteSpace(request.Codigo))
        {
            return BadRequest(new TenantCreadoResponse
            {
                Exitoso = false,
                Mensaje = "El código es requerido"
            });
        }

        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new TenantCreadoResponse
            {
                Exitoso = false,
                Mensaje = "El nombre es requerido"
            });
        }

        if (string.IsNullOrWhiteSpace(request.Subdominio))
        {
            return BadRequest(new TenantCreadoResponse
            {
                Exitoso = false,
                Mensaje = "El subdominio es requerido"
            });
        }

        if (string.IsNullOrWhiteSpace(request.AdminEmail))
        {
            return BadRequest(new TenantCreadoResponse
            {
                Exitoso = false,
                Mensaje = "El email del administrador es requerido"
            });
        }

        request.Codigo = request.Codigo.ToUpper().Trim();
        request.Subdominio = request.Subdominio.ToLower().Trim().Replace(" ", "-");

        if (!System.Text.RegularExpressions.Regex.IsMatch(request.Subdominio, @"^[a-z0-9\-]+$"))
        {
            return BadRequest(new TenantCreadoResponse
            {
                Exitoso = false,
                Mensaje = "El subdominio solo puede contener letras minúsculas, números y guiones"
            });
        }

        var creadoPor = User.Identity?.Name ?? "system";
        var result = await _tenantService.CrearTenantAsync(request, creadoPor, ct);

        if (!result.Exitoso)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Tenant creado exitosamente: {Codigo} - {Url}", result.Codigo, result.Url);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateTenant(
        int id,
        [FromBody] ActualizarTenantRequest request,
        CancellationToken ct)
    {
        var result = await _tenantService.ActualizarTenantAsync(id, request, ct);
        if (!result)
        {
            return NotFound(new { error = "Escuela no encontrada" });
        }

        return Ok(new { mensaje = "Escuela actualizada exitosamente" });
    }

    [HttpPost("{id:int}/logo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UploadLogo(
        int id,
        IFormFile logo,
        CancellationToken ct)
    {
        if (logo == null || logo.Length == 0)
        {
            return BadRequest(new { error = "No se proporcionó ningún archivo" });
        }

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(logo.ContentType.ToLower()))
        {
            return BadRequest(new { error = "El archivo debe ser una imagen (JPEG, PNG, GIF o WEBP)" });
        }

        if (logo.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { error = "El archivo no debe superar 5MB" });
        }

        var tenant = await _tenantService.GetTenantByIdAsync(id, ct);
        if (tenant == null)
        {
            return NotFound(new { error = "Escuela no encontrada" });
        }

        try
        {
            var extension = Path.GetExtension(logo.FileName).ToLower();
            var blobName = $"tenant-logos/{tenant.Codigo.ToLower()}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";

            var logoUrl = await _storageService.UploadFile(logo, blobName, "logos");

            await _tenantService.ActualizarTenantAsync(id, new ActualizarTenantRequest
            {
                LogoUrl = logoUrl
            }, ct);

            _logger.LogInformation("Logo actualizado para tenant {Id}: {LogoUrl}", id, logoUrl);

            return Ok(new { mensaje = "Logo actualizado exitosamente", logoUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir logo para tenant {Id}", id);
            return BadRequest(new { error = $"Error al subir el logo: {ex.Message}" });
        }
    }

    [HttpDelete("{id:int}/logo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteLogo(int id, CancellationToken ct)
    {
        var tenant = await _tenantService.GetTenantByIdAsync(id, ct);
        if (tenant == null)
        {
            return NotFound(new { error = "Escuela no encontrada" });
        }

        await _tenantService.ActualizarTenantAsync(id, new ActualizarTenantRequest
        {
            LogoUrl = null
        }, ct);

        return Ok(new { mensaje = "Logo eliminado exitosamente" });
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ChangeStatus(
        int id,
        [FromQuery] TenantStatus status,
        [FromQuery] string? motivo,
        CancellationToken ct)
    {
        var result = await _tenantService.CambiarStatusTenantAsync(id, status, motivo, ct);
        if (!result)
        {
            return NotFound(new { error = "Escuela no encontrada" });
        }

        _logger.LogInformation("Status de tenant {Id} cambiado a {Status}", id, status);
        return Ok(new { mensaje = $"Status actualizado a {status}" });
    }

    [HttpGet("{id:int}/stats")]
    [ProducesResponseType(typeof(TenantStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TenantStatsDto>> GetTenantStats(int id, CancellationToken ct)
    {
        var stats = await _tenantService.ObtenerEstadisticasTenantAsync(id, ct);
        return Ok(stats);
    }

    [HttpGet("planes")]
    [ProducesResponseType(typeof(List<PlanLicenciaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PlanLicenciaDto>>> GetPlanes(CancellationToken ct)
    {
        var planes = await _tenantService.ListarPlanesAsync(ct);
        return Ok(planes);
    }

    [HttpGet("current")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TenantPublicInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantPublicInfoDto>> GetCurrentTenantInfo(CancellationToken ct)
    {
        var tenantContext = _tenantService.GetCurrentTenant();
        if (tenantContext == null)
        {
            return NotFound(new { error = "No hay tenant en el contexto" });
        }

        var tenant = await _tenantService.GetTenantByIdAsync(tenantContext.IdTenant, ct);
        if (tenant == null)
        {
            return NotFound(new { error = "Tenant no encontrado" });
        }

        return Ok(new TenantPublicInfoDto
        {
            Codigo = tenant.Codigo,
            Nombre = tenant.Nombre,
            NombreCorto = tenant.NombreCorto,
            LogoUrl = tenant.LogoUrl,
            ColorPrimario = tenant.ColorPrimario,
            ColorSecundario = tenant.ColorSecundario
        });
    }

    [HttpGet("importar/plantilla")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public ActionResult DownloadTemplate()
    {
        var bytes = _tenantService.GenerarPlantillaExcel();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "plantilla_escuelas.xlsx");
    }

    [HttpPost("importar")]
    [ProducesResponseType(typeof(ImportarTenantsResultado), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportarTenantsResultado>> ImportFromExcel(
        IFormFile archivo,
        CancellationToken ct)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest(new { error = "No se proporcionó ningún archivo" });
        }

        if (!archivo.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "El archivo debe ser un Excel (.xlsx)" });
        }

        try
        {
            var filas = new List<ImportarTenantFila>();

            using var stream = archivo.OpenReadStream();
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RowsUsed().Skip(1);

            int rowNum = 2;
            foreach (var row in rows)
            {
                var codigo = row.Cell(1).GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(codigo)) continue;

                filas.Add(new ImportarTenantFila
                {
                    Fila = rowNum,
                    Codigo = codigo,
                    Nombre = row.Cell(2).GetString()?.Trim() ?? "",
                    NombreCorto = row.Cell(3).GetString()?.Trim() ?? "",
                    Subdominio = row.Cell(4).GetString()?.Trim()?.ToLower() ?? "",
                    ColorPrimario = row.Cell(5).GetString()?.Trim(),
                    EmailContacto = row.Cell(6).GetString()?.Trim(),
                    TelefonoContacto = row.Cell(7).GetString()?.Trim(),
                    IdPlanLicencia = (int)(row.Cell(8).GetDouble()),
                    AdminEmail = row.Cell(9).GetString()?.Trim() ?? "",
                    AdminNombre = row.Cell(10).GetString()?.Trim() ?? ""
                });

                rowNum++;
            }

            if (filas.Count == 0)
            {
                return BadRequest(new { error = "El archivo no contiene datos válidos" });
            }

            var creadoPor = User.Identity?.Name ?? "system";
            var resultado = await _tenantService.ImportarTenantsAsync(filas, creadoPor, ct);

            _logger.LogInformation("Importación completada: {Exitosos} exitosos, {Fallidos} fallidos de {Total} total",
                resultado.Exitosos, resultado.Fallidos, resultado.TotalFilas);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar el archivo de importación");
            return BadRequest(new { error = $"Error al procesar el archivo: {ex.Message}" });
        }
    }

    [HttpPost("importar/exportar-resultados")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public ActionResult ExportResults([FromBody] ImportarTenantsResultado resultados)
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Resultados");

        var headers = new[] { "Fila", "Código", "Nombre", "Exitoso", "Mensaje", "URL", "Email Admin", "Contraseña" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
        }

        int row = 2;
        foreach (var r in resultados.Resultados)
        {
            worksheet.Cell(row, 1).Value = r.Fila;
            worksheet.Cell(row, 2).Value = r.Codigo;
            worksheet.Cell(row, 3).Value = r.Nombre;
            worksheet.Cell(row, 4).Value = r.Exitoso ? "Sí" : "No";
            worksheet.Cell(row, 5).Value = r.Mensaje;
            worksheet.Cell(row, 6).Value = r.Url ?? "";
            worksheet.Cell(row, 7).Value = r.AdminEmail ?? "";
            worksheet.Cell(row, 8).Value = r.PasswordTemporal ?? "";

            if (!r.Exitoso)
            {
                worksheet.Row(row).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightCoral;
            }
            else
            {
                worksheet.Row(row).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;
            }

            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "resultados_importacion.xlsx");
    }
}
