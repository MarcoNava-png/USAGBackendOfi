using WebApplication2.Core.DTOs.PortalAlumno;

namespace WebApplication2.Services.Interfaces
{
    public interface IPortalAlumnoService
    {
        Task<MiPerfilDto?> ObtenerMiPerfilAsync(string userId, CancellationToken ct = default);
        Task<bool> ActualizarMiPerfilAsync(string userId, ActualizarMiPerfilRequest request, CancellationToken ct = default);
        Task<MisMateriasDto> ObtenerMisMateriasAsync(string userId, int? idPeriodoAcademico = null, CancellationToken ct = default);
        Task<MisCalificacionesDto> ObtenerMisCalificacionesAsync(string userId, int? idPeriodoAcademico = null, CancellationToken ct = default);
        Task<MiAsistenciaDto> ObtenerMiAsistenciaAsync(string userId, int? idPeriodoAcademico = null, CancellationToken ct = default);
        Task<MisPagosDto> ObtenerMisPagosAsync(string userId, CancellationToken ct = default);
        Task<MiReciboDto?> ObtenerMiReciboDetalleAsync(string userId, long idRecibo, CancellationToken ct = default);
        Task<MisDocumentosOficialesDto> ObtenerMisDocumentosOficialesAsync(string userId, CancellationToken ct = default);
        Task<MisDocumentosPendientesDto> ObtenerMisDocumentosPendientesAsync(string userId, CancellationToken ct = default);
        Task<IReadOnlyList<Core.DTOs.AspiranteDocumentoDto>> ObtenerMiExpedienteAsync(string userId, CancellationToken ct = default);
    }
}
