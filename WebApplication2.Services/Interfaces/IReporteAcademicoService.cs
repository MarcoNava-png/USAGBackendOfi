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

    // Excel
    byte[] GenerarEstudiantesPorGrupoExcel(ReporteEstudiantesGrupoDto data);
    byte[] GenerarHorarioExcel(HorarioReporteDto data);
    Task<byte[]> GenerarPlanesEstudioExcelAsync(CancellationToken ct = default);
}
