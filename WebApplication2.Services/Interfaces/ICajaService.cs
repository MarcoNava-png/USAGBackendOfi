using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Caja;
using WebApplication2.Core.DTOs.Comprobante;
using WebApplication2.Core.DTOs.Pagos;
using WebApplication2.Core.DTOs.Recibo;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Pagos;
using WebApplication2.Core.Responses.Caja;
using WebApplication2.Core.Responses.Pagos;

namespace WebApplication2.Services.Interfaces
{
    public interface ICajaService
    {
        Task<RecibosParaCobroDto> BuscarRecibosParaCobroAsync(string criterio);

        Task<RecibosParaCobroDto> BuscarTodosLosRecibosAsync(string criterio);

        Task<List<UsuarioCajeroDto>> ObtenerCajerosAsync();

        Task<ResumenCorteCajaDto> GenerarCorteCajaDetalladoAsync(string? usuarioId, DateTime fechaInicio, DateTime fechaFin);

        byte[] GenerarPdfCorteCaja(ResumenCorteCajaDto resumen);

        Task<ResumenCorteCajaDto> ObtenerResumenCorteCaja(DateTime fechaInicio, DateTime fechaFin, string? usuarioId = null);

        Task<CorteCaja> CerrarCorteCaja(string usuarioId, decimal montoInicial, string? observaciones = null);

        Task<List<CorteCaja>> ObtenerCortesCaja(string? usuarioId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);

        Task<CorteCaja?> ObtenerCorteCajaPorId(int idCorteCaja);

        Task<CorteCaja?> ObtenerCorteActivo(string usuarioId);

        Task<QuitarRecargoResultado> QuitarRecargoAsync(long idRecibo, string motivo, string nombreUsuario, string usuarioId);

        Task<ModificarDetalleResultado> ModificarDetalleReciboAsync(long idRecibo, long idReciboDetalle, decimal nuevoMonto, string motivo, string usuarioId);

        Task<ModificarRecargoResultado> ModificarRecargoReciboAsync(long idRecibo, decimal nuevoRecargo, string motivo, string usuarioId);
    }
}
