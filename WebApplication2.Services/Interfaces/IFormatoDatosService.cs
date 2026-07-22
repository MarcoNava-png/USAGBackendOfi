using WebApplication2.Core.DTOs.Formatos;

namespace WebApplication2.Services.Interfaces
{
    public interface IFormatoDatosService
    {
        List<FormatoOrigenDto> ListarOrigenes();
        List<FormatoVariableDto> CatalogoVariables(string origen);
        Task<FormatoDatosResueltos> ResolverAsync(string origen, int idEntidad, CancellationToken ct = default);
    }
}
