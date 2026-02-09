using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Campus;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CampusController : ControllerBase
    {
        private readonly ICampusService _campusService;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public CampusController(ICampusService CampusService, IAuthService authService, IMapper mapper)
        {
            _campusService = CampusService;
            _authService = authService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS},{Rol.ADMISIONES}")]
        public async Task<ActionResult<PagedResult<CampusDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 1000)
        {
            var pagination = await _campusService.GetCampuses(page, pageSize);

            var CampusesDto = _mapper.Map<IEnumerable<CampusDto>>(pagination.Items);

            var response = new PagedResult<CampusDto>
            {
                TotalItems = pagination.TotalItems,
                Items = [.. CampusesDto],
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS},{Rol.ADMISIONES}")]
        public async Task<ActionResult<CampusDto>> GetById(int id)
        {
            var campus = await _campusService.GetCampusById(id);

            if (campus == null)
            {
                return NotFound(new { message = "Campus no encontrado" });
            }

            var campusDto = _mapper.Map<CampusDto>(campus);
            return Ok(campusDto);
        }

        [HttpPost]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<ActionResult<CampusDto>> Campus([FromBody] CampusRequest request)
        {
            try
            {
                var newCampus = _mapper.Map<Campus>(request);

                var campus = await _campusService.CrearCampus(newCampus);

                var campusDto = _mapper.Map<CampusDto>(campus);

                return Ok(campusDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<IActionResult> Update([FromBody] CampusUpdateRequest request)
        {
            try
            {
                var newCampus = _mapper.Map<Campus>(request);

                await _campusService.ActualizarCampus(newCampus);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _campusService.EliminarCampus(id);
                return Ok(new { message = "Campus eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
