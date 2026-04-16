using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.Titulacion;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/titulacion/configuracion")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR}")]
    public class ConfiguracionTitulacionController : ControllerBase
    {
        private readonly IConfiguracionTitulacionService _service;

        public ConfiguracionTitulacionController(IConfiguracionTitulacionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<ConfiguracionIPESDto>>> GetConfiguraciones(CancellationToken ct)
        {
            var result = await _service.GetConfiguracionesAsync(ct);
            return Ok(result);
        }

        [HttpGet("campus/{idCampus:int}")]
        public async Task<ActionResult<ConfiguracionIPESDto>> GetPorCampus(int idCampus, CancellationToken ct)
        {
            var result = await _service.GetConfiguracionPorCampusAsync(idCampus, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("ipes")]
        public async Task<ActionResult<ConfiguracionIPESDto>> GuardarIPES([FromBody] GuardarConfiguracionIPESRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _service.GuardarConfiguracionAsync(request, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("responsable")]
        public async Task<ActionResult<ResponsableFirmaDto>> GuardarResponsable([FromBody] GuardarResponsableFirmaRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _service.GuardarResponsableAsync(request, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("credencial")]
        public async Task<ActionResult<CredencialSEPDto>> GuardarCredencial([FromBody] GuardarCredencialSEPRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _service.GuardarCredencialAsync(request, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("responsable/{idResponsable:int}/certificado-cer")]
        public async Task<ActionResult> SubirCertificadoCer(int idResponsable, IFormFile archivo, CancellationToken ct)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { error = "Archivo requerido" });

            if (!archivo.FileName.EndsWith(".cer", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "El archivo debe ser .cer" });

            try
            {
                await _service.SubirCertificadoCerAsync(idResponsable, archivo.OpenReadStream(), archivo.FileName, ct);
                return Ok(new { mensaje = "Certificado .cer subido correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("responsable/{idResponsable:int}/llave-key")]
        public async Task<ActionResult> SubirLlaveKey(int idResponsable, IFormFile archivo, CancellationToken ct)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { error = "Archivo requerido" });

            if (!archivo.FileName.EndsWith(".key", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "El archivo debe ser .key" });

            try
            {
                await _service.SubirLlaveKeyAsync(idResponsable, archivo.OpenReadStream(), archivo.FileName, ct);
                return Ok(new { mensaje = "Llave .key subida correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
