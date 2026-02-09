using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS}")]
    public sealed class ConceptosController : ControllerBase
    {
        private readonly IConceptoService _svc;

        public ConceptosController(IConceptoService svc)
        {
            _svc = svc;
        }

        [HttpPost]
        public async Task<ActionResult<ConceptoDto>> CrearConcepto(
            [FromBody] CrearConceptoDto dto,
            CancellationToken ct)
        {
            try
            {
                var creado = await _svc.CrearConceptoAsync(dto, ct);
                return CreatedAtAction(nameof(ListarConceptos), new { id = creado.IdConceptoPago }, creado);
            }
            catch (InvalidOperationException vex)
            {
                return Conflict(vex.Message);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ConceptoDto>>> ListarConceptos(
            [FromQuery] bool? soloActivos,
            [FromQuery] int? tipo,
            [FromQuery] string? busqueda,
            CancellationToken ct)
        {
            var list = await _svc.ListarAsync(soloActivos, tipo, busqueda, ct);
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ConceptoDto>> ObtenerPorId(int id, CancellationToken ct)
        {
            var concepto = await _svc.ObtenerPorIdAsync(id, ct);
            if (concepto == null)
                return NotFound(new { message = "Concepto no encontrado" });

            return Ok(concepto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ConceptoDto>> Actualizar(
            int id,
            [FromBody] ActualizarConceptoDto dto,
            CancellationToken ct)
        {
            try
            {
                var actualizado = await _svc.ActualizarAsync(id, dto, ct);
                return Ok(actualizado);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            [FromBody] CambiarEstadoDto dto,
            CancellationToken ct)
        {
            try
            {
                await _svc.CambiarEstadoAsync(id, dto.Activo, ct);
                return Ok(new { message = dto.Activo ? "Concepto activado" : "Concepto desactivado" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
        {
            try
            {
                await _svc.EliminarAsync(id, ct);
                return Ok(new { message = "Concepto eliminado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message.Contains("en uso") || ex.Message.Contains("precios asociados")
                    ? BadRequest(new { message = ex.Message })
                    : NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{idConcepto:int}/precios")]
        public async Task<ActionResult<PrecioDto>> CrearPrecioPorRuta(
            int idConcepto,
            [FromBody] CrearPrecioDto dto,
            CancellationToken ct)
        {
            try
            {
                dto.IdConceptoPago = idConcepto;
                var creado = await _svc.CrearPrecioAsync(dto, ct);
                return CreatedAtAction(nameof(ListarPrecios), new { idConcepto }, creado);
            }
            catch (InvalidOperationException vex)
            {
                return Conflict(vex.Message);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
        }

        [HttpPost("~/api/precios")]
        public async Task<ActionResult<PrecioDto>> CrearPrecio(
            [FromBody] CrearPrecioDto dto,
            CancellationToken ct)
        {
            try
            {
                var creado = await _svc.CrearPrecioAsync(dto, ct);
                return CreatedAtAction(nameof(ListarPrecios), new { idConcepto = dto.IdConceptoPago }, creado);
            }
            catch (InvalidOperationException vex)
            {
                return Conflict(vex.Message);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
        }

        [HttpGet("{idConcepto:int}/precios")]
        public async Task<ActionResult<IReadOnlyList<PrecioDto>>> ListarPrecios(
            int idConcepto,
            CancellationToken ct)
        {
            var list = await _svc.ListarPreciosAsync(idConcepto, ct);
            return Ok(list);
        }
    }
}
