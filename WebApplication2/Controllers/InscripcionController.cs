using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Inscripcion;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/inscripciones")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.COORDINADOR},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.ACADEMICO}")]
    public class InscripcionController : ControllerBase
    {
        private readonly IInscripcionService _inscripcionService;
        private readonly IGrupoService _grupoService;
        private readonly IMapper _mapper;

        public InscripcionController(IInscripcionService inscripcionService, IGrupoService grupoService, IMapper mapper)
        {
            _inscripcionService = inscripcionService;
            _grupoService = grupoService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<InscripcionDto>> Inscripcion([FromBody] InscripcionRequest request)
        {
            try
            {
                var grupoMateria = await _grupoService.GetGrupoMateriaByNameAsync(request.NombreGrupoMateria);
                if (grupoMateria == null)
                {
                    return NotFound($"GrupoMateria with name '{request.NombreGrupoMateria}' not found.");
                }

                var newInscripcion = new Inscripcion
                {
                    IdEstudiante = request.IdEstudiante,
                    IdGrupoMateria = grupoMateria.IdGrupoMateria,
                    FechaInscripcion = request.FechaInscripcion == default ? DateTime.UtcNow : request.FechaInscripcion,
                    Estado = string.IsNullOrWhiteSpace(request.Estado) ? "Inscrito" : request.Estado
                };

                var created = await _inscripcionService.CrearInscripcion(newInscripcion);

                var dto = _mapper.Map<InscripcionDto>(created);

                dto.NombreGrupoMateria = grupoMateria.IdGrupoNavigation?.NombreGrupo ?? grupoMateria.Name;

                return CreatedAtAction(nameof(Inscripcion), new { id = dto.IdInscripcion }, dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("con-recibos")]
        public async Task<ActionResult<InscripcionConPagosDto>> InscribirConRecibos(
            [FromBody] InscribirConRecibosRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _inscripcionService.InscribirConRecibosAutomaticosAsync(request, ct);
                return CreatedAtAction(nameof(InscribirConRecibos), new { id = resultado.IdInscripcion }, resultado);
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

        [HttpGet("verificar-nuevo-ingreso")]
        public async Task<ActionResult<object>> VerificarNuevoIngreso(
            [FromQuery] int idEstudiante,
            [FromQuery] int idPeriodoAcademico,
            CancellationToken ct = default)
        {
            try
            {
                var esNuevoIngreso = await _inscripcionService.EsNuevoIngresoAsync(idEstudiante, idPeriodoAcademico, ct);

                return Ok(new
                {
                    idEstudiante = idEstudiante,
                    idPeriodoAcademico = idPeriodoAcademico,
                    esNuevoIngreso = esNuevoIngreso,
                    tipoInscripcion = esNuevoIngreso ? "Nuevo Ingreso" : "Re-inscripción",
                    mensaje = esNuevoIngreso
                        ? "El estudiante es de nuevo ingreso. Se cobrará inscripción + colegiaturas"
                        : "El estudiante ya está inscrito. Solo se cobrarán colegiaturas"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("estudiante/{idEstudiante}")]
        public async Task<ActionResult<List<InscripcionDto>>> GetInscripcionesByEstudiante(int idEstudiante)
        {
            try
            {
                var inscripciones = await _inscripcionService.GetInscripcionesByEstudianteAsync(idEstudiante);
                var inscripcionesDto = _mapper.Map<List<InscripcionDto>>(inscripciones);
                return Ok(inscripcionesDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("grupomateria")]
        public async Task<ActionResult<InscripcionDto>> InscribirGrupoMateria([FromBody] InscripcionGrupoMateriaRequest request)
        {
            try
            {
                var newInscripcion = new Inscripcion
                {
                    IdEstudiante = request.IdEstudiante,
                    IdGrupoMateria = request.IdGrupoMateria,
                    FechaInscripcion = request.FechaInscripcion ?? DateTime.UtcNow,
                    Estado = "Inscrito"
                };

                var created = await _inscripcionService.CrearInscripcion(newInscripcion);
                var dto = _mapper.Map<InscripcionDto>(created);

                return CreatedAtAction(nameof(InscribirGrupoMateria), new { id = dto.IdInscripcion }, dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
