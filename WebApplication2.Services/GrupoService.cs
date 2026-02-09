using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.GestionAcademica;
using WebApplication2.Core.DTOs.Grupo;
using WebApplication2.Core.DTOs.Inscripcion;
using WebApplication2.Core.Requests.GestionAcademica;
using WebApplication2.Core.Requests.Grupo;
using WebApplication2.Core.Responses.Grupo;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using System.Linq;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;
using EstadoInscripcionEnum = WebApplication2.Core.Enums.EstadoInscripcionEnum;

namespace WebApplication2.Services
{
    public class GrupoService: IGrupoService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IInscripcionService _inscripcionService;
        private readonly IEstudianteService _estudianteService;
        private readonly IPeriodoAcademicoService _periodoAcademicoService;
        private readonly IMatriculaService _matriculaService;

        public GrupoService(
            ApplicationDbContext dbContext,
            IInscripcionService inscripcionService,
            IEstudianteService estudianteService,
            IPeriodoAcademicoService periodoAcademicoService,
            IMatriculaService matriculaService)
        {
            _dbContext = dbContext;
            _inscripcionService = inscripcionService;
            _estudianteService = estudianteService;
            _periodoAcademicoService = periodoAcademicoService;
            _matriculaService = matriculaService;
        }

        public async Task<PagedResult<Grupo>> GetGrupos(int page, int pageSize, int? idPeriodoAcademico = null)
        {
            var query = _dbContext.Grupo
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .Include(g => g.IdTurnoNavigation)
                .Include(g => g.IdPlanEstudiosNavigation)
                .Include(g => g.GrupoMateria)
                    .ThenInclude(gm => gm.Inscripcion)
                .Where(d => d.Status == Core.Enums.StatusEnum.Active);

            if (idPeriodoAcademico.HasValue)
            {
                query = query.Where(g => g.IdPeriodoAcademico == idPeriodoAcademico.Value);
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Grupo>
            {
                TotalItems = totalItems,
                Items = items,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<Grupo> GetDetalleGrupo(int idGrupo)
        {
            var grupo = await _dbContext.Grupo
                .Include(g => g.GrupoMateria)
                .ThenInclude(gm => gm.IdProfesorNavigation)
                .ThenInclude(pn => pn.IdPersonaNavigation)
                .Include(g => g.GrupoMateria)
                .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                .ThenInclude(mpn => mpn.IdMateriaNavigation)
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .Include(g => g.IdPlanEstudiosNavigation)
                .Include(g => g.IdTurnoNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo);

            if (grupo  == null)
            {
                throw new Exception("No existe grupo con el id ingresado");
            }

            return grupo;
        }

        public async Task<Grupo> CrearGrupo(Grupo grupo)
        {
            grupo.CodigoGrupo = GenerarCodigoGrupo(grupo.NumeroCuatrimestre, grupo.IdTurno, grupo.NumeroGrupo);

            await _dbContext.Grupo.AddAsync(grupo);
            await _dbContext.SaveChangesAsync();

            return grupo;
        }

        public async Task<IEnumerable<GrupoMateria>> CargarMateriasGrupo(IEnumerable<GrupoMateria> grupoMaterias)
        {
            await _dbContext.GrupoMateria.AddRangeAsync(grupoMaterias);
            await _dbContext.SaveChangesAsync();

            return grupoMaterias;
        }

        public async Task<GrupoMateria?> GetGrupoMateriaByNameAsync(string nombreGrupoMateria)
        {
            return await _dbContext.GrupoMateria
                .Include(gm => gm.IdGrupoNavigation)
                .FirstOrDefaultAsync(gm => gm.Name == nombreGrupoMateria);
        }

        public async Task<(bool Exito, string Mensaje)> EliminarGrupoAsync(int idGrupo, CancellationToken ct = default)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                var grupo = await _dbContext.Grupo
                    .Include(g => g.GrupoMateria)
                        .ThenInclude(gm => gm.Inscripcion)
                    .Include(g => g.GrupoMateria)
                        .ThenInclude(gm => gm.Horario)
                    .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo, ct);

                if (grupo == null)
                    return (false, "No se encontró el grupo");

                var tieneEstudiantesInscritos = grupo.GrupoMateria
                    .SelectMany(gm => gm.Inscripcion)
                    .Any(i => i.Status == StatusEnum.Active);

                if (tieneEstudiantesInscritos)
                    return (false, "No se puede eliminar el grupo porque tiene estudiantes inscritos. Primero debe dar de baja a los estudiantes.");

                foreach (var grupoMateria in grupo.GrupoMateria)
                {
                    if (grupoMateria.Horario != null && grupoMateria.Horario.Any())
                    {
                        _dbContext.Horario.RemoveRange(grupoMateria.Horario);
                    }

                    foreach (var inscripcion in grupoMateria.Inscripcion)
                    {
                        inscripcion.Status = StatusEnum.Deleted;
                        inscripcion.UpdatedAt = DateTime.UtcNow;
                    }

                    grupoMateria.Status = StatusEnum.Deleted;
                    grupoMateria.UpdatedAt = DateTime.UtcNow;
                }

                grupo.Status = StatusEnum.Deleted;
                grupo.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return (true, "Grupo eliminado correctamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return (false, $"Error al eliminar el grupo: {ex.Message}");
            }
        }

        public async Task<Grupo> ActualizarGrupo(Grupo newGrupo)
        {
            var item = await _dbContext.Grupo
                .SingleOrDefaultAsync(g => g.IdGrupo == newGrupo.IdGrupo);

            if (item == null)
            {
                throw new Exception("No existe grupo con el id ingresado");
            }

            item.IdPlanEstudios = newGrupo.IdPlanEstudios;
            item.IdPeriodoAcademico = newGrupo.IdPeriodoAcademico;
            item.NumeroCuatrimestre = newGrupo.NumeroCuatrimestre;
            item.NumeroGrupo = newGrupo.NumeroGrupo;
            item.IdTurno = newGrupo.IdTurno;
            item.CapacidadMaxima = newGrupo.CapacidadMaxima;
            item.Status = newGrupo.Status;

            item.CodigoGrupo = GenerarCodigoGrupo(item.NumeroCuatrimestre, item.IdTurno, item.NumeroGrupo);

            _dbContext.Grupo.Update(item);

            await _dbContext.SaveChangesAsync();

            return item;
        }

        public string GenerarCodigoGrupo(byte numeroCuatrimestre, int idTurno, byte numeroGrupo)
        {
            return $"{numeroCuatrimestre}{idTurno}{numeroGrupo}";
        }

        public async Task<Grupo?> GetGrupoPorCodigoAsync(string codigoGrupo)
        {
            return await _dbContext.Grupo
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .Include(g => g.IdTurnoNavigation)
                .Include(g => g.IdPlanEstudiosNavigation)
                .Include(g => g.GrupoMateria)
                    .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp.IdMateriaNavigation)
                .FirstOrDefaultAsync(g => g.CodigoGrupo == codigoGrupo && g.Status == StatusEnum.Active);
        }

        public async Task<List<Grupo>> BuscarGruposPorCriteriosAsync(
            int? numeroCuatrimestre = null,
            int? idTurno = null,
            int? numeroGrupo = null,
            int? idPlanEstudios = null)
        {
            var query = _dbContext.Grupo
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .Include(g => g.IdTurnoNavigation)
                .Include(g => g.IdPlanEstudiosNavigation)
                .Include(g => g.GrupoMateria)
                    .ThenInclude(gm => gm.Inscripcion)
                .Include(g => g.EstudianteGrupo)
                .Where(g => g.Status == StatusEnum.Active);

            if (numeroCuatrimestre.HasValue)
                query = query.Where(g => g.NumeroCuatrimestre == numeroCuatrimestre.Value);

            if (idTurno.HasValue)
                query = query.Where(g => g.IdTurno == idTurno.Value);

            if (numeroGrupo.HasValue)
                query = query.Where(g => g.NumeroGrupo == numeroGrupo.Value);

            if (idPlanEstudios.HasValue)
                query = query.Where(g => g.IdPlanEstudios == idPlanEstudios.Value);

            return await query.ToListAsync();
        }

        public async Task<InscripcionGrupoResultDto> InscribirEstudianteGrupoAsync(
            int idGrupo,
            int idEstudiante,
            bool forzarInscripcion = false,
            string? observaciones = null)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var grupo = await _dbContext.Grupo
                    .Include(g => g.GrupoMateria)
                        .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                            .ThenInclude(mp => mp.IdMateriaNavigation)
                    .Include(g => g.GrupoMateria)
                        .ThenInclude(gm => gm.IdProfesorNavigation)
                            .ThenInclude(p => p.IdPersonaNavigation)
                    .Include(g => g.IdPlanEstudiosNavigation)
                    .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo);

                if (grupo == null)
                    throw new InvalidOperationException($"No se encontró el grupo con ID {idGrupo}");

                var estudiante = await _dbContext.Estudiante
                    .Include(e => e.IdPersonaNavigation)
                    .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante);

                if (estudiante == null)
                    throw new InvalidOperationException($"No se encontró el estudiante con ID {idEstudiante}");

                var periodoAcademico = await _dbContext.PeriodoAcademico
                    .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == grupo.IdPeriodoAcademico);

                var validaciones = new ValidacionInscripcionGrupoDto
                {
                    EstudianteActivo = estudiante.Activo,
                    PlanEstudiosCompatible = estudiante.IdPlanActual == grupo.IdPlanEstudios,
                    PeriodoActivo = false,
                    CuposDisponibles = true,
                    SinDuplicados = true
                };

                var advertencias = new List<string>();

                if (!validaciones.EstudianteActivo && !forzarInscripcion)
                    throw new InvalidOperationException("El estudiante no está activo");

                if (!validaciones.PlanEstudiosCompatible && !forzarInscripcion)
                {
                    throw new InvalidOperationException(
                        $"El estudiante pertenece al plan {estudiante.IdPlanActual} pero el grupo es del plan {grupo.IdPlanEstudios}");
                }
                if (!validaciones.PlanEstudiosCompatible)
                    advertencias.Add("Plan de estudios diferente (inscripción forzada)");

                if (periodoAcademico != null)
                {
                    validaciones.PeriodoActivo = periodoAcademico.EsPeriodoActual ||
                        _periodoAcademicoService.EsPeriodoActivoPorFechas(periodoAcademico);

                    if (!validaciones.PeriodoActivo && !forzarInscripcion)
                    {
                        throw new InvalidOperationException(
                            $"El periodo académico '{periodoAcademico.Nombre}' no está activo. " +
                            $"Vigencia: {periodoAcademico.FechaInicio} - {periodoAcademico.FechaFin}");
                    }
                    if (!validaciones.PeriodoActivo)
                        advertencias.Add($"Periodo no activo: {periodoAcademico.Nombre} (inscripción forzada)");
                }

                var (tienePendientes, cantidadPendientes, montoPendiente) =
                    await _estudianteService.ValidarPagosPendientesAsync(idEstudiante, grupo.IdPeriodoAcademico);

                validaciones.PagosAlCorriente = !tienePendientes;

                if (tienePendientes && !forzarInscripcion)
                {
                    throw new InvalidOperationException(
                        $"El estudiante tiene {cantidadPendientes} recibo(s) pendiente(s) de pago " +
                        $"por un monto de ${montoPendiente:N2}");
                }
                if (tienePendientes)
                    advertencias.Add($"{cantidadPendientes} recibo(s) pendiente(s): ${montoPendiente:N2} (inscripción forzada)");

                var detalleInscripciones = new List<InscripcionMateriaDto>();
                int exitosas = 0;
                int fallidas = 0;

                foreach (var grupoMateria in grupo.GrupoMateria)
                {
                    var detalle = new InscripcionMateriaDto
                    {
                        IdGrupoMateria = grupoMateria.IdGrupoMateria,
                        NombreMateria = grupoMateria.IdMateriaPlanNavigation?.IdMateriaNavigation?.Nombre ?? "N/A",
                        Profesor = grupoMateria.IdProfesorNavigation != null
                            ? $"{grupoMateria.IdProfesorNavigation.IdPersonaNavigation?.Nombre} {grupoMateria.IdProfesorNavigation.IdPersonaNavigation?.ApellidoPaterno}"
                            : null,
                        Aula = grupoMateria.Aula,
                        CupoMaximo = grupoMateria.Cupo
                    };

                    try
                    {
                        var yaInscrito = await _dbContext.Inscripcion
                            .AnyAsync(i => i.IdEstudiante == idEstudiante
                                && i.IdGrupoMateria == grupoMateria.IdGrupoMateria
                                && i.Status == StatusEnum.Active);

                        if (yaInscrito && !forzarInscripcion)
                        {
                            detalle.Exitoso = false;
                            detalle.MensajeError = "Ya está inscrito en esta materia";
                            fallidas++;
                            detalleInscripciones.Add(detalle);
                            continue;
                        }

                        if (yaInscrito)
                        {
                            detalle.MensajeError = "Ya inscrito (se omitió)";
                            detalle.Exitoso = true;
                            detalleInscripciones.Add(detalle);
                            continue;
                        }

                        var inscritosEnMateria = await _dbContext.Inscripcion
                            .CountAsync(i => i.IdGrupoMateria == grupoMateria.IdGrupoMateria
                                && i.Status == StatusEnum.Active);

                        detalle.EstudiantesInscritos = inscritosEnMateria;

                        if (inscritosEnMateria >= grupoMateria.Cupo && !forzarInscripcion)
                        {
                            detalle.Exitoso = false;
                            detalle.MensajeError = $"Cupo lleno ({inscritosEnMateria}/{grupoMateria.Cupo})";
                            fallidas++;
                            detalleInscripciones.Add(detalle);
                            continue;
                        }

                        var inscripcion = new Inscripcion
                        {
                            IdEstudiante = idEstudiante,
                            IdGrupoMateria = grupoMateria.IdGrupoMateria,
                            FechaInscripcion = DateTime.UtcNow,
                            Estado = EstadoInscripcionEnum.Inscrito.ToString(),
                            Status = StatusEnum.Active,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "Sistema"
                        };

                        await _dbContext.Inscripcion.AddAsync(inscripcion);
                        detalle.IdInscripcion = inscripcion.IdInscripcion;
                        detalle.Exitoso = true;
                        exitosas++;
                    }
                    catch (Exception ex)
                    {
                        detalle.Exitoso = false;
                        detalle.MensajeError = ex.Message;
                        fallidas++;
                    }

                    detalleInscripciones.Add(detalle);
                }

                if (exitosas == 0 && !forzarInscripcion)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("No se pudo inscribir al estudiante en ninguna materia");
                }

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                validaciones.Advertencias = advertencias;

                var resultado = new InscripcionGrupoResultDto
                {
                    IdGrupo = grupo.IdGrupo,
                    CodigoGrupo = grupo.CodigoGrupo ?? "N/A",
                    NombreGrupo = grupo.NombreGrupo,
                    IdEstudiante = estudiante.IdEstudiante,
                    MatriculaEstudiante = estudiante.Matricula,
                    NombreEstudiante = $"{estudiante.IdPersonaNavigation?.Nombre} {estudiante.IdPersonaNavigation?.ApellidoPaterno}".Trim(),
                    TotalMaterias = grupo.GrupoMateria.Count,
                    MateriasInscritas = exitosas,
                    MateriasFallidas = fallidas,
                    DetalleInscripciones = detalleInscripciones,
                    Validaciones = validaciones,
                    FechaInscripcion = DateTime.UtcNow,
                    InscripcionForzada = forzarInscripcion,
                    Observaciones = observaciones
                };

                return resultado;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EstudiantesGrupoDto> GetEstudiantesDelGrupoAsync(int idGrupo)
        {
            var grupo = await _dbContext.Grupo
                .Include(g => g.GrupoMateria)
                    .ThenInclude(gm => gm.Inscripcion)
                        .ThenInclude(i => i.IdEstudianteNavigation)
                            .ThenInclude(e => e.IdPersonaNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo);

            if (grupo == null)
                throw new InvalidOperationException($"No se encontró el grupo con ID {idGrupo}");

            var estudiantesUnicos = grupo.GrupoMateria
                .SelectMany(gm => gm.Inscripcion)
                .GroupBy(i => i.IdEstudiante)
                .Select(g => new
                {
                    IdEstudiante = g.Key,
                    Estudiante = g.First().IdEstudianteNavigation,
                    MateriasInscritas = g.Count(),
                    FechaInscripcion = g.Min(i => i.FechaInscripcion)
                })
                .ToList();

            var resultado = new EstudiantesGrupoDto
            {
                IdGrupo = grupo.IdGrupo,
                CodigoGrupo = grupo.CodigoGrupo ?? "N/A",
                NombreGrupo = grupo.NombreGrupo,
                TotalEstudiantes = estudiantesUnicos.Count,
                Estudiantes = estudiantesUnicos.Select(e => new EstudianteInscritoDto
                {
                    IdEstudiante = e.IdEstudiante,
                    Matricula = e.Estudiante.Matricula,
                    NombreCompleto = $"{e.Estudiante.IdPersonaNavigation?.Nombre} {e.Estudiante.IdPersonaNavigation?.ApellidoPaterno} {e.Estudiante.IdPersonaNavigation?.ApellidoMaterno}".Trim(),
                    Email = e.Estudiante.Email ?? "",
                    MateriasInscritas = e.MateriasInscritas,
                    FechaInscripcion = e.FechaInscripcion
                }).ToList()
            };

            return resultado;
        }

        public async Task<List<GrupoMateriaDisponibleDto>> GetGruposMateriasDisponiblesAsync(
            int? idEstudiante = null,
            int? idPeriodoAcademico = null)
        {
            var query = _dbContext.GrupoMateria
                .Include(gm => gm.IdMateriaPlanNavigation)
                    .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(gm => gm.IdGrupoNavigation)
                    .ThenInclude(g => g.IdPeriodoAcademicoNavigation)
                .Include(gm => gm.IdProfesorNavigation)
                    .ThenInclude(p => p.IdPersonaNavigation)
                .Include(gm => gm.Horario)
                .Include(gm => gm.Inscripcion)
                .Where(gm => gm.Status == StatusEnum.Active);

            if (idPeriodoAcademico.HasValue)
            {
                query = query.Where(gm => gm.IdGrupoNavigation.IdPeriodoAcademico == idPeriodoAcademico.Value);
            }

            var gruposMaterias = await query.ToListAsync();

            var result = gruposMaterias.Select(gm =>
            {
                var inscritos = gm.Inscripcion.Count(i => i.Status == StatusEnum.Active);
                var disponibles = gm.Cupo - inscritos;

                var horario = gm.Horario.Any()
                    ? string.Join(", ", gm.Horario.Select(h =>
                        $"{h.IdDiaSemanaNavigation?.Nombre ?? "N/A"} {h.HoraInicio:HH:mm}-{h.HoraFin:HH:mm}"))
                    : null;

                var nombreProfesor = gm.IdProfesorNavigation != null
                    ? $"{gm.IdProfesorNavigation.IdPersonaNavigation?.Nombre} {gm.IdProfesorNavigation.IdPersonaNavigation?.ApellidoPaterno}".Trim()
                    : null;

                return new GrupoMateriaDisponibleDto
                {
                    IdGrupoMateria = gm.IdGrupoMateria,
                    IdGrupo = gm.IdGrupo,
                    IdMateriaPlan = gm.IdMateriaPlan,
                    NombreMateria = gm.IdMateriaPlanNavigation?.IdMateriaNavigation?.Nombre ?? "N/A",
                    ClaveMateria = gm.IdMateriaPlanNavigation?.IdMateriaNavigation?.Clave ?? "N/A",
                    Grupo = gm.IdGrupoNavigation?.NombreGrupo ?? "N/A",
                    NombreProfesor = nombreProfesor,
                    CupoMaximo = gm.Cupo,
                    Inscritos = inscritos,
                    Disponibles = Math.Max(0, disponibles),
                    PeriodoAcademico = gm.IdGrupoNavigation?.IdPeriodoAcademicoNavigation?.Nombre ?? "N/A",
                    Horario = horario
                };
            }).ToList();

            if (idEstudiante.HasValue)
            {
                var materiasInscritas = await _dbContext.Inscripcion
                    .Where(i => i.IdEstudiante == idEstudiante.Value && i.Status == StatusEnum.Active)
                    .Select(i => i.IdGrupoMateria)
                    .ToListAsync();

                result = result.Where(gm => !materiasInscritas.Contains(gm.IdGrupoMateria)).ToList();
            }

            return result.OrderBy(gm => gm.NombreMateria).ThenBy(gm => gm.Grupo).ToList();
        }

        public async Task<List<EstudianteInscritoDto>> GetEstudiantesPorGrupoMateriaAsync(int idGrupoMateria)
        {
            var estudiantes = await _dbContext.Inscripcion
                .Include(i => i.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(i => i.IdEstudianteNavigation.IdPlanActualNavigation)
                .Where(i => i.IdGrupoMateria == idGrupoMateria && i.Status == StatusEnum.Active)
                .OrderBy(i => i.IdEstudianteNavigation.Matricula)
                .Select(i => new EstudianteInscritoDto
                {
                    IdEstudiante = i.IdEstudiante,
                    Matricula = i.IdEstudianteNavigation.Matricula ?? string.Empty,
                    NombreCompleto = i.IdEstudianteNavigation.IdPersonaNavigation.Nombre + " " +
                                     (i.IdEstudianteNavigation.IdPersonaNavigation.ApellidoPaterno ?? "") + " " +
                                     (i.IdEstudianteNavigation.IdPersonaNavigation.ApellidoMaterno ?? ""),
                    Email = i.IdEstudianteNavigation.IdPersonaNavigation.Correo ?? string.Empty,
                    Telefono = i.IdEstudianteNavigation.IdPersonaNavigation.Telefono,
                    PlanEstudios = i.IdEstudianteNavigation.IdPlanActualNavigation != null
                        ? i.IdEstudianteNavigation.IdPlanActualNavigation.NombrePlanEstudios
                        : null,
                    IdInscripcion = i.IdInscripcion,
                    MateriasInscritas = 0,
                    FechaInscripcion = i.FechaInscripcion,
                    Estado = i.Estado ?? "Inscrito"
                })
                .ToListAsync();

            return estudiantes;
        }

        public async Task<GestionGruposPlanDto> ObtenerGruposPorPlanAsync(
            int idPlanEstudios,
            int? idPeriodoAcademico = null,
            CancellationToken ct = default)
        {
            var plan = await _dbContext.PlanEstudios
                .Include(p => p.IdPeriodicidadNavigation)
                .FirstOrDefaultAsync(p => p.IdPlanEstudios == idPlanEstudios && p.Status == StatusEnum.Active, ct);

            if (plan == null)
                return null!;

            var grupos = await _dbContext.Grupo
                .Include(g => g.IdTurnoNavigation)
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .Include(g => g.GrupoMateria)
                .Where(g => g.IdPlanEstudios == idPlanEstudios
                    && g.Status == StatusEnum.Active
                    && (!idPeriodoAcademico.HasValue || g.IdPeriodoAcademico == idPeriodoAcademico.Value))
                .ToListAsync(ct);

            int duracionPorPlan = 0;
            if (plan.DuracionMeses.HasValue && plan.DuracionMeses.Value > 0 && plan.IdPeriodicidadNavigation != null)
            {
                var mesesPorPeriodo = plan.IdPeriodicidadNavigation.MesesPorPeriodo;
                if (mesesPorPeriodo > 0)
                {
                    duracionPorPlan = (int)Math.Ceiling((double)plan.DuracionMeses.Value / mesesPorPeriodo);
                }
            }

            var materiasDelPlan = await _dbContext.MateriaPlan
                .Where(mp => mp.IdPlanEstudios == idPlanEstudios && mp.Status == StatusEnum.Active)
                .ToListAsync(ct);

            var duracionPorMaterias = materiasDelPlan.Any()
                ? materiasDelPlan.Max(mp => mp.Cuatrimestre)
                : 0;

            var duracionPorGrupos = grupos.Any()
                ? grupos.Max(g => g.NumeroCuatrimestre)
                : 0;

            var duracionCuatrimestres = Math.Max(duracionPorPlan, Math.Max(duracionPorMaterias, duracionPorGrupos));

            if (duracionCuatrimestres == 0 && grupos.Any())
                duracionCuatrimestres = 1;

            var gruposPorCuatrimestre = new List<GrupoPorCuatrimestreDto>();

            for (int cuatri = 1; cuatri <= duracionCuatrimestres; cuatri++)
            {
                var gruposDelCuatri = grupos.Where(g => g.NumeroCuatrimestre == cuatri).ToList();

                var gruposDto = new List<GrupoResumenDto>();
                foreach (var grupo in gruposDelCuatri)
                {
                    var estudiantesPorMaterias = await _dbContext.Inscripcion
                        .Where(i => i.IdGrupoMateriaNavigation.IdGrupo == grupo.IdGrupo && i.Status == StatusEnum.Active)
                        .Select(i => i.IdEstudiante)
                        .Distinct()
                        .CountAsync(ct);

                    var estudiantesDirectos = await _dbContext.EstudianteGrupo
                        .Where(eg => eg.IdGrupo == grupo.IdGrupo && eg.Status == StatusEnum.Active)
                        .CountAsync(ct);

                    var totalEstudiantes = Math.Max(estudiantesPorMaterias, estudiantesDirectos);

                    gruposDto.Add(new GrupoResumenDto
                    {
                        IdGrupo = grupo.IdGrupo,
                        NombreGrupo = grupo.NombreGrupo,
                        CodigoGrupo = grupo.CodigoGrupo,
                        NumeroGrupo = grupo.NumeroGrupo,
                        Turno = grupo.IdTurnoNavigation?.Nombre ?? "",
                        IdTurno = grupo.IdTurno,
                        PeriodoAcademico = grupo.IdPeriodoAcademicoNavigation?.Nombre ?? "",
                        IdPeriodoAcademico = grupo.IdPeriodoAcademico,
                        CapacidadMaxima = grupo.CapacidadMaxima,
                        TotalEstudiantes = totalEstudiantes,
                        CupoDisponible = grupo.CapacidadMaxima - totalEstudiantes,
                        TieneCupo = totalEstudiantes < grupo.CapacidadMaxima,
                        TotalMaterias = grupo.GrupoMateria.Count(gm => gm.Status == StatusEnum.Active)
                    });
                }

                gruposPorCuatrimestre.Add(new GrupoPorCuatrimestreDto
                {
                    NumeroCuatrimestre = cuatri,
                    Grupos = gruposDto
                });
            }

            return new GestionGruposPlanDto
            {
                IdPlanEstudios = plan.IdPlanEstudios,
                NombrePlan = plan.NombrePlanEstudios,
                ClavePlan = plan.ClavePlanEstudios,
                DuracionCuatrimestres = duracionCuatrimestres,
                Periodicidad = plan.IdPeriodicidadNavigation?.DescPeriodicidad ?? "Desconocida",
                GruposPorCuatrimestre = gruposPorCuatrimestre
            };
        }

        public async Task<GrupoResumenDto> CrearGrupoConMateriasAsync(
            CrearGrupoAcademicoRequest request,
            CancellationToken ct = default)
        {
            var plan = await _dbContext.PlanEstudios
                .FirstOrDefaultAsync(p => p.IdPlanEstudios == request.IdPlanEstudios && p.Status == StatusEnum.Active, ct);

            if (plan == null)
                throw new InvalidOperationException($"Plan de estudios {request.IdPlanEstudios} no encontrado");

            var grupoExistente = await _dbContext.Grupo
                .FirstOrDefaultAsync(g =>
                    g.IdPlanEstudios == request.IdPlanEstudios &&
                    g.NumeroCuatrimestre == request.NumeroCuatrimestre &&
                    g.NumeroGrupo == request.NumeroGrupo &&
                    g.IdTurno == request.IdTurno &&
                    g.IdPeriodoAcademico == request.IdPeriodoAcademico &&
                    g.Status == StatusEnum.Active, ct);

            if (grupoExistente != null)
                throw new InvalidOperationException($"Ya existe un grupo {request.NumeroGrupo} en el cuatrimestre {request.NumeroCuatrimestre} para este turno y período");

            var turno = await _dbContext.Turno.FindAsync(new object[] { request.IdTurno }, ct);
            if (turno == null)
                throw new InvalidOperationException($"Turno {request.IdTurno} no encontrado");

            var codigoGrupo = GenerarCodigoGrupo((byte)request.NumeroCuatrimestre, request.IdTurno, (byte)request.NumeroGrupo);

            var grupo = new Grupo
            {
                NombreGrupo = $"{request.NumeroCuatrimestre}{(char)('A' + request.NumeroGrupo - 1)} {turno.Nombre}",
                IdPlanEstudios = request.IdPlanEstudios,
                IdPeriodoAcademico = request.IdPeriodoAcademico,
                NumeroCuatrimestre = (byte)request.NumeroCuatrimestre,
                NumeroGrupo = (byte)request.NumeroGrupo,
                IdTurno = request.IdTurno,
                CapacidadMaxima = (short)request.CapacidadMaxima,
                CodigoGrupo = codigoGrupo,
                Status = StatusEnum.Active,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Grupo.Add(grupo);
            await _dbContext.SaveChangesAsync(ct);

            int totalMaterias = 0;
            if (request.CargarMateriasAutomaticamente)
            {
                var materiasDelCuatrimestre = await _dbContext.MateriaPlan
                    .Include(mp => mp.IdMateriaNavigation)
                    .Where(mp => mp.IdPlanEstudios == request.IdPlanEstudios
                        && mp.Cuatrimestre == request.NumeroCuatrimestre
                        && mp.Status == StatusEnum.Active)
                    .ToListAsync(ct);

                foreach (var materia in materiasDelCuatrimestre)
                {
                    var nombreMateria = materia.IdMateriaNavigation?.Nombre ?? $"Materia-{materia.IdMateriaPlan}";
                    var grupoMateria = new GrupoMateria
                    {
                        Name = $"{grupo.NombreGrupo} - {nombreMateria}",
                        IdGrupo = grupo.IdGrupo,
                        IdMateriaPlan = materia.IdMateriaPlan,
                        Cupo = (short)request.CapacidadMaxima,
                        Status = StatusEnum.Active,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Sistema"
                    };

                    _dbContext.GrupoMateria.Add(grupoMateria);
                }

                await _dbContext.SaveChangesAsync(ct);
                totalMaterias = materiasDelCuatrimestre.Count;
            }

            var periodo = await _dbContext.PeriodoAcademico.FindAsync(new object[] { request.IdPeriodoAcademico }, ct);

            return new GrupoResumenDto
            {
                IdGrupo = grupo.IdGrupo,
                NombreGrupo = grupo.NombreGrupo,
                CodigoGrupo = grupo.CodigoGrupo,
                NumeroGrupo = grupo.NumeroGrupo,
                Turno = turno.Nombre,
                IdTurno = grupo.IdTurno,
                PeriodoAcademico = periodo?.Nombre ?? "",
                IdPeriodoAcademico = grupo.IdPeriodoAcademico,
                CapacidadMaxima = grupo.CapacidadMaxima,
                TotalEstudiantes = 0,
                CupoDisponible = grupo.CapacidadMaxima,
                TieneCupo = true,
                TotalMaterias = totalMaterias
            };
        }

        public async Task<GrupoMateria> AgregarMateriaAlGrupoAsync(
            int idGrupo,
            int idMateriaPlan,
            int? idProfesor = null,
            string? aula = null,
            short? cupo = null,
            CancellationToken ct = default)
        {
            var grupo = await _dbContext.Grupo
                .Include(g => g.IdPlanEstudiosNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo && g.Status == StatusEnum.Active, ct);

            if (grupo == null)
                throw new KeyNotFoundException($"Grupo {idGrupo} no encontrado");

            var materiaPlan = await _dbContext.MateriaPlan
                .Include(mp => mp.IdMateriaNavigation)
                .FirstOrDefaultAsync(mp => mp.IdMateriaPlan == idMateriaPlan && mp.Status == StatusEnum.Active, ct);

            if (materiaPlan == null)
                throw new KeyNotFoundException($"Materia plan {idMateriaPlan} no encontrada");

            if (materiaPlan.IdPlanEstudios != grupo.IdPlanEstudios)
                throw new InvalidOperationException("La materia no pertenece al plan de estudios del grupo");

            var yaExiste = await _dbContext.GrupoMateria
                .AnyAsync(gm => gm.IdGrupo == idGrupo && gm.IdMateriaPlan == idMateriaPlan && gm.Status == StatusEnum.Active, ct);

            if (yaExiste)
                throw new InvalidOperationException($"La materia {materiaPlan.IdMateriaNavigation.Nombre} ya está agregada al grupo");

            var grupoMateria = new GrupoMateria
            {
                Name = $"{grupo.NombreGrupo} - {materiaPlan.IdMateriaNavigation.Nombre}",
                IdGrupo = idGrupo,
                IdMateriaPlan = idMateriaPlan,
                IdProfesor = idProfesor,
                Aula = aula,
                Cupo = cupo ?? grupo.CapacidadMaxima,
                Status = StatusEnum.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Sistema"
            };

            _dbContext.GrupoMateria.Add(grupoMateria);
            await _dbContext.SaveChangesAsync(ct);

            return grupoMateria;
        }

        public async Task<bool> QuitarMateriaDelGrupoAsync(int idGrupoMateria, CancellationToken ct = default)
        {
            var grupoMateria = await _dbContext.GrupoMateria
                .FirstOrDefaultAsync(gm => gm.IdGrupoMateria == idGrupoMateria && gm.Status == StatusEnum.Active, ct);

            if (grupoMateria == null)
                return false;

            var tieneEstudiantes = await _dbContext.Inscripcion
                .AnyAsync(i => i.IdGrupoMateria == idGrupoMateria && i.Status == StatusEnum.Active, ct);

            if (tieneEstudiantes)
                throw new InvalidOperationException("No se puede quitar la materia porque tiene estudiantes inscritos");

            grupoMateria.Status = StatusEnum.Deleted;
            grupoMateria.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }

        public async Task<List<GrupoMateriaDetalleDto>> ObtenerMateriasDelGrupoAsync(
            int idGrupo,
            CancellationToken ct = default)
        {
            var materiasDelGrupo = await _dbContext.GrupoMateria
                .Include(gm => gm.IdMateriaPlanNavigation)
                    .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(gm => gm.IdProfesorNavigation)
                    .ThenInclude(p => p.IdPersonaNavigation)
                .Include(gm => gm.Inscripcion)
                .Where(gm => gm.IdGrupo == idGrupo && gm.Status == StatusEnum.Active)
                .ToListAsync(ct);

            var resultado = materiasDelGrupo.Select(gm => new GrupoMateriaDetalleDto
            {
                IdGrupoMateria = gm.IdGrupoMateria,
                NombreGrupoMateria = gm.Name ?? "Sin nombre",
                IdMateriaPlan = gm.IdMateriaPlan,
                NombreMateria = gm.IdMateriaPlanNavigation?.IdMateriaNavigation?.Nombre ?? "Sin nombre",
                ClaveMateria = gm.IdMateriaPlanNavigation?.IdMateriaNavigation?.Clave ?? "N/A",
                Creditos = (int)(gm.IdMateriaPlanNavigation?.IdMateriaNavigation?.Creditos ?? 0),
                IdProfesor = gm.IdProfesor,
                NombreProfesor = gm.IdProfesorNavigation != null && gm.IdProfesorNavigation.IdPersonaNavigation != null
                    ? $"{gm.IdProfesorNavigation.IdPersonaNavigation.Nombre} {gm.IdProfesorNavigation.IdPersonaNavigation.ApellidoPaterno}".Trim()
                    : null,
                Aula = gm.Aula,
                Cupo = gm.Cupo,
                EstudiantesInscritos = gm.Inscripcion.Count(i => i.Status == StatusEnum.Active),
                CupoDisponible = (int)gm.Cupo - gm.Inscripcion.Count(i => i.Status == StatusEnum.Active),
                TieneCupo = gm.Inscripcion.Count(i => i.Status == StatusEnum.Active) < gm.Cupo
            }).ToList();

            return resultado;
        }

        public async Task<GrupoMateria?> ObtenerGrupoMateriaPorIdAsync(
            int idGrupoMateria,
            CancellationToken ct = default)
        {
            var grupoMateria = await _dbContext.GrupoMateria
                .Include(gm => gm.IdMateriaPlanNavigation)
                    .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(gm => gm.IdProfesorNavigation)
                    .ThenInclude(p => p.IdPersonaNavigation)
                .Include(gm => gm.IdGrupoNavigation)
                .Include(gm => gm.Horario)
                    .ThenInclude(h => h.IdDiaSemanaNavigation)
                .FirstOrDefaultAsync(gm => gm.IdGrupoMateria == idGrupoMateria && gm.Status == StatusEnum.Active, ct);

            return grupoMateria;
        }

        public async Task<PromocionAutomaticaResultDto> PromoverEstudiantesAsync(
            PromoverEstudiantesRequest request,
            CancellationToken ct = default)
        {
            var grupoActual = await _dbContext.Grupo
                .Include(g => g.IdPlanEstudiosNavigation)
                .Include(g => g.IdTurnoNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == request.IdGrupoActual && g.Status == StatusEnum.Active, ct);

            if (grupoActual == null)
                throw new KeyNotFoundException($"Grupo {request.IdGrupoActual} no encontrado");

            var materiasDelPlan = await _dbContext.MateriaPlan
                .Where(mp => mp.IdPlanEstudios == grupoActual.IdPlanEstudios && mp.Status == StatusEnum.Active)
                .ToListAsync(ct);

            var maxCuatrimestre = materiasDelPlan.Any() ? materiasDelPlan.Max(mp => mp.Cuatrimestre) : 0;

            if (grupoActual.NumeroCuatrimestre >= maxCuatrimestre)
                throw new InvalidOperationException($"El grupo está en el último cuatrimestre ({maxCuatrimestre}). No se puede promover.");

            var grupoSiguiente = await ObtenerOCrearGrupoSiguienteAsync(
                request.IdGrupoActual,
                request.IdPeriodoAcademicoDestino,
                request.CrearGrupoSiguienteAutomaticamente ?? true,
                ct);

            if (grupoSiguiente == null)
                throw new InvalidOperationException("No se pudo obtener o crear el grupo del siguiente cuatrimestre");

            var estudiantesGrupo = await _dbContext.Inscripcion
                .Where(i => i.IdGrupoMateriaNavigation.IdGrupo == request.IdGrupoActual && i.Status == StatusEnum.Active)
                .Select(i => i.IdEstudiante)
                .Distinct()
                .ToListAsync(ct);

            var resultado = new PromocionAutomaticaResultDto
            {
                IdGrupoOrigen = grupoActual.IdGrupo,
                GrupoOrigen = grupoActual.NombreGrupo,
                CuatrimestreOrigen = grupoActual.NumeroCuatrimestre,
                IdGrupoDestino = grupoSiguiente.IdGrupo,
                GrupoDestino = grupoSiguiente.NombreGrupo,
                CuatrimestreDestino = grupoSiguiente.NumeroCuatrimestre,
                Estudiantes = new List<EstudiantePromocionDto>()
            };

            int promovidos = 0;
            int noPromovidos = 0;

            foreach (var idEstudiante in estudiantesGrupo)
            {
                if (request.EstudiantesExcluidos != null && request.EstudiantesExcluidos.Contains(idEstudiante))
                    continue;

                var (puedePromover, motivo) = request.PromoverTodos ?? false
                    ? (true, "Promoción forzada (sin validación)")
                    : await ValidarPromocionEstudianteAsync(idEstudiante, grupoActual.NumeroCuatrimestre, request.PromedioMinimoPromocion ?? 70, ct);

                var estudiante = await _dbContext.Estudiante
                    .Include(e => e.IdPersonaNavigation)
                    .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

                if (puedePromover)
                {
                    await InscribirEstudianteGrupoAsync(
                        grupoSiguiente.IdGrupo,
                        idEstudiante,
                        false,
                        "Promoción automática");

                    promovidos++;
                }
                else
                {
                    noPromovidos++;
                }

                resultado.Estudiantes.Add(new EstudiantePromocionDto
                {
                    IdEstudiante = idEstudiante,
                    Matricula = estudiante?.Matricula ?? "",
                    NombreCompleto = estudiante != null
                        ? $"{estudiante.IdPersonaNavigation?.Nombre} {estudiante.IdPersonaNavigation?.ApellidoPaterno} {estudiante.IdPersonaNavigation?.ApellidoMaterno}".Trim()
                        : "",
                    FuePromovido = puedePromover,
                    Motivo = motivo
                });
            }

            resultado.TotalEstudiantesPromovidos = promovidos;
            resultado.TotalEstudiantesNoPromovidos = noPromovidos;
            resultado.Mensaje = $"Promoción completada: {promovidos} estudiantes promovidos, {noPromovidos} no promovidos";

            return resultado;
        }

        public async Task<PreviewPromocionResultDto> PreviewPromocionAsync(
            PreviewPromocionRequest request,
            CancellationToken ct = default)
        {
            var grupoActual = await _dbContext.Grupo
                .Include(g => g.IdPlanEstudiosNavigation)
                .Include(g => g.IdTurnoNavigation)
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == request.IdGrupoActual && g.Status == StatusEnum.Active, ct);

            if (grupoActual == null)
                throw new KeyNotFoundException($"Grupo {request.IdGrupoActual} no encontrado");

            var periodoDestino = await _dbContext.PeriodoAcademico
                .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == request.IdPeriodoAcademicoDestino, ct);

            if (periodoDestino == null)
                throw new KeyNotFoundException($"Periodo académico {request.IdPeriodoAcademicoDestino} no encontrado");

            var materiasDelPlan = await _dbContext.MateriaPlan
                .Where(mp => mp.IdPlanEstudios == grupoActual.IdPlanEstudios && mp.Status == StatusEnum.Active)
                .ToListAsync(ct);

            var maxCuatrimestre = materiasDelPlan.Any() ? materiasDelPlan.Max(mp => mp.Cuatrimestre) : 0;
            var siguienteCuatrimestre = grupoActual.NumeroCuatrimestre + 1;

            if (grupoActual.NumeroCuatrimestre >= maxCuatrimestre)
                throw new InvalidOperationException($"El grupo está en el último cuatrimestre ({maxCuatrimestre}). No se puede promover.");

            var grupoDestino = await _dbContext.Grupo
                .Where(g => g.IdPlanEstudios == grupoActual.IdPlanEstudios
                    && g.NumeroCuatrimestre == siguienteCuatrimestre
                    && g.NumeroGrupo == grupoActual.NumeroGrupo
                    && g.IdTurno == grupoActual.IdTurno
                    && g.IdPeriodoAcademico == request.IdPeriodoAcademicoDestino
                    && g.Status == StatusEnum.Active)
                .FirstOrDefaultAsync(ct);

            var estudiantesIds = await _dbContext.Inscripcion
                .Where(i => i.IdGrupoMateriaNavigation.IdGrupo == request.IdGrupoActual && i.Status == StatusEnum.Active)
                .Select(i => i.IdEstudiante)
                .Distinct()
                .ToListAsync(ct);

            var estudiantes = await _dbContext.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Where(e => estudiantesIds.Contains(e.IdEstudiante))
                .ToListAsync(ct);

            var recibosPorEstudiante = await _dbContext.Recibo
                .Where(r => r.IdEstudiante.HasValue
                    && estudiantesIds.Contains(r.IdEstudiante.Value)
                    && r.Status == StatusEnum.Active
                    && r.Estatus != Core.Enums.EstatusRecibo.PAGADO
                    && r.Estatus != Core.Enums.EstatusRecibo.CANCELADO)
                .GroupBy(r => r.IdEstudiante!.Value)
                .Select(g => new
                {
                    IdEstudiante = g.Key,
                    RecibosPendientes = g.Count(),
                    SaldoPendiente = g.Sum(r => r.Saldo)
                })
                .ToDictionaryAsync(x => x.IdEstudiante, ct);

            var estudiantesPreview = new List<EstudiantePreviewDto>();
            int elegibles = 0;
            int conPagosPendientes = 0;
            decimal totalSaldoPendiente = 0;

            foreach (var est in estudiantes)
            {
                var persona = est.IdPersonaNavigation;
                var nombreCompleto = persona != null
                    ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                    : "N/A";

                var tienePagos = recibosPorEstudiante.TryGetValue(est.IdEstudiante, out var pagosInfo);
                var saldo = tienePagos ? pagosInfo.SaldoPendiente : 0;
                var recibos = tienePagos ? pagosInfo.RecibosPendientes : 0;

                var (esElegible, motivo) = await ValidarPromocionEstudianteAsync(
                    est.IdEstudiante,
                    grupoActual.NumeroCuatrimestre,
                    70,
                    ct);

                if (esElegible) elegibles++;
                if (tienePagos && saldo > 0)
                {
                    conPagosPendientes++;
                    totalSaldoPendiente += saldo;
                }

                estudiantesPreview.Add(new EstudiantePreviewDto
                {
                    IdEstudiante = est.IdEstudiante,
                    Matricula = est.Matricula ?? "",
                    NombreCompleto = nombreCompleto,
                    Email = est.Email ?? persona?.Correo,
                    Telefono = persona?.Telefono,
                    EsElegible = esElegible,
                    MotivoNoElegible = esElegible ? "" : motivo,
                    TienePagosPendientes = tienePagos && saldo > 0,
                    SaldoPendiente = saldo,
                    RecibosPendientes = recibos,
                    Seleccionado = esElegible && (!tienePagos || saldo == 0)
                });
            }

            estudiantesPreview = estudiantesPreview
                .OrderByDescending(e => e.Seleccionado)
                .ThenBy(e => e.NombreCompleto)
                .ToList();

            return new PreviewPromocionResultDto
            {
                IdGrupoOrigen = grupoActual.IdGrupo,
                GrupoOrigen = grupoActual.NombreGrupo ?? "",
                CodigoGrupoOrigen = grupoActual.CodigoGrupo ?? "",
                CuatrimestreOrigen = grupoActual.NumeroCuatrimestre,
                PlanEstudios = grupoActual.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "",
                Turno = grupoActual.IdTurnoNavigation?.Nombre ?? "",

                IdGrupoDestino = grupoDestino?.IdGrupo,
                GrupoDestino = grupoDestino?.NombreGrupo,
                CodigoGrupoDestino = grupoDestino?.CodigoGrupo ?? GenerarCodigoGrupo((byte)siguienteCuatrimestre, grupoActual.IdTurno, grupoActual.NumeroGrupo),
                CuatrimestreDestino = siguienteCuatrimestre,
                GrupoDestinoExiste = grupoDestino != null,
                PeriodoDestino = periodoDestino.Nombre ?? "",

                TotalEstudiantes = estudiantes.Count,
                EstudiantesElegibles = elegibles,
                EstudiantesConPagosPendientes = conPagosPendientes,
                TotalSaldoPendiente = totalSaldoPendiente,

                Estudiantes = estudiantesPreview
            };
        }

        public async Task<(bool PuedePromover, string Motivo)> ValidarPromocionEstudianteAsync(
            int idEstudiante,
            int cuatrimestreActual,
            decimal promedioMinimo,
            CancellationToken ct = default)
        {
            var inscripciones = await _dbContext.Inscripcion
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                .Where(i => i.IdEstudiante == idEstudiante
                    && i.IdGrupoMateriaNavigation.IdMateriaPlanNavigation.Cuatrimestre == cuatrimestreActual
                    && i.Status == StatusEnum.Active)
                .ToListAsync(ct);

            if (!inscripciones.Any())
                return (false, "No tiene inscripciones registradas en este cuatrimestre");

            return (true, "Cumple con todos los requisitos de promoción");
        }

        public async Task<Grupo?> ObtenerOCrearGrupoSiguienteAsync(
            int idGrupoActual,
            int idPeriodoAcademicoDestino,
            bool crearSiNoExiste = true,
            CancellationToken ct = default)
        {
            var grupoActual = await _dbContext.Grupo
                .Include(g => g.IdPlanEstudiosNavigation)
                .Include(g => g.IdTurnoNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupoActual && g.Status == StatusEnum.Active, ct);

            if (grupoActual == null)
                throw new KeyNotFoundException($"Grupo actual {idGrupoActual} no encontrado");

            var siguienteCuatrimestre = grupoActual.NumeroCuatrimestre + 1;

            var grupoSiguiente = await _dbContext.Grupo
                .FirstOrDefaultAsync(g =>
                    g.IdPlanEstudios == grupoActual.IdPlanEstudios &&
                    g.NumeroCuatrimestre == siguienteCuatrimestre &&
                    g.NumeroGrupo == grupoActual.NumeroGrupo &&
                    g.IdTurno == grupoActual.IdTurno &&
                    g.IdPeriodoAcademico == idPeriodoAcademicoDestino &&
                    g.Status == StatusEnum.Active, ct);

            if (grupoSiguiente != null)
                return grupoSiguiente;

            if (!crearSiNoExiste)
                return null;

            var request = new CrearGrupoAcademicoRequest
            {
                IdPlanEstudios = grupoActual.IdPlanEstudios,
                IdPeriodoAcademico = idPeriodoAcademicoDestino,
                NumeroCuatrimestre = siguienteCuatrimestre,
                NumeroGrupo = grupoActual.NumeroGrupo,
                IdTurno = grupoActual.IdTurno,
                CapacidadMaxima = grupoActual.CapacidadMaxima,
                CargarMateriasAutomaticamente = true
            };

            await CrearGrupoConMateriasAsync(request, ct);

            grupoSiguiente = await _dbContext.Grupo
                .FirstOrDefaultAsync(g =>
                    g.IdPlanEstudios == grupoActual.IdPlanEstudios &&
                    g.NumeroCuatrimestre == siguienteCuatrimestre &&
                    g.NumeroGrupo == grupoActual.NumeroGrupo &&
                    g.IdTurno == grupoActual.IdTurno &&
                    g.IdPeriodoAcademico == idPeriodoAcademicoDestino &&
                    g.Status == StatusEnum.Active, ct);

            return grupoSiguiente;
        }

        public async Task ActualizarHorariosGrupoMateriaAsync(int idGrupoMateria, List<HorarioDto> horarios, CancellationToken ct = default)
        {
            var grupoMateria = await _dbContext.GrupoMateria
                .Include(gm => gm.Horario)
                .FirstOrDefaultAsync(gm => gm.IdGrupoMateria == idGrupoMateria && gm.Status == StatusEnum.Active, ct);

            if (grupoMateria == null)
                throw new KeyNotFoundException($"No se encontró la materia con ID {idGrupoMateria}");

            _dbContext.Horario.RemoveRange(grupoMateria.Horario);

            foreach (var horarioDto in horarios)
            {
                var diaSemana = await _dbContext.DiaSemana
                    .FirstOrDefaultAsync(d => d.Nombre == horarioDto.Dia, ct);

                if (diaSemana == null)
                    throw new InvalidOperationException($"Día de la semana '{horarioDto.Dia}' no válido");

                var horario = new Horario
                {
                    IdGrupoMateria = idGrupoMateria,
                    IdDiaSemana = diaSemana.IdDiaSemana,
                    HoraInicio = TimeOnly.Parse(horarioDto.HoraInicio),
                    HoraFin = TimeOnly.Parse(horarioDto.HoraFin),
                    Aula = horarioDto.Aula
                };

                _dbContext.Horario.Add(horario);
            }

            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<GrupoMateria?> AsignarProfesorAMateriaAsync(int idGrupoMateria, int? idProfesor, CancellationToken ct = default)
        {
            var grupoMateria = await _dbContext.GrupoMateria
                .Include(gm => gm.IdMateriaPlanNavigation)
                    .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(gm => gm.IdProfesorNavigation)
                    .ThenInclude(p => p.IdPersonaNavigation)
                .Include(gm => gm.Horario)
                    .ThenInclude(h => h.IdDiaSemanaNavigation)
                .Include(gm => gm.Inscripcion)
                .FirstOrDefaultAsync(gm => gm.IdGrupoMateria == idGrupoMateria && gm.Status == StatusEnum.Active, ct);

            if (grupoMateria == null)
                throw new KeyNotFoundException($"No se encontró la materia con ID {idGrupoMateria}");

            grupoMateria.IdProfesor = idProfesor;

            await _dbContext.SaveChangesAsync(ct);

            return grupoMateria;
        }

        public async Task<EstudianteGrupoResultDto> InscribirEstudianteAGrupoDirectoAsync(
            int idGrupo,
            int idEstudiante,
            string? observaciones = null,
            CancellationToken ct = default)
        {
            var resultado = new EstudianteGrupoResultDto
            {
                IdGrupo = idGrupo,
                IdEstudiante = idEstudiante
            };

            try
            {
                var grupo = await _dbContext.Grupo
                    .Include(g => g.IdPlanEstudiosNavigation)
                    .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo && g.Status == StatusEnum.Active, ct);

                if (grupo == null)
                {
                    resultado.Exitoso = false;
                    resultado.MensajeError = "Grupo no encontrado";
                    return resultado;
                }

                resultado.NombreGrupo = grupo.NombreGrupo;

                var estudiante = await _dbContext.Estudiante
                    .Include(e => e.IdPersonaNavigation)
                    .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante && e.Status == StatusEnum.Active, ct);

                if (estudiante == null)
                {
                    resultado.Exitoso = false;
                    resultado.MensajeError = "Estudiante no encontrado";
                    return resultado;
                }

                resultado.Matricula = estudiante.Matricula;
                resultado.NombreCompleto = $"{estudiante.IdPersonaNavigation?.Nombre} {estudiante.IdPersonaNavigation?.ApellidoPaterno} {estudiante.IdPersonaNavigation?.ApellidoMaterno}".Trim();

                var yaInscrito = await _dbContext.EstudianteGrupo
                    .AnyAsync(eg => eg.IdEstudiante == idEstudiante && eg.IdGrupo == idGrupo && eg.Status == StatusEnum.Active, ct);

                if (yaInscrito)
                {
                    resultado.Exitoso = false;
                    resultado.MensajeError = "El estudiante ya está inscrito en este grupo";
                    return resultado;
                }

                var estudianteGrupo = new EstudianteGrupo
                {
                    IdEstudiante = idEstudiante,
                    IdGrupo = idGrupo,
                    FechaInscripcion = DateTime.UtcNow,
                    Estado = "Inscrito",
                    Observaciones = observaciones,
                    Status = StatusEnum.Active,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.EstudianteGrupo.Add(estudianteGrupo);
                await _dbContext.SaveChangesAsync(ct);

                resultado.IdEstudianteGrupo = estudianteGrupo.IdEstudianteGrupo;
                resultado.FechaInscripcion = estudianteGrupo.FechaInscripcion;
                resultado.Estado = estudianteGrupo.Estado;
                resultado.Exitoso = true;
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.MensajeError = ex.Message;
            }

            return resultado;
        }

        public async Task<InscribirEstudiantesGrupoResponse> InscribirEstudiantesAGrupoMasivoAsync(
            InscribirEstudiantesGrupoRequest request,
            CancellationToken ct = default)
        {
            var response = new InscribirEstudiantesGrupoResponse
            {
                IdGrupo = request.IdGrupo,
                TotalProcesados = request.IdsEstudiantes.Count
            };

            var grupo = await _dbContext.Grupo
                .FirstOrDefaultAsync(g => g.IdGrupo == request.IdGrupo && g.Status == StatusEnum.Active, ct);

            if (grupo != null)
            {
                response.NombreGrupo = grupo.NombreGrupo;
            }

            foreach (var idEstudiante in request.IdsEstudiantes)
            {
                var resultado = await InscribirEstudianteAGrupoDirectoAsync(
                    request.IdGrupo,
                    idEstudiante,
                    request.Observaciones,
                    ct);

                response.Resultados.Add(resultado);

                if (resultado.Exitoso)
                    response.Exitosos++;
                else
                    response.Fallidos++;
            }

            return response;
        }

        public async Task<EstudiantesDelGrupoResponse> GetEstudiantesDelGrupoDirectoAsync(int idGrupo, CancellationToken ct = default)
        {
            var grupo = await _dbContext.Grupo
                .Include(g => g.IdPlanEstudiosNavigation)
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo, ct);

            if (grupo == null)
                throw new KeyNotFoundException($"Grupo {idGrupo} no encontrado");

            var estudiantesGrupo = await _dbContext.EstudianteGrupo
                .Include(eg => eg.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(eg => eg.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPlanActualNavigation)
                .Where(eg => eg.IdGrupo == idGrupo && eg.Status == StatusEnum.Active)
                .OrderBy(eg => eg.IdEstudianteNavigation.Matricula)
                .ToListAsync(ct);

            var response = new EstudiantesDelGrupoResponse
            {
                IdGrupo = grupo.IdGrupo,
                NombreGrupo = grupo.NombreGrupo,
                CodigoGrupo = grupo.CodigoGrupo,
                PlanEstudios = grupo.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "",
                PeriodoAcademico = grupo.IdPeriodoAcademicoNavigation?.Nombre ?? "",
                NumeroCuatrimestre = grupo.NumeroCuatrimestre,
                CapacidadMaxima = grupo.CapacidadMaxima,
                TotalEstudiantes = estudiantesGrupo.Count,
                CupoDisponible = grupo.CapacidadMaxima - estudiantesGrupo.Count,
                Estudiantes = estudiantesGrupo.Select(eg => new EstudianteEnGrupoDto
                {
                    IdEstudianteGrupo = eg.IdEstudianteGrupo,
                    IdEstudiante = eg.IdEstudiante,
                    Matricula = eg.IdEstudianteNavigation.Matricula,
                    NombreCompleto = $"{eg.IdEstudianteNavigation.IdPersonaNavigation?.Nombre} {eg.IdEstudianteNavigation.IdPersonaNavigation?.ApellidoPaterno} {eg.IdEstudianteNavigation.IdPersonaNavigation?.ApellidoMaterno}".Trim(),
                    Email = eg.IdEstudianteNavigation.Email ?? eg.IdEstudianteNavigation.IdPersonaNavigation?.Correo,
                    Telefono = eg.IdEstudianteNavigation.IdPersonaNavigation?.Celular ?? eg.IdEstudianteNavigation.IdPersonaNavigation?.Telefono,
                    FechaInscripcion = eg.FechaInscripcion,
                    Estado = eg.Estado,
                    PlanEstudios = eg.IdEstudianteNavigation.IdPlanActualNavigation?.NombrePlanEstudios
                }).ToList()
            };

            return response;
        }

        public async Task<bool> EliminarEstudianteDeGrupoAsync(int idEstudianteGrupo, CancellationToken ct = default)
        {
            var estudianteGrupo = await _dbContext.EstudianteGrupo
                .FirstOrDefaultAsync(eg => eg.IdEstudianteGrupo == idEstudianteGrupo && eg.Status == StatusEnum.Active, ct);

            if (estudianteGrupo == null)
                return false;

            estudianteGrupo.Status = StatusEnum.Deleted;
            estudianteGrupo.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<ImportarEstudiantesGrupoResponse> ImportarEstudiantesCompletoAsync(
            ImportarEstudiantesGrupoRequest request,
            CancellationToken ct = default)
        {
            var response = new ImportarEstudiantesGrupoResponse
            {
                IdGrupo = request.IdGrupo,
                TotalProcesados = request.Estudiantes.Count
            };

            var grupo = await _dbContext.Grupo
                .Include(g => g.IdPlanEstudiosNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == request.IdGrupo && g.Status == StatusEnum.Active, ct);

            if (grupo == null)
            {
                response.Resultados.Add(new EstudianteImportadoResultDto
                {
                    Fila = 0,
                    Exitoso = false,
                    MensajeError = "Grupo no encontrado"
                });
                return response;
            }

            response.NombreGrupo = grupo.NombreGrupo;
            response.PlanEstudios = grupo.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "";

            var nombrePlan = grupo.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "";
            var idPlanEstudios = grupo.IdPlanEstudios;

            int fila = 1;
            foreach (var estudianteDto in request.Estudiantes)
            {
                var resultado = new EstudianteImportadoResultDto
                {
                    Fila = fila++,
                    NombreCompleto = $"{estudianteDto.Nombre} {estudianteDto.ApellidoPaterno} {estudianteDto.ApellidoMaterno}".Trim(),
                    Curp = estudianteDto.Curp,
                    Correo = estudianteDto.Correo
                };

                using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    if (string.IsNullOrWhiteSpace(estudianteDto.Nombre))
                    {
                        resultado.Exitoso = false;
                        resultado.MensajeError = "El nombre es requerido";
                        response.Resultados.Add(resultado);
                        response.Fallidos++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(estudianteDto.ApellidoPaterno))
                    {
                        resultado.Exitoso = false;
                        resultado.MensajeError = "El apellido paterno es requerido";
                        response.Resultados.Add(resultado);
                        response.Fallidos++;
                        continue;
                    }

                    var curpLimpio = estudianteDto.Curp?.Trim().ToUpperInvariant();
                    var curpValido = !string.IsNullOrWhiteSpace(curpLimpio)
                        && curpLimpio.Length == 18
                        && curpLimpio != "CURP"
                        && !curpLimpio.StartsWith("CURP");

                    Persona? personaExistente = null;
                    if (curpValido)
                    {
                        personaExistente = await _dbContext.Persona
                            .FirstOrDefaultAsync(p => p.Curp == curpLimpio && p.Status == StatusEnum.Active, ct);
                    }

                    if (personaExistente == null && !string.IsNullOrWhiteSpace(estudianteDto.Correo))
                    {
                        var correoLimpio = estudianteDto.Correo.Trim().ToLowerInvariant();
                        personaExistente = await _dbContext.Persona
                            .FirstOrDefaultAsync(p => p.Correo == correoLimpio && p.Status == StatusEnum.Active, ct);
                    }

                    Persona persona;
                    bool personaCreada = false;

                    if (personaExistente != null)
                    {
                        persona = personaExistente;
                    }
                    else
                    {
                        persona = new Persona
                        {
                            Nombre = estudianteDto.Nombre.Trim(),
                            ApellidoPaterno = estudianteDto.ApellidoPaterno.Trim(),
                            ApellidoMaterno = estudianteDto.ApellidoMaterno?.Trim(),
                            Curp = curpValido ? curpLimpio : null,
                            Correo = estudianteDto.Correo?.Trim().ToLowerInvariant(),
                            Telefono = estudianteDto.Telefono?.Trim(),
                            Celular = estudianteDto.Celular?.Trim(),
                            FechaNacimiento = estudianteDto.GetFechaNacimientoAsDateOnly(),
                            IdGenero = estudianteDto.IdGenero,
                            Status = StatusEnum.Active,
                            CreatedAt = DateTime.UtcNow
                        };

                        _dbContext.Persona.Add(persona);
                        await _dbContext.SaveChangesAsync(ct);
                        personaCreada = true;
                        response.PersonasCreadas++;
                    }

                    resultado.IdPersona = persona.IdPersona;

                    var estudianteExistente = await _dbContext.Estudiante
                        .FirstOrDefaultAsync(e => e.IdPersona == persona.IdPersona && e.Status == StatusEnum.Active, ct);

                    Estudiante estudiante;
                    bool estudianteCreado = false;

                    if (estudianteExistente != null)
                    {
                        estudiante = estudianteExistente;
                    }
                    else
                    {
                        var matricula = !string.IsNullOrWhiteSpace(estudianteDto.Matricula)
                            ? estudianteDto.Matricula.Trim()
                            : await _matriculaService.GenerarMatriculaAsync(nombrePlan);

                        if (await _matriculaService.ExisteMatriculaAsync(matricula))
                        {
                            resultado.Exitoso = false;
                            resultado.MensajeError = $"La matrícula {matricula} ya existe";
                            await transaction.RollbackAsync(ct);
                            response.Resultados.Add(resultado);
                            response.Fallidos++;
                            continue;
                        }

                        estudiante = new Estudiante
                        {
                            Matricula = matricula,
                            IdPersona = persona.IdPersona,
                            Email = estudianteDto.Correo?.Trim().ToLowerInvariant(),
                            FechaIngreso = DateOnly.FromDateTime(DateTime.Now),
                            IdPlanActual = idPlanEstudios,
                            Activo = true,
                            Status = StatusEnum.Active,
                            CreatedAt = DateTime.UtcNow
                        };

                        _dbContext.Estudiante.Add(estudiante);
                        await _dbContext.SaveChangesAsync(ct);
                        estudianteCreado = true;
                        response.EstudiantesCreados++;

                        resultado.MatriculaGenerada = matricula;
                    }

                    resultado.IdEstudiante = estudiante.IdEstudiante;

                    var yaInscrito = await _dbContext.EstudianteGrupo
                        .AnyAsync(eg => eg.IdEstudiante == estudiante.IdEstudiante
                            && eg.IdGrupo == request.IdGrupo
                            && eg.Status == StatusEnum.Active, ct);

                    if (yaInscrito)
                    {
                        resultado.Exitoso = true;
                        resultado.MensajeError = "Ya estaba inscrito en el grupo";
                        await transaction.CommitAsync(ct);
                        response.Resultados.Add(resultado);
                        response.Exitosos++;
                        continue;
                    }

                    var estudianteGrupo = new EstudianteGrupo
                    {
                        IdEstudiante = estudiante.IdEstudiante,
                        IdGrupo = request.IdGrupo,
                        FechaInscripcion = DateTime.UtcNow,
                        Estado = "Inscrito",
                        Observaciones = request.Observaciones,
                        Status = StatusEnum.Active,
                        CreatedAt = DateTime.UtcNow
                    };

                    _dbContext.EstudianteGrupo.Add(estudianteGrupo);
                    await _dbContext.SaveChangesAsync(ct);
                    response.InscripcionesCreadas++;

                    resultado.IdEstudianteGrupo = estudianteGrupo.IdEstudianteGrupo;
                    resultado.Exitoso = true;

                    await transaction.CommitAsync(ct);
                    response.Exitosos++;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(ct);
                    resultado.Exitoso = false;
                    resultado.MensajeError = ex.Message;
                    response.Fallidos++;
                }

                response.Resultados.Add(resultado);
            }

            return response;
        }
    }
}
