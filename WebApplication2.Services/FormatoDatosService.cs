using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.Formatos;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public class FormatoDatosService : IFormatoDatosService
    {
        private readonly ApplicationDbContext _db;
        private static readonly CultureInfo Es = new("es-MX");

        private const string Institucion = "Colegio San Andrés de Guanajuato";
        private const string Cct = "11PSU0329U";
        private const string Ciudad = "León, Guanajuato";
        private const string Director = "Lic. Margarita Anda Valdez";
        private const string DirectorCargo = "Directora de Servicios Escolares";
        private const string DominioInstitucional = "usaguanajuato.edu.mx";

        private static readonly string[] Ordinales = { "", "primer", "segundo", "tercer", "cuarto", "quinto", "sexto", "séptimo", "octavo", "noveno", "décimo", "décimo primer", "décimo segundo", "décimo tercer", "décimo cuarto", "décimo quinto", "décimo sexto" };
        private static readonly string[] Numeros = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve", "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis" };

        public FormatoDatosService(ApplicationDbContext db)
        {
            _db = db;
        }

        private static string QuitarAcentos(string s) =>
            new string(s.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

        public List<FormatoOrigenDto> ListarOrigenes() => new()
        {
            new FormatoOrigenDto { Clave = "estudiante", Nombre = "Estudiante / Alumno", Descripcion = "Constancias, cartas y solicitudes por alumno." },
            new FormatoOrigenDto { Clave = "aspirante", Nombre = "Aspirante", Descripcion = "Cartas y formatos de nuevo ingreso por aspirante." },
            new FormatoOrigenDto { Clave = "grupo", Nombre = "Grupo / Académico", Descripcion = "Listados y actas por grupo (incluye tabla de alumnos)." },
            new FormatoOrigenDto { Clave = "docente", Nombre = "Docente / Profesor", Descripcion = "Horarios y formatos por docente (incluye tabla de horario)." }
        };

        public List<FormatoVariableDto> CatalogoVariables(string origen) => origen?.ToLowerInvariant() switch
        {
            "estudiante" => new()
            {
                new() { Clave = "nombre", Descripcion = "Nombre completo del alumno" },
                new() { Clave = "primer_apellido", Descripcion = "Primer apellido" },
                new() { Clave = "segundo_apellido", Descripcion = "Segundo apellido" },
                new() { Clave = "matricula", Descripcion = "Matrícula" },
                new() { Clave = "curp", Descripcion = "CURP" },
                new() { Clave = "carrera", Descripcion = "Nombre de la carrera / plan" },
                new() { Clave = "clave_plan", Descripcion = "Clave del plan de estudios" },
                new() { Clave = "rvoe", Descripcion = "RVOE de la carrera" },
                new() { Clave = "campus", Descripcion = "Campus" },
                new() { Clave = "turno", Descripcion = "Turno" },
                new() { Clave = "grupo", Descripcion = "Código del grupo actual" },
                new() { Clave = "cuatrimestre", Descripcion = "Cuatrimestre actual en palabra (ej. tercer)" },
                new() { Clave = "cuatrimestre_numero", Descripcion = "Cuatrimestre actual en número" },
                new() { Clave = "total_cuatrimestres", Descripcion = "Total de cuatrimestres del plan en palabra (ej. doce)" },
                new() { Clave = "periodo", Descripcion = "Nombre del periodo académico vigente" },
                new() { Clave = "periodo_fechas", Descripcion = "Fechas del periodo (del X al Y)" },
                new() { Clave = "correo", Descripcion = "Correo personal del alumno" },
                new() { Clave = "correo_institucional", Descripcion = "Correo institucional" },
                new() { Clave = "fecha_ingreso", Descripcion = "Fecha de ingreso (dd/MM/aaaa)" },
                new() { Clave = "fecha", Descripcion = "Fecha actual (dd/MM/aaaa)" },
                new() { Clave = "fecha_larga", Descripcion = "Fecha actual en letra (27 de junio de 2026)" },
                new() { Clave = "ciudad", Descripcion = "Ciudad de expedición" },
                new() { Clave = "institucion", Descripcion = "Nombre de la institución" },
                new() { Clave = "cct", Descripcion = "Clave de centro de trabajo (C.C.T)" },
                new() { Clave = "director", Descripcion = "Nombre del director(a) que firma" },
                new() { Clave = "director_cargo", Descripcion = "Cargo del director(a)" }
            },
            "aspirante" => new()
            {
                new() { Clave = "nombre", Descripcion = "Nombre completo del aspirante" },
                new() { Clave = "primer_apellido", Descripcion = "Primer apellido" },
                new() { Clave = "segundo_apellido", Descripcion = "Segundo apellido" },
                new() { Clave = "folio", Descripcion = "Folio del aspirante (ASP-#)" },
                new() { Clave = "curp", Descripcion = "CURP" },
                new() { Clave = "carrera", Descripcion = "Carrera de interés" },
                new() { Clave = "campus", Descripcion = "Campus" },
                new() { Clave = "periodo", Descripcion = "Periodo de ingreso" },
                new() { Clave = "correo", Descripcion = "Correo del aspirante" },
                new() { Clave = "telefono", Descripcion = "Teléfono del aspirante" },
                new() { Clave = "fecha_registro", Descripcion = "Fecha de registro (dd/MM/aaaa)" },
                new() { Clave = "fecha", Descripcion = "Fecha actual (dd/MM/aaaa)" },
                new() { Clave = "fecha_larga", Descripcion = "Fecha actual en letra" },
                new() { Clave = "ciudad", Descripcion = "Ciudad de expedición" },
                new() { Clave = "institucion", Descripcion = "Nombre de la institución" },
                new() { Clave = "cct", Descripcion = "C.C.T" },
                new() { Clave = "director", Descripcion = "Director(a) que firma" },
                new() { Clave = "director_cargo", Descripcion = "Cargo del director(a)" }
            },
            "grupo" => new()
            {
                new() { Clave = "grupo", Descripcion = "Código del grupo" },
                new() { Clave = "carrera", Descripcion = "Carrera del grupo" },
                new() { Clave = "clave_plan", Descripcion = "Clave del plan" },
                new() { Clave = "rvoe", Descripcion = "RVOE" },
                new() { Clave = "cuatrimestre", Descripcion = "Cuatrimestre del grupo (palabra)" },
                new() { Clave = "cuatrimestre_numero", Descripcion = "Cuatrimestre del grupo (número)" },
                new() { Clave = "campus", Descripcion = "Campus" },
                new() { Clave = "turno", Descripcion = "Turno" },
                new() { Clave = "periodo", Descripcion = "Periodo académico" },
                new() { Clave = "periodo_fechas", Descripcion = "Fechas del periodo" },
                new() { Clave = "total_alumnos", Descripcion = "Total de alumnos del grupo" },
                new() { Clave = "fecha", Descripcion = "Fecha actual (dd/MM/aaaa)" },
                new() { Clave = "fecha_larga", Descripcion = "Fecha actual en letra" },
                new() { Clave = "ciudad", Descripcion = "Ciudad" },
                new() { Clave = "institucion", Descripcion = "Institución" },
                new() { Clave = "cct", Descripcion = "C.C.T" },
                new() { Clave = "director", Descripcion = "Director(a)" },
                new() { Clave = "director_cargo", Descripcion = "Cargo del director(a)" },
                new() { Clave = "tabla_alumnos", Descripcion = "TABLA repetible: matricula, nombre (ponla en una fila de tabla)" }
            },
            "docente" => new()
            {
                new() { Clave = "nombre", Descripcion = "Nombre completo del docente" },
                new() { Clave = "primer_apellido", Descripcion = "Primer apellido" },
                new() { Clave = "segundo_apellido", Descripcion = "Segundo apellido" },
                new() { Clave = "curp", Descripcion = "CURP" },
                new() { Clave = "campus", Descripcion = "Campus" },
                new() { Clave = "correo", Descripcion = "Correo del docente" },
                new() { Clave = "periodo", Descripcion = "Periodo académico vigente" },
                new() { Clave = "fecha", Descripcion = "Fecha actual (dd/MM/aaaa)" },
                new() { Clave = "fecha_larga", Descripcion = "Fecha actual en letra" },
                new() { Clave = "ciudad", Descripcion = "Ciudad" },
                new() { Clave = "institucion", Descripcion = "Institución" },
                new() { Clave = "cct", Descripcion = "C.C.T" },
                new() { Clave = "director", Descripcion = "Director(a)" },
                new() { Clave = "director_cargo", Descripcion = "Cargo del director(a)" },
                new() { Clave = "clave_docente", Descripcion = "Clave del docente" },
                new() { Clave = "total_clases", Descripcion = "Total de clases a la semana" },
                new() { Clave = "lun_7 … dom_20", Descripcion = "CUADRÍCULA: clase en ese día/hora. Usa {{dia_hora}}: dia=lun,mar,mie,jue,vie,sab,dom; hora=7..20 (ej. {{mar_9}})" },
                new() { Clave = "tabla_horario", Descripcion = "TABLA repetible (Excel/Word): grupo, carrera, clave, materia, clases (ponla en una fila de tabla con {{tabla_horario}})" }
            },
            _ => new()
        };

        public async Task<FormatoDatosResueltos> ResolverAsync(string origen, int idEntidad, CancellationToken ct = default)
        {
            return origen?.ToLowerInvariant() switch
            {
                "estudiante" => await ResolverEstudianteAsync(idEntidad, ct),
                "aspirante" => await ResolverAspiranteAsync(idEntidad, ct),
                "grupo" => await ResolverGrupoAsync(idEntidad, ct),
                "docente" => await ResolverDocenteAsync(idEntidad, ct),
                _ => throw new InvalidOperationException($"Origen de datos '{origen}' no soportado.")
            };
        }

        private async Task<FormatoDatosResueltos> ResolverEstudianteAsync(int idEstudiante, CancellationToken ct)
        {
            var est = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Include(e => e.IdPlanActualNavigation)
                    .ThenInclude(p => p!.IdCampusNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct)
                ?? throw new InvalidOperationException("Estudiante no encontrado.");

            var persona = est.IdPersonaNavigation;
            var plan = est.IdPlanActualNavigation;

            var periodo = await _db.PeriodoAcademico
                .Where(p => p.EsPeriodoActual)
                .OrderByDescending(p => p.FechaInicio)
                .FirstOrDefaultAsync(ct);

            var grupo = await _db.EstudianteGrupo
                .Where(eg => eg.IdEstudiante == idEstudiante && eg.Status != StatusEnum.Deleted
                    && (periodo == null || eg.IdGrupoNavigation.IdPeriodoAcademico == periodo.IdPeriodoAcademico))
                .OrderByDescending(eg => eg.IdEstudianteGrupo)
                .Select(eg => eg.IdGrupoNavigation)
                .FirstOrDefaultAsync(ct);

            if (grupo == null)
            {
                grupo = await _db.EstudianteGrupo
                    .Where(eg => eg.IdEstudiante == idEstudiante && eg.Status != StatusEnum.Deleted)
                    .OrderByDescending(eg => eg.IdEstudianteGrupo)
                    .Select(eg => eg.IdGrupoNavigation)
                    .FirstOrDefaultAsync(ct);
            }

            int cuatriActual = grupo?.NumeroCuatrimestre ?? 0;
            string turno = "";
            if (grupo != null)
                turno = await _db.Turno.Where(t => t.IdTurno == grupo.IdTurno).Select(t => t.Nombre).FirstOrDefaultAsync(ct) ?? "";

            int totalCuatri = 0;
            if (plan != null)
                totalCuatri = await _db.MateriaPlan
                    .Where(mp => mp.IdPlanEstudios == plan.IdPlanEstudios)
                    .MaxAsync(mp => (int?)mp.Cuatrimestre, ct) ?? 0;

            string periodoFechas = "";
            if (periodo != null)
            {
                var ini = periodo.FechaInicio.ToDateTime(TimeOnly.MinValue);
                var fin = periodo.FechaFin.ToDateTime(TimeOnly.MinValue);
                periodoFechas = $"del {ini.ToString("dd 'de' MMMM", Es)} al {fin.ToString("dd 'de' MMMM 'de' yyyy", Es)}";
            }

            var nombreCompleto = $"{persona?.Nombre} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim();
            var hoy = DateTime.Now;

            var variables = new Dictionary<string, string>
            {
                ["nombre"] = nombreCompleto,
                ["nombre_alumno"] = nombreCompleto,
                ["primer_apellido"] = persona?.ApellidoPaterno ?? "",
                ["segundo_apellido"] = persona?.ApellidoMaterno ?? "",
                ["matricula"] = est.Matricula,
                ["curp"] = persona?.Curp ?? "",
                ["carrera"] = plan?.NombrePlanEstudios ?? "",
                ["clave_plan"] = plan?.ClavePlanEstudios ?? "",
                ["rvoe"] = plan?.RVOE ?? "",
                ["campus"] = plan?.IdCampusNavigation?.Nombre ?? "",
                ["turno"] = turno,
                ["grupo"] = grupo?.CodigoGrupo ?? grupo?.NombreGrupo ?? "",
                ["cuatrimestre"] = cuatriActual > 0 && cuatriActual < Ordinales.Length ? Ordinales[cuatriActual] : "",
                ["cuatrimestre_numero"] = cuatriActual > 0 ? cuatriActual.ToString() : "",
                ["total_cuatrimestres"] = totalCuatri > 0 && totalCuatri < Numeros.Length ? Numeros[totalCuatri] : totalCuatri.ToString(),
                ["periodo"] = periodo?.Nombre ?? "",
                ["periodo_fechas"] = periodoFechas,
                ["correo"] = persona?.Correo ?? "",
                ["correo_institucional"] = $"{est.Matricula.ToLower()}@{DominioInstitucional}",
                ["fecha_ingreso"] = est.FechaIngreso.ToString("dd/MM/yyyy"),
                ["fecha"] = hoy.ToString("dd/MM/yyyy"),
                ["fecha_larga"] = hoy.ToString("dd 'de' MMMM 'de' yyyy", Es),
                ["ciudad"] = Ciudad,
                ["institucion"] = Institucion,
                ["cct"] = Cct,
                ["director"] = Director,
                ["director_cargo"] = DirectorCargo
            };

            return new FormatoDatosResueltos { Variables = variables };
        }

        private async Task<FormatoDatosResueltos> ResolverAspiranteAsync(int idAspirante, CancellationToken ct)
        {
            var asp = await _db.Aspirante
                .Include(a => a.IdPersonaNavigation)
                .Include(a => a.IdPlanNavigation)
                    .ThenInclude(p => p.IdCampusNavigation)
                .Include(a => a.IdPeriodoAcademicoNavigation)
                .FirstOrDefaultAsync(a => a.IdAspirante == idAspirante, ct)
                ?? throw new InvalidOperationException("Aspirante no encontrado.");

            var persona = asp.IdPersonaNavigation;
            var plan = asp.IdPlanNavigation;
            var hoy = DateTime.Now;

            var turno = asp.TurnoId.HasValue
                ? await _db.Turno.Where(t => t.IdTurno == asp.TurnoId.Value).Select(t => t.Nombre).FirstOrDefaultAsync(ct) ?? ""
                : "";

            var variables = new Dictionary<string, string>
            {
                ["nombre"] = $"{persona?.Nombre} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim(),
                ["primer_apellido"] = persona?.ApellidoPaterno ?? "",
                ["segundo_apellido"] = persona?.ApellidoMaterno ?? "",
                ["folio"] = $"ASP-{asp.IdAspirante}",
                ["curp"] = persona?.Curp ?? "",
                ["carrera"] = plan?.NombrePlanEstudios ?? "",
                ["campus"] = plan?.IdCampusNavigation?.Nombre ?? "",
                ["turno"] = turno,
                ["periodo"] = asp.IdPeriodoAcademicoNavigation?.Nombre ?? "",
                ["correo"] = persona?.Correo ?? "",
                ["telefono"] = persona?.Telefono ?? "",
                ["fecha_registro"] = asp.FechaRegistro.ToString("dd/MM/yyyy"),
                ["fecha"] = hoy.ToString("dd/MM/yyyy"),
                ["fecha_larga"] = hoy.ToString("dd 'de' MMMM 'de' yyyy", Es),
                ["ciudad"] = Ciudad,
                ["institucion"] = Institucion,
                ["cct"] = Cct,
                ["director"] = Director,
                ["director_cargo"] = DirectorCargo
            };

            return new FormatoDatosResueltos { Variables = variables };
        }

        private async Task<FormatoDatosResueltos> ResolverGrupoAsync(int idGrupo, CancellationToken ct)
        {
            var grupo = await _db.Grupo
                .Include(g => g.IdPlanEstudiosNavigation)
                    .ThenInclude(p => p.IdCampusNavigation)
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .Include(g => g.IdTurnoNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo, ct)
                ?? throw new InvalidOperationException("Grupo no encontrado.");

            var plan = grupo.IdPlanEstudiosNavigation;
            var periodo = grupo.IdPeriodoAcademicoNavigation;
            var hoy = DateTime.Now;

            var alumnos = await _db.EstudianteGrupo
                .Where(eg => eg.IdGrupo == idGrupo && eg.Status != StatusEnum.Deleted)
                .OrderBy(eg => eg.IdEstudianteNavigation.Matricula)
                .Select(eg => new { eg.IdEstudianteNavigation.Matricula, eg.IdEstudianteNavigation.IdPersonaNavigation })
                .ToListAsync(ct);

            var filasAlumnos = alumnos.Select(a => new Dictionary<string, string>
            {
                ["matricula"] = a.Matricula,
                ["nombre"] = $"{a.IdPersonaNavigation?.Nombre} {a.IdPersonaNavigation?.ApellidoPaterno} {a.IdPersonaNavigation?.ApellidoMaterno}".Trim()
            }).ToList();

            string periodoFechas = "";
            if (periodo != null)
            {
                var ini = periodo.FechaInicio.ToDateTime(TimeOnly.MinValue);
                var fin = periodo.FechaFin.ToDateTime(TimeOnly.MinValue);
                periodoFechas = $"del {ini.ToString("dd 'de' MMMM", Es)} al {fin.ToString("dd 'de' MMMM 'de' yyyy", Es)}";
            }

            int cuatri = grupo.NumeroCuatrimestre;

            var variables = new Dictionary<string, string>
            {
                ["grupo"] = grupo.CodigoGrupo ?? grupo.NombreGrupo,
                ["carrera"] = plan?.NombrePlanEstudios ?? "",
                ["clave_plan"] = plan?.ClavePlanEstudios ?? "",
                ["rvoe"] = plan?.RVOE ?? "",
                ["cuatrimestre"] = cuatri > 0 && cuatri < Ordinales.Length ? Ordinales[cuatri] : cuatri.ToString(),
                ["cuatrimestre_numero"] = cuatri.ToString(),
                ["campus"] = plan?.IdCampusNavigation?.Nombre ?? "",
                ["turno"] = grupo.IdTurnoNavigation?.Nombre ?? "",
                ["periodo"] = periodo?.Nombre ?? "",
                ["periodo_fechas"] = periodoFechas,
                ["total_alumnos"] = filasAlumnos.Count.ToString(),
                ["fecha"] = hoy.ToString("dd/MM/yyyy"),
                ["fecha_larga"] = hoy.ToString("dd 'de' MMMM 'de' yyyy", Es),
                ["ciudad"] = Ciudad,
                ["institucion"] = Institucion,
                ["cct"] = Cct,
                ["director"] = Director,
                ["director_cargo"] = DirectorCargo
            };

            return new FormatoDatosResueltos
            {
                Variables = variables,
                Tablas = new() { ["tabla_alumnos"] = filasAlumnos }
            };
        }

        private async Task<FormatoDatosResueltos> ResolverDocenteAsync(int idProfesor, CancellationToken ct)
        {
            var prof = await _db.Profesor
                .Include(p => p.IdPersonaNavigation)
                .FirstOrDefaultAsync(p => p.IdProfesor == idProfesor, ct)
                ?? throw new InvalidOperationException("Docente no encontrado.");

            var persona = prof.IdPersonaNavigation;
            var hoy = DateTime.Now;

            var campus = prof.CampusId.HasValue
                ? await _db.Campus.Where(c => c.IdCampus == prof.CampusId.Value).Select(c => c.Nombre).FirstOrDefaultAsync(ct) ?? ""
                : "";

            if (string.IsNullOrEmpty(campus))
            {
                campus = await _db.GrupoMateria
                    .Where(gm => gm.IdProfesor == idProfesor && gm.Status != StatusEnum.Deleted)
                    .Select(gm => gm.IdGrupoNavigation.IdPlanEstudiosNavigation.IdCampusNavigation.Nombre)
                    .FirstOrDefaultAsync(ct) ?? "";
            }

            var periodoNombre = (await _db.PeriodoAcademico
                .Where(p => p.EsPeriodoActual).OrderByDescending(p => p.FechaInicio)
                .FirstOrDefaultAsync(ct))?.Nombre ?? "";

            var horarios = await _db.GrupoMateria
                .Where(gm => gm.IdProfesor == idProfesor && gm.Status != StatusEnum.Deleted)
                .SelectMany(gm => gm.Horario.Select(h => new
                {
                    Materia = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                    Clave = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Clave,
                    Grupo = gm.IdGrupoNavigation.CodigoGrupo ?? gm.IdGrupoNavigation.NombreGrupo,
                    Carrera = gm.IdGrupoNavigation.IdPlanEstudiosNavigation.NombrePlanEstudios ?? "",
                    Dia = h.IdDiaSemanaNavigation.Nombre,
                    h.HoraInicio,
                    h.HoraFin
                }))
                .ToListAsync(ct);

            var filasHorario = horarios
                .GroupBy(h => new { h.Grupo, h.Carrera, h.Clave, h.Materia })
                .Select(g => new Dictionary<string, string>
                {
                    ["grupo"] = g.Key.Grupo,
                    ["carrera"] = g.Key.Carrera,
                    ["clave"] = g.Key.Clave,
                    ["materia"] = g.Key.Materia,
                    ["clases"] = g.Count().ToString()
                }).ToList();

            var variables = new Dictionary<string, string>
            {
                ["nombre"] = $"{persona?.Nombre} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim(),
                ["primer_apellido"] = persona?.ApellidoPaterno ?? "",
                ["segundo_apellido"] = persona?.ApellidoMaterno ?? "",
                ["curp"] = persona?.Curp ?? "",
                ["campus"] = campus,
                ["correo"] = persona?.Correo ?? "",
                ["periodo"] = periodoNombre,
                ["fecha"] = hoy.ToString("dd/MM/yyyy"),
                ["fecha_larga"] = hoy.ToString("dd 'de' MMMM 'de' yyyy", Es),
                ["ciudad"] = Ciudad,
                ["institucion"] = Institucion,
                ["cct"] = Cct,
                ["director"] = Director,
                ["director_cargo"] = DirectorCargo,
                ["clave_docente"] = prof.IdProfesor.ToString(),
                ["total_clases"] = horarios.Count.ToString()
            };

            var dias3 = new Dictionary<string, string>
            {
                ["LUN"] = "lun", ["MAR"] = "mar", ["MIE"] = "mie", ["JUE"] = "jue", ["VIE"] = "vie", ["SAB"] = "sab", ["DOM"] = "dom"
            };
            for (int h = 7; h <= 20; h++)
                foreach (var d in dias3.Values)
                    variables[$"{d}_{h}"] = "";

            foreach (var ho in horarios)
            {
                var pre = QuitarAcentos(ho.Dia ?? "").ToUpperInvariant();
                if (pre.Length < 3 || !dias3.TryGetValue(pre.Substring(0, 3), out var diaKey)) continue;
                for (int h = ho.HoraInicio.Hour; h < ho.HoraFin.Hour; h++)
                {
                    if (h < 7 || h > 20) continue;
                    var key = $"{diaKey}_{h}";
                    var val = $"{ho.Materia} ({ho.Grupo})";
                    variables[key] = string.IsNullOrEmpty(variables[key]) ? val : variables[key] + " / " + val;
                }
            }

            return new FormatoDatosResueltos
            {
                Variables = variables,
                Tablas = new() { ["tabla_horario"] = filasHorario }
            };
        }
    }
}
