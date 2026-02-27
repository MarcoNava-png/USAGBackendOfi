using Microsoft.AspNetCore.Http;
using WebApplication2.Core.DTOs.DocentePortal;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IPlaneacionDocenteService
    {
        Task<List<PlaneacionDocenteDto>> GetByGrupoMateria(int idGrupoMateria, int profesorId);
        Task<PlaneacionDocente> Upload(int profesorId, int idGrupoMateria, IFormFile file, string? descripcion);
        Task Delete(int id, int profesorId);
        Task<PlaneacionDocente?> GetById(int id);
    }
}
