using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Documentos;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Documentos;
using WebApplication2.Core.Responses.Documentos;

namespace WebApplication2.Services.Interfaces
{
    public interface IDocumentoEstudianteService
    {
        #region Tipos de Documento

        Task<List<TipoDocumentoDto>> GetTiposDocumentoAsync();
        Task<TipoDocumentoDto?> GetTipoDocumentoByIdAsync(int id);
        Task<TipoDocumentoDto?> GetTipoDocumentoByClaveAsync(string clave);
        Task<TipoDocumentoEstudiante> CreateTipoDocumentoAsync(TipoDocumentoEstudiante tipo);
        Task<TipoDocumentoEstudiante> UpdateTipoDocumentoAsync(TipoDocumentoEstudiante tipo);
        Task DeleteTipoDocumentoAsync(int id);

        #endregion

        #region Solicitudes

        Task<SolicitudDocumentoDto> CrearSolicitudAsync(CrearSolicitudDocumentoRequest request, string usuarioId);

        Task<SolicitudDocumentoDto?> GetSolicitudByIdAsync(long id);

        Task<SolicitudDocumentoDto?> GetSolicitudByCodigoVerificacionAsync(Guid codigo);

        Task<SolicitudesListResponse> GetSolicitudesAsync(SolicitudesFiltro filtro);

        Task<List<SolicitudDocumentoDto>> GetSolicitudesByEstudianteAsync(int idEstudiante);

        Task ActualizarEstatusPagoAsync(long idRecibo);

        Task<SolicitudDocumentoDto> MarcarComoGeneradaAsync(long idSolicitud, string usuarioId);

        Task CancelarSolicitudAsync(long idSolicitud, string motivo, string usuarioId);

        Task<SolicitudDocumentoDto> MarcarComoEntregadoAsync(long idSolicitud, string usuarioId);

        /// <summary>
        /// Obtiene solicitudes para el panel de Control Escolar con filtros
        /// </summary>
        Task<SolicitudesPendientesDto> GetSolicitudesParaControlEscolarAsync(
            string? filtroEstatus = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string? busqueda = null,
            CancellationToken ct = default);

        /// <summary>
        /// Obtiene el contador de solicitudes listas para generar (estado PAGADO)
        /// </summary>
        Task<int> GetContadorSolicitudesListasAsync(CancellationToken ct = default);

        #endregion

        #region Verificación

        Task<VerificacionDocumentoDto> VerificarDocumentoAsync(Guid codigoVerificacion);

        #endregion

        #region Generación de Documentos

        Task<KardexEstudianteDto> GenerarKardexAsync(int idEstudiante, bool soloPeridoActual = false);

        Task<ConstanciaEstudiosDto> GenerarConstanciaAsync(long idSolicitud);

        Task<byte[]> GenerarKardexPdfAsync(long idSolicitud);

        Task<byte[]> GenerarConstanciaPdfAsync(long idSolicitud);

        #endregion

        #region Utilidades

        Task<string> GenerarFolioSolicitudAsync();

        string GenerarUrlVerificacion(Guid codigoVerificacion);

        Task<bool> PuedeGenerarAsync(long idSolicitud);

        Task ActualizarDocumentosVencidosAsync();

        #endregion
    }
}
