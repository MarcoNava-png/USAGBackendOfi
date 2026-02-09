using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IUbicacionService
    {
        Task<IEnumerable<Estado>> ObtenerEstados();
        Task<IEnumerable<Municipio>> ObtenerMunicipios(string estadoId);
        Task<IEnumerable<CodigoPostal>> ObtenerCodigosPostales(string municipioId);
    }
}
