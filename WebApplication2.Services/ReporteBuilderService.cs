using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication2.Core.DTOs.ReporteBuilder;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public interface IReporteBuilderService
    {
        List<ReporteFuenteDto> ObtenerFuentes();
        Task<ReporteResultadoDto> EjecutarAsync(EjecutarReporteRequest request, CancellationToken ct = default);
        Task<byte[]> ExportarExcelAsync(EjecutarReporteRequest request, CancellationToken ct = default);
        Task<byte[]> ExportarPdfAsync(EjecutarReporteRequest request, CancellationToken ct = default);
        Task<List<ReporteDefinicionDto>> ListarDefinicionesAsync(CancellationToken ct = default);
        Task<ReporteDefinicionDto> GuardarDefinicionAsync(ReporteDefinicionDto dto, CancellationToken ct = default);
        Task<bool> EliminarDefinicionAsync(int id, CancellationToken ct = default);
    }

    public class ReporteBuilderService : IReporteBuilderService
    {
        private readonly ApplicationDbContext _db;

        public ReporteBuilderService(ApplicationDbContext db)
        {
            _db = db;
        }

        private class CampoDef<T>
        {
            public string Clave { get; set; } = string.Empty;
            public string Etiqueta { get; set; } = string.Empty;
            public string Tipo { get; set; } = "texto";
            public bool Filtrable { get; set; }
            public string? Catalogo { get; set; }
            public Func<T, object?> Getter { get; set; } = _ => null;
            public Func<IQueryable<T>, string, IQueryable<T>>? Filtro { get; set; }
        }

        private class ReciboRow
        {
            public string? Folio { get; set; }
            public EstatusRecibo EstatusEnum { get; set; }
            public decimal Subtotal { get; set; }
            public decimal Descuento { get; set; }
            public decimal Total { get; set; }
            public decimal Saldo { get; set; }
            public DateOnly FechaEmision { get; set; }
            public DateOnly FechaVencimiento { get; set; }
            public string? Matricula { get; set; }
            public string? Estudiante { get; set; }
            public string? Periodo { get; set; }
            public int? IdPeriodoAcademico { get; set; }
        }

        private class PagoRow
        {
            public string? Folio { get; set; }
            public DateTime Fecha { get; set; }
            public string? Medio { get; set; }
            public decimal Monto { get; set; }
            public string? Moneda { get; set; }
            public EstatusPago EstatusEnum { get; set; }
            public string? Referencia { get; set; }
            public string? Matricula { get; set; }
            public string? Estudiante { get; set; }
            public string? Periodo { get; set; }
            public int? IdPeriodoAcademico { get; set; }
        }

        // ─────────────── Fuente: Estudiantes ───────────────
        private static readonly List<CampoDef<Estudiante>> CamposEstudiantes = new()
        {
            new() { Clave = "matricula", Etiqueta = "Matrícula", Filtrable = true, Getter = e => e.Matricula,
                Filtro = (q, v) => q.Where(e => e.Matricula.Contains(v)) },
            new() { Clave = "nombre", Etiqueta = "Nombre", Filtrable = true, Getter = e => e.IdPersonaNavigation.Nombre,
                Filtro = (q, v) => q.Where(e => (e.IdPersonaNavigation.Nombre ?? "").Contains(v)
                    || (e.IdPersonaNavigation.ApellidoPaterno ?? "").Contains(v)
                    || (e.IdPersonaNavigation.ApellidoMaterno ?? "").Contains(v)) },
            new() { Clave = "apellidoPaterno", Etiqueta = "Apellido paterno", Getter = e => e.IdPersonaNavigation.ApellidoPaterno },
            new() { Clave = "apellidoMaterno", Etiqueta = "Apellido materno", Getter = e => e.IdPersonaNavigation.ApellidoMaterno },
            new() { Clave = "curp", Etiqueta = "CURP", Getter = e => e.IdPersonaNavigation.Curp },
            new() { Clave = "correo", Etiqueta = "Correo", Getter = e => e.IdPersonaNavigation.Correo ?? e.Email },
            new() { Clave = "telefono", Etiqueta = "Teléfono", Getter = e => e.IdPersonaNavigation.Telefono },
            new() { Clave = "fechaNacimiento", Etiqueta = "Fecha nacimiento", Tipo = "fecha", Getter = e => e.IdPersonaNavigation.FechaNacimiento },
            new() { Clave = "plan", Etiqueta = "Plan de estudios", Filtrable = true,
                Getter = e => e.IdPlanActualNavigation != null ? e.IdPlanActualNavigation.NombrePlanEstudios : null,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(e => e.IdPlanActual == id) : q },
            new() { Clave = "campus", Etiqueta = "Campus", Filtrable = true,
                Getter = e => e.IdPlanActualNavigation != null && e.IdPlanActualNavigation.IdCampusNavigation != null ? e.IdPlanActualNavigation.IdCampusNavigation.Nombre : null,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(e => e.IdPlanActualNavigation != null && e.IdPlanActualNavigation.IdCampus == id) : q },
            new() { Clave = "fechaIngreso", Etiqueta = "Fecha ingreso", Tipo = "fecha", Getter = e => e.FechaIngreso },
            new() { Clave = "estatus", Etiqueta = "Estatus", Filtrable = true, Catalogo = "estatus-estudiante", Getter = e => e.Activo ? "Activo" : "Baja",
                Filtro = (q, v) => q.Where(e => e.Activo == (v == "true" || v.ToLower() == "activo")) },
        };

        // ─────────────── Fuente: Grupos ───────────────
        private static readonly List<CampoDef<Grupo>> CamposGrupos = new()
        {
            new() { Clave = "codigo", Etiqueta = "Código", Filtrable = true, Getter = g => g.CodigoGrupo,
                Filtro = (q, v) => q.Where(g => g.CodigoGrupo.Contains(v)) },
            new() { Clave = "nombre", Etiqueta = "Nombre", Getter = g => g.NombreGrupo },
            new() { Clave = "cuatrimestre", Etiqueta = "Cuatrimestre", Tipo = "numero", Filtrable = true, Getter = g => (int)g.NumeroCuatrimestre,
                Filtro = (q, v) => int.TryParse(v, out var n) ? q.Where(g => g.NumeroCuatrimestre == n) : q },
            new() { Clave = "turno", Etiqueta = "Turno", Getter = g => g.IdTurnoNavigation != null ? g.IdTurnoNavigation.Nombre : null },
            new() { Clave = "plan", Etiqueta = "Plan de estudios", Filtrable = true,
                Getter = g => g.IdPlanEstudiosNavigation != null ? g.IdPlanEstudiosNavigation.NombrePlanEstudios : null,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(g => g.IdPlanEstudios == id) : q },
            new() { Clave = "campus", Etiqueta = "Campus", Filtrable = true,
                Getter = g => g.IdPlanEstudiosNavigation != null && g.IdPlanEstudiosNavigation.IdCampusNavigation != null ? g.IdPlanEstudiosNavigation.IdCampusNavigation.Nombre : null,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(g => g.IdPlanEstudiosNavigation != null && g.IdPlanEstudiosNavigation.IdCampus == id) : q },
            new() { Clave = "periodo", Etiqueta = "Periodo", Filtrable = true,
                Getter = g => g.IdPeriodoAcademicoNavigation != null ? g.IdPeriodoAcademicoNavigation.Nombre : null,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(g => g.IdPeriodoAcademico == id) : q },
            new() { Clave = "capacidad", Etiqueta = "Capacidad", Tipo = "numero", Getter = g => (int)g.CapacidadMaxima },
        };

        // ─────────────── Fuente: Recibos ───────────────
        private static readonly List<CampoDef<ReciboRow>> CamposRecibos = new()
        {
            new() { Clave = "folio", Etiqueta = "Folio", Filtrable = true, Getter = r => r.Folio,
                Filtro = (q, v) => q.Where(r => (r.Folio ?? "").Contains(v)) },
            new() { Clave = "matricula", Etiqueta = "Matrícula", Filtrable = true, Getter = r => r.Matricula,
                Filtro = (q, v) => q.Where(r => (r.Matricula ?? "").Contains(v)) },
            new() { Clave = "estudiante", Etiqueta = "Estudiante", Getter = r => r.Estudiante },
            new() { Clave = "estatus", Etiqueta = "Estatus", Filtrable = true, Catalogo = "estatus-recibo", Getter = r => r.EstatusEnum.ToString(),
                Filtro = (q, v) => Enum.TryParse<EstatusRecibo>(v, true, out var e) ? q.Where(r => r.EstatusEnum == e) : q },
            new() { Clave = "subtotal", Etiqueta = "Subtotal", Tipo = "numero", Getter = r => r.Subtotal },
            new() { Clave = "descuento", Etiqueta = "Descuento", Tipo = "numero", Getter = r => r.Descuento },
            new() { Clave = "total", Etiqueta = "Total", Tipo = "numero", Getter = r => r.Total },
            new() { Clave = "saldo", Etiqueta = "Saldo", Tipo = "numero", Getter = r => r.Saldo },
            new() { Clave = "fechaEmision", Etiqueta = "Emisión", Tipo = "fecha", Getter = r => r.FechaEmision },
            new() { Clave = "fechaVencimiento", Etiqueta = "Vencimiento", Tipo = "fecha", Getter = r => r.FechaVencimiento },
            new() { Clave = "periodo", Etiqueta = "Periodo", Filtrable = true, Getter = r => r.Periodo,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(r => r.IdPeriodoAcademico == id) : q },
        };

        // ─────────────── Fuente: Aspirantes ───────────────
        private static readonly List<CampoDef<Aspirante>> CamposAspirantes = new()
        {
            new() { Clave = "nombre", Etiqueta = "Nombre", Filtrable = true, Getter = a => a.IdPersonaNavigation != null ? a.IdPersonaNavigation.Nombre : null,
                Filtro = (q, v) => q.Where(a => a.IdPersonaNavigation != null && ((a.IdPersonaNavigation.Nombre ?? "").Contains(v) || (a.IdPersonaNavigation.ApellidoPaterno ?? "").Contains(v))) },
            new() { Clave = "apellidoPaterno", Etiqueta = "Apellido paterno", Getter = a => a.IdPersonaNavigation != null ? a.IdPersonaNavigation.ApellidoPaterno : null },
            new() { Clave = "apellidoMaterno", Etiqueta = "Apellido materno", Getter = a => a.IdPersonaNavigation != null ? a.IdPersonaNavigation.ApellidoMaterno : null },
            new() { Clave = "curp", Etiqueta = "CURP", Getter = a => a.IdPersonaNavigation != null ? a.IdPersonaNavigation.Curp : null },
            new() { Clave = "correo", Etiqueta = "Correo", Getter = a => a.IdPersonaNavigation != null ? a.IdPersonaNavigation.Correo : null },
            new() { Clave = "telefono", Etiqueta = "Teléfono", Getter = a => a.IdPersonaNavigation != null ? a.IdPersonaNavigation.Telefono : null },
            new() { Clave = "plan", Etiqueta = "Plan de estudios", Filtrable = true, Getter = a => a.IdPlanNavigation != null ? a.IdPlanNavigation.NombrePlanEstudios : null,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(a => a.IdPlan == id) : q },
            new() { Clave = "campus", Etiqueta = "Campus", Filtrable = true, Getter = a => a.IdPlanNavigation != null && a.IdPlanNavigation.IdCampusNavigation != null ? a.IdPlanNavigation.IdCampusNavigation.Nombre : null,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(a => a.IdPlanNavigation != null && a.IdPlanNavigation.IdCampus == id) : q },
            new() { Clave = "estatus", Etiqueta = "Estatus", Filtrable = true, Catalogo = "estatus-aspirante", Getter = a => a.IdAspiranteEstatusNavigation != null ? a.IdAspiranteEstatusNavigation.DescEstatus : null,
                Filtro = (q, v) => q.Where(a => a.IdAspiranteEstatusNavigation != null && a.IdAspiranteEstatusNavigation.DescEstatus.Contains(v)) },
            new() { Clave = "medioContacto", Etiqueta = "Medio de contacto", Getter = a => a.IdMedioContactoNavigation != null ? a.IdMedioContactoNavigation.DescMedio : null },
            new() { Clave = "periodo", Etiqueta = "Periodo", Filtrable = true, Getter = a => a.IdPeriodoAcademicoNavigation != null ? a.IdPeriodoAcademicoNavigation.Nombre : null,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(a => a.IdPeriodoAcademico == id) : q },
            new() { Clave = "fechaRegistro", Etiqueta = "Fecha registro", Tipo = "fecha", Getter = a => a.FechaRegistro },
        };

        // ─────────────── Fuente: Calificaciones (evaluaciones capturadas) ───────────────
        private static string? NombreCompleto(Persona? p) => p == null ? null
            : ((p.Nombre ?? "") + " " + (p.ApellidoPaterno ?? "") + " " + (p.ApellidoMaterno ?? "")).Trim();

        private static readonly List<CampoDef<CalificacionDetalle>> CamposCalificaciones = new()
        {
            new() { Clave = "matricula", Etiqueta = "Matrícula", Filtrable = true,
                Getter = d => d.CalificacionParcial?.Inscripcion?.IdEstudianteNavigation?.Matricula,
                Filtro = (q, v) => q.Where(d => d.CalificacionParcial.Inscripcion.IdEstudianteNavigation.Matricula.Contains(v)) },
            new() { Clave = "estudiante", Etiqueta = "Estudiante", Filtrable = true,
                Getter = d => NombreCompleto(d.CalificacionParcial?.Inscripcion?.IdEstudianteNavigation?.IdPersonaNavigation),
                Filtro = (q, v) => q.Where(d => (d.CalificacionParcial.Inscripcion.IdEstudianteNavigation.IdPersonaNavigation.Nombre ?? "").Contains(v)
                    || (d.CalificacionParcial.Inscripcion.IdEstudianteNavigation.IdPersonaNavigation.ApellidoPaterno ?? "").Contains(v)) },
            new() { Clave = "materia", Etiqueta = "Materia", Filtrable = true,
                Getter = d => d.CalificacionParcial?.GrupoMateria?.IdMateriaPlanNavigation?.IdMateriaNavigation?.Nombre,
                Filtro = (q, v) => q.Where(d => d.CalificacionParcial.GrupoMateria.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre.Contains(v)) },
            new() { Clave = "grupo", Etiqueta = "Grupo", Filtrable = true,
                Getter = d => d.CalificacionParcial?.GrupoMateria?.IdGrupoNavigation?.CodigoGrupo,
                Filtro = (q, v) => q.Where(d => d.CalificacionParcial.GrupoMateria.IdGrupoNavigation.CodigoGrupo.Contains(v)) },
            new() { Clave = "cuatrimestre", Etiqueta = "Cuatrimestre", Tipo = "numero", Filtrable = true,
                Getter = d => d.CalificacionParcial?.GrupoMateria?.IdGrupoNavigation != null ? (int)d.CalificacionParcial.GrupoMateria.IdGrupoNavigation.NumeroCuatrimestre : (int?)null,
                Filtro = (q, v) => int.TryParse(v, out var n) ? q.Where(d => d.CalificacionParcial.GrupoMateria.IdGrupoNavigation.NumeroCuatrimestre == n) : q },
            new() { Clave = "plan", Etiqueta = "Plan de estudios", Filtrable = true,
                Getter = d => d.CalificacionParcial?.GrupoMateria?.IdGrupoNavigation?.IdPlanEstudiosNavigation?.NombrePlanEstudios,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(d => d.CalificacionParcial.GrupoMateria.IdGrupoNavigation.IdPlanEstudios == id) : q },
            new() { Clave = "campus", Etiqueta = "Campus", Filtrable = true,
                Getter = d => d.CalificacionParcial?.GrupoMateria?.IdGrupoNavigation?.IdPlanEstudiosNavigation?.IdCampusNavigation?.Nombre,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(d => d.CalificacionParcial.GrupoMateria.IdGrupoNavigation.IdPlanEstudiosNavigation.IdCampus == id) : q },
            new() { Clave = "periodo", Etiqueta = "Periodo", Filtrable = true,
                Getter = d => d.CalificacionParcial?.GrupoMateria?.IdGrupoNavigation?.IdPeriodoAcademicoNavigation?.Nombre,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(d => d.CalificacionParcial.GrupoMateria.IdGrupoNavigation.IdPeriodoAcademico == id) : q },
            new() { Clave = "profesor", Etiqueta = "Profesor",
                Getter = d => NombreCompleto(d.CalificacionParcial?.GrupoMateria?.IdProfesorNavigation?.IdPersonaNavigation) },
            new() { Clave = "parcial", Etiqueta = "Parcial", Getter = d => d.CalificacionParcial?.Parcial?.Name },
            new() { Clave = "evaluacion", Etiqueta = "Evaluación", Filtrable = true, Getter = d => d.Nombre,
                Filtro = (q, v) => q.Where(d => (d.Nombre ?? "").Contains(v)) },
            new() { Clave = "calificacion", Etiqueta = "Calificación (0-10)", Tipo = "numero",
                Getter = d => d.MaxPuntos != 0 ? decimal.Round(d.Puntos / d.MaxPuntos * 10m, 2) : 0m },
            new() { Clave = "puntos", Etiqueta = "Puntos", Tipo = "numero", Getter = d => d.Puntos },
            new() { Clave = "maxPuntos", Etiqueta = "Puntos máx.", Tipo = "numero", Getter = d => d.MaxPuntos },
            new() { Clave = "peso", Etiqueta = "Peso (%)", Tipo = "numero", Getter = d => d.PesoEvaluacion },
            new() { Clave = "fechaCaptura", Etiqueta = "Fecha captura", Tipo = "fecha", Getter = d => d.FechaCaptura },
        };

        // ─────────────── Fuente: Pagos ───────────────
        private static readonly List<CampoDef<PagoRow>> CamposPagos = new()
        {
            new() { Clave = "folio", Etiqueta = "Folio", Filtrable = true, Getter = r => r.Folio,
                Filtro = (q, v) => q.Where(r => (r.Folio ?? "").Contains(v)) },
            new() { Clave = "fecha", Etiqueta = "Fecha", Tipo = "fecha", Getter = r => r.Fecha },
            new() { Clave = "matricula", Etiqueta = "Matrícula", Filtrable = true, Getter = r => r.Matricula,
                Filtro = (q, v) => q.Where(r => (r.Matricula ?? "").Contains(v)) },
            new() { Clave = "estudiante", Etiqueta = "Estudiante", Getter = r => r.Estudiante },
            new() { Clave = "medio", Etiqueta = "Medio de pago", Filtrable = true, Getter = r => r.Medio,
                Filtro = (q, v) => q.Where(r => (r.Medio ?? "").Contains(v)) },
            new() { Clave = "monto", Etiqueta = "Monto", Tipo = "numero", Getter = r => r.Monto },
            new() { Clave = "moneda", Etiqueta = "Moneda", Getter = r => r.Moneda },
            new() { Clave = "estatus", Etiqueta = "Estatus", Filtrable = true, Catalogo = "estatus-pago", Getter = r => r.EstatusEnum.ToString(),
                Filtro = (q, v) => Enum.TryParse<EstatusPago>(v, true, out var e) ? q.Where(r => r.EstatusEnum == e) : q },
            new() { Clave = "referencia", Etiqueta = "Referencia", Getter = r => r.Referencia },
            new() { Clave = "periodo", Etiqueta = "Periodo", Filtrable = true, Getter = r => r.Periodo,
                Filtro = (q, v) => int.TryParse(v, out var id) ? q.Where(r => r.IdPeriodoAcademico == id) : q },
        };

        public List<ReporteFuenteDto> ObtenerFuentes()
        {
            return new List<ReporteFuenteDto>
            {
                new() { Clave = "estudiantes", Nombre = "Estudiantes", Campos = MapCampos(CamposEstudiantes) },
                new() { Clave = "aspirantes", Nombre = "Aspirantes", Campos = MapCampos(CamposAspirantes) },
                new() { Clave = "calificaciones", Nombre = "Calificaciones", Campos = MapCampos(CamposCalificaciones) },
                new() { Clave = "grupos", Nombre = "Grupos", Campos = MapCampos(CamposGrupos) },
                new() { Clave = "recibos", Nombre = "Recibos", Campos = MapCampos(CamposRecibos) },
                new() { Clave = "pagos", Nombre = "Pagos", Campos = MapCampos(CamposPagos) },
            };
        }

        private static List<ReporteCampoDto> MapCampos<T>(List<CampoDef<T>> campos) =>
            campos.Select(c => new ReporteCampoDto
            {
                Clave = c.Clave, Etiqueta = c.Etiqueta, Tipo = c.Tipo, Filtrable = c.Filtrable,
                Catalogo = c.Catalogo ?? (c.Filtrable ? c.Clave switch
                {
                    "plan" => "plan",
                    "campus" => "campus",
                    "periodo" => "periodo",
                    _ => null
                } : null)
            }).ToList();

        public async Task<ReporteResultadoDto> EjecutarAsync(EjecutarReporteRequest request, CancellationToken ct = default)
        {
            switch (request.Fuente)
            {
                case "estudiantes":
                    return await EjecutarGenericoAsync(BaseEstudiantes(), CamposEstudiantes, request, ct);
                case "aspirantes":
                    return await EjecutarGenericoAsync(BaseAspirantes(), CamposAspirantes, request, ct);
                case "calificaciones":
                    return await EjecutarGenericoAsync(BaseCalificaciones(), CamposCalificaciones, request, ct);
                case "grupos":
                    return await EjecutarGenericoAsync(BaseGrupos(), CamposGrupos, request, ct);
                case "recibos":
                    return await EjecutarGenericoAsync(BaseRecibos(), CamposRecibos, request, ct);
                case "pagos":
                    return await EjecutarGenericoAsync(BasePagos(), CamposPagos, request, ct);
                default:
                    throw new InvalidOperationException($"Fuente de reporte no soportada: {request.Fuente}");
            }
        }

        private IQueryable<Estudiante> BaseEstudiantes() =>
            _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Include(e => e.IdPlanActualNavigation).ThenInclude(p => p!.IdCampusNavigation)
                .Where(e => e.Status != StatusEnum.Deleted);

        private IQueryable<Aspirante> BaseAspirantes() =>
            _db.Aspirante
                .Include(a => a.IdPersonaNavigation)
                .Include(a => a.IdPlanNavigation).ThenInclude(p => p.IdCampusNavigation)
                .Include(a => a.IdAspiranteEstatusNavigation)
                .Include(a => a.IdMedioContactoNavigation)
                .Include(a => a.IdPeriodoAcademicoNavigation)
                .Where(a => a.Status != StatusEnum.Deleted && !a.EsAlumnoAutoCreado);

        private IQueryable<CalificacionDetalle> BaseCalificaciones() =>
            _db.CalificacionDetalle
                .Include(d => d.CalificacionParcial).ThenInclude(cp => cp.Inscripcion).ThenInclude(i => i.IdEstudianteNavigation).ThenInclude(e => e.IdPersonaNavigation)
                .Include(d => d.CalificacionParcial).ThenInclude(cp => cp.GrupoMateria).ThenInclude(gm => gm.IdMateriaPlanNavigation).ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(d => d.CalificacionParcial).ThenInclude(cp => cp.GrupoMateria).ThenInclude(gm => gm.IdGrupoNavigation).ThenInclude(g => g.IdPlanEstudiosNavigation).ThenInclude(p => p.IdCampusNavigation)
                .Include(d => d.CalificacionParcial).ThenInclude(cp => cp.GrupoMateria).ThenInclude(gm => gm.IdGrupoNavigation).ThenInclude(g => g.IdPeriodoAcademicoNavigation)
                .Include(d => d.CalificacionParcial).ThenInclude(cp => cp.GrupoMateria).ThenInclude(gm => gm.IdProfesorNavigation).ThenInclude(pr => pr.IdPersonaNavigation)
                .Include(d => d.CalificacionParcial).ThenInclude(cp => cp.Parcial)
                .Where(d => d.Status != StatusEnum.Deleted);

        private IQueryable<Grupo> BaseGrupos() =>
            _db.Grupo
                .Include(g => g.IdTurnoNavigation)
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .Include(g => g.IdPlanEstudiosNavigation).ThenInclude(p => p.IdCampusNavigation)
                .Where(g => g.Status == StatusEnum.Active);

        private IQueryable<ReciboRow> BaseRecibos() =>
            _db.Recibo
                .Where(r => r.Status != StatusEnum.Deleted)
                .Select(r => new ReciboRow
                {
                    Folio = r.Folio,
                    EstatusEnum = r.Estatus,
                    Subtotal = r.Subtotal,
                    Descuento = r.Descuento,
                    Total = r.Total,
                    Saldo = r.Saldo,
                    FechaEmision = r.FechaEmision,
                    FechaVencimiento = r.FechaVencimiento,
                    IdPeriodoAcademico = r.IdPeriodoAcademico,
                    Matricula = _db.Estudiante.Where(e => e.IdEstudiante == r.IdEstudiante).Select(e => e.Matricula).FirstOrDefault(),
                    Estudiante = _db.Estudiante.Where(e => e.IdEstudiante == r.IdEstudiante)
                        .Select(e => (e.IdPersonaNavigation.Nombre ?? "") + " " + (e.IdPersonaNavigation.ApellidoPaterno ?? "")).FirstOrDefault(),
                    Periodo = _db.PeriodoAcademico.Where(p => p.IdPeriodoAcademico == r.IdPeriodoAcademico).Select(p => p.Nombre).FirstOrDefault()
                });

        private IQueryable<PagoRow> BasePagos() =>
            _db.Pago
                .Where(p => p.Status != StatusEnum.Deleted)
                .Select(p => new { p, Rec = p.Aplicaciones.Select(a => a.ReciboDetalle.Recibo).FirstOrDefault() })
                .Select(x => new PagoRow
                {
                    Folio = x.p.FolioPago,
                    Fecha = x.p.FechaPagoUtc,
                    Medio = x.p.MedioPago.Descripcion ?? x.p.MedioPago.Clave,
                    Monto = x.p.Monto,
                    Moneda = x.p.Moneda,
                    EstatusEnum = x.p.Estatus,
                    Referencia = x.p.Referencia,
                    IdPeriodoAcademico = x.Rec != null ? x.Rec.IdPeriodoAcademico : null,
                    Periodo = x.Rec == null ? null : _db.PeriodoAcademico.Where(pa => pa.IdPeriodoAcademico == x.Rec.IdPeriodoAcademico).Select(pa => pa.Nombre).FirstOrDefault(),
                    Matricula = x.Rec == null ? null : _db.Estudiante.Where(e => e.IdEstudiante == x.Rec.IdEstudiante).Select(e => e.Matricula).FirstOrDefault(),
                    Estudiante = x.Rec == null ? null : _db.Estudiante.Where(e => e.IdEstudiante == x.Rec.IdEstudiante)
                        .Select(e => (e.IdPersonaNavigation.Nombre ?? "") + " " + (e.IdPersonaNavigation.ApellidoPaterno ?? "")).FirstOrDefault()
                });

        private async Task<ReporteResultadoDto> EjecutarGenericoAsync<T>(
            IQueryable<T> baseQuery, List<CampoDef<T>> campos, EjecutarReporteRequest request, CancellationToken ct)
        {
            var mapa = campos.ToDictionary(c => c.Clave, c => c);
            var columnas = (request.Columnas != null && request.Columnas.Count > 0
                ? request.Columnas.Where(mapa.ContainsKey)
                : campos.Select(c => c.Clave)).ToList();

            var agruparPor = !string.IsNullOrWhiteSpace(request.AgruparPor) && mapa.ContainsKey(request.AgruparPor!)
                ? request.AgruparPor!
                : null;
            if (agruparPor != null && !columnas.Contains(agruparPor))
                columnas.Insert(0, agruparPor);

            var query = baseQuery;
            if (request.Filtros != null)
            {
                foreach (var f in request.Filtros)
                {
                    if (string.IsNullOrWhiteSpace(f.Valor)) continue;
                    if (mapa.TryGetValue(f.Campo, out var def) && def.Filtro != null)
                        query = def.Filtro(query, f.Valor.Trim());
                }
            }

            var items = await query.ToListAsync(ct);

            var filas = items.Select(it =>
            {
                var fila = new Dictionary<string, object?>();
                foreach (var col in columnas)
                    fila[col] = mapa[col].Getter(it);
                return fila;
            }).ToList();

            var comparer = Comparer<object?>.Create(CompararValores);
            var ordenValido = !string.IsNullOrWhiteSpace(request.OrdenCampo) && mapa.ContainsKey(request.OrdenCampo!);

            if (agruparPor != null)
            {
                IOrderedEnumerable<Dictionary<string, object?>> ordenado =
                    filas.OrderBy(f => f.GetValueOrDefault(agruparPor), comparer);
                if (ordenValido)
                    ordenado = request.OrdenDescendente
                        ? ordenado.ThenByDescending(f => f.GetValueOrDefault(request.OrdenCampo!), comparer)
                        : ordenado.ThenBy(f => f.GetValueOrDefault(request.OrdenCampo!), comparer);
                filas = ordenado.ToList();
            }
            else if (ordenValido)
            {
                filas = request.OrdenDescendente
                    ? filas.OrderByDescending(f => f.GetValueOrDefault(request.OrdenCampo!), comparer).ToList()
                    : filas.OrderBy(f => f.GetValueOrDefault(request.OrdenCampo!), comparer).ToList();
            }

            var camposNumericos = columnas.Where(c => mapa[c].Tipo == "numero").ToList();
            var subtotales = new List<ReporteSubtotalDto>();
            ReporteSubtotalDto? totalGeneral = null;

            if (agruparPor != null)
            {
                foreach (var grupo in filas.GroupBy(f => ValorGrupo(f.GetValueOrDefault(agruparPor))))
                    subtotales.Add(CalcularSubtotal(grupo.Key, grupo.ToList(), camposNumericos));

                if (filas.Count > 0)
                    totalGeneral = CalcularSubtotal("Total general", filas, camposNumericos);
            }

            return new ReporteResultadoDto
            {
                Columnas = columnas.Select(c => new ReporteCampoDto
                {
                    Clave = c, Etiqueta = mapa[c].Etiqueta, Tipo = mapa[c].Tipo, Filtrable = mapa[c].Filtrable
                }).ToList(),
                Filas = filas,
                Total = filas.Count,
                AgrupadoPor = agruparPor,
                CamposNumericos = camposNumericos,
                Subtotales = subtotales,
                TotalGeneral = totalGeneral
            };
        }

        private static ReporteSubtotalDto CalcularSubtotal(string grupo, List<Dictionary<string, object?>> filas, List<string> camposNumericos)
        {
            var sub = new ReporteSubtotalDto { Grupo = grupo, Conteo = filas.Count };
            foreach (var nc in camposNumericos)
            {
                var valores = filas.Select(f => ToDecimal(f.GetValueOrDefault(nc))).Where(v => v.HasValue).Select(v => v!.Value).ToList();
                var suma = valores.Sum();
                sub.Sumas[nc] = decimal.Round(suma, 2);
                sub.Promedios[nc] = valores.Count > 0 ? decimal.Round(suma / valores.Count, 2) : 0m;
            }
            return sub;
        }

        private static string ValorGrupo(object? v) => v?.ToString() ?? "(Sin dato)";

        private static decimal? ToDecimal(object? v) => v switch
        {
            null => null,
            decimal d => d,
            int i => i,
            long l => l,
            double db => (decimal)db,
            float f => (decimal)f,
            byte b => b,
            _ => decimal.TryParse(v.ToString(), out var r) ? r : (decimal?)null
        };

        public async Task<byte[]> ExportarExcelAsync(EjecutarReporteRequest request, CancellationToken ct = default)
        {
            var resultado = await EjecutarAsync(request, ct);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Reporte");

            for (int c = 0; c < resultado.Columnas.Count; c++)
            {
                var cell = ws.Cell(1, c + 1);
                cell.Value = resultado.Columnas[c].Etiqueta;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#14356F");
                cell.Style.Font.FontColor = XLColor.White;
            }

            int fila = 2;
            string? grupoActual = null;
            List<Dictionary<string, object?>> bufferGrupo = new();

            void EscribirResumenGrupo(string etiqueta, Func<ReporteSubtotalDto, Dictionary<string, decimal>> selector, XLColor color)
            {
                if (resultado.AgrupadoPor == null || grupoActual == null) return;
                var sub = resultado.Subtotales.FirstOrDefault(s => s.Grupo == grupoActual);
                if (sub == null) return;
                var valores = selector(sub);
                for (int c = 0; c < resultado.Columnas.Count; c++)
                {
                    var clave = resultado.Columnas[c].Clave;
                    var cell = ws.Cell(fila, c + 1);
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = color;
                    if (c == 0) cell.Value = $"{etiqueta} · {grupoActual} ({sub.Conteo})";
                    else if (valores.TryGetValue(clave, out var num)) cell.Value = num;
                }
                fila++;
            }

            for (int r = 0; r < resultado.Filas.Count; r++)
            {
                if (resultado.AgrupadoPor != null)
                {
                    var g = resultado.Filas[r].GetValueOrDefault(resultado.AgrupadoPor)?.ToString() ?? "(Sin dato)";
                    if (grupoActual != null && g != grupoActual)
                    {
                        EscribirResumenGrupo("Subtotal", s => s.Sumas, XLColor.FromHtml("#DCE6F1"));
                        EscribirResumenGrupo("Promedio", s => s.Promedios, XLColor.FromHtml("#EAF1DD"));
                    }
                    grupoActual = g;
                }

                for (int c = 0; c < resultado.Columnas.Count; c++)
                {
                    var valor = resultado.Filas[r].GetValueOrDefault(resultado.Columnas[c].Clave);
                    var cell = ws.Cell(fila, c + 1);
                    switch (valor)
                    {
                        case null: cell.Value = string.Empty; break;
                        case DateOnly d: cell.Value = d.ToString("yyyy-MM-dd"); break;
                        case DateTime dt: cell.Value = dt.ToString("yyyy-MM-dd"); break;
                        case bool b: cell.Value = b ? "Sí" : "No"; break;
                        case decimal dec: cell.Value = dec; break;
                        case int i: cell.Value = i; break;
                        default: cell.Value = valor.ToString(); break;
                    }
                }
                fila++;
            }

            if (resultado.AgrupadoPor != null && grupoActual != null)
            {
                EscribirResumenGrupo("Subtotal", s => s.Sumas, XLColor.FromHtml("#DCE6F1"));
                EscribirResumenGrupo("Promedio", s => s.Promedios, XLColor.FromHtml("#EAF1DD"));
            }

            if (resultado.TotalGeneral != null)
            {
                for (int c = 0; c < resultado.Columnas.Count; c++)
                {
                    var clave = resultado.Columnas[c].Clave;
                    var cell = ws.Cell(fila, c + 1);
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#14356F");
                    cell.Style.Font.FontColor = XLColor.White;
                    if (c == 0) cell.Value = $"TOTAL GENERAL ({resultado.TotalGeneral.Conteo})";
                    else if (resultado.TotalGeneral.Sumas.TryGetValue(clave, out var num)) cell.Value = num;
                }
                fila++;
            }

            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static readonly Dictionary<string, string> NombresFuente = new()
        {
            ["estudiantes"] = "Estudiantes", ["aspirantes"] = "Aspirantes", ["calificaciones"] = "Calificaciones",
            ["grupos"] = "Grupos", ["recibos"] = "Recibos", ["pagos"] = "Pagos"
        };

        public async Task<byte[]> ExportarPdfAsync(EjecutarReporteRequest request, CancellationToken ct = default)
        {
            var res = await EjecutarAsync(request, ct);
            QuestPDF.Settings.License = LicenseType.Community;

            const int limiteFilas = 2000;
            var filas = res.Filas.Take(limiteFilas).ToList();
            var truncado = res.Total > filas.Count;

            var subPorGrupo = res.Subtotales.ToDictionary(s => s.Grupo, s => s);
            var titulo = NombresFuente.TryGetValue(request.Fuente, out var n) ? n : request.Fuente;
            var subtitulo = $"{res.Total} registros" +
                (res.AgrupadoPor != null ? $" · agrupado por {res.Columnas.FirstOrDefault(c => c.Clave == res.AgrupadoPor)?.Etiqueta ?? res.AgrupadoPor}" : "");

            var pdf = Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(18);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header().Column(col =>
                    {
                        col.Item().Text($"Reporte de {titulo}").FontSize(13).Bold().FontColor("#14356F");
                        col.Item().Text(subtitulo).FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingVertical(6).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            foreach (var _ in res.Columnas) c.RelativeColumn();
                        });

                        table.Header(h =>
                        {
                            foreach (var colDef in res.Columnas)
                                h.Cell().Background("#14356F").Padding(3).Text(colDef.Etiqueta).FontColor("#FFFFFF").Bold();
                        });

                        void FilaResumen(string etiqueta, Dictionary<string, decimal> valores, string bg)
                        {
                            for (int c = 0; c < res.Columnas.Count; c++)
                            {
                                var clave = res.Columnas[c].Clave;
                                var texto = c == 0 ? etiqueta : (valores.TryGetValue(clave, out var num) ? num.ToString("N2") : "");
                                table.Cell().Background(bg).Padding(3).Text(texto).Bold();
                            }
                        }

                        string? grupoActual = null;
                        int idx = 0;
                        foreach (var fila in filas)
                        {
                            if (res.AgrupadoPor != null)
                            {
                                var g = fila.GetValueOrDefault(res.AgrupadoPor)?.ToString() ?? "(Sin dato)";
                                if (grupoActual != null && g != grupoActual && subPorGrupo.TryGetValue(grupoActual, out var s1))
                                {
                                    FilaResumen($"Subtotal · {grupoActual} ({s1.Conteo})", s1.Sumas, "#DCE6F1");
                                    FilaResumen($"Promedio · {grupoActual}", s1.Promedios, "#EAF1DD");
                                }
                                grupoActual = g;
                            }

                            var bgFila = idx % 2 == 1 ? "#F5F7FA" : "#FFFFFF";
                            foreach (var colDef in res.Columnas)
                                table.Cell().Background(bgFila).Padding(3).Text(FormatValorPdf(fila.GetValueOrDefault(colDef.Clave)));
                            idx++;
                        }

                        if (res.AgrupadoPor != null && grupoActual != null && subPorGrupo.TryGetValue(grupoActual, out var s2))
                        {
                            FilaResumen($"Subtotal · {grupoActual} ({s2.Conteo})", s2.Sumas, "#DCE6F1");
                            FilaResumen($"Promedio · {grupoActual}", s2.Promedios, "#EAF1DD");
                        }

                        if (res.TotalGeneral != null)
                            FilaResumen($"TOTAL GENERAL ({res.TotalGeneral.Conteo})", res.TotalGeneral.Sumas, "#C7D3E8");
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text(truncado ? $"Mostrando {filas.Count} de {res.Total} filas." : "").FontSize(7).FontColor(Colors.Grey.Medium);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                });
            }).GeneratePdf();

            return pdf;
        }

        private static string FormatValorPdf(object? v) => v switch
        {
            null => "",
            DateOnly d => d.ToString("yyyy-MM-dd"),
            DateTime dt => dt.ToString("yyyy-MM-dd"),
            bool b => b ? "Sí" : "No",
            decimal dec => dec.ToString("N2"),
            _ => v.ToString() ?? ""
        };

        public async Task<List<ReporteDefinicionDto>> ListarDefinicionesAsync(CancellationToken ct = default)
        {
            var defs = await _db.ReporteDefinicion
                .Where(d => d.Status != StatusEnum.Deleted)
                .OrderBy(d => d.Nombre)
                .ToListAsync(ct);

            return defs.Select(d => new ReporteDefinicionDto
            {
                IdReporteDefinicion = d.IdReporteDefinicion,
                Nombre = d.Nombre,
                Fuente = d.Fuente,
                Columnas = System.Text.Json.JsonSerializer.Deserialize<List<string>>(d.ColumnasJson) ?? new(),
                Filtros = System.Text.Json.JsonSerializer.Deserialize<List<ReporteFiltroDto>>(d.FiltrosJson) ?? new(),
                OrdenCampo = d.OrdenCampo,
                OrdenDescendente = d.OrdenDescendente,
                AgruparPor = d.AgruparPor
            }).ToList();
        }

        public async Task<ReporteDefinicionDto> GuardarDefinicionAsync(ReporteDefinicionDto dto, CancellationToken ct = default)
        {
            ReporteDefinicion entity;
            if (dto.IdReporteDefinicion > 0)
            {
                entity = await _db.ReporteDefinicion.FirstOrDefaultAsync(d => d.IdReporteDefinicion == dto.IdReporteDefinicion, ct)
                    ?? throw new InvalidOperationException("Reporte guardado no encontrado");
            }
            else
            {
                entity = new ReporteDefinicion { CreatedAt = DateTime.UtcNow, CreatedBy = "Sistema", Status = StatusEnum.Active };
                _db.ReporteDefinicion.Add(entity);
            }

            entity.Nombre = dto.Nombre;
            entity.Fuente = dto.Fuente;
            entity.ColumnasJson = System.Text.Json.JsonSerializer.Serialize(dto.Columnas ?? new());
            entity.FiltrosJson = System.Text.Json.JsonSerializer.Serialize(dto.Filtros ?? new());
            entity.OrdenCampo = dto.OrdenCampo;
            entity.OrdenDescendente = dto.OrdenDescendente;
            entity.AgruparPor = dto.AgruparPor;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            dto.IdReporteDefinicion = entity.IdReporteDefinicion;
            return dto;
        }

        public async Task<bool> EliminarDefinicionAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.ReporteDefinicion.FirstOrDefaultAsync(d => d.IdReporteDefinicion == id, ct);
            if (entity == null) return false;
            entity.Status = StatusEnum.Deleted;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        private static int CompararValores(object? a, object? b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            if (a is IComparable ca && a.GetType() == b.GetType())
                return ca.CompareTo(b);
            return string.Compare(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
