using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.GestionAcademica;
using WebApplication2.Core.DTOs.Grupo;
using WebApplication2.Core.DTOs.Inscripcion;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.GestionAcademica;
using WebApplication2.Core.Requests.Grupo;
using WebApplication2.Core.Responses.Grupo;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/grupos")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.COORDINADOR},{Rol.DIRECTOR},{Rol.DOCENTE},{Rol.CONTROL_ESCOLAR},{Rol.ACADEMICO},{Rol.ADMISIONES}")]
    public class GrupoController : ControllerBase
    {
        private readonly IGrupoService _grupoService;
        private readonly IMapper _mapper;

        public GrupoController(IGrupoService grupoService, IMapper mapper)
        {
            _grupoService = grupoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<GrupoDto>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 1000,
            [FromQuery] int? idPeriodoAcademico = null)
        {
            var pagination = await _grupoService.GetGrupos(page, pageSize, idPeriodoAcademico);

            var gruposDto = _mapper.Map<IEnumerable<GrupoDto>>(pagination.Items);

            var response = new PagedResult<GrupoDto>
            {
                TotalItems = pagination.TotalItems,
                Items = gruposDto.ToList(),
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [HttpGet("{idGrupo}")]
        public async Task<ActionResult<GrupoDetalleDto>> GetDetalle(int idGrupo)
        {
            var grupoDetalle = await _grupoService.GetDetalleGrupo(idGrupo);

            var gruposDetaleDto = _mapper.Map<GrupoDetalleDto>(grupoDetalle);

            return Ok(gruposDetaleDto);
        }

        [HttpPost]
        public async Task<ActionResult<GrupoDto>> Grupo([FromBody] GrupoRequest request)
        {
            try
            {
                var newGrupo = _mapper.Map<Grupo>(request);

                await _grupoService.CrearGrupo(newGrupo);

                var grupoDto = _mapper.Map<GrupoDto>(newGrupo);

                return Ok(grupoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("carga-materias")]
        public async Task<ActionResult> CargaMateria([FromBody] CargaGrupoMateriasRequest request)
        {
            try
            {
                var grupoMaterias = _mapper.Map<List<GrupoMateria>>(request.GrupoMaterias);

                grupoMaterias.ForEach(gm => gm.IdGrupo = request.IdGrupo);

                await _grupoService.CargarMateriasGrupo(grupoMaterias);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] GrupoUpdateRequest request)
        {
            try
            {
                var newGrupo = _mapper.Map<Grupo>(request);

                await _grupoService.ActualizarGrupo(newGrupo);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{idGrupo:int}")]
        public async Task<ActionResult> Delete(int idGrupo, CancellationToken ct = default)
        {
            try
            {
                var (exito, mensaje) = await _grupoService.EliminarGrupoAsync(idGrupo, ct);

                if (!exito)
                    return BadRequest(new { message = mensaje });

                return Ok(new { message = mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("codigo/{codigoGrupo}")]
        public async Task<ActionResult<GrupoDetalleDto>> GetPorCodigo(string codigoGrupo)
        {
            try
            {
                var grupo = await _grupoService.GetGrupoPorCodigoAsync(codigoGrupo);

                if (grupo == null)
                    return NotFound($"No se encontró el grupo con código {codigoGrupo}");

                var grupoDto = _mapper.Map<GrupoDetalleDto>(grupo);
                return Ok(grupoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("buscar")]
        public async Task<ActionResult<List<GrupoDto>>> BuscarPorCriterios(
            [FromQuery] int? numeroCuatrimestre = null,
            [FromQuery] int? idTurno = null,
            [FromQuery] int? numeroGrupo = null,
            [FromQuery] int? idPlanEstudios = null)
        {
            try
            {
                var grupos = await _grupoService.BuscarGruposPorCriteriosAsync(
                    numeroCuatrimestre, idTurno, numeroGrupo, idPlanEstudios);

                var gruposDto = _mapper.Map<List<GrupoDto>>(grupos);
                return Ok(gruposDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("{idGrupo:int}/inscribir-estudiante")]
        public async Task<ActionResult<InscripcionGrupoResultDto>> InscribirEstudiante(
            int idGrupo,
            [FromBody] InscribirEstudianteGrupoRequest request)
        {
            try
            {
                var resultado = await _grupoService.InscribirEstudianteGrupoAsync(
                    idGrupo,
                    request.IdEstudiante,
                    request.ForzarInscripcion,
                    request.Observaciones);

                return Ok(resultado);
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


        [HttpGet("{idGrupo:int}/estudiantes")]
        public async Task<ActionResult<EstudiantesGrupoDto>> GetEstudiantesDelGrupo(int idGrupo)
        {
            try
            {
                var resultado = await _grupoService.GetEstudiantesDelGrupoAsync(idGrupo);
                return Ok(resultado);
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


        [HttpGet("gruposmaterias/disponibles")]
        public async Task<ActionResult<List<GrupoMateriaDisponibleDto>>> GetGruposMateriasDisponibles(
            [FromQuery] int? idEstudiante = null,
            [FromQuery] int? idPeriodoAcademico = null)
        {
            try
            {
                var gruposMaterias = await _grupoService.GetGruposMateriasDisponiblesAsync(idEstudiante, idPeriodoAcademico);
                return Ok(gruposMaterias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }


        [HttpGet("gruposmaterias/{idGrupoMateria:int}/estudiantes")]
        public async Task<ActionResult<List<EstudianteInscritoDto>>> GetEstudiantesPorGrupoMateria(int idGrupoMateria)
        {
            try
            {
                var estudiantes = await _grupoService.GetEstudiantesPorGrupoMateriaAsync(idGrupoMateria);
                return Ok(estudiantes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }


        [HttpGet("plan/{idPlanEstudios}")]
        public async Task<ActionResult<GestionGruposPlanDto>> GetGruposPorPlan(
            int idPlanEstudios,
            [FromQuery] int? idPeriodoAcademico = null,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _grupoService.ObtenerGruposPorPlanAsync(idPlanEstudios, idPeriodoAcademico, ct);

                if (resultado == null)
                    return NotFound(new { mensaje = "Plan de estudios no encontrado" });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }


        [HttpPost("con-materias")]
        public async Task<ActionResult<GrupoResumenDto>> CrearGrupoConMaterias(
            [FromBody] CrearGrupoAcademicoRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _grupoService.CrearGrupoConMateriasAsync(request, ct);
                return CreatedAtAction(nameof(GetDetalle), new { idGrupo = resultado.IdGrupo }, resultado);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Error = ex.Message });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                return StatusCode(500, new { Error = "Error al guardar en la base de datos", Detalle = innerMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, Detalle = ex.InnerException?.Message });
            }
        }


        [HttpPost("{idGrupo:int}/materias")]
        public async Task<ActionResult<GrupoMateria>> AgregarMateriaAlGrupo(
            int idGrupo,
            [FromBody] AgregarMateriaGrupoRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var grupoMateria = await _grupoService.AgregarMateriaAlGrupoAsync(
                    idGrupo,
                    request.IdMateriaPlan,
                    request.IdProfesor,
                    request.Aula,
                    request.Cupo,
                    ct);

                return Ok(grupoMateria);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
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


        [HttpDelete("materias/{idGrupoMateria:int}")]
        public async Task<ActionResult> QuitarMateriaDelGrupo(int idGrupoMateria, CancellationToken ct = default)
        {
            try
            {
                var resultado = await _grupoService.QuitarMateriaDelGrupoAsync(idGrupoMateria, ct);

                if (!resultado)
                    return NotFound(new { mensaje = "Materia no encontrada en el grupo" });

                return Ok(new { mensaje = "Materia quitada del grupo correctamente" });
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


        [HttpGet("{idGrupo:int}/materias")]
        public async Task<ActionResult<List<GrupoMateriaDetalleDto>>> GetMateriasDelGrupo(
            int idGrupo,
            CancellationToken ct = default)
        {
            try
            {
                var materias = await _grupoService.ObtenerMateriasDelGrupoAsync(idGrupo, ct);
                return Ok(materias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }


        [HttpGet("materias/{idGrupoMateria:int}")]
        public async Task<ActionResult<GrupoMateriaDto>> GetGrupoMateriaById(
            int idGrupoMateria,
            CancellationToken ct = default)
        {
            try
            {
                var grupoMateria = await _grupoService.ObtenerGrupoMateriaPorIdAsync(idGrupoMateria, ct);

                if (grupoMateria == null)
                    return NotFound(new { Error = $"No se encontró el grupo-materia con ID {idGrupoMateria}" });

                var grupoMateriaDto = _mapper.Map<GrupoMateriaDto>(grupoMateria);
                return Ok(grupoMateriaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }


        [HttpPost("promocion")]
        public async Task<ActionResult<PromocionAutomaticaResultDto>> PromoverEstudiantes(
            [FromBody] PromoverEstudiantesRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _grupoService.PromoverEstudiantesAsync(request, ct);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
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

        [HttpPost("promocion/preview")]
        public async Task<ActionResult<PreviewPromocionResultDto>> PreviewPromocion(
            [FromBody] PreviewPromocionRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _grupoService.PreviewPromocionAsync(request, ct);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
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

        [HttpGet("validar-promocion/{idEstudiante:int}")]
        public async Task<ActionResult> ValidarPromocionEstudiante(
            int idEstudiante,
            [FromQuery] int cuatrimestreActual,
            [FromQuery] decimal promedioMinimo = 70,
            CancellationToken ct = default)
        {
            try
            {
                var (puedePromover, motivo) = await _grupoService.ValidarPromocionEstudianteAsync(
                    idEstudiante,
                    cuatrimestreActual,
                    promedioMinimo,
                    ct);

                return Ok(new
                {
                    idEstudiante,
                    puedePromover,
                    motivo,
                    promedioMinimoRequerido = promedioMinimo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }


        [HttpPut("materias/{idGrupoMateria:int}/horarios")]
        public async Task<ActionResult> ActualizarHorarios(
            int idGrupoMateria,
            [FromBody] ActualizarHorariosRequest request,
            CancellationToken ct = default)
        {
            try
            {
                await _grupoService.ActualizarHorariosGrupoMateriaAsync(idGrupoMateria, request.HorarioJson, ct);
                return Ok(new { mensaje = "Horarios actualizados correctamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar horarios", error = ex.Message });
            }
        }


        [HttpPut("materias/{idGrupoMateria:int}/profesor")]
        public async Task<ActionResult<GrupoMateriaDetalleDto>> AsignarProfesor(
            int idGrupoMateria,
            [FromBody] AsignarProfesorRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var grupoMateria = await _grupoService.AsignarProfesorAMateriaAsync(idGrupoMateria, request.IdProfesor, ct);

                if (grupoMateria == null)
                    return NotFound(new { mensaje = "Materia no encontrada" });

                var grupoMateriaDto = new GrupoMateriaDetalleDto
                {
                    IdGrupoMateria = grupoMateria.IdGrupoMateria,
                    IdMateriaPlan = grupoMateria.IdMateriaPlan,
                    NombreMateria = grupoMateria.IdMateriaPlanNavigation?.IdMateriaNavigation?.Nombre ?? "",
                    ClaveMateria = grupoMateria.IdMateriaPlanNavigation?.IdMateriaNavigation?.Clave ?? "",
                    Creditos = (int)(grupoMateria.IdMateriaPlanNavigation?.IdMateriaNavigation?.Creditos ?? 0),
                    IdProfesor = grupoMateria.IdProfesor,
                    NombreProfesor = grupoMateria.IdProfesorNavigation != null
                        ? $"{grupoMateria.IdProfesorNavigation.IdPersonaNavigation?.Nombre} {grupoMateria.IdProfesorNavigation.IdPersonaNavigation?.ApellidoPaterno}".Trim()
                        : null,
                    Aula = grupoMateria.Aula,
                    Cupo = grupoMateria.Cupo,
                    EstudiantesInscritos = grupoMateria.Inscripcion?.Count(i => i.Status == Core.Enums.StatusEnum.Active) ?? 0,
                    CupoDisponible = grupoMateria.Cupo - (grupoMateria.Inscripcion?.Count(i => i.Status == Core.Enums.StatusEnum.Active) ?? 0),
                    TieneCupo = (grupoMateria.Inscripcion?.Count(i => i.Status == Core.Enums.StatusEnum.Active) ?? 0) < grupoMateria.Cupo,
                    NombreGrupoMateria = grupoMateria.Name ?? "",
                    HorarioJson = grupoMateria.Horario?.Select(h => new HorarioItemDto
                    {
                        Dia = h.IdDiaSemanaNavigation?.Nombre ?? "",
                        HoraInicio = h.HoraInicio.ToString("HH:mm"),
                        HoraFin = h.HoraFin.ToString("HH:mm"),
                        Aula = h.Aula
                    }).ToList()
                };

                return Ok(grupoMateriaDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al asignar profesor", error = ex.Message });
            }
        }

        [HttpPost("{idGrupo:int}/inscribir-directo")]
        public async Task<ActionResult<EstudianteGrupoResultDto>> InscribirEstudianteDirecto(
            int idGrupo,
            [FromBody] InscribirEstudianteGrupoDirectoRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _grupoService.InscribirEstudianteAGrupoDirectoAsync(
                    idGrupo,
                    request.IdEstudiante,
                    request.Observaciones,
                    ct);

                if (!resultado.Exitoso)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("{idGrupo:int}/inscribir-masivo")]
        public async Task<ActionResult<InscribirEstudiantesGrupoResponse>> InscribirEstudiantesMasivo(
            int idGrupo,
            [FromBody] InscribirEstudiantesMasivoRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var serviceRequest = new InscribirEstudiantesGrupoRequest
                {
                    IdGrupo = idGrupo,
                    IdsEstudiantes = request.IdsEstudiantes,
                    Observaciones = request.Observaciones
                };

                var resultado = await _grupoService.InscribirEstudiantesAGrupoMasivoAsync(serviceRequest, ct);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{idGrupo:int}/estudiantes-directo")]
        public async Task<ActionResult<EstudiantesDelGrupoResponse>> GetEstudiantesDelGrupoDirecto(
            int idGrupo,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _grupoService.GetEstudiantesDelGrupoDirectoAsync(idGrupo, ct);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpDelete("estudiante-grupo/{idEstudianteGrupo:int}")]
        public async Task<ActionResult> EliminarEstudianteDeGrupo(
            int idEstudianteGrupo,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _grupoService.EliminarEstudianteDeGrupoAsync(idEstudianteGrupo, ct);

                if (!resultado)
                    return NotFound(new { mensaje = "Inscripción no encontrada" });

                return Ok(new { mensaje = "Estudiante eliminado del grupo correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("{idGrupo:int}/importar-estudiantes")]
        public async Task<ActionResult<ImportarEstudiantesGrupoResponse>> ImportarEstudiantes(
            int idGrupo,
            [FromBody] ImportarEstudiantesGrupoRequest request,
            CancellationToken ct = default)
        {
            try
            {
                request.IdGrupo = idGrupo;
                var resultado = await _grupoService.ImportarEstudiantesCompletoAsync(request, ct);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
