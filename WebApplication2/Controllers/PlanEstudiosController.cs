using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.PlanEstudios;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlanEstudiosController : ControllerBase
    {
        private readonly IPlanEstudioService _planEstudioService;
        private readonly IMapper _mapper;

        public PlanEstudiosController(IPlanEstudioService planEstudioService, IMapper mapper)
        {
            _planEstudioService = planEstudioService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS},{Rol.ADMISIONES}")]
        public async Task<ActionResult<PagedResult<PlanEstudioDto>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 1000,
            [FromQuery] int? idCampus = null,
            [FromQuery] bool incluirInactivos = false)
        {
            var pagination = await _planEstudioService.GetPlanesEstudios(page, pageSize, idCampus, incluirInactivos);

            var planesEstudiosDto = _mapper.Map<IEnumerable<PlanEstudioDto>>(pagination.Items);

            var response = new PagedResult<PlanEstudioDto>
            {
                TotalItems = pagination.TotalItems,
                Items = [.. planesEstudiosDto],
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS},{Rol.ADMISIONES}")]
        public async Task<ActionResult<PlanEstudioDto>> GetById(int id)
        {
            var planEstudios = await _planEstudioService.GetPlanEstudiosById(id);

            if (planEstudios == null)
            {
                return NotFound(new { message = "Plan de estudios no encontrado" });
            }

            var planEstudiosDto = _mapper.Map<PlanEstudioDto>(planEstudios);
            return Ok(planEstudiosDto);
        }

        [HttpPost]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        public async Task<ActionResult<PlanEstudioDto>> Post([FromBody] PlanEstudiosRequest request)
        {
            try
            {
                var planEstudios = _mapper.Map<PlanEstudios>(request);

                await _planEstudioService.CrearPlanEstudios(planEstudios);

                var planEstudiosDto = _mapper.Map<PlanEstudioDto>(planEstudios);

                return Ok(planEstudiosDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        public async Task<IActionResult> Update([FromBody] PlanEstudiosUpdateRequest request)
        {
            try
            {
                var newPlanEstudios = _mapper.Map<PlanEstudios>(request);

                var planEstudios = await _planEstudioService.ActualizarPlanEstudios(newPlanEstudios);

                var planEstudiosDto = _mapper.Map<PlanEstudioDto>(planEstudios);

                return Ok(planEstudiosDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _planEstudioService.EliminarPlanEstudios(id);
                return Ok(new { message = "Plan de estudios eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/toggle")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        public async Task<ActionResult<PlanEstudioDto>> ToggleEstado(int id)
        {
            try
            {
                var planEstudios = await _planEstudioService.ToggleEstado(id);
                var planEstudiosDto = _mapper.Map<PlanEstudioDto>(planEstudios);
                return Ok(planEstudiosDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
