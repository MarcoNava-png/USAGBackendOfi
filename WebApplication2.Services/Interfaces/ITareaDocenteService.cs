using Microsoft.AspNetCore.Http;
using WebApplication2.Core.DTOs.DocentePortal;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface ITareaDocenteService
    {
        Task<TareaDocente> CrearTarea(TareaDocente tarea);
        Task<TareaDocente> ActualizarTarea(int id, int profesorId, string titulo, string? descripcion, DateTime fechaLimite, decimal puntosMaximos);
        Task EliminarTarea(int id, int profesorId);
        Task<List<TareaDocenteDto>> GetTareasByGrupoMateria(int idGrupoMateria, int profesorId);
        Task<List<EntregaTareaDto>> GetEntregasPorTarea(int idTarea, int profesorId);
        Task CalificarEntrega(int idEntrega, int profesorId, decimal calificacion, string? retroalimentacion);
        Task<EntregaTarea> SubirEntrega(int idTarea, int idEstudiante, IFormFile archivo);
        Task<List<TareaAlumnoDto>> GetTareasAlumno(int idEstudiante);
        Task<EntregaTarea?> GetEntregaById(int id);
    }
}
