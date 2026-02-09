using System.Threading;
using System.Threading.Tasks;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Inscripcion;

namespace WebApplication2.Services.Interfaces
{
    public interface IInscripcionService
    {
        Task<Inscripcion> CrearInscripcion(Inscripcion inscripcion);

        Task<InscripcionConPagosDto> InscribirConRecibosAutomaticosAsync(
            InscribirConRecibosRequest request,
            CancellationToken ct = default);

        Task<bool> EsNuevoIngresoAsync(int idEstudiante, int idPeriodoAcademico, CancellationToken ct = default);

        Task<List<Inscripcion>> GetInscripcionesByEstudianteAsync(int idEstudiante);
    }
}
