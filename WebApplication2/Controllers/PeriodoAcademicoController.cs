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
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.ACADEMICO},{Rol.ADMISIONES}")]
    public class PeriodoAcademicoController : ControllerBase
    {
        private readonly IPeriodoAcademicoService _periodoAcademicoervice;
        private readonly IMapper _mapper;
        private readonly IVentanaCapturaService _ventanaCapturaService;

        public PeriodoAcademicoController(IPeriodoAcademicoService periodoAcademicoService, IMapper mapper, IVentanaCapturaService ventanaCapturaService)
        {
            _periodoAcademicoervice = periodoAcademicoService;
            _mapper = mapper;
            _ventanaCapturaService = ventanaCapturaService;
        }

        private async Task SincronizarVentanasCapturaAsync(int idPeriodo, PeriodoAcademicoRequest request)
        {
            if (request.FechaLimiteParcial1.HasValue)
                await _ventanaCapturaService.AbrirAsync(idPeriodo, 1, request.FechaLimiteParcial1);
            if (request.FechaLimiteParcial2.HasValue)
                await _ventanaCapturaService.AbrirAsync(idPeriodo, 2, request.FechaLimiteParcial2);
            if (request.FechaLimiteParcial3.HasValue)
                await _ventanaCapturaService.AbrirAsync(idPeriodo, 3, request.FechaLimiteParcial3);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<PeriodoAcademicoDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
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
            var existente = await _periodoAcademicoervice.GetPeriodoPorClaveAsync(request.Clave);
            if (existente != null)
            {
                var estado = existente.Status == WebApplication2.Core.Enums.StatusEnum.Active ? "activo" : "inactivo";
                return BadRequest(new
                {
                    isSuccess = false,
                    messageError = $"Ya existe un periodo con la clave '{request.Clave}' (actualmente {estado}). Búscalo en la lista en vez de crear uno nuevo."
                });
            }

            var periodoAcademico = _mapper.Map<PeriodoAcademico>(request);

            await _periodoAcademicoervice.CrearPeriodoAcademico(periodoAcademico);

            await SincronizarVentanasCapturaAsync(periodoAcademico.IdPeriodoAcademico, request);

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

                await SincronizarVentanasCapturaAsync(request.IdPeriodoAcademico, request);

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
