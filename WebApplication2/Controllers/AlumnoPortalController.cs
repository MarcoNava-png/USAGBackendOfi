using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Enums;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers;

[Route("api/alumno-portal")]
[ApiController]
[Authorize(Roles = Rol.ALUMNO)]
public class AlumnoPortalController : ControllerBase
{
    private readonly ITareaDocenteService _tareaService;
    private readonly ApplicationDbContext _dbContext;

    public AlumnoPortalController(ITareaDocenteService tareaService, ApplicationDbContext dbContext)
    {
        _tareaService = tareaService;
        _dbContext = dbContext;
    }

    [HttpGet("tareas")]
    public async Task<IActionResult> GetMisTareas(CancellationToken ct)
    {
        var estudiante = await ResolveEstudiante(ct);
        if (estudiante == null) return NotFound(new { message = "Estudiante no encontrado" });

        var tareas = await _tareaService.GetTareasAlumno(estudiante.IdEstudiante);
        return Ok(tareas);
    }

    [HttpPost("tareas/{idTarea:int}/entregar")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> EntregarTarea(int idTarea, IFormFile archivo, CancellationToken ct)
    {
        var estudiante = await ResolveEstudiante(ct);
        if (estudiante == null) return NotFound(new { message = "Estudiante no encontrado" });

        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { message = "Archivo requerido" });

        var entrega = await _tareaService.SubirEntrega(idTarea, estudiante.IdEstudiante, archivo);
        return Ok(new
        {
            id = entrega.Id,
            nombreArchivo = entrega.NombreArchivo,
            fechaEntrega = entrega.FechaEntrega,
            mensaje = "Tarea entregada correctamente"
        });
    }

    private async Task<Core.Models.Estudiante?> ResolveEstudiante(CancellationToken ct)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId)) return null;
        return await _dbContext.Estudiante
            .FirstOrDefaultAsync(e => e.UsuarioId == userId && e.Status == StatusEnum.Active, ct);
    }
}
