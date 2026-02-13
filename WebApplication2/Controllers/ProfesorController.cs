using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Profesor;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Profesor;
using WebApplication2.Core.Responses.Profesor;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
    public class ProfesorController : ControllerBase
    {
        private readonly IProfesorService _profesorService;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public ProfesorController(IProfesorService profesorService, IAuthService authService, IMapper mapper)
        {
            _profesorService = profesorService;
            _authService = authService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ProfesorDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 1000)
        {
            var pagination = await _profesorService.GetAllProfesores(page, pageSize);

            var profesoresDto = _mapper.Map<IEnumerable<ProfesorDto>>(pagination.Items);

            var response = new PagedResult<ProfesorDto>
            {
                TotalItems = pagination.TotalItems,
                Items = [.. profesoresDto],
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [HttpGet("{campusId:int}")]
        public async Task<ActionResult<PagedResult<ProfesorDto>>> Get(int campusId, [FromQuery] int page = 1, [FromQuery] int pageSize = 1000)
        {
            var pagination = await _profesorService.GetProfesores(campusId, page, pageSize);

            var profesoresDto = _mapper.Map<IEnumerable<ProfesorDto>>(pagination.Items);

            var response = new PagedResult<ProfesorDto>
            {
                TotalItems = pagination.TotalItems,
                Items = [.. profesoresDto],
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ProfesorDto>> Profesor([FromBody] ProfesorRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.EmailInstitucional,
                Email = request.EmailInstitucional,
            };

            try
            {
                var signupResponse = await _authService.Signup(user, request.NoEmpleado, [Rol.DOCENTE]);

                var newProfesor = _mapper.Map<Profesor>(request);

                var profesor = await _profesorService.CrearProfesor(newProfesor);

                var profesorDto = _mapper.Map<ProfesorDto>(profesor);

                return Ok(profesorDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ProfesorUpdateRequest request)
        {
            try
            {
                var newProfesor = _mapper.Map<Profesor>(request);

                await _profesorService.ActualizarProfesor(newProfesor);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{idProfesor:int}/validar-horario")]
        public async Task<ActionResult<ValidarHorarioProfesorResponse>> ValidarHorario(
            int idProfesor,
            [FromBody] ValidarHorarioRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _profesorService.ValidarConflictosHorarioAsync(
                    idProfesor,
                    request.HorarioJson,
                    request.IdGrupoMateriaActual,
                    ct);

                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al validar horario", error = ex.Message });
            }
        }
    }
}
