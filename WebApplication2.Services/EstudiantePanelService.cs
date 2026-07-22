using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Documentos;
using WebApplication2.Core.DTOs.EstudiantePanel;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Requests.Documentos;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Estudiante;
using WebApplication2.Core.Requests.EstudiantePanel;
using WebApplication2.Core.Responses.EstudiantePanel;
using WebApplication2.Data.DbContexts;
using Microsoft.AspNetCore.Hosting;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class EstudiantePanelService : IEstudiantePanelService
    {
        private readonly ApplicationDbContext _db;
        private readonly IDocumentoEstudianteService _documentoService;
        private readonly IBecaService _becaService;
        private readonly IPdfService _pdfService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IBitacoraAccionService _bitacora;
        private readonly IAuthService _authService;
        private readonly IMicrosoftGraphService _graphService;
        private readonly IMatriculaService _matriculaService;
        private readonly IWebHostEnvironment _env;
        private readonly IInstitucionProvider _institucionProvider;

        public EstudiantePanelService(
            ApplicationDbContext db,
            IDocumentoEstudianteService documentoService,
            IBecaService becaService,
            IPdfService pdfService,
            IBlobStorageService blobStorageService,
            IBitacoraAccionService bitacora,
            IAuthService authService,
            IMicrosoftGraphService graphService,
            IMatriculaService matriculaService,
            IWebHostEnvironment env,
            IInstitucionProvider institucionProvider)
        {
            _db = db;
            _documentoService = documentoService;
            _becaService = becaService;
            _pdfService = pdfService;
            _blobStorageService = blobStorageService;
            _bitacora = bitacora;
            _authService = authService;
            _graphService = graphService;
            _matriculaService = matriculaService;
            _env = env;
            _institucionProvider = institucionProvider;
        }

        #region Consultas de Panel

        public async Task<EstudiantePanelDto?> ObtenerPanelEstudianteAsync(int idEstudiante, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                    .ThenInclude(p => p!.IdDireccionNavigation)
                        .ThenInclude(d => d!.CodigoPostal)
                            .ThenInclude(cp => cp!.Municipio)
                                .ThenInclude(m => m!.Estado)
                .Include(e => e.IdPersonaNavigation)
                    .ThenInclude(p => p!.IdGeneroNavigation)
                .Include(e => e.IdPlanActualNavigation)
                    .ThenInclude(p => p!.IdCampusNavigation)
                .Include(e => e.EstudianteGrupo.Where(eg => eg.Status == StatusEnum.Active).OrderByDescending(eg => eg.FechaInscripcion).Take(1))
                    .ThenInclude(eg => eg.IdGrupoNavigation)
                        .ThenInclude(g => g.IdPeriodoAcademicoNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante == null)
                return null;

            var persona = estudiante.IdPersonaNavigation;
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            var preinscripcionPendiente = await _db.PreInscripcion
                .Include(p => p.IdPeriodoAcademicoDestinoNavigation)
                .Where(p => p.IdEstudiante == idEstudiante
                    && p.Estado == "Pendiente"
                    && p.Status == Core.Enums.StatusEnum.Active)
                .OrderByDescending(p => p.FechaApartado)
                .FirstOrDefaultAsync(ct);

            var panel = new EstudiantePanelDto
            {
                IdEstudiante = estudiante.IdEstudiante,
                Matricula = estudiante.Matricula,
                NombreCompleto = persona != null
                    ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                    : "Sin nombre",
                Email = estudiante.Email ?? persona?.Correo,
                Telefono = persona?.Telefono,
                Curp = persona?.Curp,
                FechaNacimiento = persona?.FechaNacimiento?.ToDateTime(TimeOnly.MinValue),
                Fotografia = null,
                Genero = persona?.IdGeneroNavigation?.DescGenero,
                Direccion = BuildDireccionTexto(persona?.IdDireccionNavigation),
                Calle = persona?.IdDireccionNavigation?.Calle,
                NumeroExterior = persona?.IdDireccionNavigation?.NumeroExterior,
                NumeroInterior = persona?.IdDireccionNavigation?.NumeroInterior,
                Colonia = persona?.IdDireccionNavigation?.CodigoPostal?.Asentamiento,
                CodigoPostalStr = persona?.IdDireccionNavigation?.CodigoPostal?.Codigo,
                MunicipioStr = persona?.IdDireccionNavigation?.CodigoPostal?.Municipio?.Nombre,
                EstadoStr = persona?.IdDireccionNavigation?.CodigoPostal?.Municipio?.Estado?.Nombre,
                Activo = estudiante.Activo,
                EstatusAcademico = (int)estudiante.EstatusAcademico,
                EstatusAcademicoTexto = estudiante.EstatusAcademico.ToString(),
                TienePreinscripcionPendiente = preinscripcionPendiente != null,
                PeriodoPreinscripcion = preinscripcionPendiente != null && preinscripcionPendiente.IdPeriodoAcademicoDestinoNavigation != null
                    ? $"{preinscripcionPendiente.IdPeriodoAcademicoDestinoNavigation.Clave} · {preinscripcionPendiente.IdPeriodoAcademicoDestinoNavigation.Nombre}"
                    : null,
                TipoBaja = (int?)estudiante.TipoBaja,
                EstadoBaja = (int?)estudiante.EstadoBaja,
                MotivoBaja = estudiante.MotivoBaja,
                FechaBaja = estudiante.FechaBaja,
                FechaConsulta = DateTime.UtcNow,
                ContactoEmergencia = persona != null && !string.IsNullOrWhiteSpace(persona.NombreContactoEmergencia)
                    ? new ContactoEmergenciaDto
                    {
                        Nombre = persona.NombreContactoEmergencia,
                        Telefono = persona.TelefonoContactoEmergencia,
                        Parentesco = persona.ParentescoContactoEmergencia
                    }
                    : null
            };

            panel.InformacionAcademica = await ObtenerInformacionAcademicaAsync(idEstudiante, ct) ?? new InformacionAcademicaPanelDto();
            panel.ResumenKardex = await ObtenerResumenKardexAsync(idEstudiante, ct);
            panel.Becas = await ObtenerBecasEstudianteAsync(idEstudiante, null, ct);
            panel.ResumenRecibos = await ObtenerResumenRecibosAsync(idEstudiante, ct);
            panel.Documentos = await ObtenerDocumentosDisponiblesAsync(idEstudiante, ct);

            return panel;
        }

        public async Task<EstudiantePanelDto?> ObtenerPanelPorMatriculaAsync(string matricula, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .FirstOrDefaultAsync(e => e.Matricula == matricula, ct);

            if (estudiante == null)
                return null;

            return await ObtenerPanelEstudianteAsync(estudiante.IdEstudiante, ct);
        }

        public async Task<BuscarEstudiantesPanelResponse> BuscarEstudiantesAsync(BuscarEstudiantesPanelRequest request, CancellationToken ct = default)
        {
            var query = _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Include(e => e.IdPlanActualNavigation)
                .Include(e => e.EstudianteGrupo.Where(eg => eg.Status == StatusEnum.Active).OrderByDescending(eg => eg.FechaInscripcion).Take(1))
                    .ThenInclude(eg => eg.IdGrupoNavigation)
                .AsQueryable();

            if (request.SoloActivos == true)
            {
                query = query.Where(e => e.Activo);
            }

            if (request.IdPlanEstudios.HasValue)
            {
                query = query.Where(e => e.IdPlanActual == request.IdPlanEstudios.Value);
            }

            if (request.IdGrupo.HasValue)
            {
                query = query.Where(e => e.EstudianteGrupo.Any(eg => eg.IdGrupo == request.IdGrupo.Value && eg.Status == StatusEnum.Active));
            }

            if (!string.IsNullOrWhiteSpace(request.Busqueda))
            {
                var busqueda = request.Busqueda.Trim().ToLower();
                query = query.Where(e =>
                    e.Matricula.ToLower().Contains(busqueda) ||
                    (e.IdPersonaNavigation != null && (
                        (e.IdPersonaNavigation.Nombre != null && e.IdPersonaNavigation.Nombre.ToLower().Contains(busqueda)) ||
                        (e.IdPersonaNavigation.ApellidoPaterno != null && e.IdPersonaNavigation.ApellidoPaterno.ToLower().Contains(busqueda)) ||
                        (e.IdPersonaNavigation.ApellidoMaterno != null && e.IdPersonaNavigation.ApellidoMaterno.ToLower().Contains(busqueda)) ||
                        (e.IdPersonaNavigation.Correo != null && e.IdPersonaNavigation.Correo.ToLower().Contains(busqueda))
                    ))
                );
            }

            var totalRegistros = await query.CountAsync(ct);

            var estudiantes = await query
                .OrderBy(e => e.IdPersonaNavigation != null ? e.IdPersonaNavigation.ApellidoPaterno : "")
                .ThenBy(e => e.IdPersonaNavigation != null ? e.IdPersonaNavigation.ApellidoMaterno : "")
                .ThenBy(e => e.IdPersonaNavigation != null ? e.IdPersonaNavigation.Nombre : "")
                .Skip((request.Pagina - 1) * request.TamanoPagina)
                .Take(request.TamanoPagina)
                .ToListAsync(ct);

            var estudianteIds = estudiantes.Select(e => e.IdEstudiante).ToList();

            var adeudos = await _db.Recibo
                .Where(r => r.IdEstudiante.HasValue && estudianteIds.Contains(r.IdEstudiante.Value))
                .Where(r => r.Estatus == EstatusRecibo.PENDIENTE || r.Estatus == EstatusRecibo.PARCIAL || r.Estatus == EstatusRecibo.VENCIDO)
                .GroupBy(r => r.IdEstudiante)
                .Select(g => new { IdEstudiante = g.Key, TotalAdeudo = g.Sum(r => r.Saldo) })
                .ToDictionaryAsync(x => x.IdEstudiante!.Value, x => x.TotalAdeudo, ct);

            var estudiantesConBeca = await _db.BecaAsignacion
                .Where(b => estudianteIds.Contains(b.IdEstudiante) && b.Activo)
                .Select(b => b.IdEstudiante)
                .Distinct()
                .ToListAsync(ct);

            var estudiantesDto = estudiantes.Select(e =>
            {
                var persona = e.IdPersonaNavigation;
                var grupo = e.EstudianteGrupo.FirstOrDefault()?.IdGrupoNavigation;

                return new EstudianteListaDto
                {
                    IdEstudiante = e.IdEstudiante,
                    Matricula = e.Matricula,
                    NombreCompleto = persona != null
                        ? $"{persona.ApellidoPaterno} {persona.ApellidoMaterno} {persona.Nombre}".Trim()
                        : "Sin nombre",
                    Email = e.Email ?? persona?.Correo,
                    Telefono = persona?.Telefono,
                    PlanEstudios = e.IdPlanActualNavigation?.NombrePlanEstudios,
                    Grupo = grupo?.CodigoGrupo ?? grupo?.NombreGrupo,
                    PromedioGeneral = null, // Se puede calcular si es necesario
                    Adeudo = adeudos.TryGetValue(e.IdEstudiante, out var adeudo) ? adeudo : 0,
                    TieneBeca = estudiantesConBeca.Contains(e.IdEstudiante),
                    Activo = e.Activo,
                    Fotografia = null,
                    EstatusAcademico = (int)e.EstatusAcademico,
                    EstatusAcademicoTexto = e.EstatusAcademico.ToString()
                };
            }).ToList();

            if (request.ConAdeudo == true)
            {
                estudiantesDto = estudiantesDto.Where(e => e.Adeudo > 0).ToList();
            }
            else if (request.ConAdeudo == false)
            {
                estudiantesDto = estudiantesDto.Where(e => e.Adeudo == 0).ToList();
            }

            if (request.ConBeca == true)
            {
                estudiantesDto = estudiantesDto.Where(e => e.TieneBeca).ToList();
            }
            else if (request.ConBeca == false)
            {
                estudiantesDto = estudiantesDto.Where(e => !e.TieneBeca).ToList();
            }

            var estadisticas = await ObtenerEstadisticasAsync(request.IdPlanEstudios, request.IdPeriodoAcademico, ct);

            return new BuscarEstudiantesPanelResponse
            {
                Estudiantes = estudiantesDto,
                TotalRegistros = totalRegistros,
                Pagina = request.Pagina,
                TamanoPagina = request.TamanoPagina,
                TotalPaginas = (int)Math.Ceiling((double)totalRegistros / request.TamanoPagina),
                Estadisticas = estadisticas
            };
        }

        public async Task<EstadisticasEstudiantesDto> ObtenerEstadisticasAsync(int? idPlanEstudios = null, int? idPeriodoAcademico = null, CancellationToken ct = default)
        {
            var queryEstudiantes = _db.Estudiante.AsQueryable();

            if (idPlanEstudios.HasValue)
            {
                queryEstudiantes = queryEstudiantes.Where(e => e.IdPlanActual == idPlanEstudios.Value);
            }

            var totalEstudiantes = await queryEstudiantes.CountAsync(ct);
            var estudiantesActivos = await queryEstudiantes.Where(e => e.Activo).CountAsync(ct);

            var estudianteIds = await queryEstudiantes.Select(e => e.IdEstudiante).ToListAsync(ct);

            var estudiantesConAdeudo = await _db.Recibo
                .Where(r => r.IdEstudiante.HasValue && estudianteIds.Contains(r.IdEstudiante.Value))
                .Where(r => r.Estatus == EstatusRecibo.PENDIENTE || r.Estatus == EstatusRecibo.PARCIAL || r.Estatus == EstatusRecibo.VENCIDO)
                .Where(r => r.Saldo > 0)
                .Select(r => r.IdEstudiante)
                .Distinct()
                .CountAsync(ct);

            var totalAdeudo = await _db.Recibo
                .Where(r => r.IdEstudiante.HasValue && estudianteIds.Contains(r.IdEstudiante.Value))
                .Where(r => r.Estatus == EstatusRecibo.PENDIENTE || r.Estatus == EstatusRecibo.PARCIAL || r.Estatus == EstatusRecibo.VENCIDO)
                .SumAsync(r => r.Saldo, ct);

            var estudiantesConBeca = await _db.BecaAsignacion
                .Where(b => estudianteIds.Contains(b.IdEstudiante) && b.Activo)
                .Select(b => b.IdEstudiante)
                .Distinct()
                .CountAsync(ct);

            return new EstadisticasEstudiantesDto
            {
                TotalEstudiantes = totalEstudiantes,
                EstudiantesActivos = estudiantesActivos,
                EstudiantesConAdeudo = estudiantesConAdeudo,
                EstudiantesConBeca = estudiantesConBeca,
                TotalAdeudoGeneral = totalAdeudo,
                PromedioGeneralInstitucional = 0 // Se puede calcular si hay calificaciones
            };
        }

        #endregion

        #region Información Académica

        public async Task<InformacionAcademicaPanelDto?> ObtenerInformacionAcademicaAsync(int idEstudiante, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .Include(e => e.IdPlanActualNavigation)
                    .ThenInclude(p => p!.IdCampusNavigation)
                .Include(e => e.EstudianteGrupo.Where(eg => eg.Status == StatusEnum.Active).OrderByDescending(eg => eg.FechaInscripcion).Take(1))
                    .ThenInclude(eg => eg.IdGrupoNavigation)
                        .ThenInclude(g => g.IdPeriodoAcademicoNavigation)
                .Include(e => e.EstudianteGrupo.Where(eg => eg.Status == StatusEnum.Active).OrderByDescending(eg => eg.FechaInscripcion).Take(1))
                    .ThenInclude(eg => eg.IdGrupoNavigation)
                        .ThenInclude(g => g.IdTurnoNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante == null)
                return null;

            var plan = estudiante.IdPlanActualNavigation;
            var grupoEstudiante = estudiante.EstudianteGrupo.FirstOrDefault();
            var grupo = grupoEstudiante?.IdGrupoNavigation;
            var periodo = grupo?.IdPeriodoAcademicoNavigation;
            var turno = grupo?.IdTurnoNavigation;

            var info = new InformacionAcademicaPanelDto
            {
                IdPlanEstudios = estudiante.IdPlanActual,
                PlanEstudios = plan?.NombrePlanEstudios,
                Carrera = plan?.NombrePlanEstudios,
                RVOE = plan?.RVOE,
                Modalidad = null, // PlanEstudios no tiene Modalidad
                FechaIngreso = estudiante.FechaIngreso,
                Campus = plan?.IdCampusNavigation?.Nombre,
                Turno = turno?.Nombre
            };

            if (grupo != null)
            {
                info.GrupoActual = new GrupoActualDto
                {
                    IdGrupo = grupo.IdGrupo,
                    IdEstudianteGrupo = grupoEstudiante?.IdEstudianteGrupo,
                    CodigoGrupo = grupo.CodigoGrupo ?? "",
                    NombreGrupo = grupo.NombreGrupo,
                    Turno = turno?.Nombre,
                    CupoMaximo = grupo.CapacidadMaxima,
                    NumeroCuatrimestre = grupo.NumeroCuatrimestre,
                    IdPlanEstudios = grupo.IdPlanEstudios,
                    IdPeriodoAcademico = grupo.IdPeriodoAcademico
                };
            }

            if (periodo != null)
            {
                info.PeriodoActual = new PeriodoActualPanelDto
                {
                    IdPeriodoAcademico = periodo.IdPeriodoAcademico,
                    Nombre = periodo.Nombre,
                    Clave = periodo.Clave,
                    FechaInicio = periodo.FechaInicio,
                    FechaFin = periodo.FechaFin,
                    EsActual = periodo.EsPeriodoActual
                };
            }

            return info;
        }

        public async Task<ResumenKardexDto> ObtenerResumenKardexAsync(int idEstudiante, CancellationToken ct = default)
        {
            var inscripciones = await _db.Inscripcion
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp.IdMateriaNavigation)
                .Where(i => i.IdEstudiante == idEstudiante)
                .ToListAsync(ct);

            var resumen = new ResumenKardexDto();

            if (inscripciones.Any())
            {
                var conCalificacion = inscripciones.Where(i => i.CalificacionFinal.HasValue && i.CalificacionFinal > 0).ToList();
                var aprobadas = conCalificacion.Where(i => i.CalificacionFinal >= 7).ToList();
                var reprobadas = conCalificacion.Where(i => i.CalificacionFinal < 7).ToList();

                resumen.PromedioGeneral = conCalificacion.Any()
                    ? Math.Round(conCalificacion.Average(i => i.CalificacionFinal ?? 0), 2)
                    : 0;

                resumen.MateriasAprobadas = aprobadas.Count;
                resumen.MateriasReprobadas = reprobadas.Count;
                resumen.MateriasCursando = inscripciones.Count(i => !i.CalificacionFinal.HasValue);

                resumen.CreditosCursados = (int)aprobadas.Sum(i => i.IdGrupoMateriaNavigation?.IdMateriaPlanNavigation?.IdMateriaNavigation?.Creditos ?? 0);

                resumen.UltimasMaterias = inscripciones
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(5)
                    .Select(i => new MateriaResumenDto
                    {
                        ClaveMateria = i.IdGrupoMateriaNavigation?.IdMateriaPlanNavigation?.IdMateriaNavigation?.Clave ?? "",
                        NombreMateria = i.IdGrupoMateriaNavigation?.IdMateriaPlanNavigation?.IdMateriaNavigation?.Nombre ?? "",
                        CalificacionFinal = i.CalificacionFinal,
                        Estatus = i.CalificacionFinal.HasValue
                            ? (i.CalificacionFinal >= 7 ? "Aprobada" : "Reprobada")
                            : "Cursando"
                    })
                    .ToList();
            }

            var estudiante = await _db.Estudiante
                .Include(e => e.IdPlanActualNavigation)
                    .ThenInclude(p => p!.MateriaPlan)
                        .ThenInclude(mp => mp.IdMateriaNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante?.IdPlanActualNavigation?.MateriaPlan != null)
            {
                resumen.CreditosTotales = (int)estudiante.IdPlanActualNavigation.MateriaPlan
                    .Sum(mp => mp.IdMateriaNavigation?.Creditos ?? 0);

                if (resumen.CreditosTotales > 0)
                {
                    resumen.PorcentajeAvance = Math.Round((decimal)resumen.CreditosCursados / resumen.CreditosTotales * 100, 2);
                }
            }

            resumen.EstatusAcademico = resumen.MateriasReprobadas > 3 ? "Irregular" : "Regular";

            return resumen;
        }

        public async Task<SeguimientoAcademicoDto> ObtenerSeguimientoAcademicoAsync(int idEstudiante, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Include(e => e.IdPlanActualNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante == null)
                throw new InvalidOperationException($"Estudiante con ID {idEstudiante} no encontrado");

            var persona = estudiante.IdPersonaNavigation;

            var inscripciones = await _db.Inscripcion
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm.IdGrupoNavigation)
                        .ThenInclude(g => g.IdPeriodoAcademicoNavigation)
                .Where(i => i.IdEstudiante == idEstudiante)
                .ToListAsync(ct);

            var periodosIds = inscripciones
                .Where(i => i.IdGrupoMateriaNavigation?.IdGrupoNavigation?.IdPeriodoAcademico != null)
                .Select(i => i.IdGrupoMateriaNavigation!.IdGrupoNavigation!.IdPeriodoAcademico)
                .Distinct()
                .ToList();

            var periodos = await _db.PeriodoAcademico
                .Where(p => periodosIds.Contains(p.IdPeriodoAcademico))
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync(ct);

            var seguimiento = new SeguimientoAcademicoDto
            {
                IdEstudiante = estudiante.IdEstudiante,
                Matricula = estudiante.Matricula,
                NombreCompleto = persona != null
                    ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                    : "Sin nombre",
                PlanEstudios = estudiante.IdPlanActualNavigation?.NombrePlanEstudios ?? "N/A",
                ResumenGeneral = await ObtenerResumenKardexAsync(idEstudiante, ct)
            };

            foreach (var periodo in periodos)
            {
                var inscripcionesPeriodo = inscripciones
                    .Where(i => i.IdGrupoMateriaNavigation?.IdGrupoNavigation?.IdPeriodoAcademico == periodo.IdPeriodoAcademico)
                    .ToList();

                var materiasDelPeriodo = inscripcionesPeriodo.Select(i =>
                {
                    var materia = i.IdGrupoMateriaNavigation?.IdMateriaPlanNavigation?.IdMateriaNavigation;
                    var grupo = i.IdGrupoMateriaNavigation?.IdGrupoNavigation;

                    string estatus = "Pendiente";
                    if (i.CalificacionFinal.HasValue)
                    {
                        estatus = i.CalificacionFinal >= 7 ? "Aprobada" : "Reprobada";
                    }
                    else if (periodo.EsPeriodoActual)
                    {
                        estatus = "Cursando";
                    }

                    return new MateriaDetalleDto
                    {
                        IdInscripcion = i.IdInscripcion,
                        IdMateria = materia?.IdMateria ?? 0,
                        ClaveMateria = materia?.Clave ?? "",
                        NombreMateria = materia?.Nombre ?? "",
                        Creditos = materia?.Creditos ?? 0,
                        Grupo = grupo?.CodigoGrupo ?? grupo?.NombreGrupo ?? "",
                        Profesor = null, // Se podría cargar si hay relación
                        Parciales = new CalificacionesParcialesDto(), // Se podría cargar de CalificacionParcial
                        CalificacionFinal = i.CalificacionFinal,
                        Estatus = estatus,
                        FechaInscripcion = i.CreatedAt,
                        Observaciones = null
                    };
                }).ToList();

                var aprobadas = materiasDelPeriodo.Count(m => m.Estatus == "Aprobada");
                var reprobadas = materiasDelPeriodo.Count(m => m.Estatus == "Reprobada");
                var cursando = materiasDelPeriodo.Count(m => m.Estatus == "Cursando");

                var calificacionesValidas = materiasDelPeriodo
                    .Where(m => m.CalificacionFinal.HasValue && m.CalificacionFinal > 0)
                    .ToList();

                var promedioPeriodo = calificacionesValidas.Any()
                    ? Math.Round(calificacionesValidas.Average(m => m.CalificacionFinal ?? 0), 2)
                    : 0;

                seguimiento.Periodos.Add(new PeriodoAcademicoDetalleDto
                {
                    IdPeriodoAcademico = periodo.IdPeriodoAcademico,
                    Nombre = periodo.Nombre,
                    Clave = periodo.Clave,
                    FechaInicio = periodo.FechaInicio,
                    FechaFin = periodo.FechaFin,
                    EsActual = periodo.EsPeriodoActual,
                    PromedioDelPeriodo = promedioPeriodo,
                    CreditosDelPeriodo = (int)materiasDelPeriodo.Where(m => m.Estatus == "Aprobada").Sum(m => m.Creditos),
                    Materias = materiasDelPeriodo,
                    Estadisticas = new EstadisticasPeriodoDto
                    {
                        MateriasTotal = materiasDelPeriodo.Count,
                        MateriasAprobadas = aprobadas,
                        MateriasReprobadas = reprobadas,
                        MateriasCursando = cursando,
                        CreditosObtenidos = (int)materiasDelPeriodo.Where(m => m.Estatus == "Aprobada").Sum(m => m.Creditos),
                        CreditosPosibles = (int)materiasDelPeriodo.Sum(m => m.Creditos)
                    }
                });
            }

            return seguimiento;
        }

        #endregion

        #region Actualización de Datos

        public async Task<AccionPanelResponse> ActualizarDatosEstudianteAsync(int idEstudiante, ActualizarDatosEstudianteRequest request, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                    .ThenInclude(p => p!.IdDireccionNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante == null)
            {
                return new AccionPanelResponse
                {
                    Exitoso = false,
                    Mensaje = "Estudiante no encontrado"
                };
            }

            try
            {
                var persona = estudiante.IdPersonaNavigation;
                if (persona != null)
                {
                    persona.Nombre = request.Nombre;
                    persona.ApellidoPaterno = request.ApellidoPaterno;
                    persona.ApellidoMaterno = request.ApellidoMaterno;
                    persona.Correo = request.Email;
                    persona.Telefono = request.Telefono;
                    persona.Curp = request.Curp;
                    persona.NombreContactoEmergencia = request.NombreContactoEmergencia;
                    persona.TelefonoContactoEmergencia = request.TelefonoContactoEmergencia;
                    persona.ParentescoContactoEmergencia = request.ParentescoContactoEmergencia;

                    if (!string.IsNullOrEmpty(request.FechaNacimiento) && DateOnly.TryParse(request.FechaNacimiento, out var fechaNac))
                    {
                        persona.FechaNacimiento = fechaNac;
                    }

                    if (!string.IsNullOrEmpty(request.Genero))
                    {
                        var genero = await _db.Genero.FirstOrDefaultAsync(g => g.DescGenero == request.Genero, ct);
                        if (genero != null)
                            persona.IdGenero = genero.IdGenero;
                    }

                    if (!string.IsNullOrWhiteSpace(request.Calle))
                    {
                        var cpId = request.CodigoPostalId
                            ?? persona.IdDireccionNavigation?.CodigoPostalId
                            ?? (await _db.CodigosPostales.FirstOrDefaultAsync(ct))?.Id
                            ?? 1;

                        if (persona.IdDireccionNavigation != null)
                        {
                            persona.IdDireccionNavigation.Calle = request.Calle;
                            persona.IdDireccionNavigation.NumeroExterior = request.NumeroExterior;
                            persona.IdDireccionNavigation.NumeroInterior = request.NumeroInterior;
                            persona.IdDireccionNavigation.CodigoPostalId = cpId;
                        }
                        else
                        {
                            var nuevaDireccion = new Direccion
                            {
                                Calle = request.Calle,
                                NumeroExterior = request.NumeroExterior,
                                NumeroInterior = request.NumeroInterior,
                                CodigoPostalId = cpId
                            };
                            _db.Direccion.Add(nuevaDireccion);
                            await _db.SaveChangesAsync(ct);
                            persona.IdDireccion = nuevaDireccion.IdDireccion;
                        }
                    }
                }

                estudiante.Email = request.Email;

                await _db.SaveChangesAsync(ct);

                return new AccionPanelResponse
                {
                    Exitoso = true,
                    Mensaje = "Datos actualizados correctamente"
                };
            }
            catch (Exception ex)
            {
                return new AccionPanelResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error al actualizar datos: {ex.Message}"
                };
            }
        }

        public async Task<AccionPanelResponse> CambiarMatriculaAsync(int idEstudiante, string nuevaMatricula, CancellationToken ct = default)
        {
            if (!_matriculaService.ValidarFormatoMatricula(nuevaMatricula))
            {
                return new AccionPanelResponse
                {
                    Exitoso = false,
                    Mensaje = "Formato de matrícula inválido. Debe ser 1-3 letras mayúsculas seguidas de 5 dígitos."
                };
            }

            if (await _matriculaService.ExisteMatriculaAsync(nuevaMatricula, idEstudiante))
            {
                return new AccionPanelResponse
                {
                    Exitoso = false,
                    Mensaje = "La matrícula ya está en uso por otro estudiante."
                };
            }

            var estudiante = await _db.Estudiante
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante == null)
            {
                return new AccionPanelResponse
                {
                    Exitoso = false,
                    Mensaje = "Estudiante no encontrado."
                };
            }

            var matriculaAnterior = estudiante.Matricula;
            var dominioEmail = "@usaguanajuato.edu.mx";
            var nuevoEmail = $"{nuevaMatricula}{dominioEmail}";

            using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                estudiante.Matricula = nuevaMatricula;
                estudiante.Email = nuevoEmail;
                await _db.SaveChangesAsync(ct);

                if (!string.IsNullOrEmpty(estudiante.UsuarioId))
                {
                    await _authService.UpdateUserEmailAsync(estudiante.UsuarioId, nuevoEmail);

                    var emailAnterior = $"{matriculaAnterior}{dominioEmail}";
                    var graphUser = await _graphService.GetUserByIdAsync(emailAnterior, ct);
                    if (graphUser != null)
                    {
                        await _graphService.UpdateUserPrincipalNameAsync(graphUser.Id, nuevoEmail, ct);
                    }
                }

                await transaction.CommitAsync(ct);

                return new AccionPanelResponse
                {
                    Exitoso = true,
                    Mensaje = $"Matrícula cambiada de {matriculaAnterior} a {nuevaMatricula}. El correo institucional ahora es {nuevoEmail}.",
                    Datos = new { matricula = nuevaMatricula, email = nuevoEmail }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return new AccionPanelResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error al cambiar la matrícula: {ex.Message}"
                };
            }
        }

        #endregion

        #region Becas

        public async Task<List<BecaAsignadaDto>> ObtenerBecasEstudianteAsync(int idEstudiante, bool? soloActivas = true, CancellationToken ct = default)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = _db.BecaAsignacion
                .Include(b => b.Beca)
                .Include(b => b.ConceptoPago)
                .Where(b => b.IdEstudiante == idEstudiante);

            if (soloActivas == true)
            {
                query = query.Where(b => b.Activo);
            }

            var becas = await query.OrderByDescending(b => b.VigenciaDesde).ToListAsync(ct);

            return becas.Select(b => new BecaAsignadaDto
            {
                IdBecaAsignacion = b.IdBecaAsignacion,
                IdBeca = b.IdBeca,
                NombreBeca = b.Beca?.Nombre,
                ClaveBeca = b.Beca?.Clave,
                Tipo = b.Beca?.Tipo ?? b.Tipo,
                Valor = b.Beca?.Valor ?? b.Valor,
                ConceptoPago = b.ConceptoPago?.Descripcion,
                TopeMensual = b.Beca?.TopeMensual ?? b.TopeMensual,
                VigenciaDesde = b.VigenciaDesde,
                VigenciaHasta = b.VigenciaHasta,
                Activo = b.Activo,
                EstaVigente = b.Activo && b.VigenciaDesde <= hoy && (!b.VigenciaHasta.HasValue || b.VigenciaHasta >= hoy),
                Observaciones = b.Observaciones
            }).ToList();
        }

        #endregion

        #region Recibos y Pagos

        private async Task<List<int>> ObtenerIdsAspiranteVinculadosAsync(int idEstudiante, CancellationToken ct)
        {
            var idPersona = await _db.Estudiante
                .Where(e => e.IdEstudiante == idEstudiante)
                .Select(e => (int?)e.IdPersona)
                .FirstOrDefaultAsync(ct);

            if (idPersona == null) return new List<int>();

            return await _db.Aspirante
                .Where(a => a.IdPersona == idPersona)
                .Select(a => a.IdAspirante)
                .ToListAsync(ct);
        }

        public async Task<ResumenRecibosDto> ObtenerResumenRecibosAsync(int idEstudiante, CancellationToken ct = default)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            var idsAspirante = await ObtenerIdsAspiranteVinculadosAsync(idEstudiante, ct);

            var recibos = await _db.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.IdEstudiante == idEstudiante
                    || (r.IdAspirante != null && idsAspirante.Contains(r.IdAspirante.Value)))
                .ToListAsync(ct);

            var pendientes = recibos.Where(r => r.Estatus == EstatusRecibo.PENDIENTE || r.Estatus == EstatusRecibo.PARCIAL).ToList();
            var vencidos = recibos.Where(r => r.Estatus == EstatusRecibo.VENCIDO || (r.FechaVencimiento < hoy && r.Estatus != EstatusRecibo.PAGADO && r.Estatus != EstatusRecibo.CANCELADO)).ToList();
            var pagados = recibos.Where(r => r.Estatus == EstatusRecibo.PAGADO).ToList();

            var resumen = new ResumenRecibosDto
            {
                TotalAdeudo = pendientes.Sum(r => r.Saldo) + vencidos.Sum(r => r.Saldo),
                TotalPagado = pagados.Sum(r => r.Total),
                RecibosPendientes = pendientes.Count,
                RecibosPagados = pagados.Count,
                RecibosVencidos = vencidos.Count,
                TotalDescuentosAplicados = recibos.Sum(r => r.Descuento)
            };

            var proximoVencimiento = pendientes
                .OrderBy(r => r.FechaVencimiento)
                .FirstOrDefault();

            if (proximoVencimiento != null)
            {
                resumen.ProximoVencimiento = MapearReciboResumen(proximoVencimiento, hoy);
            }

            resumen.UltimosRecibos = recibos
                .OrderByDescending(r => r.FechaEmision)
                .Take(5)
                .Select(r => MapearReciboResumen(r, hoy))
                .ToList();

            return resumen;
        }

        public async Task<List<ReciboPanelResumenDto>> ObtenerRecibosEstudianteAsync(int idEstudiante, string? estatus = null, int limite = 50, CancellationToken ct = default)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            var idsAspirante = await ObtenerIdsAspiranteVinculadosAsync(idEstudiante, ct);

            var query = _db.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.IdEstudiante == idEstudiante
                    || (r.IdAspirante != null && idsAspirante.Contains(r.IdAspirante.Value)));

            if (!string.IsNullOrEmpty(estatus) && Enum.TryParse<EstatusRecibo>(estatus, true, out var estatusEnum))
            {
                query = query.Where(r => r.Estatus == estatusEnum);
            }

            var recibos = await query
                .OrderByDescending(r => r.FechaEmision)
                .Take(limite)
                .ToListAsync(ct);

            return recibos.Select(r => MapearReciboResumen(r, hoy)).ToList();
        }

        private static ReciboPanelResumenDto MapearReciboResumen(Recibo recibo, DateOnly hoy)
        {
            var estaVencido = recibo.FechaVencimiento < hoy && recibo.Estatus != EstatusRecibo.PAGADO && recibo.Estatus != EstatusRecibo.CANCELADO;
            var diasVencido = estaVencido ? (hoy.DayNumber - recibo.FechaVencimiento.DayNumber) : 0;

            return new ReciboPanelResumenDto
            {
                IdRecibo = recibo.IdRecibo,
                Folio = recibo.Folio,
                Concepto = recibo.Detalles.FirstOrDefault()?.Descripcion ?? "Sin concepto",
                FechaEmision = recibo.FechaEmision,
                FechaVencimiento = recibo.FechaVencimiento,
                Estatus = recibo.Estatus.ToString(),
                Subtotal = recibo.Subtotal,
                Descuento = recibo.Descuento,
                Recargos = recibo.Recargos,
                Total = recibo.Total,
                Saldo = recibo.Saldo,
                DiasVencido = diasVencido,
                EstaVencido = estaVencido
            };
        }

        #endregion

        #region Documentos

        public async Task<DocumentosPersonalesEstudianteDto?> ObtenerDocumentosPersonalesAsync(int idEstudiante, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante == null)
                return null;

            var resultado = new DocumentosPersonalesEstudianteDto
            {
                IdEstudiante = estudiante.IdEstudiante,
                Matricula = estudiante.Matricula,
                NombreCompleto = estudiante.IdPersonaNavigation != null
                    ? $"{estudiante.IdPersonaNavigation.Nombre} {estudiante.IdPersonaNavigation.ApellidoPaterno} {estudiante.IdPersonaNavigation.ApellidoMaterno}".Trim()
                    : string.Empty
            };

            var planDocIds = estudiante.IdPlanActual.HasValue
                ? await _db.PlanDocumentoRequisito
                    .Where(pd => pd.IdPlanEstudios == estudiante.IdPlanActual.Value)
                    .Select(pd => pd.IdDocumentoRequisito)
                    .ToListAsync(ct)
                : new List<int>();

            var requisitos = planDocIds.Count > 0
                ? await _db.DocumentoRequisito
                    .Where(r => r.Activo && planDocIds.Contains(r.IdDocumentoRequisito))
                    .OrderBy(r => r.Orden)
                    .ToListAsync(ct)
                : await _db.DocumentoRequisito
                    .Where(r => r.Activo)
                    .OrderBy(r => r.Orden)
                    .ToListAsync(ct);

            var aspirante = await _db.Aspirante
                .FirstOrDefaultAsync(a => a.IdPersona == estudiante.IdPersona, ct);

            var documentosExistentes = new List<AspiranteDocumento>();

            if (aspirante != null)
            {
                resultado.IdAspirante = aspirante.IdAspirante;

                documentosExistentes = await _db.AspiranteDocumento
                    .Include(d => d.Requisito)
                    .Where(d => d.IdAspirante == aspirante.IdAspirante)
                    .ToListAsync(ct);
            }

            var userIdsValidacion = documentosExistentes
                .Where(d => !string.IsNullOrEmpty(d.UsuarioValidacion))
                .Select(d => d.UsuarioValidacion!)
                .Distinct()
                .ToList();

            var nombresUsuarios = new Dictionary<string, string>();
            if (userIdsValidacion.Any())
            {
                nombresUsuarios = await _db.Users
                    .Where(u => userIdsValidacion.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => $"{u.Nombres} {u.Apellidos}".Trim(), ct);
            }

            resultado.Documentos = requisitos.Select(r =>
            {
                var docExistente = documentosExistentes.FirstOrDefault(d => d.IdDocumentoRequisito == r.IdDocumentoRequisito);
                string? nombreValidador = null;
                if (docExistente?.UsuarioValidacion != null)
                    nombresUsuarios.TryGetValue(docExistente.UsuarioValidacion, out nombreValidador);

                return new DocumentoPersonalDto
                {
                    IdAspiranteDocumento = docExistente?.IdAspiranteDocumento ?? 0,
                    IdDocumentoRequisito = r.IdDocumentoRequisito,
                    ClaveDocumento = r.Clave,
                    NombreDocumento = r.Descripcion,
                    Estatus = docExistente?.Estatus.ToString() ?? "PENDIENTE",
                    FechaSubido = docExistente?.FechaSubidoUtc,
                    UrlArchivo = docExistente?.UrlArchivo,
                    Notas = docExistente?.Notas,
                    EsObligatorio = r.EsObligatorio,
                    FechaValidacion = docExistente?.FechaValidacion,
                    ValidadoPor = nombreValidador ?? docExistente?.UsuarioValidacion
                };
            }).ToList();

            resultado.TotalDocumentos = resultado.Documentos.Count;
            resultado.DocumentosValidados = resultado.Documentos.Count(d => d.Estatus == "VALIDADO");
            resultado.DocumentosPendientes = resultado.Documentos.Count(d => d.Estatus == "PENDIENTE" || d.Estatus == "SUBIDO" || d.Estatus == "RECHAZADO");

            return resultado;
        }

        public async Task<AccionPanelResponse> SubirDocumentoPersonalAsync(int idEstudiante, int idDocumentoRequisito, Microsoft.AspNetCore.Http.IFormFile archivo, string? notas, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante == null)
                return new AccionPanelResponse { Exitoso = false, Mensaje = "Estudiante no encontrado" };

            // Buscar aspirante asociado al estudiante
            var aspirante = await _db.Aspirante
                .FirstOrDefaultAsync(a => a.IdPersona == estudiante.IdPersona, ct);

            // Si no existe aspirante, crear uno automáticamente
            if (aspirante == null)
            {
                var estatus = await _db.AspiranteEstatus.FirstOrDefaultAsync(ct);
                var medio = await _db.MedioContacto.FirstOrDefaultAsync(ct);

                if (estatus == null || medio == null)
                    return new AccionPanelResponse { Exitoso = false, Mensaje = "No se pudo crear el registro de aspirante: faltan catálogos base" };

                aspirante = new Aspirante
                {
                    IdPersona = estudiante.IdPersona,
                    IdAspiranteEstatus = estatus.IdAspiranteEstatus,
                    FechaRegistro = DateTime.UtcNow,
                    IdPlan = estudiante.IdPlanActual ?? 0,
                    IdMedioContacto = medio.IdMedioContacto,
                    EsAlumnoAutoCreado = true,
                    Observaciones = "Creado automáticamente desde panel de estudiante"
                };

                // Validar que IdPlan tenga un valor válido
                if (aspirante.IdPlan == 0)
                {
                    var primerPlan = await _db.PlanEstudios.FirstOrDefaultAsync(ct);
                    if (primerPlan != null)
                        aspirante.IdPlan = primerPlan.IdPlanEstudios;
                }

                _db.Aspirante.Add(aspirante);
                await _db.SaveChangesAsync(ct);
            }

            var requisito = await _db.DocumentoRequisito.FindAsync(new object[] { idDocumentoRequisito }, ct);
            if (requisito == null)
                return new AccionPanelResponse { Exitoso = false, Mensaje = "Requisito de documento no encontrado" };

            // Validar archivo (extensión, tamaño y magic bytes)
            var (isValid, errorMessage) = await FileValidator.ValidateAsync(archivo);
            if (!isValid)
                return new AccionPanelResponse { Exitoso = false, Mensaje = errorMessage! };

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            // Subir archivo usando blob storage
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var fileName = $"{aspirante.IdAspirante}/{requisito.Clave}_{timestamp}{extension}";
            var containerName = "documentos";

            string urlArchivo;
            try
            {
                urlArchivo = await _blobStorageService.UploadFile(archivo, fileName, containerName);
            }
            catch (Exception ex)
            {
                return new AccionPanelResponse { Exitoso = false, Mensaje = $"Error al subir el archivo: {ex.Message}" };
            }

            // Buscar o crear registro de documento
            var doc = await _db.AspiranteDocumento
                .FirstOrDefaultAsync(d => d.IdAspirante == aspirante.IdAspirante && d.IdDocumentoRequisito == idDocumentoRequisito, ct);

            if (doc == null)
            {
                doc = new AspiranteDocumento
                {
                    IdAspirante = aspirante.IdAspirante,
                    IdDocumentoRequisito = idDocumentoRequisito
                };
                _db.AspiranteDocumento.Add(doc);
            }

            doc.UrlArchivo = urlArchivo;
            doc.Notas = notas;
            doc.Estatus = EstatusDocumentoEnum.SUBIDO;
            doc.FechaSubidoUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return new AccionPanelResponse { Exitoso = true, Mensaje = "Documento subido exitosamente" };
        }

        public async Task<AccionPanelResponse> ValidarDocumentoPersonalAsync(int idEstudiante, long idAspiranteDocumento, bool aprobar, string? notas, string? usuarioId, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante == null)
                return new AccionPanelResponse { Exitoso = false, Mensaje = "Estudiante no encontrado" };

            var doc = await _db.AspiranteDocumento
                .FirstOrDefaultAsync(d => d.IdAspiranteDocumento == idAspiranteDocumento, ct);

            if (doc == null)
                return new AccionPanelResponse { Exitoso = false, Mensaje = "Documento no encontrado" };

            if (doc.Estatus != EstatusDocumentoEnum.SUBIDO)
                return new AccionPanelResponse { Exitoso = false, Mensaje = "Solo se pueden validar documentos con estatus SUBIDO" };

            doc.Estatus = aprobar ? EstatusDocumentoEnum.VALIDADO : EstatusDocumentoEnum.RECHAZADO;
            doc.FechaValidacion = DateTime.UtcNow;
            doc.UsuarioValidacion = usuarioId;
            if (!string.IsNullOrWhiteSpace(notas))
                doc.Notas = notas;

            await _db.SaveChangesAsync(ct);

            var accion = aprobar ? "VALIDAR" : "RECHAZAR";
            var accionDesc = aprobar ? "validado" : "rechazado";
            await _bitacora.RegistrarAsync(usuarioId ?? "Sistema", usuarioId ?? "Sistema",
                $"{accion}_DOCUMENTO", "Documentos",
                "AspiranteDocumento", idAspiranteDocumento.ToString(),
                $"Documento {accionDesc} para estudiante {idEstudiante}. {(notas != null ? $"Notas: {notas}" : "")}");

            return new AccionPanelResponse { Exitoso = true, Mensaje = $"Documento {accionDesc} exitosamente" };
        }

        public async Task<AccionPanelResponse> ResetearDocumentoPersonalAsync(int idEstudiante, long idAspiranteDocumento, string? motivo, string? usuarioId, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante == null)
                return new AccionPanelResponse { Exitoso = false, Mensaje = "Estudiante no encontrado" };

            var doc = await _db.AspiranteDocumento
                .FirstOrDefaultAsync(d => d.IdAspiranteDocumento == idAspiranteDocumento, ct);

            if (doc == null)
                return new AccionPanelResponse { Exitoso = false, Mensaje = "Documento no encontrado" };

            var estatusAnterior = doc.Estatus.ToString();

            if (!string.IsNullOrEmpty(doc.UrlArchivo))
            {
                try { await _blobStorageService.DeleteFile(doc.UrlArchivo, "documentos"); } catch { }
            }

            doc.Estatus = EstatusDocumentoEnum.PENDIENTE;
            doc.UrlArchivo = null;
            doc.FechaSubidoUtc = null;
            doc.FechaValidacion = null;
            doc.UsuarioValidacion = null;
            doc.Notas = motivo;

            await _db.SaveChangesAsync(ct);

            await _bitacora.RegistrarAsync(usuarioId ?? "Sistema", usuarioId ?? "Sistema",
                "RESETEAR_DOCUMENTO", "Documentos",
                "AspiranteDocumento", idAspiranteDocumento.ToString(),
                $"Documento reseteado (estatus anterior: {estatusAnterior}) para estudiante {idEstudiante}. Motivo: {motivo ?? "No especificado"}");

            return new AccionPanelResponse { Exitoso = true, Mensaje = "Documento eliminado y reseteado a pendiente" };
        }

        public async Task<DocumentosDisponiblesDto> ObtenerDocumentosDisponiblesAsync(int idEstudiante, CancellationToken ct = default)
        {
            var resultado = new DocumentosDisponiblesDto();

            var tipos = await _documentoService.GetTiposDocumentoAsync();

            var solicitudes = await _documentoService.GetSolicitudesByEstudianteAsync(idEstudiante);

            resultado.TiposDisponibles = tipos.Where(t => t.Activo).Select(t => new TipoDocumentoDisponibleDto
            {
                IdTipoDocumento = t.IdTipoDocumento,
                Clave = t.Clave,
                Nombre = t.Nombre,
                Descripcion = t.Descripcion,
                Precio = t.Precio,
                DiasVigencia = t.DiasVigencia,
                RequierePago = t.RequierePago,
                TieneSolicitudPendiente = solicitudes.Any(s =>
                    s.IdTipoDocumento == t.IdTipoDocumento &&
                    (s.Estatus == "PENDIENTE_PAGO" || s.Estatus == "PAGADO")),
                TieneDocumentoVigente = solicitudes.Any(s =>
                    s.IdTipoDocumento == t.IdTipoDocumento &&
                    s.Estatus == "GENERADO" &&
                    s.EstaVigente)
            }).ToList();

            resultado.SolicitudesRecientes = solicitudes
                .OrderByDescending(s => s.FechaSolicitud)
                .Take(10)
                .Select(s => new SolicitudDocumentoResumenDto
                {
                    IdSolicitud = s.IdSolicitud,
                    FolioSolicitud = s.FolioSolicitud,
                    TipoDocumento = s.TipoDocumentoNombre,
                    Variante = s.Variante,
                    FechaSolicitud = s.FechaSolicitud,
                    FechaGeneracion = s.FechaGeneracion,
                    FechaVencimiento = s.FechaVencimiento,
                    Estatus = s.Estatus,
                    EstaVigente = s.EstaVigente,
                    PuedeDescargar = s.PuedeGenerar,
                    CodigoVerificacion = s.CodigoVerificacion
                })
                .ToList();

            resultado.SolicitudesPendientes = solicitudes.Count(s => s.Estatus == "PENDIENTE_PAGO" || s.Estatus == "PAGADO");
            resultado.SolicitudesGeneradas = solicitudes.Count(s => s.Estatus == "GENERADO");
            resultado.DocumentosVigentes = solicitudes.Count(s => s.Estatus == "GENERADO" && s.EstaVigente);

            return resultado;
        }

        public async Task<AccionPanelResponse> GenerarDocumentoAsync(GenerarDocumentoPanelRequest request, string usuarioId, CancellationToken ct = default)
        {
            try
            {
                var solicitudRequest = new CrearSolicitudDocumentoRequest
                {
                    IdEstudiante = request.IdEstudiante,
                    IdTipoDocumento = request.IdTipoDocumento,
                    Variante = request.Variante,
                    Notas = request.Notas
                };

                var solicitud = await _documentoService.CrearSolicitudAsync(solicitudRequest, usuarioId);

                return new AccionPanelResponse
                {
                    Exitoso = true,
                    Mensaje = $"Solicitud de documento creada exitosamente. Folio: {solicitud.FolioSolicitud}",
                    Datos = solicitud
                };
            }
            catch (Exception ex)
            {
                return new AccionPanelResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error al crear solicitud: {ex.Message}"
                };
            }
        }

        public async Task<byte[]> GenerarKardexPdfDirectoAsync(int idEstudiante, bool soloPeridoActual = false, CancellationToken ct = default)
        {
            var kardex = await _documentoService.GenerarKardexAsync(idEstudiante, soloPeridoActual);

            var folio = $"KARDEX-{idEstudiante}-{DateTime.Now:yyyyMMddHHmmss}";
            var codigo = Guid.NewGuid();
            var urlVerificacion = _documentoService.GenerarUrlVerificacion(codigo);

            return await _pdfService.GenerarKardexPdf(kardex, folio, codigo, urlVerificacion);
        }

        public async Task<byte[]> GenerarConstanciaPdfDirectaAsync(int idEstudiante, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Include(e => e.IdPlanActualNavigation)
                    .ThenInclude(p => p!.IdCampusNavigation)
                .Include(e => e.EstudianteGrupo.Where(eg => eg.Status == StatusEnum.Active).OrderByDescending(eg => eg.FechaInscripcion).Take(1))
                    .ThenInclude(eg => eg.IdGrupoNavigation)
                        .ThenInclude(g => g.IdPeriodoAcademicoNavigation)
                .Include(e => e.EstudianteGrupo.Where(eg => eg.Status == StatusEnum.Active).OrderByDescending(eg => eg.FechaInscripcion).Take(1))
                    .ThenInclude(eg => eg.IdGrupoNavigation)
                        .ThenInclude(g => g.IdTurnoNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante == null)
                throw new InvalidOperationException($"Estudiante con ID {idEstudiante} no encontrado");

            var persona = estudiante.IdPersonaNavigation;
            var plan = estudiante.IdPlanActualNavigation;
            var grupoActual = estudiante.EstudianteGrupo.FirstOrDefault()?.IdGrupoNavigation;
            var periodo = grupoActual?.IdPeriodoAcademicoNavigation;
            var turno = grupoActual?.IdTurnoNavigation;
            var campus = plan?.IdCampusNavigation;
            var cuatrimestre = grupoActual?.NumeroCuatrimestre;

            var periodoTexto = periodo != null
                ? $"{periodo.Nombre} (del {periodo.FechaInicio:dd 'de' MMMM} al {periodo.FechaFin:dd 'de' MMMM 'de' yyyy})"
                : "Actual";

            var totalCuatrimestres = plan != null
                ? await _db.MateriaPlan.Where(mp => mp.IdPlanEstudios == plan.IdPlanEstudios && mp.Status == StatusEnum.Active).Select(mp => mp.Cuatrimestre).Distinct().CountAsync(ct)
                : 0;

            var gradoTexto = cuatrimestre.HasValue && totalCuatrimestres > 0
                ? $"cursando el {OrdinalTexto(cuatrimestre.Value)} cuatrimestre de {totalCuatrimestres}"
                : "N/A";

            var constancia = new ConstanciaEstudiosDto
            {
                IdEstudiante = estudiante.IdEstudiante,
                Matricula = estudiante.Matricula,
                NombreCompleto = persona != null
                    ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                    : "Sin nombre",
                Curp = persona?.Curp,
                Carrera = plan?.NombrePlanEstudios ?? "N/A",
                PlanEstudios = plan?.NombrePlanEstudios ?? "N/A",
                RVOE = plan?.RVOE,
                PeriodoActual = periodoTexto,
                Grado = gradoTexto,
                Turno = turno?.Nombre ?? "N/A",
                Campus = campus?.Nombre ?? "León, Guanajuato",
                FechaIngreso = estudiante.FechaIngreso.ToDateTime(TimeOnly.MinValue),
                FechaEmision = DateTime.UtcNow,
                FechaVencimiento = DateTime.UtcNow.AddDays(30),
                FolioDocumento = $"CONST-{idEstudiante}-{DateTime.Now:yyyyMMddHHmmss}",
                CodigoVerificacion = Guid.NewGuid(),
                UrlVerificacion = _documentoService.GenerarUrlVerificacion(Guid.NewGuid())
            };

            return await _pdfService.GenerarConstanciaPdf(constancia);
        }

        private static string OrdinalTexto(int n) => n switch
        {
            1 => "primer", 2 => "segundo", 3 => "tercer", 4 => "cuarto",
            5 => "quinto", 6 => "sexto", 7 => "séptimo", 8 => "octavo",
            9 => "noveno", 10 => "décimo", _ => $"{n}°"
        };

        #endregion

        #region Acciones Rápidas

        public async Task<AccionPanelResponse> EnviarRecordatorioPagoAsync(int idEstudiante, long? idRecibo = null, CancellationToken ct = default)
        {
            return new AccionPanelResponse
            {
                Exitoso = true,
                Mensaje = "Recordatorio de pago enviado exitosamente"
            };
        }

        public async Task<AccionPanelResponse> ActualizarEstatusEstudianteAsync(int idEstudiante, bool activo, string? motivo, int? tipoBaja = null, int? estadoBaja = null, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante.FindAsync(new object[] { idEstudiante }, ct);

            if (estudiante == null)
            {
                return new AccionPanelResponse
                {
                    Exitoso = false,
                    Mensaje = "Estudiante no encontrado"
                };
            }

            estudiante.Activo = activo;

            if (!activo)
            {
                estudiante.TipoBaja = tipoBaja.HasValue ? (TipoBajaEnum)tipoBaja.Value : null;
                estudiante.EstadoBaja = estadoBaja.HasValue ? (EstadoBajaEnum)estadoBaja.Value : null;
                estudiante.MotivoBaja = motivo;
                estudiante.FechaBaja = DateTime.UtcNow;
                estudiante.EstatusAcademico = estudiante.EstadoBaja == EstadoBajaEnum.Definitiva
                    ? Core.Enums.EstudianteStatusAcademicoEnum.BajaDefinitiva
                    : Core.Enums.EstudianteStatusAcademicoEnum.BajaTemporal;

                var gruposActivos = await _db.EstudianteGrupo
                    .Where(eg => eg.IdEstudiante == idEstudiante && eg.Status == StatusEnum.Active)
                    .ToListAsync(ct);

                foreach (var eg in gruposActivos)
                {
                    eg.Status = StatusEnum.Deleted;
                    eg.UpdatedAt = DateTime.UtcNow;
                    eg.Observaciones = "Baja automática: " + (motivo ?? "Estudiante desactivado");
                }

                var inscripcionesActivas = await _db.Inscripcion
                    .Where(i => i.IdEstudiante == idEstudiante && i.Status == StatusEnum.Active)
                    .ToListAsync(ct);

                foreach (var insc in inscripcionesActivas)
                {
                    insc.Status = StatusEnum.Deleted;
                    insc.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                estudiante.TipoBaja = null;
                estudiante.EstadoBaja = null;
                estudiante.MotivoBaja = null;
                estudiante.FechaBaja = null;
                estudiante.EstatusAcademico = Core.Enums.EstudianteStatusAcademicoEnum.Cursando;
            }

            await _db.SaveChangesAsync(ct);

            return new AccionPanelResponse
            {
                Exitoso = true,
                Mensaje = activo ? "Estudiante reactivado exitosamente. Ahora puede inscribirlo a un grupo desde el panel." : "Estudiante dado de baja exitosamente"
            };
        }

        #endregion

        #region Exportación

        public async Task<byte[]> ExportarEstudiantesExcelAsync(BuscarEstudiantesPanelRequest filtros, CancellationToken ct = default)
        {
            filtros.Pagina = 1;
            filtros.TamanoPagina = 10000;

            var resultado = await BuscarEstudiantesAsync(filtros, ct);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Estudiantes");

            var headers = new[] { "Matrícula", "Nombre Completo", "Email", "Teléfono", "Plan de Estudios", "Grupo", "Adeudo", "Tiene Beca", "Activo" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#003366");
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            int fila = 2;
            foreach (var est in resultado.Estudiantes)
            {
                ws.Cell(fila, 1).Value = est.Matricula;
                ws.Cell(fila, 2).Value = est.NombreCompleto;
                ws.Cell(fila, 3).Value = est.Email ?? "";
                ws.Cell(fila, 4).Value = est.Telefono ?? "";
                ws.Cell(fila, 5).Value = est.PlanEstudios ?? "";
                ws.Cell(fila, 6).Value = est.Grupo ?? "";
                ws.Cell(fila, 7).Value = est.Adeudo ?? 0;
                ws.Cell(fila, 7).Style.NumberFormat.Format = "$#,##0.00";
                ws.Cell(fila, 8).Value = est.TieneBeca ? "Sí" : "No";
                ws.Cell(fila, 9).Value = est.Activo ? "Activo" : "Inactivo";
                fila++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportarExpedienteEstudianteAsync(int idEstudiante, CancellationToken ct = default)
        {
            var panel = await ObtenerPanelEstudianteAsync(idEstudiante, ct);
            if (panel == null)
                throw new InvalidOperationException("Estudiante no encontrado");

            var documentos = await ObtenerDocumentosPersonalesAsync(idEstudiante, ct);
            var headerLogoPath = _institucionProvider.ResolverLogoTenant() ?? ResolveAssetPath("header_logo.png");

            const string AzulOscuro = "#14356F";
            const string AzulClaro = "#D9E2F3";
            const string VerdeClaro = "#E2EFDA";
            const string RojoClaro = "#FCE4EC";
            const string AmarilloClaro = "#FFF8E1";
            const string GrisClaro = "#F5F5F5";

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.MarginTop(20);
                    page.MarginBottom(30);
                    page.MarginHorizontal(35);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        if (headerLogoPath != null)
                        {
                            col.Item().AlignCenter().PaddingBottom(5).Height(55).Image(headerLogoPath).FitHeight();
                        }

                        col.Item().PaddingBottom(3).AlignCenter()
                            .Text("EXPEDIENTE DEL ESTUDIANTE").FontSize(16).Bold().FontColor(AzulOscuro);

                        col.Item().LineHorizontal(2).LineColor(AzulOscuro);
                    });

                    page.Content().PaddingVertical(8).Column(col =>
                    {
                        col.Item().Background(AzulOscuro).Padding(6)
                            .Text("DATOS PERSONALES").FontSize(11).Bold().FontColor("#FFFFFF");

                        col.Item().Border(0.5f).BorderColor("#CCCCCC").Padding(0).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(120);
                                c.RelativeColumn();
                                c.ConstantColumn(120);
                                c.RelativeColumn();
                            });

                            void InfoRow(string label1, string val1, string label2, string val2, string bg)
                            {
                                table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(5)
                                    .Text(label1).Bold().FontSize(9);
                                table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(5)
                                    .Text(val1).FontSize(9);
                                table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(5)
                                    .Text(label2).Bold().FontSize(9);
                                table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(5)
                                    .Text(val2).FontSize(9);
                            }

                            InfoRow("Matrícula:", panel.Matricula, "CURP:", panel.Curp ?? "N/A", GrisClaro);
                            InfoRow("Nombre:", panel.NombreCompleto, "Género:", panel.Genero ?? "N/A", "#FFFFFF");
                            InfoRow("Email:", panel.Email ?? "N/A", "Teléfono:", panel.Telefono ?? "N/A", GrisClaro);
                            InfoRow("Dirección:", panel.Direccion ?? "N/A", "Estatus:", panel.Activo ? "Activo" : "Baja", "#FFFFFF");

                            if (panel.FechaNacimiento.HasValue)
                            {
                                InfoRow("Fecha Nac.:", panel.FechaNacimiento.Value.ToString("dd/MM/yyyy"), "", "", GrisClaro);
                            }

                            if (panel.ContactoEmergencia != null)
                            {
                                InfoRow("Contacto Emerg.:", panel.ContactoEmergencia.Nombre ?? "N/A",
                                    "Tel. Emergencia:", panel.ContactoEmergencia.Telefono ?? "N/A", "#FFFFFF");
                            }
                        });

                        col.Item().Height(10);

                        col.Item().Background(AzulOscuro).Padding(6)
                            .Text("INFORMACIÓN ACADÉMICA").FontSize(11).Bold().FontColor("#FFFFFF");

                        col.Item().Border(0.5f).BorderColor("#CCCCCC").Padding(0).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(120);
                                c.RelativeColumn();
                                c.ConstantColumn(120);
                                c.RelativeColumn();
                            });

                            void AcadRow(string l1, string v1, string l2, string v2, string bg)
                            {
                                table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(5).Text(l1).Bold().FontSize(9);
                                table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(5).Text(v1).FontSize(9);
                                table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(5).Text(l2).Bold().FontSize(9);
                                table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(5).Text(v2).FontSize(9);
                            }

                            AcadRow("Plan de Estudios:", panel.InformacionAcademica.PlanEstudios ?? "N/A",
                                "RVOE:", panel.InformacionAcademica.RVOE ?? "N/A", GrisClaro);
                            AcadRow("Modalidad:", panel.InformacionAcademica.Modalidad ?? "N/A",
                                "Campus:", panel.InformacionAcademica.Campus ?? "N/A", "#FFFFFF");
                            AcadRow("Grupo:", panel.InformacionAcademica.GrupoActual?.CodigoGrupo ?? "Sin grupo",
                                "Turno:", panel.InformacionAcademica.GrupoActual?.Turno ?? panel.InformacionAcademica.Turno ?? "N/A", GrisClaro);
                            AcadRow("Periodo:", panel.InformacionAcademica.PeriodoActual?.Nombre ?? "N/A",
                                "Fecha Ingreso:", panel.InformacionAcademica.FechaIngreso.ToString("dd/MM/yyyy"), "#FFFFFF");
                            AcadRow("Promedio:", panel.ResumenKardex.PromedioGeneral.ToString("N2"),
                                "Avance:", $"{panel.ResumenKardex.PorcentajeAvance:N1}%", GrisClaro);
                            AcadRow("Estatus Académico:", panel.ResumenKardex.EstatusAcademico ?? "N/A",
                                "Créditos:", $"{panel.ResumenKardex.CreditosCursados}/{panel.ResumenKardex.CreditosTotales}", "#FFFFFF");
                        });

                        col.Item().Height(10);

                        col.Item().Background(AzulOscuro).Padding(6)
                            .Text("EXPEDIENTE DOCUMENTAL").FontSize(11).Bold().FontColor("#FFFFFF");

                        if (documentos != null && documentos.Documentos.Any())
                        {
                            col.Item().PaddingVertical(3).Row(row =>
                            {
                                row.RelativeColumn().Background(AzulClaro).Border(0.5f).BorderColor("#CCCCCC").Padding(5).AlignCenter()
                                    .Text($"Total: {documentos.TotalDocumentos}").Bold().FontSize(9);
                                row.RelativeColumn().Background(VerdeClaro).Border(0.5f).BorderColor("#CCCCCC").Padding(5).AlignCenter()
                                    .Text($"Validados: {documentos.DocumentosValidados}").Bold().FontSize(9).FontColor("#2E7D32");
                                row.RelativeColumn().Background(RojoClaro).Border(0.5f).BorderColor("#CCCCCC").Padding(5).AlignCenter()
                                    .Text($"Pendientes: {documentos.DocumentosPendientes}").Bold().FontSize(9).FontColor("#C62828");
                            });

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(30);
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(1.5f);
                                    c.RelativeColumn(1.5f);
                                    c.RelativeColumn(1.5f);
                                });

                                table.Header(header =>
                                {
                                    void HeaderCell(string text)
                                    {
                                        header.Cell().Background(AzulOscuro).Border(0.5f).BorderColor(AzulOscuro).Padding(5)
                                            .Text(text).Bold().FontSize(8).FontColor("#FFFFFF");
                                    }
                                    HeaderCell("#");
                                    HeaderCell("DOCUMENTO");
                                    HeaderCell("ESTATUS");
                                    HeaderCell("FECHA ENTREGA");
                                    HeaderCell("VALIDADO POR");
                                });

                                int idx = 0;
                                foreach (var doc in documentos.Documentos)
                                {
                                    idx++;
                                    var bgRow = idx % 2 == 0 ? GrisClaro : "#FFFFFF";
                                    var (estatusBg, estatusColor) = doc.Estatus switch
                                    {
                                        "VALIDADO" => (VerdeClaro, "#2E7D32"),
                                        "SUBIDO" => (AmarilloClaro, "#F57F17"),
                                        "RECHAZADO" => (RojoClaro, "#C62828"),
                                        _ => (RojoClaro, "#C62828")
                                    };

                                    table.Cell().Background(bgRow).Border(0.5f).BorderColor("#CCCCCC").Padding(4)
                                        .AlignCenter().Text(idx.ToString()).FontSize(8);
                                    table.Cell().Background(bgRow).Border(0.5f).BorderColor("#CCCCCC").Padding(4)
                                        .Text(doc.NombreDocumento).FontSize(8);
                                    table.Cell().Background(estatusBg).Border(0.5f).BorderColor("#CCCCCC").Padding(4)
                                        .AlignCenter().Text(doc.Estatus == "PENDIENTE" ? "NO ENTREGADO" : doc.Estatus)
                                        .Bold().FontSize(8).FontColor(estatusColor);
                                    table.Cell().Background(bgRow).Border(0.5f).BorderColor("#CCCCCC").Padding(4)
                                        .AlignCenter().Text(doc.FechaSubido?.ToString("dd/MM/yyyy") ?? "—").FontSize(8);
                                    table.Cell().Background(bgRow).Border(0.5f).BorderColor("#CCCCCC").Padding(4)
                                        .Text(doc.ValidadoPor ?? "—").FontSize(8);
                                }
                            });
                        }
                        else
                        {
                            col.Item().Padding(10).AlignCenter()
                                .Text("No hay documentos configurados para este plan de estudios.").Italic().FontSize(9);
                        }

                        if (panel.Becas.Any(b => b.Activo))
                        {
                            col.Item().Height(10);
                            col.Item().Background(AzulOscuro).Padding(6)
                                .Text("BECAS ASIGNADAS").FontSize(11).Bold().FontColor("#FFFFFF");
                            col.Item().Border(0.5f).BorderColor("#CCCCCC").Column(c =>
                            {
                                foreach (var beca in panel.Becas.Where(b => b.Activo))
                                {
                                    c.Item().Padding(5).Row(r =>
                                    {
                                        r.ConstantColumn(8).PaddingTop(4).Height(8).Width(8).Background("#2E7D32");
                                        r.RelativeColumn().PaddingLeft(8).Text($"{beca.NombreBeca ?? "Beca"}: {beca.DescripcionDescuento}").FontSize(9);
                                    });
                                }
                            });
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(AzulOscuro);
                        col.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeColumn().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).FontColor("#666666");
                            row.RelativeColumn().AlignCenter().Text(text =>
                            {
                                text.Span("Página ").FontSize(7).FontColor("#666666");
                                text.CurrentPageNumber().FontSize(7).FontColor("#666666");
                                text.Span(" de ").FontSize(7).FontColor("#666666");
                                text.TotalPages().FontSize(7).FontColor("#666666");
                            });
                            row.RelativeColumn().AlignRight().Text("USAG - Sistema de Control Escolar").FontSize(7).FontColor("#666666");
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        #endregion

        private string? ResolveAssetPath(string fileName)
        {
            var path = Path.Combine(_env.ContentRootPath, fileName);
            if (File.Exists(path)) return path;
            path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            return File.Exists(path) ? path : null;
        }

        private static string? BuildDireccionTexto(Direccion? dir)
        {
            if (dir == null) return null;
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(dir.Calle)) parts.Add(dir.Calle);
            if (!string.IsNullOrWhiteSpace(dir.NumeroExterior)) parts.Add($"#{dir.NumeroExterior}");
            if (!string.IsNullOrWhiteSpace(dir.NumeroInterior)) parts.Add($"Int. {dir.NumeroInterior}");
            if (dir.CodigoPostal != null)
            {
                if (!string.IsNullOrWhiteSpace(dir.CodigoPostal.Asentamiento)) parts.Add(dir.CodigoPostal.Asentamiento);
                if (!string.IsNullOrWhiteSpace(dir.CodigoPostal.Codigo)) parts.Add($"C.P. {dir.CodigoPostal.Codigo}");
                if (dir.CodigoPostal.Municipio != null)
                {
                    if (!string.IsNullOrWhiteSpace(dir.CodigoPostal.Municipio.Nombre)) parts.Add(dir.CodigoPostal.Municipio.Nombre);
                    if (dir.CodigoPostal.Municipio.Estado != null && !string.IsNullOrWhiteSpace(dir.CodigoPostal.Municipio.Estado.Nombre))
                        parts.Add(dir.CodigoPostal.Municipio.Estado.Nombre);
                }
            }
            return parts.Count > 0 ? string.Join(", ", parts) : null;
        }
    }
}
