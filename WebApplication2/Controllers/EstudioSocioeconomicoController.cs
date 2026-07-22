using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebApplication2.Core.DTOs.EstudioSocioeconomico;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/estudio-socioeconomico")]
    [Authorize]
    public class EstudioSocioeconomicoController : ControllerBase
    {
        private readonly IEstudioSocioeconomicoService _service;
        private readonly IConfiguration _configuration;

        public EstudioSocioeconomicoController(IEstudioSocioeconomicoService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        private string? UserId => User.FindFirst("userId")?.Value;

        [HttpGet("catalogos")]
        public async Task<IActionResult> Catalogos()
        {
            return Ok(await _service.GetCatalogosAsync());
        }

        [HttpGet("analistas")]
        public async Task<IActionResult> Analistas()
        {
            return Ok(await _service.GetAnalistasAsync());
        }

        [HttpGet("aspirante/{idAspirante:int}")]
        public async Task<IActionResult> PorAspirante(int idAspirante)
        {
            var dto = await _service.GetByAspiranteAsync(idAspirante);
            return Ok(dto);
        }

        [HttpPut("aspirante/{idAspirante:int}")]
        public async Task<IActionResult> Guardar(int idAspirante, [FromBody] EstudioSocioeconomicoRequest req)
        {
            try
            {
                var analistaId = string.IsNullOrWhiteSpace(req.AnalistaId) ? UserId : req.AnalistaId;
                var dto = await _service.UpsertAsync(idAspirante, req, analistaId, false);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("aspirante/{idAspirante:int}/token")]
        public async Task<IActionResult> GenerarLink(int idAspirante)
        {
            try
            {
                var token = await _service.GenerarTokenAsync(idAspirante);
                var baseUrl = _configuration["FrontendUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl)) baseUrl = "https://saciusag.com.mx";
                var url = $"{baseUrl.TrimEnd('/')}/estudio-socioeconomico/{token}";
                return Ok(new { token, url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("publico/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> Publico(string token)
        {
            var dto = await _service.GetByTokenAsync(token);
            if (dto == null) return NotFound(new { message = "Enlace no válido o expirado." });
            return Ok(dto);
        }

        [HttpPut("publico/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> GuardarPublico(string token, [FromBody] EstudioSocioeconomicoRequest req)
        {
            var dto = await _service.UpsertByTokenAsync(token, req);
            if (dto == null) return NotFound(new { message = "Enlace no válido o expirado." });
            return Ok(dto);
        }
    }
}
