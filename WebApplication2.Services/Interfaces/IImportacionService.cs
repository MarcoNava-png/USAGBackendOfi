using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Importacion;
using WebApplication2.Core.Requests.Importacion;
using WebApplication2.Core.Responses.Importacion;

namespace WebApplication2.Services.Interfaces
{
    public interface IImportacionService
    {
        Task<ValidarImportacionResponse> ValidarImportacionAsync(ValidarImportacionRequest request);

        Task<ImportarEstudiantesResponse> ImportarEstudiantesAsync(ImportarEstudiantesRequest request);

        Task<List<string>> GetCampusDisponiblesAsync();

        Task<List<string>> GetPlanesDisponiblesAsync();

        Task<ImportarCampusResponse> ImportarCampusAsync(ImportarCampusRequest request);

        Task<ImportarPlanesEstudiosResponse> ImportarPlanesEstudiosAsync(ImportarPlanesEstudiosRequest request);

        Task<ValidarMateriasResponse> ValidarMateriasAsync(ValidarMateriasRequest request);

        Task<ImportarMateriasResponse> ImportarMateriasAsync(ImportarMateriasRequest request);

        Task<byte[]> GenerarPlantillaMateriasAsync(int? idPlanEstudios = null);
    }
}
