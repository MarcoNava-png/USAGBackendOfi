using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.Dashboard;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Responses.Dashboard;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private static readonly TimeZoneInfo _tzMexico = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");

        private static DateTime AhoraMexico => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _tzMexico);

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardResponseDto> GetDashboardAsync(string userId, string role)
        {
            var response = new DashboardResponseDto { Rol = role };

            response.Data = role.ToLower() switch
            {
                Rol.SUPER_ADMIN => await GetAdminDashboardAsync(),
                Rol.ADMIN => await GetAdminDashboardAsync(),
                Rol.DIRECTOR => await GetDirectorDashboardAsync(userId),
                Rol.FINANZAS => await GetFinanzasDashboardAsync(),
                Rol.CONTROL_ESCOLAR => await GetControlEscolarDashboardAsync(),
                Rol.ADMISIONES => await GetAdmisionesDashboardAsync(),
                Rol.COORDINADOR => await GetCoordinadorDashboardAsync(userId),
                Rol.DOCENTE => await GetDocenteDashboardAsync(userId),
                Rol.ACADEMICO => await GetCoordinadorDashboardAsync(userId),
                Rol.ALUMNO => await GetAlumnoDashboardAsync(userId),
                _ => throw new ArgumentException($"Rol no soportado: {role}")
            };

            return response;
        }

        private async Task<CarteraVencidaDto> GetCarteraVencidaAsync(int? idCampus = null)
        {
            var inicioMes = new DateTime(AhoraMexico.Year, AhoraMexico.Month, 1);
            var hoy = DateOnly.FromDateTime(AhoraMexico);

            var baseVencidos = from r in _context.Recibo
                               where r.Estatus == EstatusRecibo.VENCIDO && r.Saldo > 0 && r.IdEstudiante != null
                               join e in _context.Estudiante on r.IdEstudiante equals e.IdEstudiante
                               join p in _context.PlanEstudios on e.IdPlanActual equals p.IdPlanEstudios
                               where !idCampus.HasValue || p.IdCampus == idCampus.Value
                               select new { r, e, p };

            var porCampus = await (from x in baseVencidos
                                   join c in _context.Campus on x.p.IdCampus equals c.IdCampus
                                   group new { x.r, x.e } by c.Nombre into g
                                   select new CarteraCampusDto
                                   {
                                       Campus = g.Key,
                                       Deuda = g.Sum(z => z.r.Saldo),
                                       Alumnos = g.Select(z => z.e.IdEstudiante).Distinct().Count()
                                   }).OrderByDescending(x => x.Deuda).ToListAsync();

            var vencidosRaw = await baseVencidos.Select(x => new { x.r.Saldo, x.r.FechaVencimiento }).ToListAsync();

            string RangoAnt(int dias) => dias <= 30 ? "1-30 días" : dias <= 60 ? "31-60 días" : dias <= 90 ? "61-90 días" : "90+ días";
            var orden = new[] { "1-30 días", "31-60 días", "61-90 días", "90+ días" };
            var porAntiguedad = vencidosRaw
                .GroupBy(x => RangoAnt(hoy.DayNumber - x.FechaVencimiento.DayNumber))
                .Select(g => new CarteraAntiguedadDto { Rango = g.Key, Deuda = g.Sum(z => z.Saldo), Recibos = g.Count() })
                .OrderBy(x => Array.IndexOf(orden, x.Rango))
                .ToList();

            var porConcepto = await (from rd in _context.ReciboDetalle
                                     join r in _context.Recibo on rd.IdRecibo equals r.IdRecibo
                                     where r.Estatus == EstatusRecibo.VENCIDO && r.Saldo > 0 && r.IdEstudiante != null
                                     join e in _context.Estudiante on r.IdEstudiante equals e.IdEstudiante
                                     join p in _context.PlanEstudios on e.IdPlanActual equals p.IdPlanEstudios
                                     where !idCampus.HasValue || p.IdCampus == idCampus.Value
                                     join cp in _context.ConceptoPago on rd.IdConceptoPago equals cp.IdConceptoPago
                                     group rd by cp.Nombre into g
                                     select new CarteraConceptoDto { Concepto = g.Key, Importe = g.Sum(z => z.Cantidad * z.PrecioUnitario) })
                                     .OrderByDescending(x => x.Importe).Take(8).ToListAsync();

            var recaudado = await (from pa in _context.PagoAplicacion
                                   join pg in _context.Pago on pa.IdPago equals pg.IdPago
                                   where pg.FechaPagoUtc >= inicioMes && pg.Estatus == EstatusPago.CONFIRMADO
                                   join rd in _context.ReciboDetalle on pa.IdReciboDetalle equals rd.IdReciboDetalle
                                   join r in _context.Recibo on rd.IdRecibo equals r.IdRecibo
                                   where r.IdEstudiante != null
                                   join e in _context.Estudiante on r.IdEstudiante equals e.IdEstudiante
                                   join p in _context.PlanEstudios on e.IdPlanActual equals p.IdPlanEstudios
                                   where !idCampus.HasValue || p.IdCampus == idCampus.Value
                                   join c in _context.Campus on p.IdCampus equals c.IdCampus
                                   group pa by c.Nombre into g
                                   select new CarteraCampusDto { Campus = g.Key, Deuda = g.Sum(z => z.MontoAplicado) })
                                   .OrderByDescending(x => x.Deuda).ToListAsync();

            return new CarteraVencidaDto
            {
                TotalVencida = vencidosRaw.Sum(x => x.Saldo),
                TotalAlumnos = porCampus.Sum(x => x.Alumnos),
                RecaudadoMesTotal = recaudado.Sum(x => x.Deuda),
                PorCampus = porCampus,
                PorAntiguedad = porAntiguedad,
                PorConcepto = porConcepto,
                RecaudadoMesPorCampus = recaudado
            };
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            var ahora = AhoraMexico;
            var hoy = DateOnly.FromDateTime(ahora);
            var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
            var hoyDateTime = ahora.Date;
            var inicioMesDateTime = new DateTime(hoy.Year, hoy.Month, 1);

            var pagosHoy = await _context.Pago
                .Where(p => p.FechaPagoUtc.Date == hoyDateTime && p.Estatus == 0)
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            var pagosMes = await _context.Pago
                .Where(p => p.FechaPagoUtc >= inicioMesDateTime && p.Estatus == 0)
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            var deudaTotal = await _context.Recibo
                .Where(r => r.Estatus == EstatusRecibo.VENCIDO)
                .SumAsync(r => (decimal?)r.Saldo) ?? 0;

            var estudiantesConDeuda = await _context.Recibo
                .Where(r => r.Estatus == EstatusRecibo.VENCIDO)
                .Select(r => r.IdEstudiante)
                .Distinct()
                .CountAsync();

            var totalEstudiantesActivos = await _context.Estudiante
                .Where(e => e.Activo)
                .CountAsync();

            var porcentajeMorosidad = totalEstudiantesActivos > 0
                ? Math.Round((decimal)estudiantesConDeuda / totalEstudiantesActivos * 100, 1)
                : 0;

            var aspirantesNuevos = await _context.Aspirante
                .Where(a => a.FechaRegistro >= inicioMesDateTime)
                .CountAsync();

            var conversiones = await _context.Aspirante
                .Where(a => a.IdAspiranteEstatus == 6 && a.FechaRegistro >= inicioMesDateTime)
                .CountAsync();

            var inscripcionesMes = await _context.Inscripcion
                .Where(i => i.FechaInscripcion >= inicioMesDateTime && i.Estado == "Inscrito")
                .CountAsync();

            var bajasMes = await _context.Inscripcion
                .Where(i => i.FechaInscripcion >= inicioMesDateTime && i.Estado == "Baja")
                .CountAsync();

            var asistenciaGlobal = await CalcularAsistenciaGlobalAsync();
            var promedioGeneral = await CalcularPromedioGeneralAsync();
            var tasaReprobacion = await CalcularTasaReprobacionAsync();

            var totalUsuarios = await _context.Users.CountAsync();
            var gruposActivos = await _context.Grupo
                .Where(g => g.IdPeriodoAcademicoNavigation.EsPeriodoActual)
                .CountAsync();
            var profesoresActivos = await _context.Profesor
                .Where(p => p.Activo)
                .CountAsync();

            var alertas = await GenerarAlertasAdminAsync();

            return new AdminDashboardDto
            {
                IngresosDia = pagosHoy,
                IngresosMes = pagosMes,
                DeudaTotal = deudaTotal,
                PorcentajeMorosidad = porcentajeMorosidad,
                TotalMorosos = estudiantesConDeuda,
                AspirantesNuevos = aspirantesNuevos,
                ConversionesDelMes = conversiones,
                InscripcionesDelMes = inscripcionesMes,
                BajasDelMes = bajasMes,
                EstudiantesActivos = totalEstudiantesActivos,
                AsistenciaGlobal = asistenciaGlobal,
                PromedioGeneral = promedioGeneral,
                TasaReprobacion = tasaReprobacion,
                TotalUsuarios = totalUsuarios,
                GruposActivos = gruposActivos,
                ProfesoresActivos = profesoresActivos,
                Alertas = alertas,
                AccionesRapidas = GetAccionesRapidasAdmin(),
                CarteraVencida = await GetCarteraVencidaAsync(null)
            };
        }

        public async Task<DirectorDashboardDto> GetDirectorDashboardAsync(string? userId = null)
        {
            var ahora = AhoraMexico;
            var hoy = DateOnly.FromDateTime(ahora);
            var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
            var inicioMesDateTime = new DateTime(hoy.Year, hoy.Month, 1);

            int? idCampus = string.IsNullOrEmpty(userId)
                ? null
                : await _context.Users.Where(u => u.Id == userId).Select(u => u.IdCampusAsignado).FirstOrDefaultAsync();

            // Subconsultas de IDs del campus (solo se usan cuando idCampus tiene valor)
            var planesCampus = _context.PlanEstudios.Where(p => p.IdCampus == idCampus).Select(p => p.IdPlanEstudios);
            var gmCampus = _context.GrupoMateria
                .Where(gm => gm.IdGrupoNavigation.IdPlanEstudiosNavigation!.IdCampus == idCampus)
                .Select(gm => gm.IdGrupoMateria);
            var estCampus = _context.Estudiante
                .Where(e => e.IdPlanActual != null && e.IdPlanActualNavigation!.IdCampus == idCampus)
                .Select(e => e.IdEstudiante);
            var recCampus = _context.Recibo
                .Where(r => r.IdEstudiante != null && estCampus.Contains(r.IdEstudiante.Value))
                .Select(r => r.IdRecibo);

            var estudiantesActivosQ = _context.Estudiante.Where(e => e.Activo);
            if (idCampus.HasValue)
                estudiantesActivosQ = estudiantesActivosQ.Where(e => e.IdPlanActual != null && e.IdPlanActualNavigation!.IdCampus == idCampus);
            var estudiantesActivos = await estudiantesActivosQ.CountAsync();

            var estudiantesMesAnteriorQ = _context.Estudiante.Where(e => e.Activo && e.FechaIngreso < inicioMes);
            if (idCampus.HasValue)
                estudiantesMesAnteriorQ = estudiantesMesAnteriorQ.Where(e => e.IdPlanActual != null && e.IdPlanActualNavigation!.IdCampus == idCampus);
            var estudiantesMesAnterior = await estudiantesMesAnteriorQ.CountAsync();

            var tendencia = estudiantesMesAnterior > 0
                ? Math.Round(((decimal)estudiantesActivos - estudiantesMesAnterior) / estudiantesMesAnterior * 100, 1)
                : 0;

            var inscripcionesMesQ = _context.Inscripcion.Where(i => i.FechaInscripcion >= inicioMesDateTime && i.Estado == "Inscrito");
            if (idCampus.HasValue)
                inscripcionesMesQ = inscripcionesMesQ.Where(i => gmCampus.Contains(i.IdGrupoMateria));
            var inscripcionesMes = await inscripcionesMesQ.CountAsync();

            var bajasMesQ = _context.SolicitudesBaja
                .Where(s => s.FechaSolicitud >= inicioMesDateTime && s.Status != Core.Enums.StatusEnum.Deleted);
            if (idCampus.HasValue)
                bajasMesQ = bajasMesQ.Where(s => estCampus.Contains(s.IdEstudiante));
            var bajasMes = await bajasMesQ.CountAsync();

            var deudaQ = _context.Recibo.Where(r => r.Estatus == EstatusRecibo.VENCIDO);
            if (idCampus.HasValue)
                deudaQ = deudaQ.Where(r => r.IdEstudiante != null && estCampus.Contains(r.IdEstudiante.Value));
            var estudiantesConDeuda = await deudaQ.Select(r => r.IdEstudiante).Distinct().CountAsync();

            var porcentajeMorosidad = estudiantesActivos > 0
                ? Math.Round((decimal)estudiantesConDeuda / estudiantesActivos * 100, 1)
                : 0;

            decimal ingresosMensuales;
            if (idCampus.HasValue)
            {
                ingresosMensuales = await _context.PagoAplicacion
                    .Where(pa => pa.Pago.FechaPagoUtc >= inicioMesDateTime && pa.Pago.Estatus == 0
                        && recCampus.Contains(pa.ReciboDetalle.IdRecibo))
                    .SumAsync(pa => (decimal?)pa.MontoAplicado) ?? 0;
            }
            else
            {
                ingresosMensuales = await _context.Pago
                    .Where(p => p.FechaPagoUtc >= inicioMesDateTime && p.Estatus == 0)
                    .SumAsync(p => (decimal?)p.Monto) ?? 0;
            }

            var promedioGeneral = await CalcularPromedioGeneralAsync(idCampus);
            var tasaReprobacion = await CalcularTasaReprobacionAsync(idCampus);
            var asistenciaGlobal = await CalcularAsistenciaGlobalAsync(idCampus);

            var programasResumen = await GetProgramasResumenAsync(idCampus);

            var alertas = await GenerarAlertasDirectorAsync();

            return new DirectorDashboardDto
            {
                EstudiantesActivos = estudiantesActivos,
                TendenciaEstudiantes = tendencia >= 0 ? $"+{tendencia}%" : $"{tendencia}%",
                InscripcionesDelMes = inscripcionesMes,
                BajasDelMes = bajasMes,
                PorcentajeMorosidad = porcentajeMorosidad,
                IngresosMensuales = ingresosMensuales,
                PromedioGeneral = promedioGeneral,
                TasaReprobacion = tasaReprobacion,
                AsistenciaGlobal = asistenciaGlobal,
                ProgramasResumen = programasResumen,
                Alertas = alertas,
                CarteraVencida = await GetCarteraVencidaAsync(idCampus)
            };
        }

        public async Task<FinanzasDashboardDto> GetFinanzasDashboardAsync()
        {
            var ahora = AhoraMexico;
            var hoy = DateOnly.FromDateTime(ahora);
            var hoyDateTime = ahora.Date;
            var inicioSemana = hoyDateTime.AddDays(-(int)hoyDateTime.DayOfWeek);
            var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
            var inicioMesDateTime = new DateTime(hoy.Year, hoy.Month, 1);

            var ingresosDia = await _context.Pago
                .Where(p => p.FechaPagoUtc.Date == hoyDateTime && p.Estatus == 0)
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            var ingresosSemana = await _context.Pago
                .Where(p => p.FechaPagoUtc >= inicioSemana && p.Estatus == 0)
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            var ingresosMes = await _context.Pago
                .Where(p => p.FechaPagoUtc >= inicioMesDateTime && p.Estatus == 0)
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            var pagosHoy = await _context.Pago
                .Where(p => p.FechaPagoUtc.Date == hoyDateTime && p.Estatus == 0)
                .CountAsync();

            var deudaTotal = await _context.Recibo
                .Where(r => r.Estatus == EstatusRecibo.VENCIDO)
                .SumAsync(r => (decimal?)r.Saldo) ?? 0;

            var totalMorosos = await _context.Recibo
                .Where(r => r.Estatus == EstatusRecibo.VENCIDO)
                .Select(r => r.IdEstudiante)
                .Distinct()
                .CountAsync();

            var topMorosos = await _context.Recibo
                .Where(r => r.Estatus == EstatusRecibo.VENCIDO && r.IdEstudiante != null)
                .GroupBy(r => r.IdEstudiante)
                .Select(g => new
                {
                    IdEstudiante = g.Key!.Value,
                    MontoAdeudado = g.Sum(r => r.Saldo),
                    RecibosVencidos = g.Count(),
                    FechaVencimiento = g.Min(r => r.FechaVencimiento)
                })
                .OrderByDescending(m => m.MontoAdeudado)
                .ToListAsync();

            var estudianteIds = topMorosos.Select(m => m.IdEstudiante).ToList();
            var estudiantes = await _context.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Include(e => e.IdPlanActualNavigation)
                .Include(e => e.EstudianteGrupo.Where(eg => eg.Status == Core.Enums.StatusEnum.Active).OrderByDescending(eg => eg.FechaInscripcion).Take(1))
                    .ThenInclude(eg => eg.IdGrupoNavigation)
                .Where(e => estudianteIds.Contains(e.IdEstudiante))
                .ToDictionaryAsync(e => e.IdEstudiante);

            var topMorososDto = topMorosos.Select(m =>
            {
                var estudiante = estudiantes.GetValueOrDefault(m.IdEstudiante);
                var grupo = estudiante?.EstudianteGrupo.FirstOrDefault()?.IdGrupoNavigation;
                return new MorosoDto
                {
                    IdEstudiante = m.IdEstudiante,
                    Matricula = estudiante?.Matricula ?? "",
                    NombreCompleto = estudiante != null
                        ? $"{estudiante.IdPersonaNavigation?.Nombre} {estudiante.IdPersonaNavigation?.ApellidoPaterno} {estudiante.IdPersonaNavigation?.ApellidoMaterno}".Trim()
                        : "",
                    Carrera = estudiante?.IdPlanActualNavigation?.NombrePlanEstudios,
                    Grupo = grupo?.CodigoGrupo ?? null,
                    MontoAdeudado = m.MontoAdeudado,
                    RecibosVencidos = m.RecibosVencidos,
                    DiasVencido = (int)(hoy.ToDateTime(TimeOnly.MinValue) - m.FechaVencimiento.ToDateTime(TimeOnly.MinValue)).TotalDays
                };
            }).ToList();

            var totalBecasMes = await _context.BecaAsignacion
                .Where(b => b.VigenciaDesde <= hoy && (b.VigenciaHasta == null || b.VigenciaHasta >= inicioMes) && b.Activo)
                .SumAsync(b => (decimal?)b.Valor) ?? 0;

            var estudiantesConBeca = await _context.BecaAsignacion
                .Where(b => b.VigenciaDesde <= hoy && (b.VigenciaHasta == null || b.VigenciaHasta >= hoy) && b.Activo)
                .Select(b => b.IdEstudiante)
                .Distinct()
                .CountAsync();

            var recibosPendientes = await _context.Recibo
                .Where(r => r.Estatus == EstatusRecibo.PENDIENTE)
                .CountAsync();

            var recibosVencidos = await _context.Recibo
                .Where(r => r.Estatus == EstatusRecibo.VENCIDO)
                .CountAsync();

            var recibosPagados = await _context.Recibo
                .Where(r => r.Estatus == EstatusRecibo.PAGADO && r.FechaEmision >= inicioMes)
                .CountAsync();

            var alertas = await GenerarAlertasFinanzasAsync();

            return new FinanzasDashboardDto
            {
                IngresosDia = ingresosDia,
                IngresosSemana = ingresosSemana,
                IngresosMes = ingresosMes,
                PagosHoy = pagosHoy,
                DeudaTotal = deudaTotal,
                TotalMorosos = totalMorosos,
                TopMorosos = topMorososDto,
                TotalBecasDelMes = totalBecasMes,
                TotalDescuentosDelMes = 0,
                EstudiantesConBeca = estudiantesConBeca,
                RecibosPendientes = recibosPendientes,
                RecibosVencidos = recibosVencidos,
                RecibosPagados = recibosPagados,
                Alertas = alertas,
                AccionesRapidas = GetAccionesRapidasFinanzas()
            };
        }

        public async Task<ControlEscolarDashboardDto> GetControlEscolarDashboardAsync()
        {
            var ahora = AhoraMexico;
            var hoy = DateOnly.FromDateTime(ahora);
            var hoyDateTime = ahora.Date;
            var inicioSemana = hoyDateTime.AddDays(-(int)hoyDateTime.DayOfWeek);
            var inicioMesDateTime = new DateTime(hoy.Year, hoy.Month, 1);

            var inscripcionesHoy = await _context.Inscripcion
                .Where(i => i.FechaInscripcion.Date == hoyDateTime && i.Estado == "Inscrito")
                .CountAsync();

            var inscripcionesSemana = await _context.Inscripcion
                .Where(i => i.FechaInscripcion >= inicioSemana && i.Estado == "Inscrito")
                .CountAsync();

            var bajasMes = await _context.Inscripcion
                .Where(i => i.FechaInscripcion >= inicioMesDateTime && i.Estado == "Baja")
                .CountAsync();

            var estudiantesPorPrograma = await _context.Estudiante
                .Where(e => e.Activo && e.IdPlanActual != null)
                .GroupBy(e => new { e.IdPlanActual, e.IdPlanActualNavigation!.NombrePlanEstudios })
                .Select(g => new EstudiantesPorProgramaDto
                {
                    IdPlanEstudios = g.Key.IdPlanActual!.Value,
                    NombrePrograma = g.Key.NombrePlanEstudios ?? "Sin nombre",
                    TotalEstudiantes = g.Count()
                })
                .ToListAsync();

            var documentosPendientes = await _context.AspiranteDocumento
                .Where(d => d.Estatus == EstatusDocumentoEnum.PENDIENTE || d.Estatus == EstatusDocumentoEnum.RECHAZADO)
                .CountAsync();

            var expedientesIncompletos = await _context.Aspirante
                .Where(a => a.Documentos.Any(d => d.Estatus != EstatusDocumentoEnum.VALIDADO))
                .CountAsync();

            var gruposActivos = await _context.Grupo
                .Where(g => g.IdPeriodoAcademicoNavigation.EsPeriodoActual)
                .CountAsync();

            var gruposSinProfesor = await _context.GrupoMateria
                .Where(gm => gm.IdProfesor == null && gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation.EsPeriodoActual)
                .CountAsync();

            var periodoActual = await _context.PeriodoAcademico
                .Where(p => p.EsPeriodoActual)
                .Select(p => new PeriodoActualDashboardDto
                {
                    IdPeriodo = p.IdPeriodoAcademico,
                    Nombre = p.Nombre,
                    FechaInicio = p.FechaInicio.ToDateTime(TimeOnly.MinValue),
                    FechaFin = p.FechaFin.ToDateTime(TimeOnly.MinValue),
                    DiasRestantes = (int)(p.FechaFin.ToDateTime(TimeOnly.MinValue) - AhoraMexico).TotalDays,
                    EsActivo = p.EsPeriodoActual
                })
                .FirstOrDefaultAsync();

            var alertas = await GenerarAlertasControlEscolarAsync();

            return new ControlEscolarDashboardDto
            {
                InscripcionesHoy = inscripcionesHoy,
                InscripcionesSemana = inscripcionesSemana,
                BajasDelMes = bajasMes,
                CambiosGrupo = 0,
                EstudiantesPorPrograma = estudiantesPorPrograma,
                DocumentosPendientes = documentosPendientes,
                ExpedientesIncompletos = expedientesIncompletos,
                GruposSinProfesor = gruposSinProfesor,
                GruposActivos = gruposActivos,
                PeriodoActual = periodoActual,
                Alertas = alertas,
                AccionesRapidas = GetAccionesRapidasControlEscolar()
            };
        }

        public async Task<AdmisionesDashboardDto> GetAdmisionesDashboardAsync()
        {
            var ahora = AhoraMexico;
            var hoy = ahora.Date;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var prospectosHoy = await _context.Aspirante
                .Where(a => a.FechaRegistro.Date == hoy)
                .CountAsync();

            var prospectosSemana = await _context.Aspirante
                .Where(a => a.FechaRegistro >= inicioSemana)
                .CountAsync();

            var prospectosDelMes = await _context.Aspirante
                .Where(a => a.FechaRegistro >= inicioMes)
                .CountAsync();

            var funnel = new FunnelAdmisionDto
            {
                Nuevo = await _context.Aspirante.Where(a => a.IdAspiranteEstatus == 1).CountAsync(),
                Contactado = await _context.Aspirante.Where(a => a.IdAspiranteEstatus == 2).CountAsync(),
                Cita = await _context.Aspirante.Where(a => a.IdAspiranteEstatus == 3).CountAsync(),
                Examen = await _context.Aspirante.Where(a => a.IdAspiranteEstatus == 4).CountAsync(),
                Aceptado = await _context.Aspirante.Where(a => a.IdAspiranteEstatus == 5).CountAsync(),
                Inscrito = await _context.Aspirante.Where(a => a.IdAspiranteEstatus == 6).CountAsync()
            };

            var conversionesDelMes = await _context.Aspirante
                .Where(a => a.IdAspiranteEstatus == 6 && a.FechaRegistro >= inicioMes)
                .CountAsync();

            var totalAspirantes = await _context.Aspirante
                .Where(a => a.FechaRegistro >= inicioMes)
                .CountAsync();

            var tasaConversion = totalAspirantes > 0
                ? Math.Round((decimal)conversionesDelMes / totalAspirantes * 100, 1)
                : 0;

            var documentosPendientes = await _context.AspiranteDocumento
                .Where(d => d.Estatus == EstatusDocumentoEnum.PENDIENTE)
                .CountAsync();

            var alertas = await GenerarAlertasAdmisionesAsync();

            return new AdmisionesDashboardDto
            {
                ProspectosHoy = prospectosHoy,
                ProspectosSemana = prospectosSemana,
                ProspectosDelMes = prospectosDelMes,
                Funnel = funnel,
                ConversionesDelMes = conversionesDelMes,
                TasaConversion = tasaConversion,
                CitasHoy = 0,
                CitasPendientes = 0,
                DocumentosPendientesAdmision = documentosPendientes,
                Alertas = alertas,
                AccionesRapidas = GetAccionesRapidasAdmisiones()
            };
        }

        public async Task<CoordinadorDashboardDto> GetCoordinadorDashboardAsync(string userId)
        {
            var asistenciaPromedio = await CalcularAsistenciaGlobalAsync();

            var gruposEnRiesgo = new List<GrupoAsistenciaDto>();

            var gruposMateriaActivos = await _context.GrupoMateria
                .Where(gm => gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation.EsPeriodoActual)
                .Select(gm => gm.IdGrupoMateria)
                .ToListAsync();

            var gruposConCalificaciones = await _context.CalificacionesParciales
                .Select(cp => cp.GrupoMateriaId)
                .Distinct()
                .ToListAsync();

            var calificacionesPendientes = gruposMateriaActivos.Count(gm => !gruposConCalificaciones.Contains(gm));

            var tasaReprobacion = await CalcularTasaReprobacionAsync();

            var docentesPendientes = await _context.Profesor
                .Where(p => p.Activo)
                .Where(p => p.GrupoMateria.Any(gm =>
                    gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation.EsPeriodoActual &&
                    !gruposConCalificaciones.Contains(gm.IdGrupoMateria)))
                .Select(p => new DocentePendienteDto
                {
                    IdProfesor = p.IdProfesor,
                    NombreCompleto = $"{p.IdPersonaNavigation.Nombre} {p.IdPersonaNavigation.ApellidoPaterno}",
                    CalificacionesPendientes = p.GrupoMateria
                        .Count(gm => gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation.EsPeriodoActual &&
                                     !gruposConCalificaciones.Contains(gm.IdGrupoMateria)),
                    AsistenciasPendientes = 0
                })
                .Take(10)
                .ToListAsync();

            var totalDocentes = await _context.Profesor.Where(p => p.Activo).CountAsync();

            var misGrupos = await _context.Grupo
                .Where(g => g.IdPeriodoAcademicoNavigation.EsPeriodoActual)
                .Select(g => new GrupoResumenDto
                {
                    IdGrupo = g.IdGrupo,
                    Nombre = $"Grupo {g.NumeroGrupo}",
                    Programa = g.IdPlanEstudiosNavigation.NombrePlanEstudios ?? "",
                    Cuatrimestre = g.NumeroCuatrimestre,
                    TotalEstudiantes = g.GrupoMateria.SelectMany(gm => gm.Inscripcion.Where(i => i.Estado == "Inscrito")).Count(),
                    PromedioGeneral = 0
                })
                .Take(10)
                .ToListAsync();

            var alertas = await GenerarAlertasCoordinadorAsync();

            return new CoordinadorDashboardDto
            {
                AsistenciaPromedio = asistenciaPromedio,
                GruposEnRiesgo = gruposEnRiesgo,
                CalificacionesPendientes = calificacionesPendientes,
                TasaReprobacionPorMateria = tasaReprobacion,
                DocentesConEntregasPendientes = docentesPendientes,
                TotalDocentes = totalDocentes,
                GruposAsignados = misGrupos.Count,
                MisGrupos = misGrupos,
                Alertas = alertas,
                AccionesRapidas = GetAccionesRapidasCoordinador()
            };
        }

        public async Task<DocenteDashboardDto> GetDocenteDashboardAsync(string userId)
        {
            var hoy = AhoraMexico;
            var diaSemana = (int)hoy.DayOfWeek;
            if (diaSemana == 0) diaSemana = 7;

            var profesor = await _context.Profesor
                .FirstOrDefaultAsync(p => p.UsuarioId == userId);

            if (profesor == null)
            {
                return new DocenteDashboardDto
                {
                    Alertas = new List<AlertaDto>
                    {
                        new AlertaDto
                        {
                            Tipo = "warning",
                            Titulo = "Perfil incompleto",
                            Mensaje = "Tu usuario no esta asociado a un perfil de docente."
                        }
                    }
                };
            }

            var clasesDeHoyRaw = await _context.Horario
                .Where(h => h.IdDiaSemana == diaSemana &&
                            h.IdGrupoMateriaNavigation.IdProfesor == profesor.IdProfesor &&
                            h.IdGrupoMateriaNavigation.IdGrupoNavigation.IdPeriodoAcademicoNavigation.EsPeriodoActual)
                .OrderBy(h => h.HoraInicio)
                .Select(h => new
                {
                    h.IdGrupoMateria,
                    Materia = h.IdGrupoMateriaNavigation.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                    NumeroGrupo = h.IdGrupoMateriaNavigation.IdGrupoNavigation.NumeroGrupo,
                    Aula = h.Aula ?? h.IdGrupoMateriaNavigation.Aula ?? "Sin asignar",
                    h.HoraInicio,
                    h.HoraFin,
                    TotalEstudiantes = h.IdGrupoMateriaNavigation.Inscripcion.Count(i => i.Estado == "Inscrito")
                })
                .ToListAsync();

            var clasesDeHoy = clasesDeHoyRaw.Select(h => new ClaseHoyDto
                {
                    IdGrupoMateria = h.IdGrupoMateria,
                    Materia = h.Materia,
                    Grupo = $"Grupo {h.NumeroGrupo}",
                    Aula = h.Aula,
                    HoraInicio = h.HoraInicio.ToTimeSpan(),
                    HoraFin = h.HoraFin.ToTimeSpan(),
                    TotalEstudiantes = h.TotalEstudiantes
                })
                .ToList();

            var gruposConCalificaciones = await _context.CalificacionesParciales
                .Select(cp => cp.GrupoMateriaId)
                .Distinct()
                .ToListAsync();

            var misGrupos = await _context.GrupoMateria
                .Where(gm => gm.IdProfesor == profesor.IdProfesor &&
                             gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation.EsPeriodoActual)
                .Select(gm => new GrupoDocenteDto
                {
                    IdGrupoMateria = gm.IdGrupoMateria,
                    Materia = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                    Grupo = $"Grupo {gm.IdGrupoNavigation.NumeroGrupo}",
                    TotalEstudiantes = gm.Inscripcion.Count(i => i.Estado == "Inscrito"),
                    PromedioGrupo = 0,
                    PorcentajeAsistencia = 0,
                    TieneCalificacionesPendientes = !gruposConCalificaciones.Contains(gm.IdGrupoMateria)
                })
                .ToListAsync();

            var asistenciasPorPasar = 0;
            var evaluacionesPendientes = misGrupos.Count(g => g.TieneCalificacionesPendientes);

            var fechasCierre = await GetFechasCierreCalificacionesAsync();

            var alertas = await GenerarAlertasDocenteAsync(profesor.IdProfesor);

            return new DocenteDashboardDto
            {
                ClasesDeHoy = clasesDeHoy,
                ProximasClases = new List<ClaseHoyDto>(),
                AsistenciasPorPasar = asistenciasPorPasar,
                EvaluacionesPendientes = evaluacionesPendientes,
                MisGrupos = misGrupos,
                FechasCierreCalificaciones = fechasCierre,
                Anuncios = new List<AnuncioDto>(),
                Alertas = alertas
            };
        }

        public async Task<AlumnoDashboardDto> GetAlumnoDashboardAsync(string userId)
        {
            var ahora = AhoraMexico;
            var hoy = DateOnly.FromDateTime(ahora);
            var diaSemana = (int)ahora.DayOfWeek;
            if (diaSemana == 0) diaSemana = 7;

            var estudiante = await _context.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Include(e => e.IdPlanActualNavigation)
                .FirstOrDefaultAsync(e => e.UsuarioId == userId);

            if (estudiante == null)
            {
                return new AlumnoDashboardDto
                {
                    Alertas = new List<AlertaDto>
                    {
                        new AlertaDto
                        {
                            Tipo = "warning",
                            Titulo = "Perfil incompleto",
                            Mensaje = "Tu usuario no esta asociado a un perfil de estudiante."
                        }
                    }
                };
            }

            var persona = estudiante.IdPersonaNavigation;
            var nombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim();

            var horarioHoyRaw = await _context.Inscripcion
                .Where(i => i.IdEstudiante == estudiante.IdEstudiante && i.Estado == "Inscrito")
                .SelectMany(i => i.IdGrupoMateriaNavigation.Horario
                    .Where(h => h.IdDiaSemana == diaSemana)
                    .Select(h => new
                    {
                        h.IdGrupoMateria,
                        Materia = h.IdGrupoMateriaNavigation.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                        Profesor = h.IdGrupoMateriaNavigation.IdProfesor != null
                            ? $"{h.IdGrupoMateriaNavigation.IdProfesorNavigation!.IdPersonaNavigation.Nombre} {h.IdGrupoMateriaNavigation.IdProfesorNavigation.IdPersonaNavigation.ApellidoPaterno}"
                            : "Sin asignar",
                        Aula = h.Aula ?? h.IdGrupoMateriaNavigation.Aula ?? "Sin asignar",
                        h.HoraInicio,
                        h.HoraFin,
                        DiaSemana = h.IdDiaSemanaNavigation.Nombre
                    }))
                .OrderBy(c => c.HoraInicio)
                .ToListAsync();

            var horarioHoy = horarioHoyRaw.Select(h => new ClaseAlumnoDto
            {
                IdGrupoMateria = h.IdGrupoMateria,
                Materia = h.Materia,
                Profesor = h.Profesor,
                Aula = h.Aula,
                HoraInicio = h.HoraInicio.ToTimeSpan(),
                HoraFin = h.HoraFin.ToTimeSpan(),
                DiaSemana = h.DiaSemana
            }).ToList();

            var calificacionesRecientes = await _context.CalificacionDetalle
                .Include(cd => cd.CalificacionParcial)
                    .ThenInclude(cp => cp.GrupoMateria)
                        .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                            .ThenInclude(mp => mp.IdMateriaNavigation)
                .Where(cd => cd.CalificacionParcial.Inscripcion.IdEstudiante == estudiante.IdEstudiante)
                .OrderByDescending(cd => cd.Id)
                .Take(5)
                .Select(cd => new CalificacionRecienteDto
                {
                    Materia = cd.CalificacionParcial.GrupoMateria.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                    TipoEvaluacion = cd.Nombre,
                    Calificacion = cd.Puntos,
                    Fecha = cd.FechaCaptura
                })
                .ToListAsync();

            var inscripciones = await _context.Inscripcion
                .Where(i => i.IdEstudiante == estudiante.IdEstudiante && i.Estado == "Inscrito")
                .Where(i => i.CalificacionFinal != null)
                .ToListAsync();

            var promedioActual = inscripciones.Any()
                ? Math.Round(inscripciones.Average(i => i.CalificacionFinal ?? 0), 1)
                : 0;

            var reciboPendiente = await _context.Recibo
                .Where(r => r.IdEstudiante == estudiante.IdEstudiante &&
                           (r.Estatus == EstatusRecibo.PENDIENTE || r.Estatus == EstatusRecibo.VENCIDO))
                .OrderBy(r => r.FechaVencimiento)
                .FirstOrDefaultAsync();

            var tieneDeuda = reciboPendiente != null;
            var montoDeuda = reciboPendiente?.Saldo;
            var proximoVencimiento = reciboPendiente?.FechaVencimiento.ToDateTime(TimeOnly.MinValue);

            var totalAsistencias = await _context.Asistencia
                .Where(a => a.Inscripcion.IdEstudiante == estudiante.IdEstudiante)
                .CountAsync();

            var asistenciasPresente = await _context.Asistencia
                .Where(a => a.Inscripcion.IdEstudiante == estudiante.IdEstudiante && a.EstadoAsistencia == EstadoAsistenciaEnum.Presente)
                .CountAsync();

            var porcentajeAsistencia = totalAsistencias > 0
                ? Math.Round((decimal)asistenciasPresente / totalAsistencias * 100, 1)
                : 100;

            var tramitesDisponibles = await _context.TiposDocumentoEstudiante
                .Where(t => t.Activo)
                .OrderBy(t => t.Orden)
                .Select(t => new TramiteDisponibleDto
                {
                    Clave = t.Clave,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion ?? "",
                    Precio = t.Precio,
                    Link = $"/dashboard/documentos-estudiante"
                })
                .ToListAsync();

            var alertas = await GenerarAlertasAlumnoAsync(estudiante.IdEstudiante);

            var cuatrimestre = await _context.Grupo
                .Where(g => g.IdPeriodoAcademicoNavigation.EsPeriodoActual &&
                            g.GrupoMateria.Any(gm => gm.Inscripcion.Any(i => i.IdEstudiante == estudiante.IdEstudiante)))
                .Select(g => g.NumeroCuatrimestre)
                .FirstOrDefaultAsync();

            return new AlumnoDashboardDto
            {
                Matricula = estudiante.Matricula,
                NombreCompleto = nombreCompleto,
                Programa = estudiante.IdPlanActualNavigation?.NombrePlanEstudios ?? "Sin programa",
                Cuatrimestre = cuatrimestre,
                HorarioHoy = horarioHoy,
                ProximasClases = new List<ClaseAlumnoDto>(),
                CalificacionesRecientes = calificacionesRecientes,
                PromedioActual = promedioActual,
                TieneDeuda = tieneDeuda,
                MontoDeuda = montoDeuda,
                ProximoVencimiento = proximoVencimiento,
                PorcentajeAsistencia = porcentajeAsistencia,
                Anuncios = new List<AnuncioDto>(),
                TramitesDisponibles = tramitesDisponibles,
                Alertas = alertas
            };
        }

        public async Task<FinanzasIndicadoresDto> GetFinanzasIndicadoresAsync()
        {
            var ahora = AhoraMexico;
            var hoy = ahora.Date;
            var hoyDateOnly = DateOnly.FromDateTime(ahora);

            // 1. Ingresos por día (últimos 7 días)
            var hace7Dias = hoy.AddDays(-6);
            var diasSemana = new[] { "Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb" };

            var pagosSemana = await _context.Pago
                .Where(p => p.FechaPagoUtc.Date >= hace7Dias && p.Estatus == 0)
                .GroupBy(p => p.FechaPagoUtc.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Monto = g.Sum(p => p.Monto),
                    Transacciones = g.Count()
                })
                .ToListAsync();

            var ingresosPorDia = new List<IngresoDiarioDto>();
            for (int i = 0; i < 7; i++)
            {
                var fecha = hace7Dias.AddDays(i);
                var pago = pagosSemana.FirstOrDefault(p => p.Fecha == fecha);
                ingresosPorDia.Add(new IngresoDiarioDto
                {
                    Dia = diasSemana[(int)fecha.DayOfWeek],
                    Fecha = fecha.ToString("yyyy-MM-dd"),
                    Monto = pago?.Monto ?? 0,
                    Transacciones = pago?.Transacciones ?? 0
                });
            }

            // 2. Ingresos mensuales (últimos 6 meses)
            var hace6Meses = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-5);
            var mesesEs = new[] { "", "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            var pagosMensuales = await _context.Pago
                .Where(p => p.FechaPagoUtc >= hace6Meses && p.Estatus == 0)
                .GroupBy(p => new { p.FechaPagoUtc.Year, p.FechaPagoUtc.Month })
                .Select(g => new
                {
                    Anio = g.Key.Year,
                    Mes = g.Key.Month,
                    Monto = g.Sum(p => p.Monto),
                    Transacciones = g.Count()
                })
                .OrderBy(g => g.Anio).ThenBy(g => g.Mes)
                .ToListAsync();

            var ingresosMensuales = new List<IngresoMensualDto>();
            for (int i = 0; i < 6; i++)
            {
                var mesDate = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-5 + i);
                var pago = pagosMensuales.FirstOrDefault(p => p.Anio == mesDate.Year && p.Mes == mesDate.Month);
                ingresosMensuales.Add(new IngresoMensualDto
                {
                    Mes = $"{mesesEs[mesDate.Month]} {mesDate.Year}",
                    Monto = pago?.Monto ?? 0,
                    Transacciones = pago?.Transacciones ?? 0
                });
            }

            // 3. Distribución de recibos por estatus
            var distribucionRecibos = await _context.Recibo
                .GroupBy(r => r.Estatus)
                .Select(g => new
                {
                    Estatus = g.Key,
                    Cantidad = g.Count(),
                    Monto = g.Sum(r => r.Saldo)
                })
                .ToListAsync();

            var nombresEstatus = new Dictionary<EstatusRecibo, string>
            {
                { EstatusRecibo.PENDIENTE, "Pendiente" },
                { EstatusRecibo.PARCIAL, "Parcial" },
                { EstatusRecibo.PAGADO, "Pagado" },
                { EstatusRecibo.VENCIDO, "Vencido" },
                { EstatusRecibo.CANCELADO, "Cancelado" },
                { EstatusRecibo.BONIFICADO, "Bonificado" }
            };

            var distribucionDto = distribucionRecibos.Select(d => new DistribucionRecibosDto
            {
                Estatus = nombresEstatus.GetValueOrDefault(d.Estatus, d.Estatus.ToString()),
                Cantidad = d.Cantidad,
                Monto = d.Monto
            }).ToList();

            // 4. Morosidad por rango de antigüedad
            var recibosVencidos = await _context.Recibo
                .Where(r => r.Estatus == EstatusRecibo.VENCIDO && r.IdEstudiante != null)
                .Select(r => new
                {
                    r.IdEstudiante,
                    r.Saldo,
                    DiasVencido = (int)(hoyDateOnly.ToDateTime(TimeOnly.MinValue) - r.FechaVencimiento.ToDateTime(TimeOnly.MinValue)).TotalDays
                })
                .ToListAsync();

            var rangos = new (string Nombre, int Min, int Max)[]
            {
                ("1-15 días", 1, 15),
                ("16-30 días", 16, 30),
                ("31-60 días", 31, 60),
                ("61-90 días", 61, 90),
                (">90 días", 91, int.MaxValue)
            };

            var morosidadPorRango = rangos.Select(rango => new MorosidadRangoDto
            {
                Rango = rango.Nombre,
                Estudiantes = recibosVencidos
                    .Where(r => r.DiasVencido >= rango.Min && r.DiasVencido <= rango.Max)
                    .Select(r => r.IdEstudiante)
                    .Distinct()
                    .Count(),
                MontoTotal = recibosVencidos
                    .Where(r => r.DiasVencido >= rango.Min && r.DiasVencido <= rango.Max)
                    .Sum(r => r.Saldo)
            }).ToList();

            // 5. Ingresos por método de pago (mes actual)
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var ingresosPorMetodo = await _context.Pago
                .Include(p => p.MedioPago)
                .Where(p => p.FechaPagoUtc >= inicioMes && p.Estatus == 0)
                .GroupBy(p => p.MedioPago.Descripcion ?? p.MedioPago.Clave)
                .Select(g => new IngresoMetodoPagoDto
                {
                    MetodoPago = g.Key,
                    Monto = g.Sum(p => p.Monto),
                    Transacciones = g.Count()
                })
                .ToListAsync();

            return new FinanzasIndicadoresDto
            {
                IngresosPorDia = ingresosPorDia,
                IngresosMensuales = ingresosMensuales,
                DistribucionRecibos = distribucionDto,
                MorosidadPorRango = morosidadPorRango,
                IngresosPorMetodoPago = ingresosPorMetodo
            };
        }

        private async Task<decimal> CalcularAsistenciaGlobalAsync(int? idCampus = null)
        {
            var baseQ = _context.Asistencia.AsQueryable();
            if (idCampus.HasValue)
            {
                var gmCampus = _context.GrupoMateria
                    .Where(gm => gm.IdGrupoNavigation.IdPlanEstudiosNavigation!.IdCampus == idCampus)
                    .Select(gm => gm.IdGrupoMateria);
                baseQ = baseQ.Where(a => gmCampus.Contains(a.GrupoMateriaId));
            }

            var totalAsistencias = await baseQ.CountAsync();
            if (totalAsistencias == 0) return 100;

            var presentes = await baseQ
                .Where(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Presente)
                .CountAsync();

            return Math.Round((decimal)presentes / totalAsistencias * 100, 1);
        }

        private async Task<decimal> CalcularPromedioGeneralAsync(int? idCampus = null)
        {
            var q = _context.Inscripcion.Where(i => i.CalificacionFinal != null && i.CalificacionFinal > 0);
            if (idCampus.HasValue)
                q = q.Where(i => i.IdGrupoMateriaNavigation.IdGrupoNavigation.IdPlanEstudiosNavigation!.IdCampus == idCampus);

            var inscripcionesConCalificacion = await q
                .Select(i => i.CalificacionFinal)
                .ToListAsync();

            if (!inscripcionesConCalificacion.Any()) return 0;

            return Math.Round(inscripcionesConCalificacion.Average() ?? 0, 1);
        }

        private async Task<decimal> CalcularTasaReprobacionAsync(int? idCampus = null)
        {
            var baseQ = _context.Inscripcion.Where(i => i.CalificacionFinal != null);
            if (idCampus.HasValue)
                baseQ = baseQ.Where(i => i.IdGrupoMateriaNavigation.IdGrupoNavigation.IdPlanEstudiosNavigation!.IdCampus == idCampus);

            var totalConCalificacion = await baseQ.CountAsync();
            if (totalConCalificacion == 0) return 0;

            var reprobados = await baseQ
                .Where(i => i.CalificacionFinal < 70)
                .CountAsync();

            return Math.Round((decimal)reprobados / totalConCalificacion * 100, 1);
        }

        private async Task<List<ProgramaResumenDto>> GetProgramasResumenAsync(int? idCampus = null)
        {
            var q = _context.PlanEstudios.AsQueryable();
            if (idCampus.HasValue)
                q = q.Where(p => p.IdCampus == idCampus);

            return await q
                .Select(p => new ProgramaResumenDto
                {
                    IdPlanEstudios = p.IdPlanEstudios,
                    Nombre = p.NombrePlanEstudios ?? "",
                    TotalEstudiantes = p.Estudiante.Count(e => e.Activo),
                    TasaRetencion = 95,
                    PromedioGeneral = 0
                })
                .Where(p => p.TotalEstudiantes > 0)
                .Take(10)
                .ToListAsync();
        }

        private async Task<List<FechaImportanteDto>> GetFechasCierreCalificacionesAsync()
        {
            var periodoActivo = await _context.PeriodoAcademico
                .Where(p => p.EsPeriodoActual)
                .FirstOrDefaultAsync();

            if (periodoActivo == null) return new List<FechaImportanteDto>();

            var fechaInicio = periodoActivo.FechaInicio.ToDateTime(TimeOnly.MinValue);
            var fechaFin = periodoActivo.FechaFin.ToDateTime(TimeOnly.MinValue);
            var duracion = (fechaFin - fechaInicio).TotalDays;
            var fechas = new List<FechaImportanteDto>();

            for (int i = 1; i <= 3; i++)
            {
                var fechaParcial = fechaInicio.AddDays(duracion / 3 * i);
                var diasRestantes = (int)(fechaParcial - AhoraMexico).TotalDays;

                if (diasRestantes > 0)
                {
                    fechas.Add(new FechaImportanteDto
                    {
                        Descripcion = $"Cierre Parcial {i}",
                        Fecha = fechaParcial,
                        DiasRestantes = diasRestantes,
                        Tipo = "calificaciones"
                    });
                }
            }

            return fechas;
        }

        private async Task<List<AlertaDto>> GenerarAlertasAdminAsync()
        {
            var alertas = new List<AlertaDto>();

            var gruposSinProfesor = await _context.GrupoMateria
                .Where(gm => gm.IdProfesor == null && gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation.EsPeriodoActual)
                .CountAsync();

            if (gruposSinProfesor > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "warning",
                    Titulo = "Grupos sin profesor",
                    Mensaje = $"Hay {gruposSinProfesor} materias sin profesor asignado",
                    Link = "/dashboard/academic-management"
                });
            }

            var recibosVencidos = await _context.Recibo
                .Where(r => r.Estatus == EstatusRecibo.VENCIDO)
                .CountAsync();

            if (recibosVencidos > 10)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "danger",
                    Titulo = "Alta morosidad",
                    Mensaje = $"{recibosVencidos} recibos vencidos en el sistema",
                    Link = "/dashboard/payments"
                });
            }

            return alertas;
        }

        private async Task<List<AlertaDto>> GenerarAlertasDirectorAsync()
        {
            var alertas = new List<AlertaDto>();

            var programasBajaMatricula = await _context.PlanEstudios
                .Where(p => p.Estudiante.Count(e => e.Activo) < 10 && p.Estudiante.Count(e => e.Activo) > 0)
                .CountAsync();

            if (programasBajaMatricula > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "warning",
                    Titulo = "Programas con baja matricula",
                    Mensaje = $"{programasBajaMatricula} programas con menos de 10 estudiantes",
                    Link = "/dashboard/study-plans"
                });
            }

            return alertas;
        }

        private async Task<List<AlertaDto>> GenerarAlertasFinanzasAsync()
        {
            var alertas = new List<AlertaDto>();
            var hoy = DateOnly.FromDateTime(AhoraMexico);
            var en7Dias = hoy.AddDays(7);

            var recibosProxVencer = await _context.Recibo
                .Where(r => r.Estatus == EstatusRecibo.PENDIENTE &&
                           r.FechaVencimiento >= hoy &&
                           r.FechaVencimiento <= en7Dias)
                .CountAsync();

            if (recibosProxVencer > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "info",
                    Titulo = "Recibos por vencer",
                    Mensaje = $"{recibosProxVencer} recibos vencen en los proximos 7 dias",
                    Link = "/dashboard/invoices"
                });
            }

            return alertas;
        }

        private async Task<List<AlertaDto>> GenerarAlertasControlEscolarAsync()
        {
            var alertas = new List<AlertaDto>();

            var estudiantesSinInscripcion = await _context.Estudiante
                .Where(e => e.Activo &&
                           !e.Inscripcion.Any(i => i.Estado == "Inscrito" &&
                                                   i.IdGrupoMateriaNavigation.IdGrupoNavigation.IdPeriodoAcademicoNavigation.EsPeriodoActual))
                .CountAsync();

            if (estudiantesSinInscripcion > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "warning",
                    Titulo = "Estudiantes sin inscripcion",
                    Mensaje = $"{estudiantesSinInscripcion} estudiantes activos sin materias inscritas",
                    Link = "/dashboard/group-enrollment"
                });
            }

            return alertas;
        }

        private async Task<List<AlertaDto>> GenerarAlertasAdmisionesAsync()
        {
            var alertas = new List<AlertaDto>();
            var hace7Dias = AhoraMexico.AddDays(-7);

            var sinSeguimiento = await _context.Aspirante
                .Where(a => a.IdAspiranteEstatus == 1 || a.IdAspiranteEstatus == 2)
                .Where(a => !_context.AspiranteBitacoraSeguimiento.Any(b => b.AspiranteId == a.IdAspirante && b.Fecha >= hace7Dias))
                .CountAsync();

            if (sinSeguimiento > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "warning",
                    Titulo = "Aspirantes sin seguimiento",
                    Mensaje = $"{sinSeguimiento} aspirantes sin contacto en 7+ dias",
                    Link = "/dashboard/applicants"
                });
            }

            return alertas;
        }

        private async Task<List<AlertaDto>> GenerarAlertasCoordinadorAsync()
        {
            var alertas = new List<AlertaDto>();

            var fechasCierre = await GetFechasCierreCalificacionesAsync();
            var proximoCierre = fechasCierre.FirstOrDefault(f => f.DiasRestantes <= 7);

            if (proximoCierre != null)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "warning",
                    Titulo = "Cierre de calificaciones proximo",
                    Mensaje = $"{proximoCierre.Descripcion} en {proximoCierre.DiasRestantes} dias",
                    Link = "/dashboard/grades"
                });
            }

            return alertas;
        }

        private async Task<List<AlertaDto>> GenerarAlertasDocenteAsync(int idProfesor)
        {
            var alertas = new List<AlertaDto>();

            var gruposConCalificaciones = await _context.CalificacionesParciales
                .Select(cp => cp.GrupoMateriaId)
                .Distinct()
                .ToListAsync();

            var gruposSinCalificaciones = await _context.GrupoMateria
                .Where(gm => gm.IdProfesor == idProfesor &&
                             gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation.EsPeriodoActual &&
                             !gruposConCalificaciones.Contains(gm.IdGrupoMateria))
                .CountAsync();

            if (gruposSinCalificaciones > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "warning",
                    Titulo = "Calificaciones pendientes",
                    Mensaje = $"Tienes {gruposSinCalificaciones} grupos sin calificaciones registradas",
                    Link = "/dashboard/grades"
                });
            }

            return alertas;
        }

        private async Task<List<AlertaDto>> GenerarAlertasAlumnoAsync(int idEstudiante)
        {
            var alertas = new List<AlertaDto>();

            var reciboVencido = await _context.Recibo
                .Where(r => r.IdEstudiante == idEstudiante && r.Estatus == EstatusRecibo.VENCIDO)
                .FirstOrDefaultAsync();

            if (reciboVencido != null)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "danger",
                    Titulo = "Pago vencido",
                    Mensaje = $"Tienes un pago vencido de ${reciboVencido.Saldo:N2}",
                    Link = "/dashboard/portal-alumno/mis-pagos"
                });
            }

            var idPersona = await _context.Estudiante
                .Where(e => e.IdEstudiante == idEstudiante)
                .Select(e => e.IdPersona)
                .FirstOrDefaultAsync();

            var aspiranteId = await _context.Aspirante
                .Where(a => a.IdPersona == idPersona)
                .OrderByDescending(a => a.IdAspirante)
                .Select(a => (int?)a.IdAspirante)
                .FirstOrDefaultAsync();

            if (aspiranteId.HasValue)
            {
                var ahora = DateTime.UtcNow;
                var limiteAlerta = ahora.AddDays(7);

                var pendientes = await _context.AspiranteDocumento
                    .Include(d => d.Requisito)
                    .Where(d => d.IdAspirante == aspiranteId.Value
                        && d.Estatus == EstatusDocumentoEnum.PENDIENTE
                        && d.Requisito != null
                        && d.Requisito.EsObligatorio)
                    .ToListAsync();

                var vencidas = pendientes.Count(d => d.FechaProrroga.HasValue && d.FechaProrroga.Value <= ahora);
                var porVencer = pendientes.Count(d => d.FechaProrroga.HasValue && d.FechaProrroga.Value > ahora && d.FechaProrroga.Value <= limiteAlerta);

                if (vencidas > 0)
                {
                    alertas.Add(new AlertaDto
                    {
                        Tipo = "danger",
                        Titulo = "Documentos con prórroga vencida",
                        Mensaje = $"Tienes {vencidas} documento(s) con prórroga vencida. Entrégalos cuanto antes para evitar problemas con tu expediente.",
                        Link = "/dashboard/portal-alumno/mi-perfil"
                    });
                }
                else if (porVencer > 0)
                {
                    alertas.Add(new AlertaDto
                    {
                        Tipo = "warning",
                        Titulo = "Documentos próximos a vencer",
                        Mensaje = $"Tienes {porVencer} documento(s) con prórroga que vence en los próximos 7 días.",
                        Link = "/dashboard/portal-alumno/mi-perfil"
                    });
                }
            }

            return alertas;
        }

        private List<AccionRapidaDto> GetAccionesRapidasAdmin() => new()
        {
            new() { Label = "Gestionar Usuarios", Icono = "users", Link = "/dashboard/users" },
            new() { Label = "Ver Reportes", Icono = "chart", Link = "/dashboard/reports" },
            new() { Label = "Configuracion", Icono = "settings", Link = "/dashboard/settings" }
        };

        private List<AccionRapidaDto> GetAccionesRapidasFinanzas() => new()
        {
            new() { Label = "Registrar Pago", Icono = "dollar", Link = "/dashboard/cashier" },
            new() { Label = "Generar Corte", Icono = "receipt", Link = "/dashboard/cashier" },
            new() { Label = "Ver Morosidad", Icono = "alert", Link = "/dashboard/invoices" }
        };

        private List<AccionRapidaDto> GetAccionesRapidasControlEscolar() => new()
        {
            new() { Label = "Inscribir Estudiante", Icono = "user-plus", Link = "/dashboard/inscriptions" },
            new() { Label = "Asignar a Grupo", Icono = "users", Link = "/dashboard/group-enrollment" },
            new() { Label = "Gestionar Documentos", Icono = "file", Link = "/dashboard/documentos-estudiante" }
        };

        private List<AccionRapidaDto> GetAccionesRapidasAdmisiones() => new()
        {
            new() { Label = "Nuevo Aspirante", Icono = "user-plus", Link = "/dashboard/applicants" },
            new() { Label = "Registrar Seguimiento", Icono = "phone", Link = "/dashboard/applicants" },
            new() { Label = "Ver Estadisticas", Icono = "chart", Link = "/dashboard/crm" }
        };

        private List<AccionRapidaDto> GetAccionesRapidasCoordinador() => new()
        {
            new() { Label = "Ver Calificaciones", Icono = "clipboard", Link = "/dashboard/grades" },
            new() { Label = "Asistencias", Icono = "check", Link = "/dashboard/attendances" },
            new() { Label = "Gestion Academica", Icono = "book", Link = "/dashboard/academic-management" }
        };
    }
}
