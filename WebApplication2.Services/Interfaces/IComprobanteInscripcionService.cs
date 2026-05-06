using WebApplication2.Core.DTOs.Comprobante;

namespace WebApplication2.Services.Interfaces;

public interface IComprobanteInscripcionService
{
    Task<ComprobanteInscripcionDto?> ObtenerAsync(int idEstudiante, string? passwordTemporal = null, CancellationToken ct = default);
    Task<byte[]> GenerarPdfAsync(int idEstudiante, string? passwordTemporal = null, CancellationToken ct = default);
}
