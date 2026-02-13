using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs.Profesor;
using WebApplication2.Core.Models;
using WebApplication2.Core.Responses.Profesor;

namespace WebApplication2.Services.Interfaces
{
    public interface IProfesorService
    {
        Task<PagedResult<Profesor>> GetAllProfesores(int page, int pageSize);
        Task<PagedResult<Profesor>> GetProfesores(int campusId, int page, int pageSize);
        Task<Profesor> CrearProfesor(Profesor profesor);
        Task<Profesor> ActualizarProfesor(Profesor newProfesor);

        Task<ValidarHorarioProfesorResponse> ValidarConflictosHorarioAsync(
            int idProfesor,
            List<HorarioValidacionDto> horariosNuevos,
            int? idGrupoMateriaActual = null,
            CancellationToken ct = default);
    }
}
