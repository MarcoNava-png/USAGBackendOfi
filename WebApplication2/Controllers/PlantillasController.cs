using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.PlantillaCobro;
using WebApplication2.Core.Requests.Recibo;
using WebApplication2.Core.Responses.Recibo;
using WebApplication2.Core.Requests.PlantillaCobro;
using WebApplication2.Core.Responses.PlantillaCobro;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/plantillas-cobro")]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS}")]
    public class PlantillasController : ControllerBase
    {
        private readonly IPlantillaCobroService _service;

        public PlantillasController(IPlantillaCobroService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PlantillaCobroDto>>> ListarPlantillas(
            [FromQuery] int? idPlanEstudios,
            [FromQuery] int? numeroCuatrimestre,
            [FromQuery] bool? soloActivas,
            [FromQuery] int? idPeriodoAcademico,
            CancellationToken ct)
        {
            try
            {
                var plantillas = await _service.ListarPlantillasAsync(
                    idPlanEstudios,
                    numeroCuatrimestre,
                    soloActivas,
                    idPeriodoAcademico,
                    ct);

                return Ok(plantillas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "Error al listar plantillas de cobro",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PlantillaCobroDto>> ObtenerPorId(int id, CancellationToken ct)
        {
            try
            {
                var plantilla = await _service.ObtenerPlantillaPorIdAsync(id, ct);

                if (plantilla == null)
                    return NotFound(new { message = "Plantilla no encontrada" });

                return Ok(plantilla);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener plantilla", error = ex.Message });
            }
        }

        [HttpGet("buscar-activa")]
        public async Task<ActionResult<PlantillaCobroDto>> BuscarActiva(
            [FromQuery] int idPlanEstudios,
            [FromQuery] int numeroCuatrimestre,
            [FromQuery] int? idPeriodoAcademico,
            [FromQuery] int? idTurno,
            [FromQuery] int? idModalidad,
            CancellationToken ct)
        {
            try
            {
                var plantilla = await _service.BuscarPlantillaActivaAsync(
                    idPlanEstudios,
                    numeroCuatrimestre,
                    idPeriodoAcademico,
                    idTurno,
                    idModalidad,
                    ct);

                if (plantilla == null)
                    return NotFound(new { message = "No se encontró una plantilla activa con los criterios especificados" });

                return Ok(plantilla);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar plantilla", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PlantillaCobroDto>> Crear(
            [FromBody] CreatePlantillaCobroDto dto,
            CancellationToken ct)
        {
            try
            {
                var usuarioId = GetCurrentUserId();
                var plantilla = await _service.CrearPlantillaAsync(dto, usuarioId, ct);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = plantilla.IdPlantillaCobro }, plantilla);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear plantilla", error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<PlantillaCobroDto>> Actualizar(
            int id,
            [FromBody] UpdatePlantillaCobroDto dto,
            CancellationToken ct)
        {
            try
            {
                var usuarioId = GetCurrentUserId();
                var plantilla = await _service.ActualizarPlantillaAsync(id, dto, usuarioId, ct);

                return Ok(plantilla);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar plantilla", error = ex.Message });
            }
        }

        [HttpPatch("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            [FromBody] CambiarEstadoPlantillaDto dto,
            CancellationToken ct)
        {
            try
            {
                await _service.CambiarEstadoPlantillaAsync(id, dto.EsActiva, ct);

                return Ok(new { message = dto.EsActiva ? "Plantilla activada" : "Plantilla desactivada" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cambiar estado", error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
        {
            try
            {
                await _service.EliminarPlantillaAsync(id, ct);

                return Ok(new { message = "Plantilla eliminada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message.Contains("en uso")
                    ? BadRequest(new { message = ex.Message })
                    : NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar plantilla", error = ex.Message });
            }
        }

        [HttpPost("{id:int}/duplicar")]
        public async Task<ActionResult<PlantillaCobroDto>> Duplicar(
            int id,
            [FromBody] CreatePlantillaCobroDto? cambios,
            CancellationToken ct)
        {
            try
            {
                var usuarioId = GetCurrentUserId();
                var plantilla = await _service.DuplicarPlantillaAsync(id, cambios, usuarioId, ct);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = plantilla.IdPlantillaCobro }, plantilla);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al duplicar plantilla", error = ex.Message });
            }
        }

        [HttpGet("cuatrimestres/{idPlanEstudios:int}")]
        public async Task<ActionResult<IReadOnlyList<int>>> ObtenerCuatrimestres(
            int idPlanEstudios,
            CancellationToken ct)
        {
            try
            {
                var cuatrimestres = await _service.ObtenerCuatrimestresPorPlanAsync(idPlanEstudios, ct);

                return Ok(cuatrimestres);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener cuatrimestres", error = ex.Message });
            }
        }

        [HttpPost("generar-recibos-masivo")]
        public async Task<ActionResult<GenerarRecibosMasivosResult>> GenerarRecibosMasivo(
            [FromBody] GenerarRecibosMasivosRequest request,
            CancellationToken ct)
        {
            try
            {
                var usuarioId = GetCurrentUserId();
                var resultado = await _service.GenerarRecibosMasivosAsync(request, usuarioId, ct);

                if (!resultado.Exitoso)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar recibos", error = ex.Message });
            }
        }

        [HttpPost("preview-recibos")]
        public ActionResult<PreviewRecibosResponse> GenerarPreviewRecibos(
            [FromBody] GenerarPreviewRecibosRequest request)
        {
            try
            {
                var preview = _service.GenerarPreviewRecibos(request);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar preview", error = ex.Message });
            }
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? "system";

            return userId;
        }
    }
}
