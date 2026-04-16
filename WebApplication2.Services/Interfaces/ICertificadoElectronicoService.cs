using WebApplication2.Core.DTOs.Titulacion;
using WebApplication2.Core.Enums;

namespace WebApplication2.Services.Interfaces
{
    public interface ICertificadoElectronicoService
    {
        Task<List<CertificadoElectronicoListDto>> ListarAsync(TipoTitulacionEnum tipo, CancellationToken ct = default);
        Task<CertificadoElectronicoDetalleDto?> ObtenerDetalleAsync(int id, CancellationToken ct = default);
        Task<CertificadoElectronicoDetalleDto> CrearDirectoAsync(CrearCertificadoDirectoRequest request, CancellationToken ct = default);
        Task<CertificadoElectronicoDetalleDto> ActualizarDirectoAsync(int id, CrearCertificadoDirectoRequest request, CancellationToken ct = default);
        Task<(int total, int enRevision, int xmlGenerados, int registrados)> ObtenerEstadisticasAsync(TipoTitulacionEnum tipo, CancellationToken ct = default);
    }
}
