using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.TarifaAdmision;
using WebApplication2.Services.Interfaces;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS},{Rol.ADMISIONES}")]
    public class TarifaAdmisionController : ControllerBase
    {
        private readonly ITarifaAdmisionService _service;
        private readonly IPdfService _pdfService;

        public TarifaAdmisionController(ITarifaAdmisionService service, IPdfService pdfService)
        {
            _service = service;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] bool? soloActivas = null, CancellationToken ct = default)
        {
            var result = await _service.ListarTarifasAsync(soloActivas, ct);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken ct = default)
        {
            var result = await _service.ObtenerPorIdAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("plan/{idPlanEstudios:int}")]
        public async Task<IActionResult> ObtenerPorPlan(int idPlanEstudios, CancellationToken ct = default)
        {
            var result = await _service.ObtenerPorPlanAsync(idPlanEstudios, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS}")]
        public async Task<IActionResult> Crear([FromBody] CrearTarifaAdmisionDto dto, CancellationToken ct = default)
        {
            var usuarioId = User.FindFirst("userId")?.Value ?? User.Identity?.Name ?? "";
            try
            {
                var result = await _service.CrearTarifaAsync(dto, usuarioId, ct);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = result.IdTarifaAdmision }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarTarifaAdmisionDto dto, CancellationToken ct = default)
        {
            var usuarioId = User.FindFirst("userId")?.Value ?? User.Identity?.Name ?? "";
            try
            {
                var result = await _service.ActualizarTarifaAsync(id, dto, usuarioId, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct = default)
        {
            var eliminado = await _service.EliminarTarifaAsync(id, ct);
            if (!eliminado) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS}")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoTarifaDto dto, CancellationToken ct = default)
        {
            var cambiado = await _service.CambiarEstadoAsync(id, dto.Activo, ct);
            if (!cambiado) return NotFound();
            return NoContent();
        }

        [HttpGet("{id:int}/aspirante/{idAspirante:int}/cotizacion-pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> CotizacionPdf(int id, int idAspirante, CancellationToken ct = default)
        {
            try
            {
                var dto = await _service.GenerarCotizacionPdfDtoAsync(id, idAspirante, ct);
                var pdfBytes = _pdfService.GenerarCotizacionAdmisionPdf(dto);
                var nombreArchivo = $"CotizacionAdmision_{dto.ClavePlan}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar cotización PDF", error = ex.Message });
            }
        }

        [HttpPost("{id:int}/generar-recibos/{idAspirante:int}")]
        public async Task<IActionResult> GenerarRecibos(int id, int idAspirante, [FromBody] GenerarRecibosAdmisionRequestDto dto, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GenerarRecibosAsync(idAspirante, id, dto.PagoCompleto, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class CambiarEstadoTarifaDto
    {
        public bool Activo { get; set; }
    }

    public class GenerarRecibosAdmisionRequestDto
    {
        public bool PagoCompleto { get; set; } = false;
    }
}
