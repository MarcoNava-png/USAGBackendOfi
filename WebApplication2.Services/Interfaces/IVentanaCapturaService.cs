using WebApplication2.Core.DTOs.VentanaCaptura;

namespace WebApplication2.Services.Interfaces
{
    public interface IVentanaCapturaService
    {
        Task<List<VentanaCapturaDto>> GetVentanasAsync(int idPeriodoAcademico, CancellationToken ct = default);
        Task<VentanaCapturaDto> AbrirAsync(int idPeriodoAcademico, int numeroParcial, DateTime? fechaLimite, CancellationToken ct = default);
        Task<VentanaCapturaDto> CerrarAsync(int idPeriodoAcademico, int numeroParcial, CancellationToken ct = default);

        Task<EstadoCapturaDto> GetEstadoAsync(int idGrupoMateria, int numeroParcial, int idProfesor, CancellationToken ct = default);
        Task<(bool permitido, string? motivo)> PuedeCapturarAsync(int idGrupoMateria, int numeroParcial, int idProfesor, CancellationToken ct = default);

        Task<SolicitudProrrogaDto> SolicitarProrrogaAsync(int idProfesor, SolicitarProrrogaRequest req, CancellationToken ct = default);
        Task<List<SolicitudProrrogaDto>> GetProrrogasAsync(string? estado, int? campusRestringido, CancellationToken ct = default);
        Task<List<SolicitudProrrogaDto>> GetMisProrrogasAsync(int idProfesor, CancellationToken ct = default);
        Task<SolicitudProrrogaDto?> ResolverProrrogaAsync(int idSolicitud, ResolverProrrogaRequest req, string userId, CancellationToken ct = default);

        Task<AvanceCapturaDto> GetAvanceAsync(int idPeriodoAcademico, int numeroParcial, int? campusRestringido, CancellationToken ct = default);
    }
}
