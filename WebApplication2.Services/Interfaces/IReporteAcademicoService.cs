using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Reportes;

namespace WebApplication2.Services.Interfaces;

public interface IReporteAcademicoService
{
    // Datos
    Task<ReporteEstudiantesGrupoDto> GetEstudiantesPorGrupoAsync(int idGrupo, CancellationToken ct = default);
    Task<BoletaCalificacionesDto> GetBoletaCalificacionesAsync(int idEstudiante, int idPeriodo, CancellationToken ct = default);
    Task<ActaCalificacionDto> GetActaCalificacionAsync(int idGrupoMateria, int? idParcial, CancellationToken ct = default);
    Task<HorarioReporteDto> GetHorarioGrupoAsync(int idGrupo, CancellationToken ct = default);
    Task<HorarioReporteDto> GetHorarioDocenteAsync(int idProfesor, int idPeriodo, CancellationToken ct = default);
    Task<ListaAsistenciaDto> GetListaAsistenciaAsync(int idGrupoMateria, CancellationToken ct = default);

    // PDFs
    byte[] GenerarEstudiantesPorGrupoPdf(ReporteEstudiantesGrupoDto data);
    byte[] GenerarBoletaCalificacionesPdf(BoletaCalificacionesDto data);
    byte[] GenerarActaCalificacionPdf(ActaCalificacionDto data);
    byte[] GenerarHorarioPdf(HorarioReporteDto data);
    byte[] GenerarListaAsistenciaPdf(ListaAsistenciaDto data);

    // Bajas
    Task<ReporteBajasDto> GetReporteBajasAsync(int? idCampus, int? idPlanEstudios, int? idPeriodo, int? mes, int? anio, CancellationToken ct = default);
    byte[] GenerarReporteBajasPdf(ReporteBajasDto data);
    byte[] GenerarReporteBajasExcel(ReporteBajasDto data);

    // Excel
    byte[] GenerarEstudiantesPorGrupoExcel(ReporteEstudiantesGrupoDto data);
    byte[] GenerarHorarioExcel(HorarioReporteDto data);
    Task<byte[]> GenerarPlanesEstudioExcelAsync(CancellationToken ct = default);

    // Alumnos inscritos por periodo
    Task<ReporteAlumnosInscritosDto> GetAlumnosInscritosAsync(int[] idsPeriodo, int? idPlanEstudios, int? idCampus, int? idGrupo = null, CancellationToken ct = default);
    byte[] GenerarAlumnosInscritosExcel(ReporteAlumnosInscritosDto data);

    // Adeudo de documentos
    Task<ReporteAdeudoDocumentosDto> GetAdeudoDocumentosAsync(int[] idsPlanEstudios, int? idPeriodoAcademico, string? tipoFiltro, int? idCampus = null, int? idGrupo = null, CancellationToken ct = default);
    byte[] GenerarAdeudoDocumentosExcel(ReporteAdeudoDocumentosDto data);
}
