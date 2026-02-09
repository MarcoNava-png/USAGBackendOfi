using WebApplication2.Core.Common;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.MateriaPlan;

namespace WebApplication2.Services.Interfaces
{
    public interface IMateriaPlanService
    {
        Task<PagedResult<MateriaPlan>> GetMateriaPlanes(int page, int pageSize);
        Task<MateriaPlan> GetMateriaPlanDetalle(int id);
        Task<MateriaPlan> CrearMateriaPlan(MateriaPlan materiaPlan);
        Task<MateriaPlan> ActualizarMateriaPlan(MateriaPlan newMateriaPlan);
        Task<(bool Exito, string Mensaje)> EliminarMateriaPlan(int id);
        Task<ImportarMateriasResponse> ImportarMateriasAsync(ImportarMateriasRequest request);
        Task<List<MateriaPlan>> GetMateriasPorPlanAsync(int idPlanEstudios);
    }
}
