using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Aspirante;
using WebApplication2.Core.Requests.PlanEstudios;
using WebApplication2.Services;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.COORDINADOR},{Rol.DIRECTOR},{Rol.DOCENTE},{Rol.CONTROL_ESCOLAR}")]
    public class CalificacionesController : ControllerBase
    {
        private readonly ICalificacionesService _calificacionesService;
        private readonly IMapper _mapper;

        public CalificacionesController(ICalificacionesService CalificacionesService, IMapper mapper)
        {
            _calificacionesService = CalificacionesService;
            _mapper = mapper;
        }

        [HttpGet("{grupoMateriaId}/{parcialId}")]
        public async Task<ActionResult<IEnumerable<CalificacionParcialResponse>>> Get(int grupoMateriaId, int parcialId)
        {
            var parcialesGrupo = await _calificacionesService.GetParcialesPorGrupo(grupoMateriaId, parcialId);

            var calificacionParcialResponse = _mapper.Map<IEnumerable<CalificacionParcialResponse>>(parcialesGrupo);

            return Ok(calificacionParcialResponse);
        }


        [HttpPost("parciales")]
        public async Task<ActionResult<CalificacionParcialResponse>> CrearParcial([FromBody] CalificacionParcialCreateRequest req)
        {
            var acta = _mapper.Map<CalificacionParcial>(req);
            acta.StatusParcial = StatusParcialEnum.Abierto;
            acta.FechaApertura = req.FechaApertura ?? DateTime.UtcNow;

            var creado = await _calificacionesService.AbrirParcial(acta);

            var dto = _mapper.Map<CalificacionParcialResponse>(creado);
            return CreatedAtAction(nameof(GetParcialById), new { id = creado.Id }, dto);
        }

        [HttpGet("parciales/{id:int}")]
        public async Task<ActionResult<CalificacionParcialResponse>> GetParcialById(int id)
        {
            var acta = await _calificacionesService.GetParcialById(id);
            if (acta is null) return NotFound();
            return _mapper.Map<CalificacionParcialResponse>(acta);
        }

        [HttpPatch("parciales/{id:int}/estado")]
        public async Task<IActionResult> CambiarEstadoParcial(int id, [FromBody] CalificacionParcialEstadoRequest req)
        {
            await _calificacionesService.CambiarEstadoParcial(id, req.StatusParcial.ToString(), User?.Identity?.Name ?? "sistema");
            return NoContent();
        }

        [HttpPost("detalle")]
        public async Task<ActionResult<CalificacionDetalleItemResponse>> UpsertDetalle([FromBody] CalificacionDetalleUpsertRequest req)
        {
            try
            {
                var entity = _mapper.Map<CalificacionDetalle>(req);
                var username = User?.Identity?.Name ?? "sistema";

                await _calificacionesService.UpsertDetalle(entity, username);

                var dto = _mapper.Map<CalificacionDetalleItemResponse>(entity);
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("parciales/{calificacionParcialId:int}/validar-pesos")]
        public async Task<ActionResult<object>> ValidarPesosEvaluacion(int calificacionParcialId)
        {
            var resultado = await _calificacionesService.ValidarPesosEvaluacion(calificacionParcialId);

            return Ok(new
            {
                esValido = resultado.EsValido,
                sumaPesos = resultado.SumaPesos,
                mensaje = resultado.Mensaje
            });
        }

        [HttpGet("concentrado/alumno/{inscripcionId:int}")]
        public async Task<ActionResult<object>> GetConcentradoAlumno(int inscripcionId)
        {
            try
            {
                var (detalles, calificacionFinal) = await _calificacionesService.GetConcentradoAlumno(inscripcionId);

                var detallesDto = _mapper.Map<IList<CalificacionDetalleItemResponse>>(detalles);

                return Ok(new
                {
                    inscripcionId = inscripcionId,
                    calificacionFinal = calificacionFinal,
                    evaluaciones = detallesDto
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        [HttpGet("concentrado/grupo/{grupoMateriaId:int}/parcial/{parcialId:int}")]
        public async Task<ActionResult<object>> GetConcentradoGrupoParcial(int grupoMateriaId, int parcialId)
        {
            var resultado = await _calificacionesService.GetConcentradoGrupoParcial(grupoMateriaId, parcialId);

            return Ok(new
            {
                grupoMateriaId = grupoMateriaId,
                parcialId = parcialId,
                calificaciones = resultado.Select(r => new
                {
                    inscripcionId = r.InscripcionId,
                    aporteParcial = r.AporteParcial
                })
            });
        }

        [HttpGet("detalles")]
        public async Task<ActionResult<PagedResult<CalificacionDetalleItemResponse>>> GetDetalles(
            [FromQuery] int grupoMateriaId = 0,
            [FromQuery] int parcialId = 0,
            [FromQuery] int inscripcionId = 0,
            [FromQuery] int tipoEvaluacionEnum = -1,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var resultado = await _calificacionesService.GetDetalles(
                grupoMateriaId, parcialId, inscripcionId, tipoEvaluacionEnum, page, pageSize);

            var itemsDto = _mapper.Map<List<CalificacionDetalleItemResponse>>(resultado.Items);

            return Ok(new PagedResult<CalificacionDetalleItemResponse>
            {
                Items = itemsDto,
                TotalItems = resultado.TotalItems,
                PageNumber = resultado.PageNumber,
                PageSize = resultado.PageSize
            });
        }
    }
}
