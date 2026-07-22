using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.ConfiguracionCalificaciones;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/configuracion-calificaciones")]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.ACADEMICO}")]
    public class ConfiguracionCalificacionesController : ControllerBase
    {
        private readonly IConfiguracionCalificacionesService _service;

        public ConfiguracionCalificacionesController(IConfiguracionCalificacionesService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ConfiguracionCalificacionesDto>> Obtener(CancellationToken ct)
        {
            return Ok(await _service.ObtenerAsync(ct));
        }

        [HttpPut]
        public async Task<ActionResult<ConfiguracionCalificacionesDto>> Guardar([FromBody] ConfiguracionCalificacionesDto dto, CancellationToken ct)
        {
            return Ok(await _service.GuardarAsync(dto, ct));
        }

        [HttpGet("parciales")]
        public async Task<ActionResult<List<ParcialDto>>> ListarParciales(CancellationToken ct)
        {
            return Ok(await _service.ListarParcialesAsync(ct));
        }

        [HttpPost("parciales")]
        public async Task<ActionResult<ParcialDto>> CrearParcial([FromBody] ParcialDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "El nombre del parcial es requerido" });
            try
            {
                return Ok(await _service.GuardarParcialAsync(dto, ct));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("parciales")]
        public async Task<ActionResult<ParcialDto>> ActualizarParcial([FromBody] ParcialDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "El nombre del parcial es requerido" });
            try
            {
                return Ok(await _service.GuardarParcialAsync(dto, ct));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("parciales/{id:int}")]
        public async Task<IActionResult> EliminarParcial(int id, CancellationToken ct)
        {
            var ok = await _service.EliminarParcialAsync(id, ct);
            return ok ? Ok(new { message = "Parcial eliminado" }) : NotFound();
        }
    }
}
