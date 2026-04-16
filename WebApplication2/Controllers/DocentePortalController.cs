using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.DocentePortal;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Asistencia;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers;

[Route("api/docente-portal")]
[ApiController]
[Authorize(Roles = Rol.ROLES_PORTAL_DOCENTE)]
public class DocentePortalController : ControllerBase
{
    private readonly IProfesorService _profesorService;
    private readonly IAsistenciaService _asistenciaService;
    private readonly ICalificacionesService _calificacionesService;
    private readonly IPlaneacionDocenteService _planeacionService;
    private readonly ITareaDocenteService _tareaService;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;

    public DocentePortalController(
        IProfesorService profesorService,
        IAsistenciaService asistenciaService,
        ICalificacionesService calificacionesService,
        IPlaneacionDocenteService planeacionService,
        ITareaDocenteService tareaService,
        IMapper mapper,
        ApplicationDbContext dbContext)
    {
        _profesorService = profesorService;
        _asistenciaService = asistenciaService;
        _calificacionesService = calificacionesService;
        _planeacionService = planeacionService;
        _tareaService = tareaService;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    // ──────────────── PERFIL ────────────────

    [HttpGet("perfil")]
    public async Task<IActionResult> GetPerfil(CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "No se encontró un perfil de profesor para este usuario" });

        var totalGrupos = await _dbContext.GrupoMateria
            .CountAsync(gm => gm.IdProfesor == profesor.IdProfesor && gm.Status == StatusEnum.Active, ct);

        var totalEstudiantes = await _dbContext.GrupoMateria
            .Where(gm => gm.IdProfesor == profesor.IdProfesor && gm.Status == StatusEnum.Active)
            .SelectMany(gm => gm.Inscripcion)
            .Select(i => i.IdEstudiante)
            .Distinct()
            .CountAsync(ct);

        var persona = profesor.IdPersonaNavigation;
        return Ok(new DocentePerfilDto
        {
            IdProfesor = profesor.IdProfesor,
            NoEmpleado = profesor.NoEmpleado,
            NombreCompleto = $"{persona?.Nombre} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim(),
            Nombre = persona?.Nombre,
            ApellidoPaterno = persona?.ApellidoPaterno,
            ApellidoMaterno = persona?.ApellidoMaterno,
            EmailInstitucional = profesor.EmailInstitucional,
            Correo = persona?.Correo,
            Telefono = persona?.Celular ?? persona?.Telefono,
            Curp = persona?.Curp,
            FechaNacimiento = persona?.FechaNacimiento,
            CampusNombre = profesor.Campus?.Nombre,
            TotalGrupos = totalGrupos,
            TotalEstudiantes = totalEstudiantes
        });
    }

    [HttpPut("perfil")]
    public async Task<IActionResult> UpdatePerfil([FromBody] DocentePerfilUpdateRequest request, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var persona = await _dbContext.Persona.FindAsync(new object[] { profesor.IdPersona }, ct);
        if (persona == null) return NotFound(new { message = "Persona no encontrada" });

        if (request.Telefono != null) persona.Celular = request.Telefono;
        if (request.Correo != null) persona.Correo = request.Correo;

        await _dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    // ──────────────── MIS GRUPOS ────────────────

    [HttpGet("mis-grupos")]
    public async Task<IActionResult> GetMisGrupos(CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var grupos = await _dbContext.GrupoMateria
            .Include(gm => gm.IdMateriaPlanNavigation).ThenInclude(mp => mp.IdMateriaNavigation)
            .Include(gm => gm.IdGrupoNavigation).ThenInclude(g => g.IdPeriodoAcademicoNavigation)
            .Include(gm => gm.IdGrupoNavigation).ThenInclude(g => g.IdPlanEstudiosNavigation)
            .Include(gm => gm.Horario).ThenInclude(h => h.IdDiaSemanaNavigation)
            .Include(gm => gm.Inscripcion)
            .Where(gm => gm.IdProfesor == profesor.IdProfesor && gm.Status == StatusEnum.Active)
            .OrderByDescending(gm => gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation.FechaInicio)
            .ThenBy(gm => gm.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre)
            .ToListAsync(ct);

        var result = grupos.Select(gm => new GrupoMateriaDocenteDto
        {
            IdGrupoMateria = gm.IdGrupoMateria,
            NombreMateria = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
            ClaveMateria = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Clave,
            CodigoGrupo = gm.IdGrupoNavigation.CodigoGrupo ?? gm.IdGrupoNavigation.NombreGrupo,
            NombreGrupo = gm.IdGrupoNavigation.NombreGrupo,
            PlanEstudios = gm.IdGrupoNavigation.IdPlanEstudiosNavigation?.NombrePlanEstudios
                           ?? gm.IdGrupoNavigation.IdPlanEstudiosNavigation?.ClavePlanEstudios,
            NumeroCuatrimestre = gm.IdGrupoNavigation.NumeroCuatrimestre,
            Aula = gm.Aula,
            TotalInscritos = gm.Inscripcion.Count,
            Cupo = gm.Cupo,
            PeriodoNombre = gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation?.Nombre,
            Horarios = gm.Horario.Select(h => new HorarioItemDto
            {
                Dia = h.IdDiaSemanaNavigation?.Nombre ?? "N/A",
                HoraInicio = h.HoraInicio.ToString("HH:mm"),
                HoraFin = h.HoraFin.ToString("HH:mm"),
                Aula = h.Aula ?? gm.Aula
            }).OrderBy(h => h.Dia).ToList()
        }).ToList();

        return Ok(result);
    }

    [HttpGet("mis-grupos/{idGrupoMateria:int}")]
    public async Task<IActionResult> GetGrupoDetalle(int idGrupoMateria, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var gm = await _dbContext.GrupoMateria
            .Include(g => g.IdMateriaPlanNavigation).ThenInclude(mp => mp.IdMateriaNavigation)
            .Include(g => g.IdGrupoNavigation).ThenInclude(g => g.IdPeriodoAcademicoNavigation)
            .Include(g => g.IdGrupoNavigation).ThenInclude(g => g.IdPlanEstudiosNavigation)
            .Include(g => g.Horario).ThenInclude(h => h.IdDiaSemanaNavigation)
            .Include(g => g.Inscripcion).ThenInclude(i => i.IdEstudianteNavigation).ThenInclude(e => e.IdPersonaNavigation)
            .FirstOrDefaultAsync(g => g.IdGrupoMateria == idGrupoMateria
                                   && g.IdProfesor == profesor.IdProfesor
                                   && g.Status == StatusEnum.Active, ct);

        if (gm == null) return NotFound(new { message = "Grupo-materia no encontrado o no te pertenece" });

        return Ok(new GrupoMateriaDocenteDto
        {
            IdGrupoMateria = gm.IdGrupoMateria,
            NombreMateria = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
            ClaveMateria = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Clave,
            CodigoGrupo = gm.IdGrupoNavigation.CodigoGrupo ?? gm.IdGrupoNavigation.NombreGrupo,
            NombreGrupo = gm.IdGrupoNavigation.NombreGrupo,
            PlanEstudios = gm.IdGrupoNavigation.IdPlanEstudiosNavigation?.NombrePlanEstudios,
            NumeroCuatrimestre = gm.IdGrupoNavigation.NumeroCuatrimestre,
            Aula = gm.Aula,
            TotalInscritos = gm.Inscripcion.Count,
            Cupo = gm.Cupo,
            PeriodoNombre = gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation?.Nombre,
            Horarios = gm.Horario.Select(h => new HorarioItemDto
            {
                Dia = h.IdDiaSemanaNavigation?.Nombre ?? "N/A",
                HoraInicio = h.HoraInicio.ToString("HH:mm"),
                HoraFin = h.HoraFin.ToString("HH:mm"),
                Aula = h.Aula ?? gm.Aula
            }).ToList()
        });
    }

    // ──────────────── ASISTENCIA ────────────────

    [HttpGet("asistencia/{idGrupoMateria:int}/fecha/{fecha}")]
    public async Task<IActionResult> GetAsistenciaPorFecha(int idGrupoMateria, string fecha, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, idGrupoMateria, ct);
        if (!ownership) return Forbid();

        if (!DateTime.TryParse(fecha, out var fechaParsed))
            return BadRequest(new { message = "Formato de fecha inválido. Use yyyy-MM-dd" });

        var result = await _asistenciaService.GetAsistenciasPorGrupoMateriaYFecha(idGrupoMateria, fechaParsed);
        return Ok(result);
    }

    [HttpPost("asistencia/registrar")]
    public async Task<IActionResult> RegistrarAsistencia([FromBody] RegistrarAsistenciasRequest request, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, request.IdGrupoMateria, ct);
        if (!ownership) return Forbid();

        if (request.Fecha > DateTime.Now.Date)
            return BadRequest(new { message = "No se puede registrar asistencia para fechas futuras" });

        var resultado = await _asistenciaService.RegistrarAsistenciasPorFecha(
            request.IdGrupoMateria,
            request.Fecha,
            request.Asistencias,
            profesor.IdProfesor
        );

        return Ok(new
        {
            mensaje = $"Se registraron {resultado.Count} asistencias correctamente",
            totalRegistradas = resultado.Count,
            fecha = request.Fecha.ToString("yyyy-MM-dd")
        });
    }

    [HttpGet("asistencia/{idGrupoMateria:int}/resumen")]
    public async Task<IActionResult> GetResumenAsistencia(int idGrupoMateria, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, idGrupoMateria, ct);
        if (!ownership) return Forbid();

        var result = await _asistenciaService.GetResumenAsistenciasPorGrupoMateria(idGrupoMateria);
        return Ok(result);
    }

    // ──────────────── CALIFICACIONES ────────────────

    [HttpGet("calificaciones/{grupoMateriaId:int}/parciales")]
    public async Task<IActionResult> GetParciales(int grupoMateriaId, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, grupoMateriaId, ct);
        if (!ownership) return Forbid();

        // Return status of parciales 1, 2, 3
        var result = new List<object>();
        for (int parcialId = 1; parcialId <= 3; parcialId++)
        {
            var parciales = await _calificacionesService.GetParcialesPorGrupo(grupoMateriaId, parcialId);
            var mapped = _mapper.Map<IEnumerable<CalificacionParcialResponse>>(parciales);
            var first = mapped.FirstOrDefault();
            result.Add(new
            {
                parcialId,
                nombre = $"Parcial {parcialId}",
                status = first?.StatusParcial ?? "Sin abrir",
                totalActas = mapped.Count(),
                calificacionParcialId = first?.Id
            });
        }
        return Ok(result);
    }

    [HttpPost("calificaciones/parciales")]
    public async Task<IActionResult> CrearParcial([FromBody] CalificacionParcialCreateRequest req, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, req.GrupoMateriaId, ct);
        if (!ownership) return Forbid();

        req.ProfesorId = profesor.IdProfesor;

        var acta = _mapper.Map<CalificacionParcial>(req);
        acta.StatusParcial = StatusParcialEnum.Abierto;
        acta.FechaApertura = req.FechaApertura ?? DateTime.UtcNow;

        if (acta.InscripcionId <= 0)
        {
            var primeraInscripcion = await _dbContext.Inscripcion
                .Where(i => i.IdGrupoMateria == req.GrupoMateriaId && i.Status == StatusEnum.Active)
                .Select(i => i.IdInscripcion)
                .FirstOrDefaultAsync();

            if (primeraInscripcion <= 0)
                return BadRequest(new { message = "No hay estudiantes inscritos en esta materia." });

            acta.InscripcionId = primeraInscripcion;
        }

        var creado = await _calificacionesService.AbrirParcial(acta);
        var dto = _mapper.Map<CalificacionParcialResponse>(creado);
        return Ok(dto);
    }

    [HttpPatch("calificaciones/parciales/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoParcialDocente(int id, [FromBody] CalificacionParcialEstadoRequest req, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, req.GrupoMateriaId, ct);
        if (!ownership) return Forbid();

        await _calificacionesService.CambiarEstadoParcial(id, req.StatusParcial.ToString(), User?.Identity?.Name ?? "sistema");
        return NoContent();
    }

    [HttpPost("calificaciones/detalle")]
    public async Task<IActionResult> UpsertDetalleDocente([FromBody] CalificacionDetalleUpsertRequest req, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, req.GrupoMateriaId, ct);
        if (!ownership) return Forbid();

        var entity = _mapper.Map<CalificacionDetalle>(req);
        var username = User?.Identity?.Name ?? "sistema";
        await _calificacionesService.UpsertDetalle(entity, username);

        var dto = _mapper.Map<CalificacionDetalleItemResponse>(entity);
        return Ok(dto);
    }

    [HttpGet("calificaciones/concentrado/{grupoMateriaId:int}/{parcialId:int}")]
    public async Task<IActionResult> GetConcentradoDocente(int grupoMateriaId, int parcialId, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, grupoMateriaId, ct);
        if (!ownership) return Forbid();

        var resultado = await _calificacionesService.GetConcentradoGrupoParcial(grupoMateriaId, parcialId);
        return Ok(new
        {
            grupoMateriaId,
            parcialId,
            calificaciones = resultado.Select(r => new
            {
                inscripcionId = r.InscripcionId,
                aporteParcial = r.AporteParcial
            })
        });
    }

    [HttpGet("calificaciones/detalles")]
    public async Task<IActionResult> GetDetallesDocente(
        [FromQuery] int grupoMateriaId,
        [FromQuery] int parcialId = 0,
        [FromQuery] int inscripcionId = 0,
        [FromQuery] int tipoEvaluacionEnum = -1,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken ct = default)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, grupoMateriaId, ct);
        if (!ownership) return Forbid();

        var resultado = await _calificacionesService.GetDetalles(
            grupoMateriaId, parcialId, inscripcionId, tipoEvaluacionEnum, page, pageSize);
        var itemsDto = _mapper.Map<List<CalificacionDetalleItemResponse>>(resultado.Items);

        return Ok(new
        {
            items = itemsDto,
            totalItems = resultado.TotalItems,
            pageNumber = resultado.PageNumber,
            pageSize = resultado.PageSize
        });
    }

    [HttpGet("calificaciones/parciales/{calificacionParcialId:int}/validar-pesos")]
    public async Task<IActionResult> ValidarPesosDocente(int calificacionParcialId, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var resultado = await _calificacionesService.ValidarPesosEvaluacion(calificacionParcialId);
        return Ok(new
        {
            esValido = resultado.EsValido,
            sumaPesos = resultado.SumaPesos,
            mensaje = resultado.Mensaje
        });
    }

    // ──────────────── PLANEACIONES ────────────────

    [HttpGet("planeaciones/{idGrupoMateria:int}")]
    public async Task<IActionResult> GetPlaneaciones(int idGrupoMateria, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, idGrupoMateria, ct);
        if (!ownership) return Forbid();

        var result = await _planeacionService.GetByGrupoMateria(idGrupoMateria, profesor.IdProfesor);
        return Ok(result);
    }

    [HttpPost("planeaciones")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> UploadPlaneacion(
        [FromForm] int idGrupoMateria,
        [FromForm] string? descripcion,
        IFormFile archivo,
        CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, idGrupoMateria, ct);
        if (!ownership) return Forbid();

        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { message = "Archivo requerido" });

        var entity = await _planeacionService.Upload(profesor.IdProfesor, idGrupoMateria, archivo, descripcion);
        return Ok(new PlaneacionDocenteDto
        {
            Id = entity.Id,
            IdGrupoMateria = entity.IdGrupoMateria,
            NombreArchivo = entity.NombreArchivo,
            UrlArchivo = entity.UrlArchivo,
            Descripcion = entity.Descripcion,
            TipoArchivo = entity.TipoArchivo,
            TamanoBytes = entity.TamanoBytes,
            FechaSubida = entity.FechaSubida
        });
    }

    [HttpDelete("planeaciones/{id:int}")]
    public async Task<IActionResult> DeletePlaneacion(int id, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        await _planeacionService.Delete(id, profesor.IdProfesor);
        return NoContent();
    }

    // ──────────────── TAREAS ────────────────

    [HttpGet("tareas/{idGrupoMateria:int}")]
    public async Task<IActionResult> GetTareas(int idGrupoMateria, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, idGrupoMateria, ct);
        if (!ownership) return Forbid();

        var result = await _tareaService.GetTareasByGrupoMateria(idGrupoMateria, profesor.IdProfesor);
        return Ok(result);
    }

    [HttpPost("tareas")]
    public async Task<IActionResult> CrearTarea([FromBody] CrearTareaRequest request, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var ownership = await ValidateOwnership(profesor.IdProfesor, request.IdGrupoMateria, ct);
        if (!ownership) return Forbid();

        var tarea = new Core.Models.TareaDocente
        {
            IdGrupoMateria = request.IdGrupoMateria,
            IdProfesor = profesor.IdProfesor,
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            FechaLimite = request.FechaLimite,
            PuntosMaximos = request.PuntosMaximos,
            Activa = true
        };

        var created = await _tareaService.CrearTarea(tarea);
        return Ok(new { id = created.Id, mensaje = "Tarea creada correctamente" });
    }

    [HttpPut("tareas/{id:int}")]
    public async Task<IActionResult> ActualizarTarea(int id, [FromBody] CrearTareaRequest request, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        await _tareaService.ActualizarTarea(id, profesor.IdProfesor, request.Titulo, request.Descripcion, request.FechaLimite, request.PuntosMaximos);
        return NoContent();
    }

    [HttpDelete("tareas/{id:int}")]
    public async Task<IActionResult> EliminarTarea(int id, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        await _tareaService.EliminarTarea(id, profesor.IdProfesor);
        return NoContent();
    }

    [HttpGet("tareas/{idTarea:int}/entregas")]
    public async Task<IActionResult> GetEntregas(int idTarea, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        var result = await _tareaService.GetEntregasPorTarea(idTarea, profesor.IdProfesor);
        return Ok(result);
    }

    [HttpPut("tareas/entregas/{idEntrega:int}/calificar")]
    public async Task<IActionResult> CalificarEntrega(int idEntrega, [FromBody] CalificarEntregaRequest request, CancellationToken ct)
    {
        var profesor = await ResolveProfesor(ct);
        if (profesor == null) return NotFound(new { message = "Profesor no encontrado" });

        await _tareaService.CalificarEntrega(idEntrega, profesor.IdProfesor, request.Calificacion, request.Retroalimentacion);
        return NoContent();
    }

    // ──────────────── HELPERS ────────────────

    private async Task<bool> ValidateOwnership(int profesorId, int idGrupoMateria, CancellationToken ct)
    {
        return await _dbContext.GrupoMateria
            .AnyAsync(gm => gm.IdGrupoMateria == idGrupoMateria
                         && gm.IdProfesor == profesorId
                         && gm.Status == StatusEnum.Active, ct);
    }

    private async Task<Core.Models.Profesor?> ResolveProfesor(CancellationToken ct)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId)) return null;
        return await _profesorService.GetProfesorByUsuarioId(userId, ct);
    }
}

public class DocentePerfilUpdateRequest
{
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
}

public class CrearTareaRequest
{
    public int IdGrupoMateria { get; set; }
    public string Titulo { get; set; } = null!;
    public string? Descripcion { get; set; }
    public DateTime FechaLimite { get; set; }
    public decimal PuntosMaximos { get; set; }
}

public class CalificarEntregaRequest
{
    public decimal Calificacion { get; set; }
    public string? Retroalimentacion { get; set; }
}
