using WebApplication2.Core.DTOs.EstudioSocioeconomico;

namespace WebApplication2.Services.Interfaces
{
    public interface IEstudioSocioeconomicoService
    {
        Task<CatalogosEstudioDto> GetCatalogosAsync();
        Task<List<AnalistaDto>> GetAnalistasAsync();
        Task<EstudioSocioeconomicoDto?> GetByAspiranteAsync(int idAspirante);
        Task<EstudioSocioeconomicoDto> UpsertAsync(int idAspirante, EstudioSocioeconomicoRequest req, string? analistaId, bool porAspirante);
        Task<string> GenerarTokenAsync(int idAspirante);
        Task<EstudioPublicoDto?> GetByTokenAsync(string token);
        Task<EstudioSocioeconomicoDto?> UpsertByTokenAsync(string token, EstudioSocioeconomicoRequest req);
    }
}
