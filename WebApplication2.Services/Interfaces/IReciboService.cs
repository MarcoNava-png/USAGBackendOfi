using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Recibo;
using WebApplication2.Core.Requests.Pagos;

namespace WebApplication2.Services.Interfaces
{
    public interface IReciboService
    {
        Task<RecalcularDescuentosResultDto> RecalcularDescuentosConvenioAspiranteAsync(int idAspirante, CancellationToken ct);
        Task<ReciboDto> GenerarReciboAspiranteConConceptoAsync(int idAspirante, int idConceptoPago, int diasVencimiento, CancellationToken ct);
        Task<IReadOnlyList<ReciboDto>> GenerarRecibosAsync(GenerarRecibosDto dto, CancellationToken ct);
        Task<ReciboDto?> ObtenerAsync(long idRecibo, CancellationToken ct);
        Task<IReadOnlyList<ReciboDto>> ListarPorPeriodoAsync(int idPeriodoAcademico, int? idEstudiante, CancellationToken ct);
        Task<IReadOnlyList<ReciboDto>> ListarPorAspiranteAsync(int idAspirante, CancellationToken ct);
        Task<int> RecalcularRecargosAsync(int idPeriodoAcademico, DateOnly? fechaCorte, CancellationToken ct);
        Task<ReciboDto> GenerarReciboAspiranteAsync(int idAspirante, decimal monto, string concepto, int diasVencimiento, CancellationToken ct);
        Task<int> RepararRecibosSinDetallesAsync(CancellationToken ct);
        Task<bool> EliminarReciboAsync(long idRecibo, CancellationToken ct);

        Task<ReciboPdfDto?> ObtenerParaPdfAsync(long idRecibo, CancellationToken ct);

        Task<ReciboBusquedaResultadoDto> BuscarRecibosAsync(ReciboBusquedaFiltrosDto filtros, CancellationToken ct);

        Task<ReciboEstadisticasDto> ObtenerEstadisticasAsync(int? idPeriodoAcademico, CancellationToken ct);

        Task<ReciboBusquedaResultadoDto> BuscarPorMatriculaAsync(string matricula, CancellationToken ct);

        Task<byte[]> ExportarExcelAsync(ReciboBusquedaFiltrosDto filtros, CancellationToken ct);

        Task<ReciboDto> CancelarReciboAsync(long idRecibo, string usuario, string? motivo, CancellationToken ct);

        Task<ReciboDto> ReversarReciboAsync(long idRecibo, string usuario, string? motivo, CancellationToken ct);

        Task<ReciboDto?> BuscarPorFolioAsync(string folio, CancellationToken ct);

        Task<CarteraVencidaReporteDto> ObtenerCarteraVencidaAsync(int? idPeriodoAcademico, int? diasVencidoMinimo, CancellationToken ct);

        Task<byte[]> ExportarCarteraVencidaExcelAsync(int? idPeriodoAcademico, int? diasVencidoMinimo, CancellationToken ct);

        Task<IngresosPeriodoReporteDto> ObtenerIngresosAsync(int idPeriodoAcademico, DateOnly? fechaInicio, DateOnly? fechaFin, CancellationToken ct);

        Task<byte[]> ExportarIngresosExcelAsync(int idPeriodoAcademico, DateOnly? fechaInicio, DateOnly? fechaFin, CancellationToken ct);
    }
}
