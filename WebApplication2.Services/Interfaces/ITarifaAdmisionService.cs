using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.TarifaAdmision;

namespace WebApplication2.Services.Interfaces
{
    public interface ITarifaAdmisionService
    {
        Task<IReadOnlyList<TarifaAdmisionDto>> ListarTarifasAsync(bool? soloActivas = null, bool? esConvenioEmpresarial = null, CancellationToken ct = default);
        Task<TarifaAdmisionDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task<TarifaAdmisionDto?> ObtenerPorPlanAsync(int idPlanEstudios, CancellationToken ct = default);
        Task<TarifaAdmisionDto> CrearTarifaAsync(CrearTarifaAdmisionDto dto, string usuarioCreador, CancellationToken ct = default);
        Task<TarifaAdmisionDto> ActualizarTarifaAsync(int id, ActualizarTarifaAdmisionDto dto, string usuarioModificador, CancellationToken ct = default);
        Task<bool> EliminarTarifaAsync(int id, CancellationToken ct = default);
        Task<bool> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default);
        Task<GenerarRecibosAdmisionResultDto> GenerarRecibosAsync(int idAspirante, int idTarifaAdmision, bool pagoCompleto, List<int>? conceptosIncluidos = null, decimal descuentoPorcentaje = 0, CancellationToken ct = default);
        Task<GenerarRecibosAdmisionResultDto> GenerarRecibosV2Async(int idAspirante, int idTarifaAdmision, GenerarRecibosAdmisionRequestV2Dto request, CancellationToken ct = default);
        Task<CotizacionAdmisionPdfDto> GenerarCotizacionPdfDtoAsync(int idTarifaAdmision, int idAspirante, CancellationToken ct = default);
        Task<CotizacionAdmisionPdfDto> GenerarCotizacionPdfDtoV2Async(int idTarifaAdmision, int idAspirante, CotizacionAdmisionRequestDto request, CancellationToken ct = default);
    }
}
