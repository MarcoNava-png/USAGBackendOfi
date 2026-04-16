using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.Empresa;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS},{Rol.ADMISIONES}")]
    public class EmpresaController : ControllerBase
    {
        private readonly IEmpresaService _service;

        public EmpresaController(IEmpresaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] bool? soloActivas = null, CancellationToken ct = default)
        {
            var result = await _service.ListarEmpresasAsync(soloActivas, ct);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken ct = default)
        {
            var result = await _service.ObtenerPorIdAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS}")]
        public async Task<IActionResult> Crear([FromBody] CrearEmpresaDto dto, CancellationToken ct = default)
        {
            var usuarioId = User.FindFirst("userId")?.Value ?? User.Identity?.Name ?? "";
            try
            {
                var result = await _service.CrearEmpresaAsync(dto, usuarioId, ct);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = result.IdEmpresa }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarEmpresaDto dto, CancellationToken ct = default)
        {
            var usuarioId = User.FindFirst("userId")?.Value ?? User.Identity?.Name ?? "";
            try
            {
                var result = await _service.ActualizarEmpresaAsync(id, dto, usuarioId, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct = default)
        {
            var eliminado = await _service.EliminarEmpresaAsync(id, ct);
            if (!eliminado) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS}")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoEmpresaDto dto, CancellationToken ct = default)
        {
            var cambiado = await _service.CambiarEstadoAsync(id, dto.Activo, ct);
            if (!cambiado) return NotFound();
            return NoContent();
        }
    }

    public class CambiarEstadoEmpresaDto
    {
        public bool Activo { get; set; }
    }
}
