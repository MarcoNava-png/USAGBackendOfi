using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.GestionAcademica;
using WebApplication2.Core.DTOs.Grupo;
using WebApplication2.Core.DTOs.Inscripcion;
using WebApplication2.Core.Requests.GestionAcademica;
using WebApplication2.Core.Requests.Grupo;
using WebApplication2.Core.Responses.Grupo;
using WebApplication2.Core.DTOs.AccesoAlumnoDocente;
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
        private readonly IAccesoAlumnoDocenteService _accesoService;
        private readonly IMicrosoftGraphService _graphService;

        public GrupoService(
            ApplicationDbContext dbContext,
            IInscripcionService inscripcionService,
            IEstudianteService estudianteService,
            IPeriodoAcademicoService periodoAcademicoService,
            IMatriculaService matriculaService,
            IAccesoAlumnoDocenteService accesoService,
            IMicrosoftGraphService graphService)
        {
            _dbContext = dbContext;
            _inscripcionService = inscripcionService;
            _estudianteService = estudianteService;
            _periodoAcademicoService = periodoAcademicoService;
            _matriculaService = matriculaService;
            _accesoService = accesoService;
            _graphService = graphService;
        }

        public async Task<PagedResult<Grupo>> GetGrupos(int page, int pageSize, int? idPeriodoAcademico = null)
        {
            var query = _dbContext.Grupo
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .Include(g => g.IdTurnoNavigation)
                .Include(g => g.IdPlanEstudiosNavigation)
                    .ThenInclude(p => p.IdCampusNavigation)
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
            int? idPlanEstudios = null,
            int? idPeriodoAcademico = null)
        {
            var query = _dbContext.Grupo
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .Include(g => g.IdTurnoNavigation)
                .Include(g => g.IdPlanEstudiosNavigation)
                    .ThenInclude(p => p.IdCampusNavigation)
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

            if (idPeriodoAcademico.HasValue)
                query = query.Where(g => g.IdPeriodoAcademico == idPeriodoAcademico.Value);

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

                // Vínculo estudiante-grupo (necesario para que el alumno aparezca en su grupo).
                var yaEnGrupo = await _dbContext.EstudianteGrupo
                    .AnyAsync(eg => eg.IdEstudiante == idEstudiante && eg.IdGrupo == idGrupo && eg.Status == StatusEnum.Active);
                if (!yaEnGrupo)
                {
                    await _dbContext.EstudianteGrupo.AddAsync(new EstudianteGrupo
                    {
                        IdEstudiante = idEstudiante,
                        IdGrupo = idGrupo,
                        FechaInscripcion = DateTime.UtcNow,
                        Estado = "Inscrito",
                        Observaciones = observaciones,
                        Status = StatusEnum.Active,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Sistema"
                    });
                }

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
                        var existente = await _dbContext.Inscripcion
                            .FirstOrDefaultAsync(i => i.IdEstudiante == idEstudiante
                                && i.IdGrupoMateria == grupoMateria.IdGrupoMateria);

                        if (existente != null)
                        {
                            if (existente.Status != StatusEnum.Active)
                            {
                                existente.Status = StatusEnum.Active;
                                existente.Estado = EstadoInscripcionEnum.Inscrito.ToString();
                                existente.UpdatedAt = DateTime.UtcNow;
                                detalle.MensajeError = "Reactivada";
                            }
                            else
                            {
                                detalle.MensajeError = "Ya inscrito (se mantuvo)";
                            }
                            detalle.IdInscripcion = existente.IdInscripcion;
                            detalle.Exitoso = true;
                            exitosas++;
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

                if (estudiante.IdPlanActual != grupo.IdPlanEstudios)
                {
                    estudiante.IdPlanActual = grupo.IdPlanEstudios;
                    estudiante.UpdatedAt = DateTime.UtcNow;
                }

                var apartados = await _dbContext.PreInscripcion
                    .Where(pi => pi.IdEstudiante == idEstudiante
                        && pi.IdPeriodoAcademicoDestino == grupo.IdPeriodoAcademico
                        && pi.Estado == "Pendiente"
                        && pi.Status == StatusEnum.Active)
                    .ToListAsync();
                foreach (var ap in apartados)
                {
                    ap.Estado = "Completada";
                    ap.Status = StatusEnum.Deleted;
                    ap.UpdatedAt = DateTime.UtcNow;
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
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo);

            if (grupo == null)
                throw new InvalidOperationException($"No se encontró el grupo con ID {idGrupo}");

            var asignados = await _dbContext.EstudianteGrupo
                .Include(eg => eg.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Where(eg => eg.IdGrupo == idGrupo && eg.Status == StatusEnum.Active)
                .AsNoTracking()
                .ToListAsync();

            var materiasPorEstudiante = await _dbContext.GrupoMateria
                .Where(gm => gm.IdGrupo == idGrupo)
                .SelectMany(gm => gm.Inscripcion.Where(i => i.Status == StatusEnum.Active))
                .GroupBy(i => i.IdEstudiante)
                .Select(g => new { IdEstudiante = g.Key, Materias = g.Count(), FechaInscripcion = g.Min(i => i.FechaInscripcion) })
                .ToDictionaryAsync(x => x.IdEstudiante, x => new { x.Materias, x.FechaInscripcion });

            var dtoList = new List<EstudianteInscritoDto>();
            var idsIncluidos = new HashSet<int>();

            foreach (var eg in asignados)
            {
                var e = eg.IdEstudianteNavigation;
                if (e == null) continue;
                materiasPorEstudiante.TryGetValue(eg.IdEstudiante, out var mat);
                dtoList.Add(new EstudianteInscritoDto
                {
                    IdEstudiante = eg.IdEstudiante,
                    Matricula = e.Matricula ?? "",
                    NombreCompleto = $"{e.IdPersonaNavigation?.Nombre} {e.IdPersonaNavigation?.ApellidoPaterno} {e.IdPersonaNavigation?.ApellidoMaterno}".Trim(),
                    Email = e.Email ?? "",
                    MateriasInscritas = mat?.Materias ?? 0,
                    FechaInscripcion = eg.FechaInscripcion,
                    Activo = e.Activo,
                    EstatusAcademico = (int)e.EstatusAcademico,
                    EstatusAcademicoTexto = e.EstatusAcademico.ToString()
                });
                idsIncluidos.Add(eg.IdEstudiante);
            }

            var faltantesIds = materiasPorEstudiante.Keys.Where(id => !idsIncluidos.Contains(id)).ToList();
            if (faltantesIds.Count > 0)
            {
                var faltantes = await _dbContext.Estudiante
                    .Include(e => e.IdPersonaNavigation)
                    .Where(e => faltantesIds.Contains(e.IdEstudiante))
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var e in faltantes)
                {
                    var mat = materiasPorEstudiante[e.IdEstudiante];
                    dtoList.Add(new EstudianteInscritoDto
                    {
                        IdEstudiante = e.IdEstudiante,
                        Matricula = e.Matricula ?? "",
                        NombreCompleto = $"{e.IdPersonaNavigation?.Nombre} {e.IdPersonaNavigation?.ApellidoPaterno} {e.IdPersonaNavigation?.ApellidoMaterno}".Trim(),
                        Email = e.Email ?? "",
                        MateriasInscritas = mat.Materias,
                        FechaInscripcion = mat.FechaInscripcion,
                        Activo = e.Activo,
                        EstatusAcademico = (int)e.EstatusAcademico,
                        EstatusAcademicoTexto = e.EstatusAcademico.ToString()
                    });
                }
            }

            // Marcar alumnos ya promovidos: tienen grupo activo en un periodo posterior al de este grupo.
            var fechaInicioGrupo = await _dbContext.PeriodoAcademico
                .Where(p => p.IdPeriodoAcademico == grupo.IdPeriodoAcademico)
                .Select(p => (DateOnly?)p.FechaInicio)
                .FirstOrDefaultAsync();

            if (fechaInicioGrupo.HasValue && dtoList.Count > 0)
            {
                var idsTodos = dtoList.Select(d => d.IdEstudiante).ToList();
                var posteriores = await _dbContext.EstudianteGrupo
                    .Where(eg => idsTodos.Contains(eg.IdEstudiante)
                        && eg.Status == StatusEnum.Active
                        && eg.IdGrupo != idGrupo
                        && eg.IdGrupoNavigation.IdPeriodoAcademicoNavigation.FechaInicio > fechaInicioGrupo.Value)
                    .Select(eg => new
                    {
                        eg.IdEstudiante,
                        Nombre = eg.IdGrupoNavigation.IdPeriodoAcademicoNavigation.Nombre,
                        Fecha = eg.IdGrupoNavigation.IdPeriodoAcademicoNavigation.FechaInicio
                    })
                    .ToListAsync();

                var promDict = posteriores
                    .GroupBy(x => x.IdEstudiante)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Fecha).First().Nombre);

                foreach (var d in dtoList)
                {
                    if (promDict.TryGetValue(d.IdEstudiante, out var destino))
                    {
                        d.Promovido = true;
                        d.PromovidoA = destino?.Trim();
                    }
                }
            }

            return new EstudiantesGrupoDto
            {
                IdGrupo = grupo.IdGrupo,
                CodigoGrupo = grupo.CodigoGrupo ?? "N/A",
                NombreGrupo = grupo.NombreGrupo,
                TotalEstudiantes = dtoList.Count,
                Estudiantes = dtoList.OrderBy(x => x.NombreCompleto).ToList()
            };
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

            if (estudiantes.Count > 0)
                return estudiantes;

            var grupoMateria = await _dbContext.GrupoMateria
                .FirstOrDefaultAsync(gm => gm.IdGrupoMateria == idGrupoMateria);

            if (grupoMateria == null)
                return estudiantes;

            var estudiantesGrupo = await _dbContext.EstudianteGrupo
                .Include(eg => eg.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(eg => eg.IdEstudianteNavigation.IdPlanActualNavigation)
                .Where(eg => eg.IdGrupo == grupoMateria.IdGrupo && eg.Status == StatusEnum.Active)
                .OrderBy(eg => eg.IdEstudianteNavigation.Matricula)
                .ToListAsync();

            var inscripcionesCreadas = new List<Inscripcion>();
            foreach (var eg in estudiantesGrupo)
            {
                var inscripcion = new Inscripcion
                {
                    IdEstudiante = eg.IdEstudiante,
                    IdGrupoMateria = idGrupoMateria,
                    FechaInscripcion = DateTime.UtcNow,
                    Estado = EstadoInscripcionEnum.Inscrito.ToString(),
                    Status = StatusEnum.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Sistema"
                };
                inscripcionesCreadas.Add(inscripcion);
                await _dbContext.Inscripcion.AddAsync(inscripcion);
            }

            if (inscripcionesCreadas.Count > 0)
                await _dbContext.SaveChangesAsync();

            return estudiantesGrupo.Select((eg, idx) => new EstudianteInscritoDto
            {
                IdEstudiante = eg.IdEstudiante,
                Matricula = eg.IdEstudianteNavigation.Matricula ?? string.Empty,
                NombreCompleto = eg.IdEstudianteNavigation.IdPersonaNavigation.Nombre + " " +
                                 (eg.IdEstudianteNavigation.IdPersonaNavigation.ApellidoPaterno ?? "") + " " +
                                 (eg.IdEstudianteNavigation.IdPersonaNavigation.ApellidoMaterno ?? ""),
                Email = eg.IdEstudianteNavigation.IdPersonaNavigation.Correo ?? string.Empty,
                Telefono = eg.IdEstudianteNavigation.IdPersonaNavigation.Telefono,
                PlanEstudios = eg.IdEstudianteNavigation.IdPlanActualNavigation != null
                    ? eg.IdEstudianteNavigation.IdPlanActualNavigation.NombrePlanEstudios
                    : null,
                IdInscripcion = inscripcionesCreadas[idx].IdInscripcion,
                MateriasInscritas = 0,
                FechaInscripcion = eg.FechaInscripcion,
                Estado = eg.Estado ?? "Inscrito"
            }).ToList();
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

            var estudiantesDelGrupo = await _dbContext.EstudianteGrupo
                .Where(eg => eg.IdGrupo == idGrupo && eg.Status == StatusEnum.Active)
                .Select(eg => eg.IdEstudiante)
                .ToListAsync(ct);

            if (estudiantesDelGrupo.Count > 0)
            {
                foreach (var idEstudiante in estudiantesDelGrupo)
                {
                    _dbContext.Inscripcion.Add(new Inscripcion
                    {
                        IdEstudiante = idEstudiante,
                        IdGrupoMateria = grupoMateria.IdGrupoMateria,
                        FechaInscripcion = DateTime.UtcNow,
                        Estado = "Inscrito",
                        Status = StatusEnum.Active,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Sistema"
                    });
                }
                await _dbContext.SaveChangesAsync(ct);
            }

            return grupoMateria;
        }

        public async Task<SincronizacionInscripcionesResultDto> SincronizarInscripcionesGrupoAsync(
            int idGrupo,
            CancellationToken ct = default)
        {
            var resultado = new SincronizacionInscripcionesResultDto { IdGrupo = idGrupo };

            var grupo = await _dbContext.Grupo
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo && g.Status == StatusEnum.Active, ct);
            if (grupo == null)
                throw new KeyNotFoundException($"Grupo {idGrupo} no encontrado");

            resultado.NombreGrupo = grupo.NombreGrupo;
            resultado.CodigoGrupo = grupo.CodigoGrupo;

            var estudiantes = await _dbContext.EstudianteGrupo
                .Where(eg => eg.IdGrupo == idGrupo && eg.Status == StatusEnum.Active)
                .Select(eg => eg.IdEstudiante)
                .ToListAsync(ct);

            var materias = await _dbContext.GrupoMateria
                .Where(gm => gm.IdGrupo == idGrupo && gm.Status == StatusEnum.Active)
                .Select(gm => gm.IdGrupoMateria)
                .ToListAsync(ct);

            resultado.TotalEstudiantes = estudiantes.Count;
            resultado.TotalMaterias = materias.Count;

            var existentes = await _dbContext.Inscripcion
                .Where(i => estudiantes.Contains(i.IdEstudiante)
                    && materias.Contains(i.IdGrupoMateria)
                    && i.Status == StatusEnum.Active)
                .Select(i => new { i.IdEstudiante, i.IdGrupoMateria })
                .ToListAsync(ct);

            var existentesSet = existentes.Select(e => (e.IdEstudiante, e.IdGrupoMateria)).ToHashSet();
            int creadas = 0;

            foreach (var idEst in estudiantes)
            {
                foreach (var idGm in materias)
                {
                    if (existentesSet.Contains((idEst, idGm))) continue;
                    _dbContext.Inscripcion.Add(new Inscripcion
                    {
                        IdEstudiante = idEst,
                        IdGrupoMateria = idGm,
                        FechaInscripcion = DateTime.UtcNow,
                        Estado = "Inscrito",
                        Status = StatusEnum.Active,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Sistema"
                    });
                    creadas++;
                }
            }

            if (creadas > 0)
                await _dbContext.SaveChangesAsync(ct);

            resultado.InscripcionesCreadas = creadas;
            resultado.YaEstabaSincronizado = creadas == 0;
            return resultado;
        }

        public async Task<bool> QuitarMateriaDelGrupoAsync(int idGrupoMateria, bool forzar = false, bool conservarHistorial = false, CancellationToken ct = default)
        {
            var grupoMateria = await _dbContext.GrupoMateria
                .FirstOrDefaultAsync(gm => gm.IdGrupoMateria == idGrupoMateria && gm.Status == StatusEnum.Active, ct);

            if (grupoMateria == null)
                return false;

            var inscripciones = await _dbContext.Inscripcion
                .Where(i => i.IdGrupoMateria == idGrupoMateria && i.Status == StatusEnum.Active)
                .ToListAsync(ct);

            if (inscripciones.Any() && !forzar && !conservarHistorial)
                throw new InvalidOperationException($"No se puede quitar la materia porque tiene {inscripciones.Count} estudiante(s) inscrito(s). Use 'forzar' para cancelar las inscripciones o 'conservarHistorial' para quitarla manteniendo las calificaciones.");

            var ahora = DateTime.UtcNow;

            if (inscripciones.Any() && !conservarHistorial)
            {
                var inscripcionIds = inscripciones.Select(i => i.IdInscripcion).ToList();

                var calificacionesParciales = await _dbContext.CalificacionesParciales
                    .Where(cp => inscripcionIds.Contains(cp.InscripcionId) && cp.Status == StatusEnum.Active)
                    .ToListAsync(ct);
                foreach (var cp in calificacionesParciales)
                {
                    cp.Status = StatusEnum.Deleted;
                    cp.UpdatedAt = ahora;
                }

                var parcialIds = calificacionesParciales.Select(cp => cp.Id).ToList();
                var detalles = await _dbContext.CalificacionDetalle
                    .Where(cd => parcialIds.Contains(cd.CalificacionParcialId) && cd.Status == StatusEnum.Active)
                    .ToListAsync(ct);
                foreach (var d in detalles)
                {
                    d.Status = StatusEnum.Deleted;
                    d.UpdatedAt = ahora;
                }

                foreach (var i in inscripciones)
                {
                    i.Status = StatusEnum.Deleted;
                    i.UpdatedAt = ahora;
                }
            }

            var horarios = await _dbContext.Horario
                .Where(h => h.IdGrupoMateria == idGrupoMateria && h.Status == StatusEnum.Active)
                .ToListAsync(ct);
            foreach (var h in horarios)
            {
                h.Status = StatusEnum.Deleted;
                h.UpdatedAt = ahora;
            }

            grupoMateria.Status = StatusEnum.Deleted;
            grupoMateria.UpdatedAt = ahora;
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
                .Include(gm => gm.Horario)
                    .ThenInclude(h => h.IdDiaSemanaNavigation)
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
                TieneCupo = gm.Inscripcion.Count(i => i.Status == StatusEnum.Active) < gm.Cupo,
                HorarioJson = gm.Horario?.Select(h => new HorarioItemDto
                {
                    Dia = h.IdDiaSemanaNavigation?.Nombre ?? "",
                    HoraInicio = h.HoraInicio.ToString("HH:mm"),
                    HoraFin = h.HoraFin.ToString("HH:mm"),
                    Aula = h.Aula
                }).ToList()
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
            {
                // Alumnos inscritos "directo al grupo" (sin materias, ej. equivalencias):
                // no tienen inscripciones a materias; validar por su vínculo directo al grupo del cuatrimestre.
                var inscritoDirecto = await _dbContext.EstudianteGrupo
                    .AnyAsync(eg => eg.IdEstudiante == idEstudiante
                        && eg.Status == StatusEnum.Active
                        && eg.IdGrupoNavigation.NumeroCuatrimestre == cuatrimestreActual
                        && eg.IdGrupoNavigation.Status == StatusEnum.Active, ct);

                if (inscritoDirecto)
                    return (true, "Inscripción directa (sin materias): elegible para promoción");

                return (false, "No tiene inscripciones registradas en este cuatrimestre");
            }

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

        public async Task<List<PeriodoConEstudiantesDto>> ObtenerPeriodosConEstudiantesAsync(CancellationToken ct = default)
        {
            var gruposActivos = await _dbContext.Grupo
                .Where(g => g.Status == StatusEnum.Active)
                .Select(g => new { g.IdGrupo, g.IdPeriodoAcademico })
                .ToListAsync(ct);
            var grupoPeriodo = gruposActivos
                .GroupBy(g => g.IdGrupo)
                .ToDictionary(g => g.Key, g => g.First().IdPeriodoAcademico);

            var pares = await _dbContext.Inscripcion
                .Where(i => i.Status == StatusEnum.Active)
                .Select(i => new { i.IdEstudiante, IdGrupo = i.IdGrupoMateriaNavigation.IdGrupo })
                .Distinct()
                .ToListAsync(ct);

            var conteo = new Dictionary<int, HashSet<int>>();
            foreach (var par in pares)
            {
                if (!grupoPeriodo.TryGetValue(par.IdGrupo, out var idPeriodo)) continue;
                if (!conteo.TryGetValue(idPeriodo, out var set))
                {
                    set = new HashSet<int>();
                    conteo[idPeriodo] = set;
                }
                set.Add(par.IdEstudiante);
            }

            var periodoIds = conteo.Keys.ToList();
            var periodos = await _dbContext.PeriodoAcademico
                .Include(p => p.IdPeriodicidadNavigation)
                .Where(p => periodoIds.Contains(p.IdPeriodoAcademico))
                .ToListAsync(ct);

            return periodos
                .Select(p => new PeriodoConEstudiantesDto
                {
                    IdPeriodoAcademico = p.IdPeriodoAcademico,
                    Nombre = p.Nombre,
                    Clave = p.Clave,
                    Periodicidad = p.IdPeriodicidadNavigation?.DescPeriodicidad ?? "",
                    Anio = p.FechaInicio.Year,
                    EsPeriodoActual = p.EsPeriodoActual,
                    TotalEstudiantes = conteo[p.IdPeriodoAcademico].Count
                })
                .OrderByDescending(r => r.Anio)
                .ThenBy(r => r.Nombre)
                .ToList();
        }

        public async Task<PromocionMasivaPreviewDto> PromocionMasivaPreviewAsync(int idPeriodoOrigen, int idPeriodoDestino, CancellationToken ct = default)
        {
            var periodoOrigen = await _dbContext.PeriodoAcademico
                .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == idPeriodoOrigen, ct)
                ?? throw new KeyNotFoundException($"Periodo origen {idPeriodoOrigen} no encontrado");
            var periodoDestino = await _dbContext.PeriodoAcademico
                .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == idPeriodoDestino, ct)
                ?? throw new KeyNotFoundException($"Periodo destino {idPeriodoDestino} no encontrado");

            var grupos = await _dbContext.Grupo
                .Include(g => g.IdPlanEstudiosNavigation).ThenInclude(p => p.IdCampusNavigation)
                .Include(g => g.IdTurnoNavigation)
                .Where(g => g.IdPeriodoAcademico == idPeriodoOrigen && g.Status == StatusEnum.Active)
                .OrderBy(g => g.IdPlanEstudios).ThenBy(g => g.NumeroCuatrimestre)
                .ToListAsync(ct);

            var estudiantesEnDestino = await ObtenerEstudiantesEnPeriodoAsync(idPeriodoDestino, ct);

            var maxCuatriPorPlan = new Dictionary<int, int>();
            var dto = new PromocionMasivaPreviewDto
            {
                IdPeriodoOrigen = idPeriodoOrigen,
                PeriodoOrigen = periodoOrigen.Nombre,
                IdPeriodoDestino = idPeriodoDestino,
                PeriodoDestino = periodoDestino.Nombre
            };

            var procesados = new HashSet<int>();

            foreach (var grupo in grupos)
            {
                var maxCuatri = await ObtenerMaxCuatrimestreAsync(grupo.IdPlanEstudios, maxCuatriPorPlan, ct);
                var esUltimo = maxCuatri > 0 && grupo.NumeroCuatrimestre >= maxCuatri;

                var idsPorInscripcion = await _dbContext.Inscripcion
                    .Where(i => i.IdGrupoMateriaNavigation.IdGrupo == grupo.IdGrupo && i.Status == StatusEnum.Active)
                    .Select(i => i.IdEstudiante).Distinct().ToListAsync(ct);

                // También incluir alumnos inscritos "directo al grupo" (sin materias, ej. equivalencias)
                var idsPorGrupoDirecto = await _dbContext.EstudianteGrupo
                    .Where(eg => eg.IdGrupo == grupo.IdGrupo && eg.Status == StatusEnum.Active)
                    .Select(eg => eg.IdEstudiante).Distinct().ToListAsync(ct);

                var estudiantesIds = idsPorInscripcion.Union(idsPorGrupoDirecto).Distinct().ToList();

                var recibos = await ObtenerSaldosPorEstudianteAsync(estudiantesIds, ct);

                var grupoDto = new PromocionMasivaGrupoDto
                {
                    IdGrupo = grupo.IdGrupo,
                    Campus = grupo.IdPlanEstudiosNavigation?.IdCampusNavigation?.Nombre ?? "",
                    PlanEstudios = grupo.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "",
                    CodigoGrupo = grupo.CodigoGrupo ?? "",
                    NombreGrupo = grupo.NombreGrupo ?? "",
                    Turno = grupo.IdTurnoNavigation?.Nombre ?? "",
                    CuatrimestreOrigen = grupo.NumeroCuatrimestre,
                    CuatrimestreDestino = esUltimo ? null : grupo.NumeroCuatrimestre + 1,
                    EsUltimoCuatrimestre = esUltimo,
                    TotalEstudiantes = estudiantesIds.Count
                };

                foreach (var idEst in estudiantesIds)
                {
                    if (procesados.Contains(idEst)) continue;
                    procesados.Add(idEst);

                    var saldo = recibos.TryGetValue(idEst, out var s) ? s : 0;
                    if (saldo > 0) { grupoDto.ConAdeudo++; grupoDto.SaldoPendiente += saldo; }

                    if (estudiantesEnDestino.Contains(idEst))
                    {
                        grupoDto.ExcluidosNuevoIngreso++;
                        continue;
                    }

                    var (puede, _) = await ValidarPromocionEstudianteAsync(idEst, grupo.NumeroCuatrimestre, 70, ct);
                    if (!puede)
                    {
                        grupoDto.ConError++;
                        continue;
                    }

                    if (esUltimo) grupoDto.AEgresar++;
                    else grupoDto.APromover++;
                }

                dto.Grupos.Add(grupoDto);
            }

            dto.TotalGrupos = dto.Grupos.Count;
            dto.TotalEstudiantes = dto.Grupos.Sum(g => g.TotalEstudiantes);
            dto.TotalAPromover = dto.Grupos.Sum(g => g.APromover);
            dto.TotalAEgresar = dto.Grupos.Sum(g => g.AEgresar);
            dto.TotalExcluidosNuevoIngreso = dto.Grupos.Sum(g => g.ExcluidosNuevoIngreso);
            dto.TotalConAdeudo = dto.Grupos.Sum(g => g.ConAdeudo);
            dto.TotalSaldoPendiente = dto.Grupos.Sum(g => g.SaldoPendiente);
            dto.TotalConError = dto.Grupos.Sum(g => g.ConError);

            return dto;
        }

        public async Task<PromocionMasivaResultDto> PromocionMasivaAsync(PromocionMasivaRequest request, CancellationToken ct = default)
        {
            var periodoOrigen = await _dbContext.PeriodoAcademico
                .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == request.IdPeriodoOrigen, ct)
                ?? throw new KeyNotFoundException($"Periodo origen {request.IdPeriodoOrigen} no encontrado");
            _ = await _dbContext.PeriodoAcademico
                .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == request.IdPeriodoDestino, ct)
                ?? throw new KeyNotFoundException($"Periodo destino {request.IdPeriodoDestino} no encontrado");

            var gruposExcluidos = request.GruposExcluidos != null ? new HashSet<int>(request.GruposExcluidos) : new HashSet<int>();
            var estudiantesExcluidos = request.EstudiantesExcluidos != null ? new HashSet<int>(request.EstudiantesExcluidos) : new HashSet<int>();

            var grupos = await _dbContext.Grupo
                .Include(g => g.IdPlanEstudiosNavigation).ThenInclude(p => p.IdCampusNavigation)
                .Where(g => g.IdPeriodoAcademico == request.IdPeriodoOrigen && g.Status == StatusEnum.Active)
                .OrderBy(g => g.IdPlanEstudios).ThenBy(g => g.NumeroCuatrimestre)
                .ToListAsync(ct);

            var estudiantesEnDestino = await ObtenerEstudiantesEnPeriodoAsync(request.IdPeriodoDestino, ct);

            var maxCuatriPorPlan = new Dictionary<int, int>();
            var result = new PromocionMasivaResultDto();
            var procesados = new HashSet<int>();
            int gruposCreados = 0;

            foreach (var grupo in grupos)
            {
                if (gruposExcluidos.Contains(grupo.IdGrupo)) continue;

                var maxCuatri = await ObtenerMaxCuatrimestreAsync(grupo.IdPlanEstudios, maxCuatriPorPlan, ct);
                var esUltimo = maxCuatri > 0 && grupo.NumeroCuatrimestre >= maxCuatri;

                var campus = grupo.IdPlanEstudiosNavigation?.IdCampusNavigation?.Nombre ?? "";
                var plan = grupo.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "";
                var nombreGrupo = grupo.NombreGrupo ?? grupo.CodigoGrupo ?? "";

                var idsPorInscripcion = await _dbContext.Inscripcion
                    .Where(i => i.IdGrupoMateriaNavigation.IdGrupo == grupo.IdGrupo && i.Status == StatusEnum.Active)
                    .Select(i => i.IdEstudiante).Distinct().ToListAsync(ct);

                // También incluir alumnos inscritos "directo al grupo" (sin materias, ej. equivalencias)
                var idsPorGrupoDirecto = await _dbContext.EstudianteGrupo
                    .Where(eg => eg.IdGrupo == grupo.IdGrupo && eg.Status == StatusEnum.Active)
                    .Select(eg => eg.IdEstudiante).Distinct().ToListAsync(ct);

                var estudiantesIds = idsPorInscripcion.Union(idsPorGrupoDirecto).Distinct().ToList();

                var recibos = await ObtenerSaldosPorEstudianteAsync(estudiantesIds, ct);

                var estudiantesData = await _dbContext.Estudiante
                    .Include(e => e.IdPersonaNavigation)
                    .Where(e => estudiantesIds.Contains(e.IdEstudiante))
                    .ToListAsync(ct);
                var estDict = estudiantesData.ToDictionary(e => e.IdEstudiante);

                Grupo? grupoDestino = null;
                bool grupoDestinoResuelto = false;

                foreach (var idEst in estudiantesIds)
                {
                    if (procesados.Contains(idEst)) continue;
                    if (estudiantesExcluidos.Contains(idEst)) continue;
                    procesados.Add(idEst);

                    estDict.TryGetValue(idEst, out var estudiante);
                    var persona = estudiante?.IdPersonaNavigation;
                    var nombre = persona != null
                        ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                        : "";
                    var saldo = recibos.TryGetValue(idEst, out var s) ? s : 0;

                    var item = new PromocionMasivaItemDto
                    {
                        IdEstudiante = idEst,
                        Matricula = estudiante?.Matricula ?? "",
                        NombreCompleto = nombre,
                        Campus = campus,
                        PlanEstudios = plan,
                        Grupo = nombreGrupo,
                        Cuatrimestre = grupo.NumeroCuatrimestre,
                        Periodo = periodoOrigen.Nombre,
                        TieneAdeudo = saldo > 0,
                        SaldoPendiente = saldo
                    };

                    if (estudiantesEnDestino.Contains(idEst))
                    {
                        item.Accion = "Error";
                        item.Detalle = "Excluido: ya tiene grupo en el periodo destino (nuevo ingreso)";
                        result.Errores.Add(item);
                        continue;
                    }

                    var (puede, motivo) = await ValidarPromocionEstudianteAsync(idEst, grupo.NumeroCuatrimestre, 70, ct);
                    if (!puede)
                    {
                        item.Accion = "Error";
                        item.Detalle = motivo;
                        result.Errores.Add(item);
                        continue;
                    }

                    try
                    {
                        if (esUltimo)
                        {
                            if (estudiante != null)
                            {
                                estudiante.EstatusAcademico = Core.Enums.EstudianteStatusAcademicoEnum.Egresado;
                                await _dbContext.SaveChangesAsync(ct);
                            }
                            item.Accion = "Egresado";
                            item.Detalle = "Egresado del último cuatrimestre";
                            result.Egresados.Add(item);
                        }
                        else
                        {
                            if (!grupoDestinoResuelto)
                            {
                                var existiaAntes = await _dbContext.Grupo.AnyAsync(g =>
                                    g.IdPlanEstudios == grupo.IdPlanEstudios
                                    && g.NumeroCuatrimestre == grupo.NumeroCuatrimestre + 1
                                    && g.NumeroGrupo == grupo.NumeroGrupo
                                    && g.IdTurno == grupo.IdTurno
                                    && g.IdPeriodoAcademico == request.IdPeriodoDestino
                                    && g.Status == StatusEnum.Active, ct);
                                grupoDestino = await ObtenerOCrearGrupoSiguienteAsync(grupo.IdGrupo, request.IdPeriodoDestino, true, ct);
                                if (grupoDestino != null && !existiaAntes) gruposCreados++;
                                grupoDestinoResuelto = true;
                            }

                            if (grupoDestino == null)
                            {
                                item.Accion = "Error";
                                item.Detalle = "No se pudo crear/obtener el grupo destino";
                                result.Errores.Add(item);
                                continue;
                            }

                            await InscribirEstudianteGrupoAsync(grupoDestino.IdGrupo, idEst, false, "Promoción masiva");
                            item.Accion = "Promovido";
                            item.Detalle = $"Promovido a {grupoDestino.NombreGrupo ?? grupoDestino.CodigoGrupo}";
                            result.Promovidos.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        item.Accion = "Error";
                        item.Detalle = ex.InnerException?.Message ?? ex.Message;
                        result.Errores.Add(item);
                    }
                }
            }

            result.TotalPromovidos = result.Promovidos.Count;
            result.TotalEgresados = result.Egresados.Count;
            result.TotalErrores = result.Errores.Count;
            result.GruposCreados = gruposCreados;
            result.Mensaje = $"Promoción masiva completada: {result.TotalPromovidos} promovidos, {result.TotalEgresados} egresados, {result.TotalErrores} con error.";

            return result;
        }

        private async Task<HashSet<int>> ObtenerEstudiantesEnPeriodoAsync(int idPeriodo, CancellationToken ct)
        {
            var gruposPeriodoIds = await _dbContext.Grupo
                .Where(g => g.IdPeriodoAcademico == idPeriodo && g.Status == StatusEnum.Active)
                .Select(g => g.IdGrupo).ToListAsync(ct);

            if (gruposPeriodoIds.Count == 0) return new HashSet<int>();

            var ids = await _dbContext.Inscripcion
                .Where(i => i.Status == StatusEnum.Active && gruposPeriodoIds.Contains(i.IdGrupoMateriaNavigation.IdGrupo))
                .Select(i => i.IdEstudiante).Distinct().ToListAsync(ct);

            var idsDirecto = await _dbContext.EstudianteGrupo
                .Where(eg => eg.Status == StatusEnum.Active && gruposPeriodoIds.Contains(eg.IdGrupo))
                .Select(eg => eg.IdEstudiante).Distinct().ToListAsync(ct);

            var set = new HashSet<int>(ids);
            set.UnionWith(idsDirecto);
            return set;
        }

        private async Task<int> ObtenerMaxCuatrimestreAsync(int idPlanEstudios, Dictionary<int, int> cache, CancellationToken ct)
        {
            if (cache.TryGetValue(idPlanEstudios, out var maxCuatri)) return maxCuatri;

            maxCuatri = await _dbContext.MateriaPlan
                .Where(mp => mp.IdPlanEstudios == idPlanEstudios && mp.Status == StatusEnum.Active)
                .Select(mp => (int?)mp.Cuatrimestre).MaxAsync(ct) ?? 0;

            if (maxCuatri == 0)
            {
                // Plan sin currículo cargado (MateriaPlan vacío, ej. equivalencias):
                // estimar por la duración del plan (4 meses por cuatrimestre) y respetar
                // el corrimiento de numeración tomando el cuatrimestre más alto de sus grupos activos.
                var duracionMeses = await _dbContext.PlanEstudios
                    .Where(p => p.IdPlanEstudios == idPlanEstudios)
                    .Select(p => p.DuracionMeses)
                    .FirstOrDefaultAsync(ct) ?? 0;
                var porDuracion = duracionMeses > 0 ? (int)Math.Ceiling(duracionMeses / 4.0) : 0;

                var maxCuatriGrupos = await _dbContext.Grupo
                    .Where(g => g.IdPlanEstudios == idPlanEstudios && g.Status == StatusEnum.Active)
                    .Select(g => (int?)g.NumeroCuatrimestre).MaxAsync(ct) ?? 0;

                maxCuatri = Math.Max(porDuracion, maxCuatriGrupos);
            }

            cache[idPlanEstudios] = maxCuatri;
            return maxCuatri;
        }

        private async Task<Dictionary<int, decimal>> ObtenerSaldosPorEstudianteAsync(List<int> estudiantesIds, CancellationToken ct)
        {
            if (estudiantesIds.Count == 0) return new Dictionary<int, decimal>();

            return await _dbContext.Recibo
                .Where(r => r.IdEstudiante.HasValue
                    && estudiantesIds.Contains(r.IdEstudiante.Value)
                    && r.Status == StatusEnum.Active
                    && r.Estatus != Core.Enums.EstatusRecibo.PAGADO
                    && r.Estatus != Core.Enums.EstatusRecibo.CANCELADO)
                .GroupBy(r => r.IdEstudiante!.Value)
                .Select(g => new { Id = g.Key, Saldo = g.Sum(x => x.Saldo) })
                .ToDictionaryAsync(x => x.Id, x => x.Saldo, ct);
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

                var materiasGrupo = await _dbContext.GrupoMateria
                    .Where(gm => gm.IdGrupo == idGrupo && gm.Status == StatusEnum.Active)
                    .Select(gm => gm.IdGrupoMateria)
                    .ToListAsync(ct);

                var inscripcionesExistentes = await _dbContext.Inscripcion
                    .Where(i => i.IdEstudiante == idEstudiante
                        && materiasGrupo.Contains(i.IdGrupoMateria))
                    .ToListAsync(ct);

                foreach (var idGrupoMateria in materiasGrupo)
                {
                    var existente = inscripcionesExistentes.FirstOrDefault(i => i.IdGrupoMateria == idGrupoMateria);
                    if (existente != null)
                    {
                        if (existente.Status != StatusEnum.Active)
                        {
                            existente.Status = StatusEnum.Active;
                            existente.Estado = "Inscrito";
                            existente.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        _dbContext.Inscripcion.Add(new Inscripcion
                        {
                            IdEstudiante = idEstudiante,
                            IdGrupoMateria = idGrupoMateria,
                            FechaInscripcion = DateTime.UtcNow,
                            Estado = "Inscrito",
                            Status = StatusEnum.Active,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = estudianteGrupo.CreatedBy ?? string.Empty
                        });
                    }
                }

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

            var materiasGrupo = await _dbContext.GrupoMateria
                .Where(gm => gm.IdGrupo == estudianteGrupo.IdGrupo)
                .Select(gm => gm.IdGrupoMateria)
                .ToListAsync(ct);

            var inscripciones = await _dbContext.Inscripcion
                .Where(i => i.IdEstudiante == estudianteGrupo.IdEstudiante
                    && materiasGrupo.Contains(i.IdGrupoMateria)
                    && i.Status == StatusEnum.Active)
                .ToListAsync(ct);

            foreach (var insc in inscripciones)
            {
                insc.Status = StatusEnum.Deleted;
                insc.UpdatedAt = DateTime.UtcNow;
            }

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
                            .FirstOrDefaultAsync(p => p.Curp!.ToUpper() == curpLimpio.ToUpper() && p.Status == StatusEnum.Active, ct);
                    }

                    if (personaExistente == null && !string.IsNullOrWhiteSpace(estudianteDto.Correo))
                    {
                        var correoLimpio = estudianteDto.Correo.Trim().ToLowerInvariant();
                        personaExistente = await _dbContext.Persona
                            .FirstOrDefaultAsync(p => p.Correo!.ToLower() == correoLimpio && p.Status == StatusEnum.Active, ct);
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

        public async Task<AgregarEstudianteIrregularResponse> AgregarEstudianteIrregularAGrupoAsync(
            AgregarEstudianteIrregularRequest request,
            CancellationToken ct = default)
        {
            var response = new AgregarEstudianteIrregularResponse();
            var datos = request.Datos;

            var grupo = await _dbContext.Grupo
                .Include(g => g.IdPlanEstudiosNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == request.IdGrupo && g.Status == StatusEnum.Active, ct);

            if (grupo == null)
            {
                response.Exitoso = false;
                response.Mensaje = "Grupo no encontrado";
                return response;
            }

            if (string.IsNullOrWhiteSpace(datos.Nombre) || string.IsNullOrWhiteSpace(datos.ApellidoPaterno))
            {
                response.Exitoso = false;
                response.Mensaje = "El nombre y el apellido paterno son obligatorios";
                return response;
            }

            var nombrePlan = grupo.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "";
            var idPlanEstudios = grupo.IdPlanEstudios;

            int idEstudianteFinal;
            string matriculaFinal;
            string nombreCompleto = $"{datos.Nombre} {datos.ApellidoPaterno} {datos.ApellidoMaterno}".Trim();

            using (var transaction = await _dbContext.Database.BeginTransactionAsync(ct))
            {
                try
                {
                    var curpLimpio = datos.Curp?.Trim().ToUpperInvariant();
                    var curpValido = !string.IsNullOrWhiteSpace(curpLimpio)
                        && curpLimpio.Length == 18
                        && curpLimpio != "CURP"
                        && !curpLimpio.StartsWith("CURP");

                    Persona? personaExistente = null;
                    if (curpValido)
                    {
                        personaExistente = await _dbContext.Persona
                            .FirstOrDefaultAsync(p => p.Curp!.ToUpper() == curpLimpio.ToUpper() && p.Status == StatusEnum.Active, ct);
                    }

                    if (personaExistente == null && !string.IsNullOrWhiteSpace(datos.Correo))
                    {
                        var correoLimpio = datos.Correo.Trim().ToLowerInvariant();
                        personaExistente = await _dbContext.Persona
                            .FirstOrDefaultAsync(p => p.Correo!.ToLower() == correoLimpio && p.Status == StatusEnum.Active, ct);
                    }

                    Persona persona;
                    if (personaExistente != null)
                    {
                        persona = personaExistente;
                    }
                    else
                    {
                        persona = new Persona
                        {
                            Nombre = datos.Nombre.Trim(),
                            ApellidoPaterno = datos.ApellidoPaterno.Trim(),
                            ApellidoMaterno = datos.ApellidoMaterno?.Trim(),
                            Curp = curpValido ? curpLimpio : null,
                            Correo = datos.Correo?.Trim().ToLowerInvariant(),
                            Telefono = datos.Telefono?.Trim(),
                            Celular = datos.Celular?.Trim(),
                            FechaNacimiento = datos.GetFechaNacimientoAsDateOnly(),
                            IdGenero = datos.IdGenero,
                            Status = StatusEnum.Active,
                            CreatedAt = DateTime.UtcNow
                        };

                        _dbContext.Persona.Add(persona);
                        await _dbContext.SaveChangesAsync(ct);
                    }

                    var estudianteExistente = await _dbContext.Estudiante
                        .FirstOrDefaultAsync(e => e.IdPersona == persona.IdPersona && e.Status == StatusEnum.Active, ct);

                    Estudiante estudiante;
                    if (estudianteExistente != null)
                    {
                        estudiante = estudianteExistente;
                    }
                    else
                    {
                        var matricula = !string.IsNullOrWhiteSpace(datos.Matricula)
                            ? datos.Matricula.Trim()
                            : await _matriculaService.GenerarMatriculaAsync(nombrePlan);

                        if (await _matriculaService.ExisteMatriculaAsync(matricula))
                        {
                            await transaction.RollbackAsync(ct);
                            response.Exitoso = false;
                            response.Mensaje = $"La matrícula {matricula} ya existe";
                            return response;
                        }

                        estudiante = new Estudiante
                        {
                            Matricula = matricula,
                            IdPersona = persona.IdPersona,
                            Email = datos.Correo?.Trim().ToLowerInvariant(),
                            FechaIngreso = DateOnly.FromDateTime(DateTime.Now),
                            IdPlanActual = idPlanEstudios,
                            Activo = true,
                            Status = StatusEnum.Active,
                            CreatedAt = DateTime.UtcNow
                        };

                        _dbContext.Estudiante.Add(estudiante);
                        await _dbContext.SaveChangesAsync(ct);
                    }

                    var vinculoExistente = await _dbContext.EstudianteGrupo
                        .FirstOrDefaultAsync(eg => eg.IdEstudiante == estudiante.IdEstudiante
                            && eg.IdGrupo == request.IdGrupo
                            && eg.Status == StatusEnum.Active, ct);

                    EstudianteGrupo estudianteGrupo;
                    if (vinculoExistente != null)
                    {
                        estudianteGrupo = vinculoExistente;
                    }
                    else
                    {
                        estudianteGrupo = new EstudianteGrupo
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
                    }

                    var materiasGrupo = await _dbContext.GrupoMateria
                        .Where(gm => gm.IdGrupo == request.IdGrupo && gm.Status == StatusEnum.Active)
                        .Select(gm => gm.IdGrupoMateria)
                        .ToListAsync(ct);

                    var inscripcionesActivas = await _dbContext.Inscripcion
                        .Where(i => i.IdEstudiante == estudiante.IdEstudiante
                            && materiasGrupo.Contains(i.IdGrupoMateria)
                            && i.Status == StatusEnum.Active)
                        .Select(i => i.IdGrupoMateria)
                        .ToListAsync(ct);

                    var inscripcionesEliminadas = await _dbContext.Inscripcion
                        .Where(i => i.IdEstudiante == estudiante.IdEstudiante
                            && materiasGrupo.Contains(i.IdGrupoMateria)
                            && i.Status == StatusEnum.Deleted)
                        .ToListAsync(ct);

                    int materiasInscritas = 0;
                    foreach (var idGrupoMateria in materiasGrupo.Except(inscripcionesActivas))
                    {
                        var reactivar = inscripcionesEliminadas.FirstOrDefault(i => i.IdGrupoMateria == idGrupoMateria);
                        if (reactivar != null)
                        {
                            reactivar.Status = StatusEnum.Active;
                            reactivar.Estado = "Inscrito";
                            reactivar.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            _dbContext.Inscripcion.Add(new Inscripcion
                            {
                                IdEstudiante = estudiante.IdEstudiante,
                                IdGrupoMateria = idGrupoMateria,
                                FechaInscripcion = DateTime.UtcNow,
                                Estado = "Inscrito",
                                Status = StatusEnum.Active,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = estudianteGrupo.CreatedBy ?? string.Empty
                            });
                        }
                        materiasInscritas++;
                    }

                    await _dbContext.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    idEstudianteFinal = estudiante.IdEstudiante;
                    matriculaFinal = estudiante.Matricula;

                    response.IdEstudiante = estudiante.IdEstudiante;
                    response.Matricula = estudiante.Matricula;
                    response.IdEstudianteGrupo = estudianteGrupo.IdEstudianteGrupo;
                    response.MateriasInscritas = materiasInscritas;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(ct);
                    response.Exitoso = false;
                    response.Mensaje = $"Error al crear el estudiante: {ex.Message}";
                    return response;
                }
            }

            response.Exitoso = true;
            response.Mensaje = "Estudiante agregado al grupo correctamente";

            var crearAcceso = request.CrearAcceso || request.CrearCorreoM365;
            if (crearAcceso)
            {
                var dominio = string.IsNullOrWhiteSpace(request.Dominio)
                    ? "usaguanajuato.edu.mx"
                    : request.Dominio.Trim().TrimStart('@').ToLowerInvariant();
                var usuarioCorreo = string.IsNullOrWhiteSpace(request.UsuarioCorreo)
                    ? matriculaFinal
                    : request.UsuarioCorreo.Trim();

                var emailInstitucional = !string.IsNullOrWhiteSpace(request.EmailInstitucional)
                    ? request.EmailInstitucional.Trim().ToLowerInvariant()
                    : $"{usuarioCorreo.ToLowerInvariant()}@{dominio}";

                var accesoResult = await _accesoService.CrearAccesoAsync(new CrearAccesoRequest
                {
                    Tipo = "alumno",
                    EntidadId = idEstudianteFinal,
                    EmailPersonalizado = emailInstitucional,
                    PasswordPersonalizada = request.PasswordPersonalizada
                }, ct);

                if (accesoResult.Exito)
                {
                    response.AccesoCreado = true;
                    response.EmailAcceso = emailInstitucional;
                    response.PasswordTemporal = accesoResult.PasswordTemporal;

                    if (request.CrearCorreoM365)
                    {
                        try
                        {
                            var mailNickname = emailInstitucional.Split('@')[0];
                            var graphResult = await _graphService.CreateUserAsync(new WebApplication2.Core.Requests.MicrosoftGraph.CreateUserRequest
                            {
                                DisplayName = nombreCompleto,
                                UserPrincipalName = emailInstitucional,
                                MailNickname = mailNickname,
                                Password = accesoResult.PasswordTemporal ?? request.PasswordPersonalizada ?? string.Empty,
                                ForceChangePasswordNextSignIn = true,
                                GivenName = datos.Nombre.Trim(),
                                Surname = $"{datos.ApellidoPaterno} {datos.ApellidoMaterno}".Trim(),
                                JobTitle = "Estudiante",
                                Department = nombrePlan
                            }, ct);

                            response.CorreoM365Creado = graphResult.Success;
                            response.MensajeM365 = graphResult.Message;
                        }
                        catch (Exception ex)
                        {
                            response.CorreoM365Creado = false;
                            response.MensajeM365 = $"No se pudo crear el buzón en Microsoft 365: {ex.Message}";
                        }
                    }
                }
                else
                {
                    response.AccesoCreado = false;
                    response.Mensaje += $" (No se creó la cuenta de acceso: {accesoResult.Mensaje})";
                }
            }

            return response;
        }

        public async Task<CambioGrupoResultDto> CambiarEstudianteDeGrupoAsync(CambioGrupoRequestDto request, CancellationToken ct = default)
        {
            var estudianteGrupo = await _dbContext.EstudianteGrupo
                .Include(eg => eg.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(eg => eg.IdGrupoNavigation)
                .FirstOrDefaultAsync(eg => eg.IdEstudianteGrupo == request.IdEstudianteGrupo && eg.Status == StatusEnum.Active, ct);

            if (estudianteGrupo == null)
                return new CambioGrupoResultDto { Exitoso = false, Mensaje = "Inscripción de estudiante no encontrada" };

            var grupoOrigen = estudianteGrupo.IdGrupoNavigation;
            var estudiante = estudianteGrupo.IdEstudianteNavigation;
            var persona = estudiante.IdPersonaNavigation;
            var nombreEstudiante = $"{persona?.Nombre} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim();
            var matricula = estudiante.Matricula;

            var grupoDestino = await _dbContext.Grupo
                .Include(g => g.EstudianteGrupo.Where(eg => eg.Status == StatusEnum.Active))
                .FirstOrDefaultAsync(g => g.IdGrupo == request.IdGrupoDestino && g.Status == StatusEnum.Active, ct);

            if (grupoDestino == null)
                return new CambioGrupoResultDto { Exitoso = false, Mensaje = "Grupo destino no encontrado" };

            if (grupoOrigen.IdPeriodoAcademico != grupoDestino.IdPeriodoAcademico)
                return new CambioGrupoResultDto { Exitoso = false, Mensaje = "El grupo destino pertenece a un período académico diferente" };

            if (!request.Avanzado)
            {
                if (grupoOrigen.IdPlanEstudios != grupoDestino.IdPlanEstudios)
                    return new CambioGrupoResultDto { Exitoso = false, Mensaje = "El grupo destino pertenece a un plan de estudios diferente" };

                if (grupoOrigen.NumeroCuatrimestre != grupoDestino.NumeroCuatrimestre)
                    return new CambioGrupoResultDto
                    {
                        Exitoso = false,
                        Mensaje = "No se puede transferir a un grupo de diferente cuatrimestre. Debe dar de baja al estudiante y reinscribirlo manualmente."
                    };
            }

            var estudiantesActivos = grupoDestino.EstudianteGrupo.Count;
            if (estudiantesActivos >= grupoDestino.CapacidadMaxima)
                return new CambioGrupoResultDto { Exitoso = false, Mensaje = "El grupo destino no tiene cupo disponible" };

            var yaInscrito = await _dbContext.EstudianteGrupo
                .AnyAsync(eg => eg.IdEstudiante == estudiante.IdEstudiante
                    && eg.IdGrupo == request.IdGrupoDestino
                    && eg.Status == StatusEnum.Active, ct);

            if (yaInscrito)
                return new CambioGrupoResultDto { Exitoso = false, Mensaje = "El estudiante ya está inscrito en el grupo destino" };

            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                estudianteGrupo.Status = StatusEnum.Deleted;
                estudianteGrupo.UpdatedAt = DateTime.UtcNow;
                estudianteGrupo.Observaciones = $"Cambio de grupo: {grupoOrigen.NombreGrupo} → {grupoDestino.NombreGrupo}";

                var nuevoEstudianteGrupo = new EstudianteGrupo
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    IdGrupo = request.IdGrupoDestino,
                    FechaInscripcion = DateTime.UtcNow,
                    Estado = "Inscrito",
                    Observaciones = $"Cambio de grupo: {grupoOrigen.NombreGrupo} → {grupoDestino.NombreGrupo}",
                    Status = StatusEnum.Active,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.EstudianteGrupo.Add(nuevoEstudianteGrupo);

                int materiasReinscritas = 0;
                bool planCambiado = grupoOrigen.IdPlanEstudios != grupoDestino.IdPlanEstudios;
                bool rehacerMaterias = grupoOrigen.IdGrupo != grupoDestino.IdGrupo;

                if (rehacerMaterias)
                {
                    var materiasOrigen = await _dbContext.GrupoMateria
                        .Where(gm => gm.IdGrupo == grupoOrigen.IdGrupo && gm.Status == StatusEnum.Active)
                        .Select(gm => gm.IdGrupoMateria)
                        .ToListAsync(ct);

                    var inscOrigen = await _dbContext.Inscripcion
                        .Where(i => i.IdEstudiante == estudiante.IdEstudiante
                            && materiasOrigen.Contains(i.IdGrupoMateria)
                            && i.Status == StatusEnum.Active)
                        .ToListAsync(ct);
                    foreach (var insc in inscOrigen)
                    {
                        insc.Status = StatusEnum.Deleted;
                        insc.UpdatedAt = DateTime.UtcNow;
                    }

                    var materiasDestino = await _dbContext.GrupoMateria
                        .Where(gm => gm.IdGrupo == grupoDestino.IdGrupo && gm.Status == StatusEnum.Active)
                        .Select(gm => gm.IdGrupoMateria)
                        .ToListAsync(ct);

                    var inscDestinoExistentes = await _dbContext.Inscripcion
                        .Where(i => i.IdEstudiante == estudiante.IdEstudiante
                            && materiasDestino.Contains(i.IdGrupoMateria))
                        .ToListAsync(ct);

                    foreach (var idGrupoMateria in materiasDestino)
                    {
                        var existente = inscDestinoExistentes.FirstOrDefault(i => i.IdGrupoMateria == idGrupoMateria);
                        if (existente != null)
                        {
                            existente.Status = StatusEnum.Active;
                            existente.Estado = "Inscrito";
                            existente.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            _dbContext.Inscripcion.Add(new Inscripcion
                            {
                                IdEstudiante = estudiante.IdEstudiante,
                                IdGrupoMateria = idGrupoMateria,
                                FechaInscripcion = DateTime.UtcNow,
                                Estado = "Inscrito",
                                Status = StatusEnum.Active,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = "cambio-grupo-avanzado"
                            });
                        }
                        materiasReinscritas++;
                    }

                    if (planCambiado)
                    {
                        estudiante.IdPlanActual = grupoDestino.IdPlanEstudios;
                        estudiante.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return new CambioGrupoResultDto
                {
                    Exitoso = true,
                    Mensaje = "Cambio de grupo realizado exitosamente",
                    GrupoOrigen = grupoOrigen.NombreGrupo,
                    GrupoDestino = grupoDestino.NombreGrupo,
                    NombreEstudiante = nombreEstudiante,
                    Matricula = matricula,
                    CuatrimestreOrigen = grupoOrigen.NumeroCuatrimestre,
                    CuatrimestreDestino = grupoDestino.NumeroCuatrimestre,
                    PlanCambiado = planCambiado,
                    MateriasReinscritas = materiasReinscritas
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return new CambioGrupoResultDto { Exitoso = false, Mensaje = $"Error al realizar el cambio: {ex.Message}" };
            }
        }

        public async Task<CuatrimestresAnterioresPreviewDto> ObtenerPreviewCuatrimestresAnterioresAsync(int idGrupoOrigen, CancellationToken ct = default)
        {
            var grupo = await _dbContext.Grupo
                .Include(g => g.IdPlanEstudiosNavigation)
                .Include(g => g.IdTurnoNavigation)
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupoOrigen && g.Status == StatusEnum.Active, ct)
                ?? throw new InvalidOperationException($"Grupo {idGrupoOrigen} no encontrado");

            var cohorteIds = await _dbContext.EstudianteGrupo
                .Where(eg => eg.IdGrupo == idGrupoOrigen && eg.Status == StatusEnum.Active)
                .Select(eg => eg.IdEstudiante)
                .ToListAsync(ct);
            var totalEstudiantes = cohorteIds.Count;

            var periodicidad = grupo.IdPeriodoAcademicoNavigation.IdPeriodicidad;
            var periodos = await _dbContext.PeriodoAcademico
                .Where(p => p.Status == StatusEnum.Active && p.IdPeriodicidad == periodicidad)
                .OrderBy(p => p.FechaInicio)
                .ToListAsync(ct);
            var idxActual = periodos.FindIndex(p => p.IdPeriodoAcademico == grupo.IdPeriodoAcademico);

            var previos = new List<CuatrimestrePrevioDto>();
            for (int c = 1; c < grupo.NumeroCuatrimestre; c++)
            {
                var totalMaterias = await _dbContext.MateriaPlan
                    .CountAsync(mp => mp.IdPlanEstudios == grupo.IdPlanEstudios
                        && mp.Cuatrimestre == c
                        && mp.Status == StatusEnum.Active, ct);

                PeriodoAcademico? sugerido = null;
                if (idxActual >= 0)
                {
                    var idx = idxActual - (grupo.NumeroCuatrimestre - c);
                    if (idx >= 0 && idx < periodos.Count)
                        sugerido = periodos[idx];
                }

                var mejor = cohorteIds.Count == 0 ? null : (await _dbContext.EstudianteGrupo
                    .Where(eg => eg.Status == StatusEnum.Active
                        && cohorteIds.Contains(eg.IdEstudiante)
                        && eg.IdGrupoNavigation.IdPlanEstudios == grupo.IdPlanEstudios
                        && eg.IdGrupoNavigation.NumeroCuatrimestre == c
                        && eg.IdGrupoNavigation.Status == StatusEnum.Active)
                    .GroupBy(eg => new
                    {
                        eg.IdGrupo,
                        eg.IdGrupoNavigation.CodigoGrupo,
                        eg.IdGrupoNavigation.IdPeriodoAcademico,
                        Periodo = eg.IdGrupoNavigation.IdPeriodoAcademicoNavigation.Nombre
                    })
                    .Select(g => new
                    {
                        g.Key.IdGrupo,
                        g.Key.CodigoGrupo,
                        g.Key.IdPeriodoAcademico,
                        g.Key.Periodo,
                        Count = g.Count()
                    })
                    .ToListAsync(ct))
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefault();

                previos.Add(new CuatrimestrePrevioDto
                {
                    NumeroCuatrimestre = c,
                    TotalMateriasEnPlan = totalMaterias,
                    IdGrupoExistente = mejor?.IdGrupo,
                    GrupoExistenteInfo = mejor != null ? $"{mejor.CodigoGrupo} · {mejor.Periodo}" : null,
                    AlumnosCohorteInscritos = mejor?.Count,
                    IdPeriodoSugerido = mejor?.IdPeriodoAcademico ?? sugerido?.IdPeriodoAcademico,
                    PeriodoSugerido = mejor?.Periodo ?? sugerido?.Nombre
                });
            }

            return new CuatrimestresAnterioresPreviewDto
            {
                IdGrupoOrigen = grupo.IdGrupo,
                NombreGrupo = grupo.NombreGrupo,
                CodigoGrupo = grupo.CodigoGrupo,
                IdPlanEstudios = grupo.IdPlanEstudios,
                PlanEstudios = grupo.IdPlanEstudiosNavigation?.NombrePlanEstudios
                    ?? grupo.IdPlanEstudiosNavigation?.ClavePlanEstudios ?? "",
                NumeroCuatrimestreActual = grupo.NumeroCuatrimestre,
                NumeroGrupo = grupo.NumeroGrupo,
                IdTurno = grupo.IdTurno,
                Turno = grupo.IdTurnoNavigation?.Nombre ?? "",
                IdPeriodoActual = grupo.IdPeriodoAcademico,
                PeriodoActual = grupo.IdPeriodoAcademicoNavigation?.Nombre ?? "",
                TotalEstudiantes = totalEstudiantes,
                CuatrimestresPrevios = previos
            };
        }

        public async Task<GenerarCuatrimestresAnterioresResultado> GenerarCuatrimestresAnterioresAsync(
            GenerarCuatrimestresAnterioresRequest request,
            CancellationToken ct = default)
        {
            var grupoOrigen = await _dbContext.Grupo
                .FirstOrDefaultAsync(g => g.IdGrupo == request.IdGrupoOrigen && g.Status == StatusEnum.Active, ct)
                ?? throw new InvalidOperationException($"Grupo origen {request.IdGrupoOrigen} no encontrado");

            var periodicidadOrigen = (await _dbContext.PeriodoAcademico
                .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == grupoOrigen.IdPeriodoAcademico, ct))?.IdPeriodicidad ?? 1;

            var cohorteIds = await _dbContext.EstudianteGrupo
                .Where(eg => eg.IdGrupo == request.IdGrupoOrigen && eg.Status == StatusEnum.Active)
                .Select(eg => eg.IdEstudiante)
                .ToListAsync(ct);

            var estudiantes = new List<EstudianteEnGrupoDto>();
            if (request.CopiarEstudiantes)
            {
                var resp = await GetEstudiantesDelGrupoDirectoAsync(request.IdGrupoOrigen, ct);
                estudiantes = resp.Estudiantes;
            }

            var resultado = new GenerarCuatrimestresAnterioresResultado { IdGrupoOrigen = grupoOrigen.IdGrupo };

            foreach (var item in request.Cuatrimestres.OrderBy(c => c.NumeroCuatrimestre))
            {
                if (item.NumeroCuatrimestre < 1 || item.NumeroCuatrimestre >= grupoOrigen.NumeroCuatrimestre)
                    continue;

                var gen = new CuatrimestreGeneradoDto { NumeroCuatrimestre = item.NumeroCuatrimestre };

                int? grupoCohorteId = null;
                if (cohorteIds.Count > 0)
                {
                    var candidatos = await _dbContext.EstudianteGrupo
                        .Where(eg => eg.Status == StatusEnum.Active
                            && cohorteIds.Contains(eg.IdEstudiante)
                            && eg.IdGrupoNavigation.IdPlanEstudios == grupoOrigen.IdPlanEstudios
                            && eg.IdGrupoNavigation.NumeroCuatrimestre == item.NumeroCuatrimestre
                            && eg.IdGrupoNavigation.Status == StatusEnum.Active)
                        .GroupBy(eg => eg.IdGrupo)
                        .Select(g => new { IdGrupo = g.Key, Count = g.Count() })
                        .ToListAsync(ct);
                    grupoCohorteId = candidatos.OrderByDescending(x => x.Count).Select(x => (int?)x.IdGrupo).FirstOrDefault();
                }

                int idGrupo;
                int idPeriodo;

                if (grupoCohorteId != null)
                {
                    var g = await _dbContext.Grupo.FirstAsync(x => x.IdGrupo == grupoCohorteId.Value, ct);
                    idGrupo = g.IdGrupo;
                    idPeriodo = g.IdPeriodoAcademico;
                    gen.GrupoYaExistia = true;
                    gen.NombreGrupo = g.NombreGrupo;
                    gen.CodigoGrupo = g.CodigoGrupo;
                    gen.TotalMaterias = await _dbContext.GrupoMateria
                        .CountAsync(gm => gm.IdGrupo == idGrupo && gm.Status == StatusEnum.Active, ct);
                    resultado.TotalGruposReutilizados++;
                }
                else
                {
                    if (item.NuevoPeriodo != null)
                    {
                        var clave = string.IsNullOrWhiteSpace(item.NuevoPeriodo.Clave)
                            ? $"HIST-{item.NuevoPeriodo.FechaInicio:yyyyMMdd}"
                            : item.NuevoPeriodo.Clave!.Trim();

                        var periodoExistente = await _dbContext.PeriodoAcademico
                            .FirstOrDefaultAsync(p => p.Clave == clave && p.Status == StatusEnum.Active, ct);

                        if (periodoExistente != null)
                        {
                            idPeriodo = periodoExistente.IdPeriodoAcademico;
                        }
                        else
                        {
                            var nuevo = new PeriodoAcademico
                            {
                                Clave = clave,
                                Nombre = item.NuevoPeriodo.Nombre.Trim(),
                                IdPeriodicidad = periodicidadOrigen,
                                FechaInicio = item.NuevoPeriodo.FechaInicio,
                                FechaFin = item.NuevoPeriodo.FechaFin,
                                EsPeriodoActual = false,
                                Status = StatusEnum.Active,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = "Sistema"
                            };
                            _dbContext.PeriodoAcademico.Add(nuevo);
                            await _dbContext.SaveChangesAsync(ct);
                            idPeriodo = nuevo.IdPeriodoAcademico;
                            gen.PeriodoCreado = true;
                            resultado.TotalPeriodosCreados++;
                        }
                    }
                    else if (item.IdPeriodoAcademico.HasValue)
                    {
                        idPeriodo = item.IdPeriodoAcademico.Value;
                    }
                    else
                    {
                        gen.Advertencias.Add("Sin periodo asignado; se omitió este cuatrimestre.");
                        resultado.Cuatrimestres.Add(gen);
                        continue;
                    }

                    var ocupados = await _dbContext.Grupo
                        .Where(g => g.IdPlanEstudios == grupoOrigen.IdPlanEstudios
                            && g.NumeroCuatrimestre == item.NumeroCuatrimestre
                            && g.IdTurno == grupoOrigen.IdTurno
                            && g.IdPeriodoAcademico == idPeriodo
                            && g.Status == StatusEnum.Active)
                        .Select(g => g.NumeroGrupo)
                        .ToListAsync(ct);
                    int numeroGrupo = grupoOrigen.NumeroGrupo;
                    while (ocupados.Contains((byte)numeroGrupo))
                        numeroGrupo++;

                    var creado = await CrearGrupoConMateriasAsync(new Core.Requests.GestionAcademica.CrearGrupoAcademicoRequest
                    {
                        IdPlanEstudios = grupoOrigen.IdPlanEstudios,
                        IdPeriodoAcademico = idPeriodo,
                        NumeroCuatrimestre = item.NumeroCuatrimestre,
                        NumeroGrupo = numeroGrupo,
                        IdTurno = grupoOrigen.IdTurno,
                        CapacidadMaxima = grupoOrigen.CapacidadMaxima,
                        CargarMateriasAutomaticamente = true
                    }, ct);
                    idGrupo = creado.IdGrupo;
                    gen.NombreGrupo = creado.NombreGrupo;
                    gen.CodigoGrupo = creado.CodigoGrupo;
                    gen.TotalMaterias = creado.TotalMaterias;
                    resultado.TotalGruposCreados++;
                }

                gen.IdGrupo = idGrupo;
                gen.IdPeriodoAcademico = idPeriodo;
                gen.PeriodoAcademico = (await _dbContext.PeriodoAcademico.FindAsync(new object[] { idPeriodo }, ct))?.Nombre ?? "";

                foreach (var est in estudiantes)
                {
                    try
                    {
                        var r = await InscribirEstudianteGrupoAsync(idGrupo, est.IdEstudiante, forzarInscripcion: true,
                            observaciones: "Generación retroactiva de cuatrimestre anterior");
                        if (r.MateriasInscritas > 0 && r.MateriasFallidas == 0)
                        {
                            gen.EstudiantesInscritos++;
                        }
                        else
                        {
                            gen.EstudiantesConAdvertencia++;
                            if (r.MateriasFallidas > 0)
                                gen.Advertencias.Add($"{est.Matricula}: {r.MateriasFallidas} materia(s) no inscritas");
                        }
                    }
                    catch (Exception ex)
                    {
                        gen.EstudiantesConAdvertencia++;
                        gen.Advertencias.Add($"{est.Matricula}: {ex.Message}");
                    }
                }

                resultado.Cuatrimestres.Add(gen);
            }

            return resultado;
        }
    }
}
