using WebApplication2.Core.DTOs.Titulacion;

namespace WebApplication2.Services.Interfaces
{
    public interface IConfiguracionTitulacionService
    {
        Task<List<ConfiguracionIPESDto>> GetConfiguracionesAsync(CancellationToken ct = default);
        Task<ConfiguracionIPESDto?> GetConfiguracionPorCampusAsync(int idCampus, CancellationToken ct = default);
        Task<ConfiguracionIPESDto> GuardarConfiguracionAsync(GuardarConfiguracionIPESRequest request, CancellationToken ct = default);
        Task<ResponsableFirmaDto> GuardarResponsableAsync(GuardarResponsableFirmaRequest request, CancellationToken ct = default);
        Task<CredencialSEPDto> GuardarCredencialAsync(GuardarCredencialSEPRequest request, CancellationToken ct = default);
        Task SubirCertificadoCerAsync(int idResponsable, Stream archivo, string nombreArchivo, CancellationToken ct = default);
        Task SubirLlaveKeyAsync(int idResponsable, Stream archivo, string nombreArchivo, CancellationToken ct = default);
    }
}
