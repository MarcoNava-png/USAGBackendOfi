using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication2.Core.DTOs.PortalAlumno;
using WebApplication2.Core.Enums;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class PortalAlumnoService : IPortalAlumnoService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PortalAlumnoService> _logger;

        public PortalAlumnoService(ApplicationDbContext db, ILogger<PortalAlumnoService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<MiPerfilDto?> ObtenerMiPerfilAsync(string userId, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .AsNoTracking()
                .Include(e => e.IdPersonaNavigation)
                    .ThenInclude(p => p!.IdDireccionNavigation)
                        .ThenInclude(d => d!.CodigoPostal)
                            .ThenInclude(cp => cp!.Municipio)
                                .ThenInclude(m => m!.Estado)
                .Include(e => e.IdPersonaNavigation)
                    .ThenInclude(p => p!.IdGeneroNavigation)
                .Include(e => e.IdPersonaNavigation)
                    .ThenInclude(p => p!.IdEstadoCivilNavigation)
                .Include(e => e.IdPlanActualNavigation)
                    .ThenInclude(p => p!.IdCampusNavigation)
                .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.Activo, ct);

            if (estudiante == null) return null;

            var persona = estudiante.IdPersonaNavigation;
            var direccion = persona?.IdDireccionNavigation;

            return new MiPerfilDto
            {
                IdEstudiante = estudiante.IdEstudiante,
                Matricula = estudiante.Matricula,
                NombreCompleto = $"{persona?.Nombre} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim(),
                Nombre = persona?.Nombre,
                ApellidoPaterno = persona?.ApellidoPaterno,
                ApellidoMaterno = persona?.ApellidoMaterno,
                Email = estudiante.Email ?? persona?.Correo,
                Telefono = persona?.Telefono,
                Celular = persona?.Celular,
                Curp = persona?.Curp,
                Rfc = persona?.Rfc,
                FechaNacimiento = persona?.FechaNacimiento,
                Genero = persona?.IdGeneroNavigation?.DescGenero,
                EstadoCivil = persona?.IdEstadoCivilNavigation?.DescEstadoCivil,
                Nacionalidad = persona?.Nacionalidad,
                Direccion = direccion != null ? new MiDireccionDto
                {
                    Calle = direccion.Calle,
                    NumeroExterior = direccion.NumeroExterior,
                    NumeroInterior = direccion.NumeroInterior,
                    Colonia = direccion.CodigoPostal?.Asentamiento,
                    CodigoPostal = direccion.CodigoPostal?.Codigo,
                    Municipio = direccion.CodigoPostal?.Municipio?.Nombre,
                    Estado = direccion.CodigoPostal?.Municipio?.Estado?.Nombre
                } : null,
                ContactoEmergencia = persona != null ? new MiContactoEmergenciaDto
                {
                    Nombre = persona.NombreContactoEmergencia,
                    Telefono = persona.TelefonoContactoEmergencia,
                    Parentesco = persona.ParentescoContactoEmergencia
                } : null,
                PlanEstudios = estudiante.IdPlanActualNavigation?.NombrePlanEstudios,
                ClavePlanEstudios = estudiante.IdPlanActualNavigation?.ClavePlanEstudios,
                Campus = estudiante.IdPlanActualNavigation?.IdCampusNavigation?.Nombre,
                FechaIngreso = estudiante.FechaIngreso
            };
        }

        public async Task<bool> ActualizarMiPerfilAsync(string userId, ActualizarMiPerfilRequest request, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.Activo, ct);

            if (estudiante == null) return false;

            var persona = estudiante.IdPersonaNavigation;

            if (!string.IsNullOrWhiteSpace(request.Telefono))
                persona.Telefono = request.Telefono;

            if (!string.IsNullOrWhiteSpace(request.Celular))
                persona.Celular = request.Celular;

            if (request.ContactoEmergencia != null)
            {
                persona.NombreContactoEmergencia = request.ContactoEmergencia.Nombre;
                persona.TelefonoContactoEmergencia = request.ContactoEmergencia.Telefono;
                persona.ParentescoContactoEmergencia = request.ContactoEmergencia.Parentesco;
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<MisMateriasDto> ObtenerMisMateriasAsync(string userId, int? idPeriodoAcademico = null, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.Activo, ct);

            if (estudiante == null) return new MisMateriasDto();

            var inscripcionesQuery = _db.Inscripcion
                .AsNoTracking()
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm!.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp!.IdMateriaNavigation)
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm!.IdGrupoNavigation)
                        .ThenInclude(g => g!.IdPeriodoAcademicoNavigation)
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm!.IdProfesorNavigation)
                        .ThenInclude(p => p!.IdPersonaNavigation)
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm!.Horario)
                        .ThenInclude(h => h.IdDiaSemanaNavigation)
                .Where(i => i.IdEstudiante == estudiante.IdEstudiante && i.Status == StatusEnum.Active);

            if (idPeriodoAcademico.HasValue)
            {
                inscripcionesQuery = inscripcionesQuery.Where(i =>
                    i.IdGrupoMateriaNavigation!.IdGrupoNavigation!.IdPeriodoAcademico == idPeriodoAcademico);
            }
            else
            {
                inscripcionesQuery = inscripcionesQuery.Where(i =>
                    i.IdGrupoMateriaNavigation!.IdGrupoNavigation!.IdPeriodoAcademicoNavigation!.EsPeriodoActual);
            }

            var inscripciones = await inscripcionesQuery
                .OrderBy(i => i.IdGrupoMateriaNavigation!.IdMateriaPlanNavigation!.IdMateriaNavigation!.Nombre)
                .ToListAsync(ct);

            var materias = new List<MiMateriaInscritaDto>();
            string? periodoNombre = null;

            foreach (var inscripcion in inscripciones)
            {
                var gm = inscripcion.IdGrupoMateriaNavigation;
                if (gm == null) continue;

                var materia = gm.IdMateriaPlanNavigation?.IdMateriaNavigation;
                if (materia == null) continue;

                var grupo = gm.IdGrupoNavigation;
                var docente = gm.IdProfesorNavigation?.IdPersonaNavigation;

                if (periodoNombre == null)
                    periodoNombre = grupo?.IdPeriodoAcademicoNavigation?.Nombre;

                var horarios = (gm.Horario ?? new List<Core.Models.Horario>())
                    .OrderBy(h => h.IdDiaSemana)
                    .ThenBy(h => h.HoraInicio)
                    .Select(h => new MiHorarioClaseDto
                    {
                        IdHorario = h.IdHorario,
                        IdDiaSemana = h.IdDiaSemana,
                        DiaSemana = h.IdDiaSemanaNavigation?.Nombre ?? string.Empty,
                        HoraInicio = h.HoraInicio.ToString("HH:mm"),
                        HoraFin = h.HoraFin.ToString("HH:mm"),
                        Aula = h.Aula ?? gm.Aula
                    }).ToList();

                materias.Add(new MiMateriaInscritaDto
                {
                    IdInscripcion = inscripcion.IdInscripcion,
                    IdGrupoMateria = gm.IdGrupoMateria,
                    IdMateria = materia.IdMateria,
                    ClaveMateria = materia.Clave,
                    NombreMateria = materia.Nombre,
                    Creditos = materia.Creditos,
                    GrupoCodigo = grupo?.CodigoGrupo ?? string.Empty,
                    NumeroCuatrimestre = grupo?.NumeroCuatrimestre,
                    Aula = gm.Aula,
                    Docente = docente != null ? $"{docente.Nombre} {docente.ApellidoPaterno}".Trim() : null,
                    FechaInscripcion = inscripcion.FechaInscripcion,
                    Estado = inscripcion.Estado,
                    CalificacionFinal = inscripcion.CalificacionFinal,
                    Horarios = horarios
                });
            }

            return new MisMateriasDto
            {
                TotalMaterias = materias.Count,
                MateriasConCalificacionFinal = materias.Count(m => m.CalificacionFinal.HasValue),
                MateriasEnCurso = materias.Count(m => !m.CalificacionFinal.HasValue),
                PeriodoAcademico = periodoNombre,
                Materias = materias
            };
        }

        public async Task<MisCalificacionesDto> ObtenerMisCalificacionesAsync(string userId, int? idPeriodoAcademico = null, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.Activo, ct);

            if (estudiante == null) return new MisCalificacionesDto();

            var inscripcionesQuery = _db.Inscripcion
                .AsNoTracking()
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm!.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp!.IdMateriaNavigation)
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm!.IdGrupoNavigation)
                        .ThenInclude(g => g!.IdPeriodoAcademicoNavigation)
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm!.IdProfesorNavigation)
                        .ThenInclude(p => p!.IdPersonaNavigation)
                .Where(i => i.IdEstudiante == estudiante.IdEstudiante && i.Status == StatusEnum.Active);

            if (idPeriodoAcademico.HasValue)
            {
                inscripcionesQuery = inscripcionesQuery.Where(i =>
                    i.IdGrupoMateriaNavigation!.IdGrupoNavigation!.IdPeriodoAcademico == idPeriodoAcademico);
            }

            var inscripciones = await inscripcionesQuery.ToListAsync(ct);

            var materias = new List<MiMateriaCalificacionDto>();

            foreach (var inscripcion in inscripciones)
            {
                var gm = inscripcion.IdGrupoMateriaNavigation;
                if (gm == null) continue;

                var materia = gm.IdMateriaPlanNavigation?.IdMateriaNavigation;
                if (materia == null) continue;

                var calificacionesParciales = await _db.CalificacionesParciales
                    .AsNoTracking()
                    .Include(cp => cp.Parcial)
                    .Where(cp => cp.InscripcionId == inscripcion.IdInscripcion && cp.Status == StatusEnum.Active)
                    .OrderBy(cp => cp.Parcial!.Orden)
                    .ToListAsync(ct);

                var calificacionDetalles = await _db.CalificacionDetalle
                    .AsNoTracking()
                    .Where(cd => cd.GrupoMateriaId == gm.IdGrupoMateria && cd.Status == StatusEnum.Active)
                    .ToListAsync(ct);

                var misParciales = calificacionesParciales.Select(cp =>
                {
                    var detallesParcial = calificacionDetalles
                        .Where(cd => cd.CalificacionParcialId == cp.Id)
                        .ToList();

                    decimal? calif = null;
                    if (detallesParcial.Count > 0)
                    {
                        var totalPeso = detallesParcial.Sum(d => d.PesoEvaluacion);
                        if (totalPeso > 0)
                        {
                            var promedioPonderado = detallesParcial
                                .Sum(d => d.MaxPuntos > 0 ? (d.Puntos / d.MaxPuntos) * d.PesoEvaluacion * 10m : 0m);
                            calif = Math.Round(promedioPonderado / totalPeso, 2);
                        }
                    }

                    var publicado = cp.StatusParcial == StatusParcialEnum.Cerrado;

                    return new MiParcialDto
                    {
                        IdParciales = cp.Id,
                        NumeroParcial = cp.Parcial?.Orden ?? 0,
                        Calificacion = publicado ? calif : null,
                        Publicado = publicado,
                        FechaPublicacion = cp.FechaCierre,
                        Evaluaciones = publicado
                            ? detallesParcial.Select(d => new MiEvaluacionDto
                            {
                                IdCalificacionDetalle = d.Id,
                                Descripcion = d.Nombre ?? "Evaluacion",
                                Tipo = d.TipoEvaluacionEnum.ToString(),
                                Peso = d.PesoEvaluacion,
                                PuntajeMaximo = d.MaxPuntos,
                                Puntaje = d.Puntos,
                                FechaAplicacion = d.FechaAplicacion
                            }).ToList()
                            : new List<MiEvaluacionDto>()
                    };
                }).ToList();

                decimal? calificacionFinal = inscripcion.CalificacionFinal;
                if (!calificacionFinal.HasValue && misParciales.Count > 0 && misParciales.All(p => p.Calificacion.HasValue && p.Publicado))
                {
                    calificacionFinal = Math.Round(misParciales.Average(p => p.Calificacion!.Value), 2);
                }

                var estatus = calificacionFinal.HasValue
                    ? (calificacionFinal.Value >= 6 ? "APROBADA" : "REPROBADA")
                    : "EN_CURSO";

                var docente = gm.IdProfesorNavigation?.IdPersonaNavigation;

                materias.Add(new MiMateriaCalificacionDto
                {
                    IdMateria = materia.IdMateria,
                    ClaveMateria = materia.Clave,
                    NombreMateria = materia.Nombre,
                    NombreDocente = docente != null ? $"{docente.Nombre} {docente.ApellidoPaterno}".Trim() : null,
                    GrupoCodigo = gm.IdGrupoNavigation?.CodigoGrupo,
                    PeriodoAcademico = gm.IdGrupoNavigation?.IdPeriodoAcademicoNavigation?.Nombre,
                    Creditos = (int)materia.Creditos,
                    CalificacionFinal = calificacionFinal,
                    Estatus = estatus,
                    Parciales = misParciales
                });
            }

            var aprobadas = materias.Count(m => m.Estatus == "APROBADA");
            var reprobadas = materias.Count(m => m.Estatus == "REPROBADA");
            var enCurso = materias.Count(m => m.Estatus == "EN_CURSO");

            decimal? promedio = null;
            var conCalif = materias.Where(m => m.CalificacionFinal.HasValue).ToList();
            if (conCalif.Count > 0)
                promedio = Math.Round(conCalif.Average(m => m.CalificacionFinal!.Value), 2);

            return new MisCalificacionesDto
            {
                PromedioGeneral = promedio,
                MateriasAprobadas = aprobadas,
                MateriasReprobadas = reprobadas,
                MateriasEnCurso = enCurso,
                Materias = materias
            };
        }

        public async Task<MiAsistenciaDto> ObtenerMiAsistenciaAsync(string userId, int? idPeriodoAcademico = null, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.Activo, ct);

            if (estudiante == null) return new MiAsistenciaDto();

            var misInscripciones = await _db.Inscripcion
                .AsNoTracking()
                .Where(i => i.IdEstudiante == estudiante.IdEstudiante && i.Status == StatusEnum.Active)
                .Select(i => i.IdInscripcion)
                .ToListAsync(ct);

            var asistenciasQuery = _db.Asistencia
                .AsNoTracking()
                .Include(a => a.GrupoMateria)
                    .ThenInclude(gm => gm!.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp!.IdMateriaNavigation)
                .Include(a => a.GrupoMateria)
                    .ThenInclude(gm => gm!.IdGrupoNavigation)
                .Where(a => misInscripciones.Contains(a.InscripcionId) && a.Status == StatusEnum.Active);

            if (idPeriodoAcademico.HasValue)
            {
                asistenciasQuery = asistenciasQuery.Where(a =>
                    a.GrupoMateria!.IdGrupoNavigation!.IdPeriodoAcademico == idPeriodoAcademico);
            }

            var asistencias = await asistenciasQuery.ToListAsync(ct);

            var porMateria = asistencias
                .GroupBy(a => new { a.GrupoMateriaId, Materia = a.GrupoMateria!.IdMateriaPlanNavigation!.IdMateriaNavigation! })
                .Select(g =>
                {
                    var total = g.Count();
                    var pres = g.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Presente);
                    var falt = g.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Ausente);
                    var reta = g.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Retardo);
                    var just = g.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Justificada);
                    var gm = g.First().GrupoMateria!;

                    return new MiAsistenciaMateriaDto
                    {
                        IdMateria = g.Key.Materia.IdMateria,
                        ClaveMateria = g.Key.Materia.Clave,
                        NombreMateria = g.Key.Materia.Nombre,
                        GrupoCodigo = gm.IdGrupoNavigation?.CodigoGrupo,
                        Porcentaje = total > 0 ? Math.Round((decimal)(pres + just) / total * 100, 2) : 0,
                        TotalClases = total,
                        Asistencias = pres,
                        Faltas = falt,
                        Retardos = reta,
                        Justificadas = just,
                        Detalles = g.OrderByDescending(a => a.FechaSesion).Select(a => new MiAsistenciaDetalleDto
                        {
                            Fecha = DateOnly.FromDateTime(a.FechaSesion),
                            Estatus = a.EstadoAsistencia.ToString().ToUpper(),
                            Observacion = a.Observaciones
                        }).ToList()
                    };
                }).ToList();

            var totalGlobal = asistencias.Count;
            var presGlobal = asistencias.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Presente);
            var faltGlobal = asistencias.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Ausente);
            var retaGlobal = asistencias.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Retardo);
            var justGlobal = asistencias.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Justificada);

            return new MiAsistenciaDto
            {
                PorcentajeGeneral = totalGlobal > 0 ? Math.Round((decimal)(presGlobal + justGlobal) / totalGlobal * 100, 2) : 0,
                TotalClases = totalGlobal,
                Asistencias = presGlobal,
                Faltas = faltGlobal,
                Retardos = retaGlobal,
                Justificadas = justGlobal,
                Materias = porMateria
            };
        }

        public async Task<MisPagosDto> ObtenerMisPagosAsync(string userId, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.Activo, ct);

            if (estudiante == null) return new MisPagosDto();

            var recibos = await _db.Recibo
                .AsNoTracking()
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.ConceptoPago)
                .Where(r => r.IdEstudiante == estudiante.IdEstudiante && r.Estatus != EstatusRecibo.CANCELADO)
                .OrderByDescending(r => r.FechaEmision)
                .ToListAsync(ct);

            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            var misRecibos = recibos.Select(r => new MiReciboDto
            {
                IdRecibo = r.IdRecibo,
                Folio = r.Folio,
                FechaEmision = r.FechaEmision,
                FechaVencimiento = r.FechaVencimiento,
                Estatus = r.Estatus.ToString(),
                Subtotal = r.Subtotal,
                Descuento = r.Descuento,
                Recargos = r.Recargos,
                Total = r.Total,
                Saldo = r.Saldo,
                Notas = r.Notas,
                Vencido = r.Saldo > 0 && r.FechaVencimiento < hoy,
                DiasVencido = r.Saldo > 0 && r.FechaVencimiento < hoy
                    ? hoy.DayNumber - r.FechaVencimiento.DayNumber
                    : 0,
                Detalles = r.Detalles.Select(d => new MiReciboDetalleDto
                {
                    IdReciboDetalle = d.IdReciboDetalle,
                    IdConceptoPago = d.IdConceptoPago,
                    Concepto = d.ConceptoPago?.Nombre,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Importe = d.Importe
                }).ToList()
            }).ToList();

            return new MisPagosDto
            {
                TotalAdeudo = recibos.Sum(r => r.Total),
                TotalPagado = recibos.Sum(r => r.Total - r.Saldo),
                SaldoPendiente = recibos.Sum(r => r.Saldo),
                RecibosPendientes = recibos.Count(r => r.Estatus == EstatusRecibo.PENDIENTE || r.Estatus == EstatusRecibo.PARCIAL),
                RecibosVencidos = misRecibos.Count(r => r.Vencido),
                RecibosPagados = recibos.Count(r => r.Estatus == EstatusRecibo.PAGADO),
                Recibos = misRecibos
            };
        }

        public async Task<MiReciboDto?> ObtenerMiReciboDetalleAsync(string userId, long idRecibo, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.Activo, ct);

            if (estudiante == null) return null;

            var recibo = await _db.Recibo
                .AsNoTracking()
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.ConceptoPago)
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Aplicaciones)
                        .ThenInclude(a => a.Pago)
                            .ThenInclude(p => p.MedioPago)
                .FirstOrDefaultAsync(r => r.IdRecibo == idRecibo && r.IdEstudiante == estudiante.IdEstudiante, ct);

            if (recibo == null) return null;

            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            var pagos = recibo.Detalles
                .SelectMany(d => d.Aplicaciones.Select(a => new MiPagoAplicadoDto
                {
                    IdPago = a.Pago.IdPago,
                    FolioPago = a.Pago.FolioPago,
                    FechaPago = a.Pago.FechaPagoUtc,
                    MedioPago = a.Pago.MedioPago?.Descripcion ?? a.Pago.MedioPago?.Clave,
                    MontoAplicado = a.MontoAplicado,
                    Referencia = a.Pago.Referencia
                }))
                .GroupBy(p => p.IdPago)
                .Select(g => new MiPagoAplicadoDto
                {
                    IdPago = g.Key,
                    FolioPago = g.First().FolioPago,
                    FechaPago = g.First().FechaPago,
                    MedioPago = g.First().MedioPago,
                    MontoAplicado = g.Sum(x => x.MontoAplicado),
                    Referencia = g.First().Referencia
                })
                .OrderByDescending(p => p.FechaPago)
                .ToList();

            return new MiReciboDto
            {
                IdRecibo = recibo.IdRecibo,
                Folio = recibo.Folio,
                FechaEmision = recibo.FechaEmision,
                FechaVencimiento = recibo.FechaVencimiento,
                Estatus = recibo.Estatus.ToString(),
                Subtotal = recibo.Subtotal,
                Descuento = recibo.Descuento,
                Recargos = recibo.Recargos,
                Total = recibo.Total,
                Saldo = recibo.Saldo,
                Notas = recibo.Notas,
                Vencido = recibo.Saldo > 0 && recibo.FechaVencimiento < hoy,
                DiasVencido = recibo.Saldo > 0 && recibo.FechaVencimiento < hoy
                    ? hoy.DayNumber - recibo.FechaVencimiento.DayNumber
                    : 0,
                Detalles = recibo.Detalles.Select(d => new MiReciboDetalleDto
                {
                    IdReciboDetalle = d.IdReciboDetalle,
                    IdConceptoPago = d.IdConceptoPago,
                    Concepto = d.ConceptoPago?.Nombre,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Importe = d.Importe
                }).ToList(),
                Pagos = pagos
            };
        }

        public async Task<MisDocumentosOficialesDto> ObtenerMisDocumentosOficialesAsync(string userId, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.Activo, ct);

            if (estudiante == null) return new MisDocumentosOficialesDto();

            var tiposDisponibles = await _db.TiposDocumentoEstudiante
                .AsNoTracking()
                .Where(t => t.Activo)
                .OrderBy(t => t.Orden)
                .Select(t => new DocumentoOficialDisponibleDto
                {
                    IdTipoDocumento = t.IdTipoDocumento,
                    Clave = t.Clave,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    Precio = t.Precio,
                    RequierePago = t.RequierePago,
                    DiasVigencia = t.DiasVigencia
                })
                .ToListAsync(ct);

            var solicitudes = await _db.SolicitudesDocumento
                .AsNoTracking()
                .Include(s => s.TipoDocumento)
                .Include(s => s.Recibo)
                .Where(s => s.IdEstudiante == estudiante.IdEstudiante)
                .OrderByDescending(s => s.FechaSolicitud)
                .Select(s => new MiSolicitudDocumentoDto
                {
                    IdSolicitud = (int)s.IdSolicitud,
                    FolioSolicitud = s.FolioSolicitud,
                    IdTipoDocumento = s.IdTipoDocumento,
                    NombreTipoDocumento = s.TipoDocumento!.Nombre,
                    Variante = s.Variante.ToString(),
                    FechaSolicitud = s.FechaSolicitud,
                    FechaGeneracion = s.FechaGeneracion,
                    FechaVencimiento = s.FechaVencimiento,
                    Estatus = s.Estatus.ToString(),
                    RequierePago = s.TipoDocumento!.RequierePago,
                    IdRecibo = s.IdRecibo,
                    MontoRecibo = s.Recibo != null ? s.Recibo.Total : (decimal?)null,
                    EstatusRecibo = s.Recibo != null ? s.Recibo.Estatus.ToString() : null,
                    CodigoVerificacion = s.CodigoVerificacion.ToString()
                })
                .ToListAsync(ct);

            return new MisDocumentosOficialesDto
            {
                Disponibles = tiposDisponibles,
                MisSolicitudes = solicitudes
            };
        }

        public async Task<MisDocumentosPendientesDto> ObtenerMisDocumentosPendientesAsync(string userId, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.Activo, ct);

            if (estudiante == null)
                return new MisDocumentosPendientesDto();

            var aspirante = await _db.Aspirante
                .AsNoTracking()
                .Where(a => a.IdPersona == estudiante.IdPersona)
                .OrderByDescending(a => a.IdAspirante)
                .FirstOrDefaultAsync(ct);

            if (aspirante == null)
                return new MisDocumentosPendientesDto();

            var ahora = DateTime.UtcNow;

            var docs = await _db.AspiranteDocumento
                .AsNoTracking()
                .Include(d => d.Requisito)
                .Where(d => d.IdAspirante == aspirante.IdAspirante
                    && d.Estatus == Core.Enums.EstatusDocumentoEnum.PENDIENTE
                    && d.Requisito != null
                    && d.Requisito.EsObligatorio)
                .ToListAsync(ct);

            var items = docs.Select(d =>
            {
                var tieneProrroga = d.FechaProrroga.HasValue;
                var vigente = tieneProrroga && d.FechaProrroga!.Value > ahora;
                var vencida = tieneProrroga && d.FechaProrroga!.Value <= ahora;
                int? diasRestantes = vigente ? (int)Math.Ceiling((d.FechaProrroga!.Value - ahora).TotalDays) : null;

                return new MiDocumentoPendienteDto
                {
                    IdAspiranteDocumento = d.IdAspiranteDocumento,
                    Clave = d.Requisito?.Clave ?? string.Empty,
                    Descripcion = d.Requisito?.Descripcion ?? string.Empty,
                    Estatus = vigente ? "PRORROGA_VIGENTE" : (vencida ? "PRORROGA_VENCIDA" : d.Estatus.ToString()),
                    EsObligatorio = true,
                    FechaProrroga = d.FechaProrroga,
                    MotivoProrroga = d.MotivoProrroga,
                    TieneProrrogaVigente = vigente,
                    ProrrogaVencida = vencida,
                    DiasRestantes = diasRestantes
                };
            })
            .OrderBy(x => x.FechaProrroga ?? DateTime.MaxValue)
            .ToList();

            return new MisDocumentosPendientesDto
            {
                TotalPendientes = items.Count,
                ConProrrogaVigente = items.Count(i => i.TieneProrrogaVigente),
                ConProrrogaVencida = items.Count(i => i.ProrrogaVencida),
                SinProrroga = items.Count(i => !i.TieneProrrogaVigente && !i.ProrrogaVencida),
                ProximoVencimiento = items.Where(i => i.TieneProrrogaVigente).Min(i => i.FechaProrroga),
                Documentos = items
            };
        }

        public async Task<IReadOnlyList<Core.DTOs.AspiranteDocumentoDto>> ObtenerMiExpedienteAsync(string userId, CancellationToken ct = default)
        {
            var estudiante = await _db.Estudiante
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.Activo, ct);
            if (estudiante == null) return new List<Core.DTOs.AspiranteDocumentoDto>();

            var idAspirante = await _db.Aspirante
                .AsNoTracking()
                .Where(a => a.IdPersona == estudiante.IdPersona)
                .OrderByDescending(a => a.IdAspirante)
                .Select(a => (int?)a.IdAspirante)
                .FirstOrDefaultAsync(ct);
            if (idAspirante == null) return new List<Core.DTOs.AspiranteDocumentoDto>();

            var docs = await _db.AspiranteDocumento
                .AsNoTracking()
                .Include(d => d.Requisito)
                .Where(d => d.IdAspirante == idAspirante.Value)
                .OrderBy(d => d.Requisito!.Descripcion)
                .Select(d => new Core.DTOs.AspiranteDocumentoDto
                {
                    IdAspiranteDocumento = d.IdAspiranteDocumento,
                    IdDocumentoRequisito = d.IdDocumentoRequisito,
                    Clave = d.Requisito != null ? d.Requisito.Clave : string.Empty,
                    Descripcion = d.Requisito != null ? d.Requisito.Descripcion : string.Empty,
                    Estatus = d.Estatus,
                    UrlArchivo = d.UrlArchivo,
                    Notas = d.Notas,
                    FechaProrroga = d.FechaProrroga,
                    MotivoProrroga = d.MotivoProrroga
                })
                .ToListAsync(ct);

            return docs;
        }
    }
}
