using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.PeriodoAcademico;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
    public class PeriodoAcademicoController : ControllerBase
    {
        private readonly IPeriodoAcademicoService _periodoAcademicoervice;
        private readonly IMapper _mapper;

        public PeriodoAcademicoController(IPeriodoAcademicoService periodoAcademicoService, IMapper mapper)
        {
            _periodoAcademicoervice = periodoAcademicoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<PeriodoAcademicoDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 1000)
        {
            var pagination = await _periodoAcademicoervice.GetPeriodosAcademicos(page, pageSize);

            var periodosAcademicosDto = _mapper.Map<IEnumerable<PeriodoAcademicoDto>>(pagination.Items);

            var response = new PagedResult<PeriodoAcademicoDto>
            {
                TotalItems = pagination.TotalItems,
                Items = [.. periodosAcademicosDto],
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<PeriodoAcademicoDto>> Post([FromBody] PeriodoAcademicoRequest request)
        {
            var periodoAcademico = _mapper.Map<PeriodoAcademico>(request);

            await _periodoAcademicoervice.CrearPeriodoAcademico(periodoAcademico);

            var periodoAcademicoDto = _mapper.Map<PeriodoAcademicoDto>(periodoAcademico);

            return Ok(periodoAcademicoDto);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] PeriodoAcademicoUpdateRequest request)
        {
            try
            {
                var newPeriodoAcademico = _mapper.Map<PeriodoAcademico>(request);

                var periodoAcademico = await _periodoAcademicoervice.ActualizarPeriodoAcademico(newPeriodoAcademico);

                var periodoAcademicoDto = _mapper.Map<PeriodoAcademicoDto>(periodoAcademico);

                return Ok(periodoAcademicoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("actual")]
        public async Task<ActionResult<PeriodoAcademicoDto>> GetPeriodoActual()
        {
            try
            {
                var periodoActual = await _periodoAcademicoervice.GetPeriodoActualAsync();

                if (periodoActual == null)
                    return NotFound(new { mensaje = "No hay un periodo académico marcado como actual" });

                var periodoDto = _mapper.Map<PeriodoAcademicoDto>(periodoActual);
                return Ok(periodoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("{idPeriodoAcademico}/marcar-actual")]
        public async Task<ActionResult<PeriodoAcademicoDto>> MarcarComoActual(int idPeriodoAcademico)
        {
            try
            {
                var periodo = await _periodoAcademicoervice.MarcarComoPeriodoActualAsync(idPeriodoAcademico);
                var periodoDto = _mapper.Map<PeriodoAcademicoDto>(periodo);

                return Ok(new
                {
                    mensaje = $"El periodo '{periodo.Nombre}' ha sido marcado como actual",
                    periodo = periodoDto
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PeriodoAcademicoDto>> GetPorId(int id)
        {
            try
            {
                var periodo = await _periodoAcademicoervice.GetPeriodoAcademicoPorIdAsync(id);

                if (periodo == null)
                    return NotFound(new { mensaje = "Periodo académico no encontrado" });

                var periodoDto = _mapper.Map<PeriodoAcademicoDto>(periodo);
                return Ok(periodoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _periodoAcademicoervice.EliminarPeriodoAcademicoAsync(id);
                return Ok(new { mensaje = "Periodo académico eliminado exitosamente" });
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
