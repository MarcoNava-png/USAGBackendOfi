using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IPlantillaReporteService
    {
        Task<List<PlantillaReporte>> ListarAsync(string? categoria = null, CancellationToken ct = default);
        Task<PlantillaReporte?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default);
        Task<PlantillaReporte> CrearAsync(string nombre, string codigo, string categoria, string? descripcion, Stream archivo, string nombreArchivo, string variablesJson, CancellationToken ct = default);
        Task<PlantillaReporte> ActualizarArchivoAsync(int id, Stream archivo, string nombreArchivo, CancellationToken ct = default);
        Task EliminarAsync(int id, CancellationToken ct = default);
        Task<byte[]> GenerarDocumentoAsync(string codigo, Dictionary<string, string> variables, Dictionary<string, List<Dictionary<string, string>>>? tablas = null, CancellationToken ct = default);
        Dictionary<string, string> GetVariablesDisponibles(string codigo);
    }
}
