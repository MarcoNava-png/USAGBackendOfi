using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/onlyoffice")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.COORDINADOR},{Rol.ACADEMICO}")]
    public class OnlyOfficeController : ControllerBase
    {
        private readonly IPlantillaReporteService _plantillaService;
        private readonly IConfiguration _config;
        private readonly ILogger<OnlyOfficeController> _logger;
        private static readonly HttpClient _httpClient = new();

        public OnlyOfficeController(IPlantillaReporteService plantillaService, IConfiguration config, ILogger<OnlyOfficeController> logger)
        {
            _plantillaService = plantillaService;
            _config = config;
            _logger = logger;
        }

        [HttpGet("document/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetDocument(int id, [FromQuery] string? token, CancellationToken ct)
        {
            var secret = _config["OnlyOffice:Secret"];
            if (string.IsNullOrEmpty(secret) || token != secret)
                return Unauthorized();

            var plantillas = await _plantillaService.ListarAsync(null, ct);
            var plantilla = plantillas.FirstOrDefault(p => p.Id == id);
            if (plantilla == null) return NotFound();

            if (!System.IO.File.Exists(plantilla.RutaArchivo))
                return NotFound(new { error = "Archivo no encontrado" });

            var bytes = await System.IO.File.ReadAllBytesAsync(plantilla.RutaArchivo, ct);
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", plantilla.NombreArchivoOriginal);
        }

        [HttpPost("callback/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> Callback(int id, [FromQuery] string? token, CancellationToken ct)
        {
            var secret = _config["OnlyOffice:Secret"];
            if (string.IsNullOrEmpty(secret) || token != secret)
                return Unauthorized();

            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync(ct);
                var json = JsonDocument.Parse(body);
                var status = json.RootElement.GetProperty("status").GetInt32();

                if (status == 2 || status == 6)
                {
                    var url = json.RootElement.GetProperty("url").GetString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        var docBytes = await _httpClient.GetByteArrayAsync(url, ct);

                        var plantillas = await _plantillaService.ListarAsync(null, ct);
                        var plantilla = plantillas.FirstOrDefault(p => p.Id == id);
                        if (plantilla != null && System.IO.File.Exists(plantilla.RutaArchivo))
                        {
                            await System.IO.File.WriteAllBytesAsync(plantilla.RutaArchivo, docBytes, ct);
                        }
                    }
                }

                return Ok(new { error = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnlyOffice callback error para documento {DocumentId}", id);
                return Ok(new { error = 0 });
            }
        }

        [HttpGet("config/{id:int}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.COORDINADOR},{Rol.ACADEMICO}")]
        public async Task<ActionResult> GetEditorConfig(int id, CancellationToken ct)
        {
            var plantillas = await _plantillaService.ListarAsync(null, ct);
            var plantilla = plantillas.FirstOrDefault(p => p.Id == id);
            if (plantilla == null) return NotFound();

            var internalUrl = "http://usag-backend:8080";
            var onlyOfficeSecret = _config["OnlyOffice:Secret"] ?? "";

            var fileExt = Path.GetExtension(plantilla.RutaArchivo).TrimStart('.').ToLower();
            var docType = fileExt == "xlsx" || fileExt == "xls" ? "cell" : "word";

            var config = new
            {
                document = new
                {
                    fileType = fileExt,
                    key = $"{id}_{plantilla.UpdatedAt?.Ticks ?? plantilla.CreatedAt.Ticks}_{DateTime.UtcNow.Ticks}",
                    title = plantilla.NombreArchivoOriginal,
                    url = $"{internalUrl}/api/onlyoffice/document/{id}?token={onlyOfficeSecret}"
                },
                editorConfig = new
                {
                    callbackUrl = $"{internalUrl}/api/onlyoffice/callback/{id}?token={onlyOfficeSecret}",
                    lang = "es",
                    mode = "edit",
                    customization = new
                    {
                        autosave = true,
                        forcesave = true,
                        compactHeader = true
                    }
                },
                documentType = docType
            };

            return Ok(config);
        }
    }
}
