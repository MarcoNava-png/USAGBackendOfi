using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.PreInscripcion;
using WebApplication2.Core.Requests.PreInscripcion;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/preinscripciones")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.COORDINADOR},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.ACADEMICO}")]
    public class PreInscripcionController : ControllerBase
    {
        private readonly IPreInscripcionService _preInscripcionService;

        public PreInscripcionController(IPreInscripcionService preInscripcionService)
        {
            _preInscripcionService = preInscripcionService;
        }

        [HttpPost]
        public async Task<ActionResult<PreInscripcionDto>> Apartar(
            [FromBody] ApartarPreInscripcionRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var dto = await _preInscripcionService.ApartarParaPeriodoAsync(request, ct);
                return CreatedAtAction(nameof(Apartar), new { id = dto.IdPreInscripcion }, dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("pendientes")]
        public async Task<ActionResult<List<PreInscripcionDto>>> GetPendientes(
            [FromQuery] int? idPlanEstudios,
            [FromQuery] int? idPeriodoAcademico,
            CancellationToken ct = default)
        {
            try
            {
                var pendientes = await _preInscripcionService.GetPendientesAsync(idPlanEstudios, idPeriodoAcademico, ct);
                return Ok(pendientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("{id}/asignar-grupo")]
        public async Task<ActionResult<PreInscripcionDto>> AsignarGrupo(
            int id,
            [FromBody] AsignarGrupoPreinscripcionRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var dto = await _preInscripcionService.AsignarGrupoAsync(id, request.IdGrupo, ct);
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("{id}/cancelar")]
        public async Task<ActionResult<PreInscripcionDto>> Cancelar(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                var dto = await _preInscripcionService.CancelarAsync(id, ct);
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
