using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Comprobante;
using WebApplication2.Core.DTOs.Pagos;
using WebApplication2.Core.Requests.Pagos;
using WebApplication2.Core.Responses.Pagos;

namespace WebApplication2.Services.Interfaces
{
    public interface IPagoService
    {
        Task<long> RegistrarPagoAsync(RegistrarPagoDto dto, CancellationToken ct);
        Task<IReadOnlyList<long>> AplicarPagoAsync(AplicarPagoDto dto, CancellationToken ct);
        Task<PagoDto?> ObtenerAsync(long idPago, CancellationToken ct);
        Task<IReadOnlyList<PagoDto>> ListarPorFechaAsync(DateTime fechaInicio, DateTime fechaFin, string? usuarioId, CancellationToken ct);

        Task<RegistrarYAplicarPagoResultDto> RegistrarYAplicarPagoAsync(RegistrarYAplicarPagoDto dto, CancellationToken ct);

        Task<ComprobantePagoDto?> ObtenerDatosComprobanteAsync(long idPago, CancellationToken ct);

        Task<PagoRegistradoResponse> RegistrarPagoCajaAsync(RegistrarPagoCajaRequest request, string usuarioId, CancellationToken ct);
    }
}
