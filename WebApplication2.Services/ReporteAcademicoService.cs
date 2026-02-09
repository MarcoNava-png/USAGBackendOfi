using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Reportes;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services;

public class ReporteAcademicoService : IReporteAcademicoService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    private static readonly string ColorAzulOscuro = "#003366";
    private static readonly string ColorAzulClaro = "#0088CC";
    private static readonly string ColorGris = "#666666";
    private static readonly string ColorGrisClaro = "#F5F5F5";

    public ReporteAcademicoService(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // ──────────────── DATA QUERIES ────────────────

    public async Task<ReporteEstudiantesGrupoDto> GetEstudiantesPorGrupoAsync(int idGrupo, CancellationToken ct = default)
    {
        var grupo = await _context.Grupo
            .Include(g => g.IdPlanEstudiosNavigation)
            .Include(g => g.IdPeriodoAcademicoNavigation)
            .Include(g => g.IdTurnoNavigation)
            .Include(g => g.EstudianteGrupo)
                .ThenInclude(eg => eg.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPersonaNavigation)
            .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo, ct)
            ?? throw new InvalidOperationException("Grupo no encontrado");

        var estudiantes = grupo.EstudianteGrupo
            .OrderBy(eg => eg.IdEstudianteNavigation.IdPersonaNavigation?.ApellidoPaterno)
            .ThenBy(eg => eg.IdEstudianteNavigation.IdPersonaNavigation?.ApellidoMaterno)
            .ThenBy(eg => eg.IdEstudianteNavigation.IdPersonaNavigation?.Nombre)
            .Select(eg =>
            {
                var p = eg.IdEstudianteNavigation.IdPersonaNavigation;
                return new EstudianteGrupoItemDto
                {
                    IdEstudiante = eg.IdEstudiante,
                    Matricula = eg.IdEstudianteNavigation.Matricula,
                    NombreCompleto = $"{p?.ApellidoPaterno} {p?.ApellidoMaterno} {p?.Nombre}".Trim(),
                    Email = eg.IdEstudianteNavigation.Email ?? p?.Correo,
                    Telefono = p?.Celular ?? p?.Telefono,
                    Estado = eg.Estado ?? "Inscrito"
                };
            }).ToList();

        return new ReporteEstudiantesGrupoDto
        {
            NombreGrupo = grupo.NombreGrupo,
            PlanEstudios = grupo.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? grupo.IdPlanEstudiosNavigation?.ClavePlanEstudios ?? "N/A",
            PeriodoAcademico = grupo.IdPeriodoAcademicoNavigation?.Nombre ?? "N/A",
            Turno = grupo.IdTurnoNavigation?.Nombre ?? "N/A",
            TotalEstudiantes = estudiantes.Count,
            Estudiantes = estudiantes
        };
    }

    public async Task<BoletaCalificacionesDto> GetBoletaCalificacionesAsync(int idEstudiante, int idPeriodo, CancellationToken ct = default)
    {
        var estudiante = await _context.Estudiante
            .Include(e => e.IdPersonaNavigation)
            .Include(e => e.IdPlanActualNavigation).ThenInclude(p => p!.IdCampusNavigation)
            .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct)
            ?? throw new InvalidOperationException("Estudiante no encontrado");

        var periodo = await _context.PeriodoAcademico.FindAsync(new object[] { idPeriodo }, ct)
            ?? throw new InvalidOperationException("Periodo académico no encontrado");

        var inscripciones = await _context.Inscripcion
            .Include(i => i.IdGrupoMateriaNavigation)
                .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                    .ThenInclude(mp => mp.IdMateriaNavigation)
            .Include(i => i.IdGrupoMateriaNavigation)
                .ThenInclude(gm => gm.IdGrupoNavigation)
            .Where(i => i.IdEstudiante == idEstudiante
                && i.IdGrupoMateriaNavigation.IdGrupoNavigation.IdPeriodoAcademico == idPeriodo)
            .ToListAsync(ct);

        var parciales = await _context.Parciales.OrderBy(p => p.Orden).ToListAsync(ct);

        var materias = new List<MateriaBoletaDto>();
        foreach (var insc in inscripciones)
        {
            var materia = insc.IdGrupoMateriaNavigation.IdMateriaPlanNavigation.IdMateriaNavigation;

            var calParciales = await _context.CalificacionesParciales
                .Include(cp => cp.Parcial)
                .Where(cp => cp.GrupoMateriaId == insc.IdGrupoMateria && cp.InscripcionId == insc.IdInscripcion)
                .ToListAsync(ct);

            var detallesPorParcial = new Dictionary<int, decimal>();
            foreach (var cp in calParciales)
            {
                var detalles = await _context.CalificacionDetalle
                    .Where(cd => cd.CalificacionParcialId == cp.Id)
                    .ToListAsync(ct);

                if (detalles.Any())
                {
                    var sumaPonderada = detalles.Sum(d => d.PesoEvaluacion > 0 ? (d.Puntos / d.MaxPuntos) * d.PesoEvaluacion * 100 : 0);
                    var sumaPesos = detalles.Sum(d => d.PesoEvaluacion);
                    detallesPorParcial[cp.ParcialId] = sumaPesos > 0 ? Math.Round(sumaPonderada / (sumaPesos * 100) * 100, 1) : 0;
                }
            }

            var p1Id = parciales.ElementAtOrDefault(0)?.Id;
            var p2Id = parciales.ElementAtOrDefault(1)?.Id;
            var p3Id = parciales.ElementAtOrDefault(2)?.Id;

            materias.Add(new MateriaBoletaDto
            {
                ClaveMateria = materia.Clave,
                NombreMateria = materia.Nombre,
                Creditos = (int)materia.Creditos,
                P1 = p1Id.HasValue && detallesPorParcial.ContainsKey(p1Id.Value) ? detallesPorParcial[p1Id.Value] : null,
                P2 = p2Id.HasValue && detallesPorParcial.ContainsKey(p2Id.Value) ? detallesPorParcial[p2Id.Value] : null,
                P3 = p3Id.HasValue && detallesPorParcial.ContainsKey(p3Id.Value) ? detallesPorParcial[p3Id.Value] : null,
                CalificacionFinal = insc.CalificacionFinal,
                Estado = insc.Estado
            });
        }

        var promedioGeneral = materias.Where(m => m.CalificacionFinal.HasValue).Select(m => m.CalificacionFinal!.Value).DefaultIfEmpty(0).Average();
        var persona = estudiante.IdPersonaNavigation;

        return new BoletaCalificacionesDto
        {
            Matricula = estudiante.Matricula,
            NombreEstudiante = $"{persona?.Nombre} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim(),
            PlanEstudios = estudiante.IdPlanActualNavigation?.NombrePlanEstudios ?? "N/A",
            PeriodoAcademico = periodo.Nombre,
            Campus = estudiante.IdPlanActualNavigation?.IdCampusNavigation?.Nombre,
            Materias = materias,
            PromedioGeneral = Math.Round(promedioGeneral, 2)
        };
    }

    public async Task<ActaCalificacionDto> GetActaCalificacionAsync(int idGrupoMateria, int? idParcial, CancellationToken ct = default)
    {
        var gm = await _context.GrupoMateria
            .Include(g => g.IdGrupoNavigation).ThenInclude(gr => gr.IdPeriodoAcademicoNavigation)
            .Include(g => g.IdMateriaPlanNavigation).ThenInclude(mp => mp.IdMateriaNavigation)
            .Include(g => g.IdProfesorNavigation).ThenInclude(p => p!.IdPersonaNavigation)
            .Include(g => g.Inscripcion).ThenInclude(i => i.IdEstudianteNavigation).ThenInclude(e => e.IdPersonaNavigation)
            .FirstOrDefaultAsync(g => g.IdGrupoMateria == idGrupoMateria, ct)
            ?? throw new InvalidOperationException("Grupo-materia no encontrado");

        string? nombreParcial = null;
        if (idParcial.HasValue)
        {
            var parcial = await _context.Parciales.FindAsync(new object[] { idParcial.Value }, ct);
            nombreParcial = parcial?.Name;
        }

        var alumnos = new List<AlumnoActaDto>();
        foreach (var insc in gm.Inscripcion.OrderBy(i => i.IdEstudianteNavigation.IdPersonaNavigation?.ApellidoPaterno))
        {
            decimal? calificacion = null;

            if (idParcial.HasValue)
            {
                var cp = await _context.CalificacionesParciales
                    .FirstOrDefaultAsync(c => c.GrupoMateriaId == idGrupoMateria
                        && c.InscripcionId == insc.IdInscripcion
                        && c.ParcialId == idParcial.Value, ct);

                if (cp != null)
                {
                    var detalles = await _context.CalificacionDetalle
                        .Where(d => d.CalificacionParcialId == cp.Id)
                        .ToListAsync(ct);

                    if (detalles.Any())
                    {
                        var sumaPonderada = detalles.Sum(d => d.PesoEvaluacion > 0 ? (d.Puntos / d.MaxPuntos) * d.PesoEvaluacion * 100 : 0);
                        var sumaPesos = detalles.Sum(d => d.PesoEvaluacion);
                        calificacion = sumaPesos > 0 ? Math.Round(sumaPonderada / (sumaPesos * 100) * 100, 1) : 0;
                    }
                }
            }
            else
            {
                calificacion = insc.CalificacionFinal;
            }

            var persona = insc.IdEstudianteNavigation.IdPersonaNavigation;
            alumnos.Add(new AlumnoActaDto
            {
                Matricula = insc.IdEstudianteNavigation.Matricula,
                NombreCompleto = $"{persona?.ApellidoPaterno} {persona?.ApellidoMaterno} {persona?.Nombre}".Trim(),
                Calificacion = calificacion,
                Estado = insc.Estado
            });
        }

        var profPersona = gm.IdProfesorNavigation?.IdPersonaNavigation;
        return new ActaCalificacionDto
        {
            NombreGrupo = gm.IdGrupoNavigation.NombreGrupo,
            NombreMateria = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
            ClaveMateria = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Clave,
            NombreProfesor = profPersona != null ? $"{profPersona.Nombre} {profPersona.ApellidoPaterno} {profPersona.ApellidoMaterno}".Trim() : "Sin asignar",
            PeriodoAcademico = gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation?.Nombre ?? "N/A",
            NombreParcial = nombreParcial ?? "Final",
            Alumnos = alumnos
        };
    }

    public async Task<HorarioReporteDto> GetHorarioGrupoAsync(int idGrupo, CancellationToken ct = default)
    {
        var grupo = await _context.Grupo
            .Include(g => g.IdPlanEstudiosNavigation)
            .Include(g => g.IdPeriodoAcademicoNavigation)
            .Include(g => g.GrupoMateria)
                .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                    .ThenInclude(mp => mp.IdMateriaNavigation)
            .Include(g => g.GrupoMateria)
                .ThenInclude(gm => gm.IdProfesorNavigation)
                    .ThenInclude(p => p!.IdPersonaNavigation)
            .Include(g => g.GrupoMateria)
                .ThenInclude(gm => gm.Horario)
                    .ThenInclude(h => h.IdDiaSemanaNavigation)
            .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo, ct)
            ?? throw new InvalidOperationException("Grupo no encontrado");

        var bloques = grupo.GrupoMateria
            .SelectMany(gm => gm.Horario.Select(h =>
            {
                var prof = gm.IdProfesorNavigation?.IdPersonaNavigation;
                return new BloqueHorarioDto
                {
                    DiaSemana = h.IdDiaSemanaNavigation?.Nombre ?? "N/A",
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    NombreMateria = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                    Profesor = prof != null ? $"{prof.Nombre} {prof.ApellidoPaterno}".Trim() : "Sin asignar",
                    Aula = h.Aula ?? gm.Aula
                };
            }))
            .OrderBy(b => ObtenerOrdenDia(b.DiaSemana))
            .ThenBy(b => b.HoraInicio)
            .ToList();

        return new HorarioReporteDto
        {
            Titulo = $"Horario de {grupo.NombreGrupo}",
            Subtitulo = $"{grupo.IdPlanEstudiosNavigation?.NombrePlanEstudios} - {grupo.IdPeriodoAcademicoNavigation?.Nombre}",
            Bloques = bloques
        };
    }

    public async Task<HorarioReporteDto> GetHorarioDocenteAsync(int idProfesor, int idPeriodo, CancellationToken ct = default)
    {
        var profesor = await _context.Profesor
            .Include(p => p.IdPersonaNavigation)
            .FirstOrDefaultAsync(p => p.IdProfesor == idProfesor, ct)
            ?? throw new InvalidOperationException("Profesor no encontrado");

        var periodo = await _context.PeriodoAcademico.FindAsync(new object[] { idPeriodo }, ct)
            ?? throw new InvalidOperationException("Periodo académico no encontrado");

        var grupoMaterias = await _context.GrupoMateria
            .Include(gm => gm.IdGrupoNavigation)
            .Include(gm => gm.IdMateriaPlanNavigation).ThenInclude(mp => mp.IdMateriaNavigation)
            .Include(gm => gm.Horario).ThenInclude(h => h.IdDiaSemanaNavigation)
            .Where(gm => gm.IdProfesor == idProfesor && gm.IdGrupoNavigation.IdPeriodoAcademico == idPeriodo)
            .ToListAsync(ct);

        var bloques = grupoMaterias
            .SelectMany(gm => gm.Horario.Select(h => new BloqueHorarioDto
            {
                DiaSemana = h.IdDiaSemanaNavigation?.Nombre ?? "N/A",
                HoraInicio = h.HoraInicio,
                HoraFin = h.HoraFin,
                NombreMateria = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                Grupo = gm.IdGrupoNavigation.NombreGrupo,
                Aula = h.Aula ?? gm.Aula
            }))
            .OrderBy(b => ObtenerOrdenDia(b.DiaSemana))
            .ThenBy(b => b.HoraInicio)
            .ToList();

        var persona = profesor.IdPersonaNavigation;
        return new HorarioReporteDto
        {
            Titulo = $"Horario de {persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim(),
            Subtitulo = periodo.Nombre,
            Bloques = bloques
        };
    }

    public async Task<ListaAsistenciaDto> GetListaAsistenciaAsync(int idGrupoMateria, CancellationToken ct = default)
    {
        var gm = await _context.GrupoMateria
            .Include(g => g.IdGrupoNavigation).ThenInclude(gr => gr.IdPeriodoAcademicoNavigation)
            .Include(g => g.IdMateriaPlanNavigation).ThenInclude(mp => mp.IdMateriaNavigation)
            .Include(g => g.IdProfesorNavigation).ThenInclude(p => p!.IdPersonaNavigation)
            .Include(g => g.Inscripcion).ThenInclude(i => i.IdEstudianteNavigation).ThenInclude(e => e.IdPersonaNavigation)
            .FirstOrDefaultAsync(g => g.IdGrupoMateria == idGrupoMateria, ct)
            ?? throw new InvalidOperationException("Grupo-materia no encontrado");

        var profPersona = gm.IdProfesorNavigation?.IdPersonaNavigation;
        var alumnos = gm.Inscripcion
            .OrderBy(i => i.IdEstudianteNavigation.IdPersonaNavigation?.ApellidoPaterno)
            .ThenBy(i => i.IdEstudianteNavigation.IdPersonaNavigation?.ApellidoMaterno)
            .Select(i =>
            {
                var p = i.IdEstudianteNavigation.IdPersonaNavigation;
                return new AlumnoListaDto
                {
                    Matricula = i.IdEstudianteNavigation.Matricula,
                    NombreCompleto = $"{p?.ApellidoPaterno} {p?.ApellidoMaterno} {p?.Nombre}".Trim()
                };
            }).ToList();

        return new ListaAsistenciaDto
        {
            NombreGrupo = gm.IdGrupoNavigation.NombreGrupo,
            NombreMateria = gm.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
            NombreProfesor = profPersona != null ? $"{profPersona.Nombre} {profPersona.ApellidoPaterno} {profPersona.ApellidoMaterno}".Trim() : "Sin asignar",
            PeriodoAcademico = gm.IdGrupoNavigation.IdPeriodoAcademicoNavigation?.Nombre ?? "N/A",
            Alumnos = alumnos
        };
    }

    // ──────────────── PDF GENERATION ────────────────

    public byte[] GenerarEstudiantesPorGrupoPdf(ReporteEstudiantesGrupoDto data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginVertical(30);
                page.MarginHorizontal(40);

                page.Header().Element(c => ComposeHeader(c, $"Lista de Estudiantes - {data.NombreGrupo}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Info block
                    col.Item().PaddingBottom(10).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        table.Cell().Text($"Plan de Estudios: {data.PlanEstudios}").FontSize(9).FontColor(ColorGris);
                        table.Cell().Text($"Periodo: {data.PeriodoAcademico}").FontSize(9).FontColor(ColorGris);
                        table.Cell().Text($"Turno: {data.Turno}").FontSize(9).FontColor(ColorGris);
                        table.Cell().Text($"Total: {data.TotalEstudiantes} estudiantes").FontSize(9).Bold().FontColor(ColorAzulOscuro);
                    });

                    // Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(30);  // #
                            c.ConstantColumn(80);  // Matrícula
                            c.RelativeColumn(3);   // Nombre
                            c.RelativeColumn(2);   // Email
                            c.ConstantColumn(90);  // Teléfono
                            c.ConstantColumn(60);  // Estado
                        });

                        // Header
                        table.Header(header =>
                        {
                            foreach (var h in new[] { "#", "Matrícula", "Nombre Completo", "Email", "Teléfono", "Estado" })
                            {
                                header.Cell().Background(ColorAzulOscuro).Padding(4)
                                    .Text(h).FontSize(8).FontColor(Colors.White).Bold();
                            }
                        });

                        for (int i = 0; i < data.Estudiantes.Count; i++)
                        {
                            var est = data.Estudiantes[i];
                            var bgColor = i % 2 == 0 ? "#FFFFFF" : ColorGrisClaro;

                            table.Cell().Background(bgColor).Padding(3).Text($"{i + 1}").FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).Text(est.Matricula).FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).Text(est.NombreCompleto).FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).Text(est.Email ?? "").FontSize(7);
                            table.Cell().Background(bgColor).Padding(3).Text(est.Telefono ?? "").FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).Text(est.Estado).FontSize(8);
                        }
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerarBoletaCalificacionesPdf(BoletaCalificacionesDto data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginVertical(30);
                page.MarginHorizontal(40);

                page.Header().Element(c => ComposeHeader(c, "Boleta de Calificaciones"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().PaddingBottom(10).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        table.Cell().Text($"Alumno: {data.NombreEstudiante}").FontSize(9).Bold().FontColor(ColorAzulOscuro);
                        table.Cell().Text($"Matrícula: {data.Matricula}").FontSize(9).FontColor(ColorGris);
                        table.Cell().Text($"Plan: {data.PlanEstudios}").FontSize(9).FontColor(ColorGris);
                        table.Cell().Text($"Periodo: {data.PeriodoAcademico}").FontSize(9).FontColor(ColorGris);
                        if (data.Campus != null)
                            table.Cell().Text($"Campus: {data.Campus}").FontSize(9).FontColor(ColorGris);
                    });

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(60);  // Clave
                            c.RelativeColumn(3);   // Materia
                            c.ConstantColumn(50);  // Créditos
                            c.ConstantColumn(45);  // P1
                            c.ConstantColumn(45);  // P2
                            c.ConstantColumn(45);  // P3
                            c.ConstantColumn(50);  // Final
                            c.ConstantColumn(65);  // Estado
                        });

                        table.Header(header =>
                        {
                            foreach (var h in new[] { "Clave", "Materia", "Créditos", "P1", "P2", "P3", "Final", "Estado" })
                            {
                                header.Cell().Background(ColorAzulOscuro).Padding(4)
                                    .Text(h).FontSize(8).FontColor(Colors.White).Bold();
                            }
                        });

                        for (int i = 0; i < data.Materias.Count; i++)
                        {
                            var m = data.Materias[i];
                            var bg = i % 2 == 0 ? "#FFFFFF" : ColorGrisClaro;

                            table.Cell().Background(bg).Padding(3).Text(m.ClaveMateria).FontSize(8);
                            table.Cell().Background(bg).Padding(3).Text(m.NombreMateria).FontSize(8);
                            table.Cell().Background(bg).Padding(3).AlignCenter().Text($"{m.Creditos}").FontSize(8);
                            table.Cell().Background(bg).Padding(3).AlignCenter().Text(m.P1?.ToString("F1") ?? "-").FontSize(8);
                            table.Cell().Background(bg).Padding(3).AlignCenter().Text(m.P2?.ToString("F1") ?? "-").FontSize(8);
                            table.Cell().Background(bg).Padding(3).AlignCenter().Text(m.P3?.ToString("F1") ?? "-").FontSize(8);
                            table.Cell().Background(bg).Padding(3).AlignCenter().Text(m.CalificacionFinal?.ToString("F1") ?? "-").FontSize(8).Bold();
                            table.Cell().Background(bg).Padding(3).Text(m.Estado ?? "").FontSize(8);
                        }
                    });

                    col.Item().PaddingTop(15).AlignRight()
                        .Text($"Promedio General: {data.PromedioGeneral:F2}")
                        .FontSize(12).Bold().FontColor(ColorAzulOscuro);
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerarActaCalificacionPdf(ActaCalificacionDto data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginVertical(30);
                page.MarginHorizontal(40);

                page.Header().Element(c => ComposeHeader(c, $"Acta de Calificación - {data.NombreParcial}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().PaddingBottom(10).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        table.Cell().Text($"Materia: {data.NombreMateria} ({data.ClaveMateria})").FontSize(9).Bold().FontColor(ColorAzulOscuro);
                        table.Cell().Text($"Grupo: {data.NombreGrupo}").FontSize(9).FontColor(ColorGris);
                        table.Cell().Text($"Profesor: {data.NombreProfesor}").FontSize(9).FontColor(ColorGris);
                        table.Cell().Text($"Periodo: {data.PeriodoAcademico}").FontSize(9).FontColor(ColorGris);
                    });

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(30);  // #
                            c.ConstantColumn(90);  // Matrícula
                            c.RelativeColumn(3);   // Nombre
                            c.ConstantColumn(70);  // Calificación
                            c.ConstantColumn(70);  // Estado
                        });

                        table.Header(header =>
                        {
                            foreach (var h in new[] { "#", "Matrícula", "Nombre Completo", "Calificación", "Estado" })
                            {
                                header.Cell().Background(ColorAzulOscuro).Padding(4)
                                    .Text(h).FontSize(8).FontColor(Colors.White).Bold();
                            }
                        });

                        for (int i = 0; i < data.Alumnos.Count; i++)
                        {
                            var a = data.Alumnos[i];
                            var bg = i % 2 == 0 ? "#FFFFFF" : ColorGrisClaro;

                            table.Cell().Background(bg).Padding(3).Text($"{i + 1}").FontSize(8);
                            table.Cell().Background(bg).Padding(3).Text(a.Matricula).FontSize(8);
                            table.Cell().Background(bg).Padding(3).Text(a.NombreCompleto).FontSize(8);
                            table.Cell().Background(bg).Padding(3).AlignCenter().Text(a.Calificacion?.ToString("F1") ?? "-").FontSize(9).Bold();
                            table.Cell().Background(bg).Padding(3).Text(a.Estado ?? "").FontSize(8);
                        }
                    });

                    // Signature lines
                    col.Item().PaddingTop(40).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().LineHorizontal(1).LineColor(ColorGris);
                            c.Item().AlignCenter().Text("Firma del Profesor").FontSize(8).FontColor(ColorGris);
                        });
                        row.ConstantItem(40);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().LineHorizontal(1).LineColor(ColorGris);
                            c.Item().AlignCenter().Text("Vo. Bo. Coordinación").FontSize(8).FontColor(ColorGris);
                        });
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerarHorarioPdf(HorarioReporteDto data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.MarginVertical(30);
                page.MarginHorizontal(40);

                page.Header().Element(c => ComposeHeader(c, data.Titulo));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    if (data.Subtitulo != null)
                        col.Item().PaddingBottom(10).Text(data.Subtitulo).FontSize(10).FontColor(ColorGris);

                    var dias = new[] { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado" };
                    var horasUnicas = data.Bloques
                        .Select(b => b.HoraInicio)
                        .Distinct()
                        .OrderBy(h => h)
                        .ToList();

                    if (horasUnicas.Count == 0)
                    {
                        col.Item().Padding(20).AlignCenter().Text("No hay horarios registrados").FontSize(10).FontColor(ColorGris);
                        return;
                    }

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(70); // Hora
                            foreach (var _ in dias) c.RelativeColumn();
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background(ColorAzulOscuro).Padding(4).Text("Hora").FontSize(8).FontColor(Colors.White).Bold();
                            foreach (var dia in dias)
                                header.Cell().Background(ColorAzulOscuro).Padding(4).AlignCenter().Text(dia).FontSize(8).FontColor(Colors.White).Bold();
                        });

                        for (int i = 0; i < horasUnicas.Count; i++)
                        {
                            var hora = horasUnicas[i];
                            var bloqueEnHora = data.Bloques.Where(b => b.HoraInicio == hora).ToList();
                            var fin = bloqueEnHora.FirstOrDefault()?.HoraFin ?? hora.AddHours(1);
                            var bg = i % 2 == 0 ? "#FFFFFF" : ColorGrisClaro;

                            table.Cell().Background(bg).Padding(3).Text($"{hora:HH:mm}-{fin:HH:mm}").FontSize(7);

                            foreach (var dia in dias)
                            {
                                var bloque = bloqueEnHora.FirstOrDefault(b => b.DiaSemana.Equals(dia, StringComparison.OrdinalIgnoreCase));
                                if (bloque != null)
                                {
                                    table.Cell().Background("#E8F4FD").Border(0.5f).BorderColor(ColorAzulClaro).Padding(3).Column(c =>
                                    {
                                        c.Item().Text(bloque.NombreMateria).FontSize(7).Bold().FontColor(ColorAzulOscuro);
                                        if (bloque.Profesor != null) c.Item().Text(bloque.Profesor).FontSize(6).FontColor(ColorGris);
                                        if (bloque.Grupo != null) c.Item().Text(bloque.Grupo).FontSize(6).FontColor(ColorGris);
                                        if (bloque.Aula != null) c.Item().Text($"Aula: {bloque.Aula}").FontSize(6).FontColor(ColorGris);
                                    });
                                }
                                else
                                {
                                    table.Cell().Background(bg).Padding(3).Text("").FontSize(7);
                                }
                            }
                        }
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerarListaAsistenciaPdf(ListaAsistenciaDto data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginVertical(30);
                page.MarginHorizontal(40);

                page.Header().Element(c => ComposeHeader(c, "Lista de Asistencia"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().PaddingBottom(10).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        table.Cell().Text($"Materia: {data.NombreMateria}").FontSize(9).Bold().FontColor(ColorAzulOscuro);
                        table.Cell().Text($"Grupo: {data.NombreGrupo}").FontSize(9).FontColor(ColorGris);
                        table.Cell().Text($"Profesor: {data.NombreProfesor}").FontSize(9).FontColor(ColorGris);
                        table.Cell().Text($"Periodo: {data.PeriodoAcademico}").FontSize(9).FontColor(ColorGris);
                    });

                    // Generate columns for ~20 days of attendance
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(30);  // #
                            c.ConstantColumn(75);  // Matrícula
                            c.RelativeColumn(2);   // Nombre
                            for (int d = 0; d < 20; d++)
                                c.ConstantColumn(18); // Day columns
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(ColorAzulOscuro).Padding(2).Text("#").FontSize(7).FontColor(Colors.White).Bold();
                            header.Cell().Background(ColorAzulOscuro).Padding(2).Text("Matrícula").FontSize(7).FontColor(Colors.White).Bold();
                            header.Cell().Background(ColorAzulOscuro).Padding(2).Text("Nombre").FontSize(7).FontColor(Colors.White).Bold();
                            for (int d = 1; d <= 20; d++)
                                header.Cell().Background(ColorAzulOscuro).Padding(1).AlignCenter().Text($"{d}").FontSize(6).FontColor(Colors.White).Bold();
                        });

                        for (int i = 0; i < data.Alumnos.Count; i++)
                        {
                            var a = data.Alumnos[i];
                            var bg = i % 2 == 0 ? "#FFFFFF" : ColorGrisClaro;

                            table.Cell().Background(bg).Padding(2).Text($"{i + 1}").FontSize(7);
                            table.Cell().Background(bg).Padding(2).Text(a.Matricula).FontSize(7);
                            table.Cell().Background(bg).Padding(2).Text(a.NombreCompleto).FontSize(7);
                            for (int d = 0; d < 20; d++)
                                table.Cell().Background(bg).Border(0.5f).BorderColor("#CCCCCC").Padding(1).Text("").FontSize(7);
                        }
                    });

                    col.Item().PaddingTop(10).Text("A = Asistencia  |  F = Falta  |  R = Retardo  |  J = Justificada")
                        .FontSize(8).FontColor(ColorGris);
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    // ──────────────── EXCEL GENERATION ────────────────

    public byte[] GenerarEstudiantesPorGrupoExcel(ReporteEstudiantesGrupoDto data)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Estudiantes");

        // Title
        ws.Cell(1, 1).Value = $"Lista de Estudiantes - {data.NombreGrupo}";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(ColorAzulOscuro);
        ws.Range(1, 1, 1, 6).Merge();

        ws.Cell(2, 1).Value = $"Plan: {data.PlanEstudios} | Periodo: {data.PeriodoAcademico} | Turno: {data.Turno} | Total: {data.TotalEstudiantes}";
        ws.Range(2, 1, 2, 6).Merge();
        ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml(ColorGris);

        var headers = new[] { "#", "Matrícula", "Nombre Completo", "Email", "Teléfono", "Estado" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(4, i + 1).Value = headers[i];
            ws.Cell(4, i + 1).Style.Font.Bold = true;
            ws.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ColorAzulOscuro);
            ws.Cell(4, i + 1).Style.Font.FontColor = XLColor.White;
        }

        for (int i = 0; i < data.Estudiantes.Count; i++)
        {
            var est = data.Estudiantes[i];
            int row = i + 5;
            ws.Cell(row, 1).Value = i + 1;
            ws.Cell(row, 2).Value = est.Matricula;
            ws.Cell(row, 3).Value = est.NombreCompleto;
            ws.Cell(row, 4).Value = est.Email ?? "";
            ws.Cell(row, 5).Value = est.Telefono ?? "";
            ws.Cell(row, 6).Value = est.Estado;

            if (i % 2 != 0)
                ws.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.FromHtml(ColorGrisClaro);
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerarHorarioExcel(HorarioReporteDto data)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Horario");

        ws.Cell(1, 1).Value = data.Titulo;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(ColorAzulOscuro);
        ws.Range(1, 1, 1, 7).Merge();

        if (data.Subtitulo != null)
        {
            ws.Cell(2, 1).Value = data.Subtitulo;
            ws.Range(2, 1, 2, 7).Merge();
        }

        var dias = new[] { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado" };
        ws.Cell(4, 1).Value = "Hora";
        ws.Cell(4, 1).Style.Font.Bold = true;
        ws.Cell(4, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ColorAzulOscuro);
        ws.Cell(4, 1).Style.Font.FontColor = XLColor.White;

        for (int i = 0; i < dias.Length; i++)
        {
            ws.Cell(4, i + 2).Value = dias[i];
            ws.Cell(4, i + 2).Style.Font.Bold = true;
            ws.Cell(4, i + 2).Style.Fill.BackgroundColor = XLColor.FromHtml(ColorAzulOscuro);
            ws.Cell(4, i + 2).Style.Font.FontColor = XLColor.White;
        }

        var horasUnicas = data.Bloques.Select(b => b.HoraInicio).Distinct().OrderBy(h => h).ToList();

        for (int i = 0; i < horasUnicas.Count; i++)
        {
            var hora = horasUnicas[i];
            int row = i + 5;
            var fin = data.Bloques.First(b => b.HoraInicio == hora).HoraFin;
            ws.Cell(row, 1).Value = $"{hora:HH:mm}-{fin:HH:mm}";

            foreach (var dia in dias)
            {
                var colIdx = Array.IndexOf(dias, dia) + 2;
                var bloque = data.Bloques.FirstOrDefault(b => b.HoraInicio == hora && b.DiaSemana.Equals(dia, StringComparison.OrdinalIgnoreCase));
                if (bloque != null)
                {
                    var text = bloque.NombreMateria;
                    if (bloque.Profesor != null) text += $"\n{bloque.Profesor}";
                    if (bloque.Grupo != null) text += $"\n{bloque.Grupo}";
                    if (bloque.Aula != null) text += $"\nAula: {bloque.Aula}";
                    ws.Cell(row, colIdx).Value = text;
                    ws.Cell(row, colIdx).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F4FD");
                    ws.Cell(row, colIdx).Style.Alignment.WrapText = true;
                }
            }
        }

        ws.Columns().AdjustToContents();
        ws.Column(1).Width = 15;
        for (int i = 2; i <= 7; i++) ws.Column(i).Width = 25;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GenerarPlanesEstudioExcelAsync(CancellationToken ct = default)
    {
        var planes = await _context.PlanEstudios
            .Include(p => p.IdCampusNavigation)
            .Include(p => p.IdPeriodicidadNavigation)
            .Include(p => p.IdNivelEducativoNavigation)
            .Include(p => p.MateriaPlan).ThenInclude(mp => mp.IdMateriaNavigation)
            .Where(p => p.Status == Core.Enums.StatusEnum.Active)
            .ToListAsync(ct);

        using var workbook = new XLWorkbook();

        // Sheet 1: Plans summary
        var wsPlan = workbook.Worksheets.Add("Planes de Estudio");
        wsPlan.Cell(1, 1).Value = "Planes de Estudio Activos";
        wsPlan.Cell(1, 1).Style.Font.Bold = true;
        wsPlan.Cell(1, 1).Style.Font.FontSize = 14;
        wsPlan.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(ColorAzulOscuro);
        wsPlan.Range(1, 1, 1, 8).Merge();

        var headers = new[] { "Clave", "Nombre", "RVOE", "Campus", "Nivel Educativo", "Periodicidad", "Duración (meses)", "Mín. Aprobatoria" };
        for (int i = 0; i < headers.Length; i++)
        {
            wsPlan.Cell(3, i + 1).Value = headers[i];
            wsPlan.Cell(3, i + 1).Style.Font.Bold = true;
            wsPlan.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ColorAzulOscuro);
            wsPlan.Cell(3, i + 1).Style.Font.FontColor = XLColor.White;
        }

        int row = 4;
        foreach (var plan in planes)
        {
            wsPlan.Cell(row, 1).Value = plan.ClavePlanEstudios;
            wsPlan.Cell(row, 2).Value = plan.NombrePlanEstudios ?? "";
            wsPlan.Cell(row, 3).Value = plan.RVOE ?? "";
            wsPlan.Cell(row, 4).Value = plan.IdCampusNavigation?.Nombre ?? "";
            wsPlan.Cell(row, 5).Value = plan.IdNivelEducativoNavigation?.DescNivelEducativo ?? "";
            wsPlan.Cell(row, 6).Value = plan.IdPeriodicidadNavigation?.DescPeriodicidad ?? "";
            wsPlan.Cell(row, 7).Value = plan.DuracionMeses ?? 0;
            wsPlan.Cell(row, 8).Value = plan.MinimaAprobatoriaFinal;
            row++;
        }
        wsPlan.Columns().AdjustToContents();

        // Sheet 2: Materias detail
        var wsMat = workbook.Worksheets.Add("Malla Curricular");
        wsMat.Cell(1, 1).Value = "Malla Curricular por Plan";
        wsMat.Cell(1, 1).Style.Font.Bold = true;
        wsMat.Cell(1, 1).Style.Font.FontSize = 14;
        wsMat.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(ColorAzulOscuro);
        wsMat.Range(1, 1, 1, 7).Merge();

        var matHeaders = new[] { "Plan de Estudios", "Cuatrimestre", "Clave Materia", "Nombre Materia", "Créditos", "Hrs Teoría", "Hrs Práctica" };
        for (int i = 0; i < matHeaders.Length; i++)
        {
            wsMat.Cell(3, i + 1).Value = matHeaders[i];
            wsMat.Cell(3, i + 1).Style.Font.Bold = true;
            wsMat.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ColorAzulOscuro);
            wsMat.Cell(3, i + 1).Style.Font.FontColor = XLColor.White;
        }

        row = 4;
        foreach (var plan in planes)
        {
            foreach (var mp in plan.MateriaPlan.OrderBy(m => m.Cuatrimestre).ThenBy(m => m.IdMateriaNavigation.Nombre))
            {
                var mat = mp.IdMateriaNavigation;
                wsMat.Cell(row, 1).Value = plan.NombrePlanEstudios ?? plan.ClavePlanEstudios;
                wsMat.Cell(row, 2).Value = mp.Cuatrimestre;
                wsMat.Cell(row, 3).Value = mat.Clave;
                wsMat.Cell(row, 4).Value = mat.Nombre;
                wsMat.Cell(row, 5).Value = mat.Creditos;
                wsMat.Cell(row, 6).Value = mat.HorasTeoria;
                wsMat.Cell(row, 7).Value = mat.HorasPractica;
                row++;
            }
        }
        wsMat.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ──────────────── HELPERS ────────────────

    private void ComposeHeader(IContainer container, string titulo)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                var logoPath = Path.Combine(_env.ContentRootPath, "logo_usag.png");
                if (!File.Exists(logoPath))
                    logoPath = Path.Combine(Directory.GetCurrentDirectory(), "logo_usag.png");

                if (File.Exists(logoPath))
                {
                    row.ConstantItem(60).Image(logoPath);
                }

                row.RelativeItem().PaddingLeft(10).Column(c =>
                {
                    c.Item().Text("UNIVERSIDAD SAN ANDRÉS DE GUANAJUATO").FontSize(12).Bold().FontColor(ColorAzulOscuro);
                    c.Item().Text("Veni Vidi Vici").FontSize(8).Italic().FontColor(ColorAzulClaro);
                    c.Item().Text(titulo).FontSize(10).Bold().FontColor(ColorAzulClaro);
                });
            });

            col.Item().PaddingTop(5).LineHorizontal(2).LineColor(ColorAzulOscuro);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(ColorGrisClaro);
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(ColorGris);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Página ").FontSize(7).FontColor(ColorGris);
                    text.CurrentPageNumber().FontSize(7).FontColor(ColorGris);
                    text.Span(" de ").FontSize(7).FontColor(ColorGris);
                    text.TotalPages().FontSize(7).FontColor(ColorGris);
                });
            });
        });
    }

    private static int ObtenerOrdenDia(string dia)
    {
        return dia.ToLower() switch
        {
            "lunes" => 1,
            "martes" => 2,
            "miercoles" or "miércoles" => 3,
            "jueves" => 4,
            "viernes" => 5,
            "sabado" or "sábado" => 6,
            "domingo" => 7,
            _ => 99
        };
    }
}
