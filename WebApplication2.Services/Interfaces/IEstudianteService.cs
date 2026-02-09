using WebApplication2.Core.Common;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IEstudianteService
    {
        Task<PagedResult<Estudiante>> GetEstudiantes(int page, int pageSize);
        Task<Estudiante> GetEstudianteDetalle(int id);
        Task<Estudiante?> GetEstudianteByMatricula(string matricula);
        Task<Estudiante> CrearEstudiante(Estudiante estudiante);
        Task<Estudiante> ActualizarEstudiante(Estudiante newEstudiante);

        Task<(bool TienePendientes, int CantidadPendientes, decimal MontoPendiente)> ValidarPagosPendientesAsync(
            int idEstudiante,
            int? idPeriodoAcademico = null);

        Task<bool> PuedeInscribirseAsync(int idEstudiante, int? idPeriodoAcademico = null);

        Task<PagedResult<Estudiante>> GetEstudiantesSinGrupo(int idPlanEstudios, int idPeriodoAcademico, int page, int pageSize);
    }
}
