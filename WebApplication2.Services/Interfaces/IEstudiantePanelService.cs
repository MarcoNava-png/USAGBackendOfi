using Microsoft.AspNetCore.Http;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.EstudiantePanel;
using WebApplication2.Core.Requests.Estudiante;
using WebApplication2.Core.Requests.EstudiantePanel;
using WebApplication2.Core.Responses.EstudiantePanel;

namespace WebApplication2.Services.Interfaces
{
    public interface IEstudiantePanelService
    {
        #region Consultas de Panel

        Task<EstudiantePanelDto?> ObtenerPanelEstudianteAsync(int idEstudiante, CancellationToken ct = default);

        Task<EstudiantePanelDto?> ObtenerPanelPorMatriculaAsync(string matricula, CancellationToken ct = default);

        Task<BuscarEstudiantesPanelResponse> BuscarEstudiantesAsync(BuscarEstudiantesPanelRequest request, CancellationToken ct = default);

        Task<EstadisticasEstudiantesDto> ObtenerEstadisticasAsync(int? idPlanEstudios = null, int? idPeriodoAcademico = null, CancellationToken ct = default);

        #endregion

        #region Información Académica

        Task<InformacionAcademicaPanelDto?> ObtenerInformacionAcademicaAsync(int idEstudiante, CancellationToken ct = default);

        Task<ResumenKardexDto> ObtenerResumenKardexAsync(int idEstudiante, CancellationToken ct = default);

        Task<SeguimientoAcademicoDto> ObtenerSeguimientoAcademicoAsync(int idEstudiante, CancellationToken ct = default);

        #endregion

        #region Actualización de Datos

        Task<AccionPanelResponse> ActualizarDatosEstudianteAsync(int idEstudiante, ActualizarDatosEstudianteRequest request, CancellationToken ct = default);

        #endregion

        #region Becas

        Task<List<BecaAsignadaDto>> ObtenerBecasEstudianteAsync(int idEstudiante, bool? soloActivas = true, CancellationToken ct = default);

        #endregion

        #region Recibos y Pagos

        Task<ResumenRecibosDto> ObtenerResumenRecibosAsync(int idEstudiante, CancellationToken ct = default);

        Task<List<ReciboPanelResumenDto>> ObtenerRecibosEstudianteAsync(int idEstudiante, string? estatus = null, int limite = 50, CancellationToken ct = default);

        #endregion

        #region Documentos

        Task<DocumentosPersonalesEstudianteDto?> ObtenerDocumentosPersonalesAsync(int idEstudiante, CancellationToken ct = default);

        Task<AccionPanelResponse> SubirDocumentoPersonalAsync(int idEstudiante, int idDocumentoRequisito, IFormFile archivo, string? notas, CancellationToken ct = default);

        Task<AccionPanelResponse> ValidarDocumentoPersonalAsync(int idEstudiante, long idAspiranteDocumento, bool aprobar, string? notas, string? usuarioId, CancellationToken ct = default);

        Task<DocumentosDisponiblesDto> ObtenerDocumentosDisponiblesAsync(int idEstudiante, CancellationToken ct = default);

        Task<AccionPanelResponse> GenerarDocumentoAsync(GenerarDocumentoPanelRequest request, string usuarioId, CancellationToken ct = default);

        Task<byte[]> GenerarKardexPdfDirectoAsync(int idEstudiante, bool soloPeridoActual = false, CancellationToken ct = default);

        Task<byte[]> GenerarConstanciaPdfDirectaAsync(int idEstudiante, CancellationToken ct = default);

        #endregion

        #region Acciones Rápidas

        Task<AccionPanelResponse> EnviarRecordatorioPagoAsync(int idEstudiante, long? idRecibo = null, CancellationToken ct = default);

        Task<AccionPanelResponse> ActualizarEstatusEstudianteAsync(int idEstudiante, bool activo, string? motivo, CancellationToken ct = default);

        #endregion

        #region Exportación

        Task<byte[]> ExportarEstudiantesExcelAsync(BuscarEstudiantesPanelRequest filtros, CancellationToken ct = default);

        Task<byte[]> ExportarExpedienteEstudianteAsync(int idEstudiante, CancellationToken ct = default);

        #endregion
    }
}
