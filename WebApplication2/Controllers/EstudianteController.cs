using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Estudiante;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/estudiantes")]
    [Produces("application/json")]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.FINANZAS}")]
    public class EstudianteController : ControllerBase
    {
        private readonly IEstudianteService _estudianteService;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IAspiranteService _aspiranteService;
        private readonly ICatalogoService _catalogoService;

        public EstudianteController(
            IEstudianteService estudianteService,
            IMapper mapper,
            IAuthService authService,
            IConfiguration configuration,
            IAspiranteService aspiranteService,
            ICatalogoService catalogoService)
        {
            _estudianteService = estudianteService;
            _mapper = mapper;
            _authService = authService;
            _configuration = configuration;
            _aspiranteService = aspiranteService;
            _catalogoService = catalogoService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<EstudianteDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 1000)
        {
            var pagination = await _estudianteService.GetEstudiantes(page, pageSize);
            var estudiantesDto = _mapper.Map<IEnumerable<EstudianteDto>>(pagination.Items);

            var response = new PagedResult<EstudianteDto>
            {
                TotalItems = pagination.TotalItems,
                Items = [.. estudiantesDto],
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EstudianteDetalleDto>> Get(int id)
        {
            var estudiante = await _estudianteService.GetEstudianteDetalle(id);
            if (estudiante is null) return NotFound();

            var estudianteDto = _mapper.Map<EstudianteDetalleDto>(estudiante);
            return Ok(estudianteDto);
        }

        [HttpGet("matricula/{matricula}")]
        public async Task<ActionResult<EstudianteDto>> GetByMatricula(string matricula)
        {
            var estudiante = await _estudianteService.GetEstudianteByMatricula(matricula);
            if (estudiante is null) return NotFound(new { error = "Estudiante no encontrado" });

            var estudianteDto = _mapper.Map<EstudianteDto>(estudiante);
            return Ok(estudianteDto);
        }

        [HttpGet("sin-grupo")]
        public async Task<ActionResult<PagedResult<EstudianteDto>>> GetSinGrupo(
            [FromQuery] int idPlanEstudios,
            [FromQuery] int idPeriodoAcademico,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 1000)
        {
            var pagination = await _estudianteService.GetEstudiantesSinGrupo(idPlanEstudios, idPeriodoAcademico, page, pageSize);
            var estudiantesDto = _mapper.Map<IEnumerable<EstudianteDto>>(pagination.Items);

            var response = new PagedResult<EstudianteDto>
            {
                TotalItems = pagination.TotalItems,
                Items = [.. estudiantesDto],
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EstudianteDto>> Post([FromBody] EstudianteRequest request)
        {
            try
            {
                var newEstudiante = new Estudiante
                {
                    Matricula = request.Matricula,
                    IdPersona = request.IdPersona,
                    FechaIngreso = DateOnly.FromDateTime(DateTime.Now),
                    IdPlanActual = request.IdPlanActual,
                    Activo = true,
                };

                var estudiante = await _estudianteService.CrearEstudiante(newEstudiante);

                var aspirante = await _aspiranteService.GetAspiranteByPersonaId(estudiante.IdPersona);
                if (aspirante != null)
                {
                    var estatus = await _catalogoService.GetEstatusAspirante();
                    var admitido = estatus.FirstOrDefault(e => e.DescEstatus == "Admitido");
                    if (admitido != null)
                    {
                        aspirante.IdAspiranteEstatus = admitido.IdAspiranteEstatus;
                        await _aspiranteService.ActualizarAspirante(aspirante);
                    }
                }

                var estudianteDto = _mapper.Map<EstudianteDto>(estudiante);

                return Ok(estudianteDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("matricular")]
        public async Task<IActionResult> Matricular([FromBody] EstudianteMatricularRequest request)
        {
            var newEstudiante = _mapper.Map<Estudiante>(request);

            var dominioMatricula = _configuration["AppConfig:DominioMatricula"];

            var user = new ApplicationUser
            {
                UserName = $"{request.Matricula}{dominioMatricula}",
                Email = $"{request.Matricula}{dominioMatricula}",
            };

            try
            {
                await _authService.Signup(user, request.Matricula, [Rol.ALUMNO]);

                var usuario = await _authService.GetUserByEmail(user.Email);
                if (usuario == null) return Problem("No se pudo recuperar el usuario recién creado.");

                newEstudiante.UsuarioId = usuario.Id;
                newEstudiante.Email = usuario.Email;

                var estudiante = await _estudianteService.ActualizarEstudiante(newEstudiante);
                var estudianteDto = _mapper.Map<EstudianteDto>(estudiante);

                return Ok(estudianteDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
