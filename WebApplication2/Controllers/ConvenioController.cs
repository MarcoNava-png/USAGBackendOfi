using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Convenio;
using WebApplication2.Core.Requests.Convenio;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConvenioController : ControllerBase
    {
        private readonly IConvenioService _convenioService;

        public ConvenioController(IConvenioService convenioService)
        {
            _convenioService = convenioService;
        }

        [HttpGet]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS},{Rol.ADMISIONES}")]
        public async Task<ActionResult<IEnumerable<ConvenioDto>>> Get(
            [FromQuery] bool? soloActivos = null,
            [FromQuery] int? idCampus = null,
            [FromQuery] int? idPlanEstudios = null,
            CancellationToken ct = default)
        {
            var convenios = await _convenioService.ListarConveniosAsync(soloActivos, idCampus, idPlanEstudios, ct);
            return Ok(convenios);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS},{Rol.ADMISIONES}")]
        public async Task<ActionResult<ConvenioDto>> GetById(int id, CancellationToken ct = default)
        {
            var convenio = await _convenioService.ObtenerPorIdAsync(id, ct);

            if (convenio == null)
            {
                return NotFound(new { message = "Convenio no encontrado" });
            }

            return Ok(convenio);
        }

        [HttpGet("activos")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS},{Rol.ADMISIONES},{Rol.CONTROL_ESCOLAR}")]
        public async Task<ActionResult<IEnumerable<ConvenioDto>>> GetActivos(
            [FromQuery] int? idCampus = null,
            [FromQuery] int? idPlanEstudios = null,
            CancellationToken ct = default)
        {
            var convenios = await _convenioService.ListarConveniosAsync(true, idCampus, idPlanEstudios, ct);
            return Ok(convenios);
        }

        [HttpPost]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<ActionResult<ConvenioDto>> Create([FromBody] CrearConvenioDto dto, CancellationToken ct = default)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "sistema";
                var convenio = await _convenioService.CrearConvenioAsync(dto, usuarioId, ct);
                return CreatedAtAction(nameof(GetById), new { id = convenio.IdConvenio }, convenio);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<ActionResult<ConvenioDto>> Update(int id, [FromBody] ActualizarConvenioDto dto, CancellationToken ct = default)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "sistema";
                var convenio = await _convenioService.ActualizarConvenioAsync(id, dto, usuarioId, ct);
                return Ok(convenio);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                var resultado = await _convenioService.EliminarConvenioAsync(id, ct);

                if (!resultado)
                {
                    return NotFound(new { message = "Convenio no encontrado" });
                }

                return Ok(new { message = "Convenio eliminado correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/estado")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoConvenioRequest request, CancellationToken ct = default)
        {
            var resultado = await _convenioService.CambiarEstadoConvenioAsync(id, request.Activo, ct);

            if (!resultado)
            {
                return NotFound(new { message = "Convenio no encontrado" });
            }

            return Ok(new { message = $"Convenio {(request.Activo ? "activado" : "desactivado")} correctamente" });
        }


        [HttpGet("aspirante/{idAspirante}/disponibles")]
        [Authorize(Roles = Rol.ROLES_ADMISIONES)]
        public async Task<ActionResult<IEnumerable<ConvenioDisponibleDto>>> GetConveniosDisponiblesParaAspirante(
            int idAspirante,
            CancellationToken ct = default)
        {
            try
            {
                var convenios = await _convenioService.ObtenerConveniosDisponiblesParaAspiranteAsync(idAspirante, ct);
                return Ok(convenios);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("aspirante/{idAspirante}")]
        [Authorize(Roles = Rol.ROLES_ADMISIONES)]
        public async Task<ActionResult<IEnumerable<AspiranteConvenioDto>>> GetConveniosAspirante(
            int idAspirante,
            CancellationToken ct = default)
        {
            var convenios = await _convenioService.ObtenerConveniosAspiranteAsync(idAspirante, ct);
            return Ok(convenios);
        }

        [HttpPost("aspirante/asignar")]
        [Authorize(Roles = Rol.ROLES_ADMISIONES)]
        public async Task<ActionResult<AspiranteConvenioDto>> AsignarConvenioAspirante(
            [FromBody] AsignarConvenioAspiranteDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "sistema";
                var asignacion = await _convenioService.AsignarConvenioAspiranteAsync(dto, usuarioId, ct);
                return Ok(asignacion);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("aspirante/{idAspiranteConvenio}/estatus")]
        [Authorize(Roles = Rol.ROLES_ADMISIONES)]
        public async Task<IActionResult> CambiarEstatusConvenioAspirante(
            int idAspiranteConvenio,
            [FromBody] CambiarEstatusConvenioAspiranteRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _convenioService.CambiarEstatusConvenioAspiranteAsync(
                    idAspiranteConvenio, request.Estatus, ct);

                if (!resultado)
                {
                    return NotFound(new { message = "Asignacion de convenio no encontrada" });
                }

                return Ok(new { message = $"Estatus cambiado a '{request.Estatus}'" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("aspirante/{idAspiranteConvenio}")]
        [Authorize(Roles = Rol.ROLES_ADMISIONES)]
        public async Task<IActionResult> EliminarConvenioAspirante(int idAspiranteConvenio, CancellationToken ct = default)
        {
            var resultado = await _convenioService.EliminarConvenioAspiranteAsync(idAspiranteConvenio, ct);

            if (!resultado)
            {
                return NotFound(new { message = "Asignacion de convenio no encontrada" });
            }

            return Ok(new { message = "Convenio eliminado del aspirante" });
        }


        [HttpGet("{id}/calcular-descuento")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS},{Rol.ADMISIONES},{Rol.CONTROL_ESCOLAR}")]
        public async Task<ActionResult<CalculoDescuentoConvenioDto>> CalcularDescuento(
            int id,
            [FromQuery] decimal monto,
            CancellationToken ct = default)
        {
            try
            {
                var calculo = await _convenioService.CalcularDescuentoConvenioAsync(id, monto, ct);
                return Ok(calculo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("aspirante/{idAspirante}/calcular-descuento-total")]
        [Authorize(Roles = Rol.ROLES_ADMISIONES)]
        public async Task<ActionResult<object>> CalcularDescuentoTotalAspirante(
            int idAspirante,
            [FromQuery] decimal monto,
            [FromQuery] string? tipoConcepto = null,
            CancellationToken ct = default)
        {
            var descuento = await _convenioService.CalcularDescuentoTotalAspiranteAsync(idAspirante, monto, tipoConcepto, ct);
            return Ok(new
            {
                montoOriginal = monto,
                descuento = descuento,
                montoFinal = monto - descuento,
                tipoConcepto = tipoConcepto ?? "TODOS"
            });
        }
    }
}
