using System.Threading;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs.PreInscripcion;
using WebApplication2.Core.Requests.PreInscripcion;

namespace WebApplication2.Services.Interfaces
{
    public interface IPreInscripcionService
    {
        Task<PreInscripcionDto> ApartarParaPeriodoAsync(ApartarPreInscripcionRequest request, CancellationToken ct = default);

        Task<List<PreInscripcionDto>> GetPendientesAsync(int? idPlanEstudios = null, int? idPeriodoAcademico = null, CancellationToken ct = default);

        Task<PreInscripcionDto> AsignarGrupoAsync(int idPreInscripcion, int idGrupo, CancellationToken ct = default);

        Task<PreInscripcionDto> CancelarAsync(int idPreInscripcion, CancellationToken ct = default);
    }
}
