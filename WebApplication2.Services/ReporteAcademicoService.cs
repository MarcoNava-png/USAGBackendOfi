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

    // Colores institucionales para listado por grupos
    private static readonly string ColorInstitucionalAzulClaro = "#D9E2F3";
    private static readonly string ColorInstitucionalAzulOscuro = "#2E74B5";

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
            .Where(eg => eg.Status == Core.Enums.StatusEnum.Active)
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
            CodigoGrupo = grupo.CodigoGrupo ?? grupo.NombreGrupo,
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

        var grupo = inscripciones.FirstOrDefault()?.IdGrupoMateriaNavigation?.IdGrupoNavigation;

        return new BoletaCalificacionesDto
        {
            Matricula = estudiante.Matricula,
            NombreEstudiante = $"{persona?.Nombre} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim(),
            PlanEstudios = estudiante.IdPlanActualNavigation?.NombrePlanEstudios ?? "N/A",
            PeriodoAcademico = periodo.Nombre,
            Campus = estudiante.IdPlanActualNavigation?.IdCampusNavigation?.Nombre,
            Grupo = grupo?.NombreGrupo ?? grupo?.CodigoGrupo,
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
        var watermarkPath = ResolveFilePath("watermark_listado.png");
        var minFilas = 30;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginTop(30);
                page.MarginBottom(40);
                page.MarginHorizontal(40);

                if (watermarkPath != null)
                {
                    page.Background().AlignCenter().AlignMiddle()
                        .Padding(100)
                        .Image(watermarkPath).FitArea();
                }

                page.Header().Column(col =>
                {
                    col.Item().PaddingBottom(15).AlignCenter()
                        .Text("Listado Por Grupo").FontSize(16).Bold();

                    col.Item().PaddingBottom(3).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                        table.Cell().Border(0.5f).BorderColor("#000000").Padding(5)
                            .Text(text =>
                            {
                                text.Span("CARRERA: ").Bold().FontSize(10);
                                text.Span(data.PlanEstudios).FontSize(10);
                            });
                        table.Cell().Border(0.5f).BorderColor("#000000").Padding(5)
                            .Text(text =>
                            {
                                text.Span("PERIODO: ").Bold().FontSize(10);
                                text.Span(data.PeriodoAcademico).FontSize(10);
                            });

                        table.Cell().Border(0.5f).BorderColor("#000000").Padding(5).Text("").FontSize(10);
                        table.Cell().Border(0.5f).BorderColor("#000000").Padding(5)
                            .Text(text =>
                            {
                                text.Span("GRUPO: ").Bold().FontSize(10);
                                text.Span(data.CodigoGrupo).FontSize(10);
                            });
                    });
                });

                page.Content().PaddingTop(5).Column(contentCol =>
                {
                    contentCol.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(90);
                            c.ConstantColumn(70);
                            c.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(0.5f).BorderColor("#000000").Padding(5)
                                .AlignCenter().Text("MATRÍCULA").FontSize(9).Bold();
                            header.Cell().Border(0.5f).BorderColor("#000000").Padding(5)
                                .AlignCenter().Text("ESTATUS").FontSize(9).Bold();
                            header.Cell().Border(0.5f).BorderColor("#000000").Padding(5)
                                .Text("NOMBRE").FontSize(9).Bold();
                        });

                        var totalFilas = Math.Max(data.Estudiantes.Count, minFilas);

                        for (int i = 0; i < totalFilas; i++)
                        {
                            if (i < data.Estudiantes.Count)
                            {
                                var est = data.Estudiantes[i];
                                table.Cell().Border(0.5f).BorderColor("#000000").Padding(4)
                                    .AlignCenter().Text(est.Matricula).FontSize(9);
                                table.Cell().Border(0.5f).BorderColor("#000000").Padding(4)
                                    .AlignCenter().Text(est.Estado).FontSize(9);
                                table.Cell().Border(0.5f).BorderColor("#000000").Padding(4)
                                    .Text(est.NombreCompleto).FontSize(9);
                            }
                            else
                            {
                                table.Cell().Border(0.5f).BorderColor("#000000").Padding(4).Height(18).Text("");
                                table.Cell().Border(0.5f).BorderColor("#000000").Padding(4).Height(18).Text("");
                                table.Cell().Border(0.5f).BorderColor("#000000").Padding(4).Height(18).Text("");
                            }
                        }
                    });
                });

                page.Footer().Column(col =>
                {
                    col.Item().PaddingTop(20).AlignCenter().Column(firma =>
                    {
                        firma.Item().AlignCenter().Text("_____________").FontSize(10);
                        firma.Item().AlignCenter().Text("Firma del Docente.").FontSize(10);
                    });
                });
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

                    var dias = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
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
        var ws = workbook.Worksheets.Add("Listado Por Grupo");

        // Title
        ws.Cell(1, 1).Value = "Listado Por Grupo";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(ColorInstitucionalAzulOscuro);
        ws.Range(1, 1, 1, 4).Merge();

        // Info rows with institutional blue background
        ws.Cell(3, 1).Value = "CARRERA:";
        ws.Cell(3, 1).Style.Font.Bold = true;
        ws.Cell(3, 2).Value = data.PlanEstudios;
        ws.Cell(3, 3).Value = "PERIODO:";
        ws.Cell(3, 3).Style.Font.Bold = true;
        ws.Cell(3, 4).Value = data.PeriodoAcademico;
        ws.Range(3, 1, 3, 4).Style.Fill.BackgroundColor = XLColor.FromHtml(ColorInstitucionalAzulClaro);

        ws.Cell(4, 3).Value = "GRUPO:";
        ws.Cell(4, 3).Style.Font.Bold = true;
        ws.Cell(4, 4).Value = data.CodigoGrupo;
        ws.Range(4, 1, 4, 4).Style.Fill.BackgroundColor = XLColor.FromHtml(ColorInstitucionalAzulClaro);

        // Column headers
        var headers = new[] { "#", "Matrícula", "Nombre Completo", "Estatus" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(6, i + 1).Value = headers[i];
            ws.Cell(6, i + 1).Style.Font.Bold = true;
            ws.Cell(6, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ColorInstitucionalAzulOscuro);
            ws.Cell(6, i + 1).Style.Font.FontColor = XLColor.White;
            ws.Cell(6, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        for (int i = 0; i < data.Estudiantes.Count; i++)
        {
            var est = data.Estudiantes[i];
            int row = i + 7;
            ws.Cell(row, 1).Value = i + 1;
            ws.Cell(row, 2).Value = est.Matricula;
            ws.Cell(row, 3).Value = est.NombreCompleto;
            ws.Cell(row, 4).Value = est.Estado;

            if (i % 2 != 0)
                ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml(ColorInstitucionalAzulClaro);
        }

        // Add borders to the data table
        var lastRow = 6 + data.Estudiantes.Count;
        if (data.Estudiantes.Count > 0)
        {
            ws.Range(6, 1, lastRow, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(6, 1, lastRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        ws.Column(1).Width = 6;
        ws.Column(2).Width = 16;
        ws.Column(3).Width = 40;
        ws.Column(4).Width = 14;

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
        ws.Range(1, 1, 1, 8).Merge();

        if (data.Subtitulo != null)
        {
            ws.Cell(2, 1).Value = data.Subtitulo;
            ws.Range(2, 1, 2, 8).Merge();
        }

        var dias = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
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
        for (int i = 2; i <= 8; i++) ws.Column(i).Width = 25;

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

    private string? ResolveFilePath(string fileName)
    {
        var path = Path.Combine(_env.ContentRootPath, fileName);
        if (File.Exists(path)) return path;
        path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        return File.Exists(path) ? path : null;
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

    public async Task<ReporteBajasDto> GetReporteBajasAsync(int? idCampus, int? idPlanEstudios, int? idPeriodo, int? mes, int? anio, CancellationToken ct = default)
    {
        var query = _context.Estudiante
            .Include(e => e.IdPersonaNavigation)
            .Include(e => e.IdPlanActualNavigation)
            .Include(e => e.EstudianteGrupo)
                .ThenInclude(eg => eg.IdGrupoNavigation)
            .Where(e => !e.Activo && e.FechaBaja.HasValue);

        if (idCampus.HasValue)
            query = query.Where(e => e.IdPlanActualNavigation != null && e.IdPlanActualNavigation.IdCampus == idCampus.Value);

        if (idPlanEstudios.HasValue)
            query = query.Where(e => e.IdPlanActual == idPlanEstudios.Value);

        if (idPeriodo.HasValue)
        {
            var periodo = await _context.PeriodoAcademico.FindAsync(new object[] { idPeriodo.Value }, ct);
            if (periodo != null)
            {
                var inicio = periodo.FechaInicio.ToDateTime(TimeOnly.MinValue);
                var fin = periodo.FechaFin.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(e => e.FechaBaja >= inicio && e.FechaBaja <= fin);
            }
        }

        if (mes.HasValue && anio.HasValue)
        {
            var inicioMes = new DateTime(anio.Value, mes.Value, 1);
            var finMes = inicioMes.AddMonths(1).AddTicks(-1);
            query = query.Where(e => e.FechaBaja >= inicioMes && e.FechaBaja <= finMes);
        }

        var estudiantes = await query.OrderByDescending(e => e.FechaBaja).ToListAsync(ct);
        var estudianteIds = estudiantes.Select(e => e.IdEstudiante).ToList();

        var recibosDict = await _context.Recibo
            .Where(r => r.IdEstudiante.HasValue && estudianteIds.Contains(r.IdEstudiante.Value))
            .GroupBy(r => r.IdEstudiante!.Value)
            .Select(g => new
            {
                IdEstudiante = g.Key,
                SaldoPendiente = g.Where(r => r.Estatus == Core.Enums.EstatusRecibo.PENDIENTE
                    || r.Estatus == Core.Enums.EstatusRecibo.PARCIAL
                    || r.Estatus == Core.Enums.EstatusRecibo.VENCIDO).Sum(r => r.Saldo),
                TotalPagado = g.Where(r => r.Estatus == Core.Enums.EstatusRecibo.PAGADO).Sum(r => r.Total)
            })
            .ToDictionaryAsync(x => x.IdEstudiante, ct);

        var ultimosPagos = await _context.Recibo
            .Where(r => r.IdEstudiante.HasValue && estudianteIds.Contains(r.IdEstudiante.Value))
            .SelectMany(r => r.Detalles)
            .SelectMany(d => d.Aplicaciones)
            .Where(pa => pa.Pago.Estatus == Core.Enums.EstatusPago.CONFIRMADO)
            .GroupBy(pa => pa.ReciboDetalle.Recibo.IdEstudiante!.Value)
            .Select(g => new
            {
                IdEstudiante = g.Key,
                UltimoPago = g.Max(pa => pa.Pago.FechaPagoUtc),
                MontoUltimoPago = g.OrderByDescending(pa => pa.Pago.FechaPagoUtc).First().Pago.Monto
            })
            .ToDictionaryAsync(x => x.IdEstudiante, ct);

        string? nombrePlan = null;
        if (idPlanEstudios.HasValue)
        {
            nombrePlan = await _context.PlanEstudios
                .Where(p => p.IdPlanEstudios == idPlanEstudios.Value)
                .Select(p => p.NombrePlanEstudios)
                .FirstOrDefaultAsync(ct);
        }

        string? nombrePeriodo = null;
        if (idPeriodo.HasValue)
        {
            nombrePeriodo = await _context.PeriodoAcademico
                .Where(p => p.IdPeriodoAcademico == idPeriodo.Value)
                .Select(p => p.Nombre)
                .FirstOrDefaultAsync(ct);
        }

        string? mesFiltroTexto = null;
        if (mes.HasValue && anio.HasValue)
        {
            var meses = new[] { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            mesFiltroTexto = $"{meses[mes.Value]} {anio.Value}";
        }

        var items = estudiantes.Select(e =>
        {
            var persona = e.IdPersonaNavigation;
            var ultimoGrupo = e.EstudianteGrupo
                .OrderByDescending(eg => eg.FechaInscripcion)
                .FirstOrDefault();

            recibosDict.TryGetValue(e.IdEstudiante, out var recInfo);
            ultimosPagos.TryGetValue(e.IdEstudiante, out var pagoInfo);

            return new EstudianteBajaItemDto
            {
                IdEstudiante = e.IdEstudiante,
                Matricula = e.Matricula,
                NombreCompleto = persona != null
                    ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                    : "Sin nombre",
                PlanEstudios = e.IdPlanActualNavigation?.NombrePlanEstudios,
                UltimoGrupo = ultimoGrupo?.IdGrupoNavigation?.CodigoGrupo ?? ultimoGrupo?.IdGrupoNavigation?.NombreGrupo,
                TipoBaja = e.TipoBaja switch
                {
                    Core.Enums.TipoBajaEnum.Administrativa => "Administrativa",
                    Core.Enums.TipoBajaEnum.Academica => "Académica",
                    _ => "No especificado"
                },
                EstadoBaja = e.EstadoBaja switch
                {
                    Core.Enums.EstadoBajaEnum.Temporal => "Temporal",
                    Core.Enums.EstadoBajaEnum.Definitiva => "Definitiva",
                    _ => "No especificado"
                },
                MotivoBaja = e.MotivoBaja,
                FechaBaja = e.FechaBaja,
                Email = e.Email ?? persona?.Correo,
                Telefono = persona?.Telefono,
                SaldoPendiente = recInfo?.SaldoPendiente ?? 0,
                TotalPagado = recInfo?.TotalPagado ?? 0,
                UltimoPago = pagoInfo?.UltimoPago,
                MontoUltimoPago = pagoInfo?.MontoUltimoPago
            };
        }).ToList();

        var resultado = new ReporteBajasDto
        {
            PlanEstudios = nombrePlan,
            PeriodoAcademico = nombrePeriodo,
            MesFiltro = mesFiltroTexto,
            TotalBajas = items.Count,
            BajasTemporales = estudiantes.Count(e => e.EstadoBaja == Core.Enums.EstadoBajaEnum.Temporal),
            BajasDefinitivas = estudiantes.Count(e => e.EstadoBaja == Core.Enums.EstadoBajaEnum.Definitiva),
            BajasAdministrativas = estudiantes.Count(e => e.TipoBaja == Core.Enums.TipoBajaEnum.Administrativa),
            BajasAcademicas = estudiantes.Count(e => e.TipoBaja == Core.Enums.TipoBajaEnum.Academica),
            TotalSaldoPendiente = items.Sum(i => i.SaldoPendiente),
            Estudiantes = items
        };

        return resultado;
    }

    public byte[] GenerarReporteBajasPdf(ReporteBajasDto data)
    {
        var headerLogoPath = ResolveFilePath("header_logo.png");

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.MarginTop(20);
                page.MarginBottom(30);
                page.MarginHorizontal(30);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Column(col =>
                {
                    if (headerLogoPath != null)
                        col.Item().AlignCenter().PaddingBottom(5).Height(50).Image(headerLogoPath).FitHeight();

                    col.Item().PaddingBottom(3).AlignCenter()
                        .Text("REPORTE DE BAJAS").FontSize(16).Bold().FontColor(ColorInstitucionalAzulOscuro);

                    col.Item().PaddingBottom(5).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        void FiltroCell(string label, string value)
                        {
                            table.Cell().Background(ColorInstitucionalAzulClaro).Border(0.5f).BorderColor("#999999").Padding(4)
                                .Text(text =>
                                {
                                    text.Span($"{label}: ").Bold().FontSize(8);
                                    text.Span(value).FontSize(8);
                                });
                        }

                        FiltroCell("Plan de Estudios", data.PlanEstudios ?? "Todos");
                        FiltroCell("Periodo", data.PeriodoAcademico ?? "Todos");
                        FiltroCell("Mes", data.MesFiltro ?? "Todos");
                        FiltroCell("Total Bajas", data.TotalBajas.ToString());
                    });

                    col.Item().PaddingBottom(3).Row(row =>
                    {
                        void StatBox(string label, int val, string bg, string color)
                        {
                            row.RelativeColumn().Background(bg).Border(0.5f).BorderColor("#999999").Padding(4).AlignCenter()
                                .Text($"{label}: {val}").Bold().FontSize(8).FontColor(color);
                        }
                        StatBox("Temporales", data.BajasTemporales, "#FFF8E1", "#F57F17");
                        StatBox("Definitivas", data.BajasDefinitivas, "#FCE4EC", "#C62828");
                        StatBox("Administrativas", data.BajasAdministrativas, "#E3F2FD", "#1565C0");
                        StatBox("Académicas", data.BajasAcademicas, "#F3E5F5", "#6A1B9A");
                        row.RelativeColumn().Background("#FCE4EC").Border(0.5f).BorderColor("#999999").Padding(4).AlignCenter()
                            .Text($"Saldo Pendiente: ${data.TotalSaldoPendiente:N2}").Bold().FontSize(8).FontColor("#C62828");
                    });

                    col.Item().LineHorizontal(1.5f).LineColor(ColorInstitucionalAzulOscuro);
                });

                page.Content().PaddingVertical(5).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(20);
                        c.ConstantColumn(65);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1.5f);
                        c.ConstantColumn(55);
                        c.ConstantColumn(65);
                        c.ConstantColumn(60);
                        c.RelativeColumn(1.5f);
                        c.ConstantColumn(60);
                        c.ConstantColumn(70);
                        c.ConstantColumn(70);
                    });

                    table.Header(header =>
                    {
                        void H(string text)
                        {
                            header.Cell().Background(ColorInstitucionalAzulOscuro).Border(0.5f)
                                .BorderColor(ColorInstitucionalAzulOscuro).Padding(3)
                                .Text(text).Bold().FontSize(6.5f).FontColor("#FFFFFF");
                        }
                        H("#");
                        H("MATRÍCULA");
                        H("NOMBRE");
                        H("PLAN");
                        H("GRUPO");
                        H("TIPO");
                        H("ESTADO");
                        H("MOTIVO");
                        H("FECHA BAJA");
                        H("SALDO PEND.");
                        H("ÚLTIMO PAGO");
                    });

                    for (int i = 0; i < data.Estudiantes.Count; i++)
                    {
                        var est = data.Estudiantes[i];
                        var bg = i % 2 == 0 ? "#FFFFFF" : ColorGrisClaro;

                        var (tipoBg, tipoColor) = est.TipoBaja switch
                        {
                            "Administrativa" => ("#E3F2FD", "#1565C0"),
                            "Académica" => ("#F3E5F5", "#6A1B9A"),
                            _ => (bg, "#333333")
                        };

                        var (estadoBg, estadoColor) = est.EstadoBaja switch
                        {
                            "Temporal" => ("#FFF8E1", "#F57F17"),
                            "Definitiva" => ("#FCE4EC", "#C62828"),
                            _ => (bg, "#333333")
                        };

                        void Cell(string text, string cellBg)
                        {
                            table.Cell().Background(cellBg).Border(0.5f).BorderColor("#CCCCCC").Padding(2)
                                .Text(text).FontSize(6.5f);
                        }

                        Cell((i + 1).ToString(), bg);
                        Cell(est.Matricula, bg);
                        Cell(est.NombreCompleto, bg);
                        Cell(est.PlanEstudios ?? "—", bg);
                        Cell(est.UltimoGrupo ?? "—", bg);

                        table.Cell().Background(tipoBg).Border(0.5f).BorderColor("#CCCCCC").Padding(2)
                            .Text(est.TipoBaja).Bold().FontSize(6.5f).FontColor(tipoColor);

                        table.Cell().Background(estadoBg).Border(0.5f).BorderColor("#CCCCCC").Padding(2)
                            .Text(est.EstadoBaja).Bold().FontSize(6.5f).FontColor(estadoColor);

                        Cell(est.MotivoBaja ?? "—", bg);
                        Cell(est.FechaBaja?.ToString("dd/MM/yyyy") ?? "—", bg);

                        var saldoBg = est.SaldoPendiente > 0 ? "#FCE4EC" : bg;
                        table.Cell().Background(saldoBg).Border(0.5f).BorderColor("#CCCCCC").Padding(2)
                            .Text($"${est.SaldoPendiente:N2}").FontSize(6.5f)
                            .FontColor(est.SaldoPendiente > 0 ? "#C62828" : "#333333");

                        Cell(est.UltimoPago.HasValue
                            ? $"{est.UltimoPago.Value:dd/MM/yy} ${est.MontoUltimoPago:N0}"
                            : "—", bg);
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(ColorInstitucionalAzulOscuro);
                    col.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeColumn().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(ColorGris);
                        row.RelativeColumn().AlignCenter().Text(text =>
                        {
                            text.Span("Página ").FontSize(7).FontColor(ColorGris);
                            text.CurrentPageNumber().FontSize(7).FontColor(ColorGris);
                            text.Span(" de ").FontSize(7).FontColor(ColorGris);
                            text.TotalPages().FontSize(7).FontColor(ColorGris);
                        });
                        row.RelativeColumn().AlignRight().Text("USAG - Reporte de Bajas").FontSize(7).FontColor(ColorGris);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerarReporteBajasExcel(ReporteBajasDto data)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("Reporte de Bajas");

        ws.Cell(1, 1).Value = "REPORTE DE BAJAS - UNIVERSIDAD SAN ANDRÉS DE GUANAJUATO";
        ws.Range(1, 1, 1, 9).Merge();
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Cell(2, 1).Value = $"Plan: {data.PlanEstudios ?? "Todos"} | Periodo: {data.PeriodoAcademico ?? "Todos"} | Mes: {data.MesFiltro ?? "Todos"} | Total: {data.TotalBajas} | Saldo Pendiente: ${data.TotalSaldoPendiente:N2}";
        ws.Range(2, 1, 2, 12).Merge();
        ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var headers = new[] { "#", "Matrícula", "Nombre Completo", "Plan de Estudios", "Último Grupo", "Tipo Baja", "Estado Baja", "Motivo", "Fecha Baja", "Saldo Pendiente", "Total Pagado", "Último Pago" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(4, i + 1).Value = headers[i];
            ws.Cell(4, i + 1).Style.Font.Bold = true;
            ws.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#14356F");
            ws.Cell(4, i + 1).Style.Font.FontColor = XLColor.White;
            ws.Cell(4, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        for (int i = 0; i < data.Estudiantes.Count; i++)
        {
            var est = data.Estudiantes[i];
            var row = i + 5;

            ws.Cell(row, 1).Value = i + 1;
            ws.Cell(row, 2).Value = est.Matricula;
            ws.Cell(row, 3).Value = est.NombreCompleto;
            ws.Cell(row, 4).Value = est.PlanEstudios ?? "";
            ws.Cell(row, 5).Value = est.UltimoGrupo ?? "";
            ws.Cell(row, 6).Value = est.TipoBaja;
            ws.Cell(row, 7).Value = est.EstadoBaja;
            ws.Cell(row, 8).Value = est.MotivoBaja ?? "";
            ws.Cell(row, 9).Value = est.FechaBaja?.ToString("dd/MM/yyyy") ?? "";
            ws.Cell(row, 10).Value = est.SaldoPendiente;
            ws.Cell(row, 10).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 11).Value = est.TotalPagado;
            ws.Cell(row, 11).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 12).Value = est.UltimoPago?.ToString("dd/MM/yyyy") ?? "";

            if (est.SaldoPendiente > 0)
                ws.Cell(row, 10).Style.Font.FontColor = XLColor.Red;

            if (i % 2 == 1)
            {
                ws.Range(row, 1, row, 12).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
            }

            for (int c = 1; c <= 12; c++)
                ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
