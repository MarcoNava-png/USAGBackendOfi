using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Grupo;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Asistencia;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.COORDINADOR},{Rol.DIRECTOR},{Rol.DOCENTE},{Rol.CONTROL_ESCOLAR}")]
    public class AsistenciaController : ControllerBase
    {
        private readonly IAsistenciaService _asistenciaService;
        private readonly IMapper _mapper;

        public AsistenciaController(IAsistenciaService asistenciaService, IMapper mapper)
        {
            _asistenciaService = asistenciaService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<AsistenciaDto>> RegistrarAsistencia([FromBody] AsistenciaRegistroRequest request)
        {
            try
            {
                var asistencia = _mapper.Map<Asistencia>(request);

                int profesorId = 1; 

                var asistenciaCreada = await _asistenciaService.RegistrarAsistencia(asistencia, profesorId);

                asistenciaCreada = await _asistenciaService.GetAsistenciaById(asistenciaCreada.IdAsistencia);

                var dto = _mapper.Map<AsistenciaDto>(asistenciaCreada);
                return CreatedAtAction(nameof(GetAsistenciaById), new { id = asistenciaCreada.IdAsistencia }, dto);
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


        [HttpPost("masiva")]
        public async Task<ActionResult<object>> RegistrarAsistenciaMasiva([FromBody] AsistenciaMasivaRequest request)
        {
            try
            {
                var asistencias = _mapper.Map<List<Asistencia>>(request.Asistencias);

                int profesorId = 1;

                var asistenciasCreadas = await _asistenciaService.RegistrarAsistenciaMasiva(asistencias, profesorId);

                return Ok(new
                {
                    message = $"Se registraron {asistenciasCreadas.Count} asistencias correctamente",
                    totalRegistradas = asistenciasCreadas.Count
                });
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

        [HttpPut("{id:int}")]
        public async Task<ActionResult<AsistenciaDto>> ActualizarAsistencia(int id, [FromBody] AsistenciaUpdateRequest request)
        {
            try
            {
                if (id != request.IdAsistencia)
                    return BadRequest("El ID de la URL no coincide con el ID del body");

                var asistencia = new Asistencia
                {
                    IdAsistencia = request.IdAsistencia,
                    EstadoAsistencia = request.EstadoAsistencia,
                    Observaciones = request.Observaciones
                };

                var usuario = User?.Identity?.Name ?? "sistema";
                var asistenciaActualizada = await _asistenciaService.ActualizarAsistencia(asistencia, usuario);

                asistenciaActualizada = await _asistenciaService.GetAsistenciaById(asistenciaActualizada.IdAsistencia);

                var dto = _mapper.Map<AsistenciaDto>(asistenciaActualizada);
                return Ok(dto);
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
        public async Task<ActionResult<AsistenciaDto>> GetAsistenciaById(int id)
        {
            try
            {
                var asistencia = await _asistenciaService.GetAsistenciaById(id);

                if (asistencia == null)
                    return NotFound($"No se encontró la asistencia con ID {id}");

                var dto = _mapper.Map<AsistenciaDto>(asistencia);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("grupo-materia/{grupoMateriaId:int}")]
        public async Task<ActionResult<object>> GetAsistenciasPorGrupoMateria(
            int grupoMateriaId,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var asistencias = await _asistenciaService.GetAsistenciasPorGrupoMateria(grupoMateriaId, fechaInicio, fechaFin);
                var dtos = _mapper.Map<List<AsistenciaDto>>(asistencias);

                return Ok(new
                {
                    grupoMateriaId = grupoMateriaId,
                    fechaInicio = fechaInicio,
                    fechaFin = fechaFin,
                    total = dtos.Count,
                    asistencias = dtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("inscripcion/{inscripcionId:int}")]
        public async Task<ActionResult<object>> GetAsistenciasPorInscripcion(int inscripcionId)
        {
            try
            {
                var asistencias = await _asistenciaService.GetAsistenciasPorInscripcion(inscripcionId);
                var dtos = _mapper.Map<List<AsistenciaDto>>(asistencias);

                return Ok(new
                {
                    inscripcionId = inscripcionId,
                    total = dtos.Count,
                    asistencias = dtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetAsistenciasConFiltros(
            [FromQuery] int? grupoMateriaId = null,
            [FromQuery] int? inscripcionId = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? estadoAsistencia = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var resultado = await _asistenciaService.GetAsistenciasConFiltros(
                    grupoMateriaId, inscripcionId, fechaInicio, fechaFin, estadoAsistencia, page, pageSize);

                var dtos = _mapper.Map<List<AsistenciaDto>>(resultado.Items);

                return Ok(new
                {
                    items = dtos,
                    totalItems = resultado.TotalItems,
                    pageNumber = resultado.PageNumber,
                    pageSize = resultado.PageSize,
                    totalPages = (int)Math.Ceiling(resultado.TotalItems / (double)resultado.PageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("reporte/estudiante/{inscripcionId:int}")]
        public async Task<ActionResult<ReporteAsistenciaEstudianteDto>> GetReporteAsistenciaEstudiante(int inscripcionId)
        {
            try
            {
                var asistencias = await _asistenciaService.GetAsistenciasPorInscripcion(inscripcionId);
                var estadisticas = await _asistenciaService.GetEstadisticasAsistencia(inscripcionId);

                var asistenciasDto = _mapper.Map<List<AsistenciaDto>>(asistencias);

                var reporte = new ReporteAsistenciaEstudianteDto
                {
                    InscripcionId = inscripcionId,
                    NombreEstudiante = asistencias.FirstOrDefault()?.Inscripcion?.IdEstudianteNavigation?.IdPersonaNavigation?.Nombre ?? "N/A",
                    Matricula = asistencias.FirstOrDefault()?.Inscripcion?.IdEstudianteNavigation?.Matricula ?? "N/A",
                    NombreMateria = asistencias.FirstOrDefault()?.GrupoMateria?.IdMateriaPlanNavigation?.IdMateriaNavigation?.Nombre ?? "N/A",
                    TotalSesiones = estadisticas.TotalSesiones,
                    TotalPresentes = estadisticas.Presentes,
                    TotalAusentes = estadisticas.Ausentes,
                    TotalRetardos = estadisticas.Retardos,
                    TotalJustificadas = estadisticas.Justificadas,
                    PorcentajeAsistencia = estadisticas.PorcentajeAsistencia,
                    Asistencias = asistenciasDto
                };

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("estadisticas/inscripcion/{inscripcionId:int}")]
        public async Task<ActionResult<object>> GetEstadisticasAsistencia(int inscripcionId)
        {
            try
            {
                var estadisticas = await _asistenciaService.GetEstadisticasAsistencia(inscripcionId);

                return Ok(new
                {
                    inscripcionId = inscripcionId,
                    totalSesiones = estadisticas.TotalSesiones,
                    presentes = estadisticas.Presentes,
                    ausentes = estadisticas.Ausentes,
                    retardos = estadisticas.Retardos,
                    justificadas = estadisticas.Justificadas,
                    porcentajeAsistencia = estadisticas.PorcentajeAsistencia
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("registrar")]
        public async Task<ActionResult> RegistrarAsistencias([FromBody] RegistrarAsistenciasRequest request)
        {
            try
            {
                if (request.Fecha > DateTime.Now.Date)
                {
                    return BadRequest(new { Error = "No se puede registrar asistencia para fechas futuras" });
                }

                int profesorId = 1; 

                var resultado = await _asistenciaService.RegistrarAsistenciasPorFecha(
                    request.IdGrupoMateria,
                    request.Fecha,
                    request.Asistencias,
                    profesorId
                );

                return Ok(new
                {
                    mensaje = $"Se registraron {resultado.Count} asistencias correctamente",
                    totalRegistradas = resultado.Count,
                    fecha = request.Fecha.ToString("yyyy-MM-dd")
                });
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

        [HttpGet("grupo-materia/{idGrupoMateria:int}/fecha/{fecha}")]
        public async Task<ActionResult<List<AsistenciaEstudianteDto>>> GetAsistenciasPorFecha(
            int idGrupoMateria,
            string fecha)
        {
            try
            {
                if (!DateTime.TryParse(fecha, out DateTime fechaParsed))
                {
                    return BadRequest(new { Error = "Formato de fecha inválido. Use formato YYYY-MM-DD" });
                }

                var asistencias = await _asistenciaService.GetAsistenciasPorGrupoMateriaYFecha(
                    idGrupoMateria,
                    fechaParsed
                );

                return Ok(asistencias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("grupo-materia/{idGrupoMateria:int}/dias-clase")]
        public async Task<ActionResult<DiasClaseMateriaDto>> GetDiasClaseMateria(int idGrupoMateria)
        {
            try
            {
                var diasClase = await _asistenciaService.GetDiasClaseMateria(idGrupoMateria);

                if (diasClase == null)
                {
                    return NotFound(new { Error = $"No se encontró el grupo-materia con ID {idGrupoMateria}" });
                }

                return Ok(diasClase);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("grupo-materia/{idGrupoMateria:int}/resumen")]
        public async Task<ActionResult<List<ResumenAsistenciasDto>>> GetResumenAsistencias(int idGrupoMateria)
        {
            try
            {
                var resumen = await _asistenciaService.GetResumenAsistenciasPorGrupoMateria(idGrupoMateria);
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
