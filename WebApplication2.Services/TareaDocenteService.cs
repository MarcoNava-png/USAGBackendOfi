using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.DocentePortal;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class TareaDocenteService : ITareaDocenteService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBlobStorageService _blobStorageService;
        private const string Container = "entregas-tareas";

        public TareaDocenteService(ApplicationDbContext dbContext, IBlobStorageService blobStorageService)
        {
            _dbContext = dbContext;
            _blobStorageService = blobStorageService;
        }

        public async Task<TareaDocente> CrearTarea(TareaDocente tarea)
        {
            tarea.FechaCreacion = DateTime.UtcNow;
            tarea.CreatedAt = DateTime.UtcNow;
            tarea.CreatedBy = "sistema";
            tarea.Status = StatusEnum.Active;

            _dbContext.Set<TareaDocente>().Add(tarea);
            await _dbContext.SaveChangesAsync();
            return tarea;
        }

        public async Task<TareaDocente> ActualizarTarea(int id, int profesorId, string titulo, string? descripcion, DateTime fechaLimite, decimal puntosMaximos)
        {
            var tarea = await _dbContext.Set<TareaDocente>()
                .FirstOrDefaultAsync(t => t.Id == id && t.IdProfesor == profesorId && t.Status == StatusEnum.Active)
                ?? throw new InvalidOperationException("Tarea no encontrada");

            tarea.Titulo = titulo;
            tarea.Descripcion = descripcion;
            tarea.FechaLimite = fechaLimite;
            tarea.PuntosMaximos = puntosMaximos;
            tarea.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return tarea;
        }

        public async Task EliminarTarea(int id, int profesorId)
        {
            var tarea = await _dbContext.Set<TareaDocente>()
                .FirstOrDefaultAsync(t => t.Id == id && t.IdProfesor == profesorId && t.Status == StatusEnum.Active)
                ?? throw new InvalidOperationException("Tarea no encontrada");

            tarea.Status = StatusEnum.Deleted;
            tarea.Activa = false;
            tarea.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<TareaDocenteDto>> GetTareasByGrupoMateria(int idGrupoMateria, int profesorId)
        {
            return await _dbContext.Set<TareaDocente>()
                .Include(t => t.GrupoMateria).ThenInclude(gm => gm!.IdMateriaPlanNavigation).ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(t => t.GrupoMateria).ThenInclude(gm => gm!.IdGrupoNavigation)
                .Include(t => t.Entregas)
                .Where(t => t.IdGrupoMateria == idGrupoMateria
                         && t.IdProfesor == profesorId
                         && t.Status == StatusEnum.Active)
                .OrderByDescending(t => t.FechaCreacion)
                .Select(t => new TareaDocenteDto
                {
                    Id = t.Id,
                    IdGrupoMateria = t.IdGrupoMateria,
                    NombreMateria = t.GrupoMateria!.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                    CodigoGrupo = t.GrupoMateria.IdGrupoNavigation.CodigoGrupo ?? t.GrupoMateria.IdGrupoNavigation.NombreGrupo,
                    Titulo = t.Titulo,
                    Descripcion = t.Descripcion,
                    FechaCreacion = t.FechaCreacion,
                    FechaLimite = t.FechaLimite,
                    PuntosMaximos = t.PuntosMaximos,
                    Activa = t.Activa,
                    TotalEntregas = t.Entregas.Count(e => e.Status == StatusEnum.Active),
                    TotalPendientes = t.Entregas.Count(e => e.Status == StatusEnum.Active && !e.Revisada)
                })
                .ToListAsync();
        }

        public async Task<List<EntregaTareaDto>> GetEntregasPorTarea(int idTarea, int profesorId)
        {
            var tarea = await _dbContext.Set<TareaDocente>()
                .FirstOrDefaultAsync(t => t.Id == idTarea && t.IdProfesor == profesorId && t.Status == StatusEnum.Active)
                ?? throw new InvalidOperationException("Tarea no encontrada");

            return await _dbContext.Set<EntregaTarea>()
                .Include(e => e.Estudiante).ThenInclude(est => est!.IdPersonaNavigation)
                .Where(e => e.IdTarea == idTarea && e.Status == StatusEnum.Active)
                .OrderByDescending(e => e.FechaEntrega)
                .Select(e => new EntregaTareaDto
                {
                    Id = e.Id,
                    IdTarea = e.IdTarea,
                    IdEstudiante = e.IdEstudiante,
                    NombreAlumno = $"{e.Estudiante!.IdPersonaNavigation!.Nombre} {e.Estudiante.IdPersonaNavigation.ApellidoPaterno} {e.Estudiante.IdPersonaNavigation.ApellidoMaterno}".Trim(),
                    Matricula = e.Estudiante.Matricula,
                    NombreArchivo = e.NombreArchivo,
                    UrlArchivo = e.UrlArchivo,
                    TipoArchivo = e.TipoArchivo,
                    TamanoBytes = e.TamanoBytes,
                    FechaEntrega = e.FechaEntrega,
                    Calificacion = e.Calificacion,
                    Retroalimentacion = e.Retroalimentacion,
                    Revisada = e.Revisada
                })
                .ToListAsync();
        }

        public async Task CalificarEntrega(int idEntrega, int profesorId, decimal calificacion, string? retroalimentacion)
        {
            var entrega = await _dbContext.Set<EntregaTarea>()
                .Include(e => e.Tarea)
                .FirstOrDefaultAsync(e => e.Id == idEntrega && e.Tarea!.IdProfesor == profesorId && e.Status == StatusEnum.Active)
                ?? throw new InvalidOperationException("Entrega no encontrada");

            entrega.Calificacion = calificacion;
            entrega.Retroalimentacion = retroalimentacion;
            entrega.Revisada = true;
            entrega.FechaRevision = DateTime.UtcNow;
            entrega.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<EntregaTarea> SubirEntrega(int idTarea, int idEstudiante, IFormFile archivo)
        {
            var tarea = await _dbContext.Set<TareaDocente>()
                .FirstOrDefaultAsync(t => t.Id == idTarea && t.Activa && t.Status == StatusEnum.Active)
                ?? throw new InvalidOperationException("Tarea no encontrada o inactiva");

            if (DateTime.UtcNow > tarea.FechaLimite)
                throw new InvalidOperationException("La fecha limite para esta tarea ya paso");

            var validation = await FileValidator.ValidateAsync(archivo);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            // Check if student already submitted
            var existing = await _dbContext.Set<EntregaTarea>()
                .FirstOrDefaultAsync(e => e.IdTarea == idTarea && e.IdEstudiante == idEstudiante && e.Status == StatusEnum.Active);

            if (existing != null)
            {
                // Delete old file and replace
                try { await _blobStorageService.DeleteFile(Path.GetFileName(existing.UrlArchivo), Container); } catch { }
                existing.Status = StatusEnum.Deleted;
            }

            var extension = Path.GetExtension(archivo.FileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var blobName = $"t{idTarea}/{idEstudiante}_{timestamp}{extension}";

            var url = await _blobStorageService.UploadFile(archivo, blobName, Container);

            var entrega = new EntregaTarea
            {
                IdTarea = idTarea,
                IdEstudiante = idEstudiante,
                NombreArchivo = archivo.FileName,
                UrlArchivo = url,
                TipoArchivo = extension,
                TamanoBytes = archivo.Length,
                FechaEntrega = DateTime.UtcNow,
                Revisada = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "sistema",
                Status = StatusEnum.Active
            };

            _dbContext.Set<EntregaTarea>().Add(entrega);
            await _dbContext.SaveChangesAsync();
            return entrega;
        }

        public async Task<List<TareaAlumnoDto>> GetTareasAlumno(int idEstudiante)
        {
            // Get all groups the student is enrolled in
            var grupoMateriaIds = await _dbContext.Inscripcion
                .Where(i => i.IdEstudiante == idEstudiante && i.Status == StatusEnum.Active)
                .Select(i => i.IdGrupoMateria)
                .ToListAsync();

            return await _dbContext.Set<TareaDocente>()
                .Include(t => t.GrupoMateria).ThenInclude(gm => gm!.IdMateriaPlanNavigation).ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(t => t.GrupoMateria).ThenInclude(gm => gm!.IdGrupoNavigation)
                .Include(t => t.Profesor).ThenInclude(p => p!.IdPersonaNavigation)
                .Include(t => t.Entregas)
                .Where(t => grupoMateriaIds.Contains(t.IdGrupoMateria)
                         && t.Activa
                         && t.Status == StatusEnum.Active)
                .OrderByDescending(t => t.FechaLimite)
                .Select(t => new TareaAlumnoDto
                {
                    Id = t.Id,
                    NombreMateria = t.GrupoMateria!.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                    CodigoGrupo = t.GrupoMateria.IdGrupoNavigation.CodigoGrupo ?? t.GrupoMateria.IdGrupoNavigation.NombreGrupo,
                    Titulo = t.Titulo,
                    Descripcion = t.Descripcion,
                    FechaLimite = t.FechaLimite,
                    PuntosMaximos = t.PuntosMaximos,
                    Entregada = t.Entregas.Any(e => e.IdEstudiante == idEstudiante && e.Status == StatusEnum.Active),
                    Calificacion = t.Entregas
                        .Where(e => e.IdEstudiante == idEstudiante && e.Status == StatusEnum.Active)
                        .Select(e => e.Calificacion)
                        .FirstOrDefault(),
                    Retroalimentacion = t.Entregas
                        .Where(e => e.IdEstudiante == idEstudiante && e.Status == StatusEnum.Active)
                        .Select(e => e.Retroalimentacion)
                        .FirstOrDefault(),
                    NombreProfesor = t.Profesor != null && t.Profesor.IdPersonaNavigation != null
                        ? $"{t.Profesor.IdPersonaNavigation.Nombre} {t.Profesor.IdPersonaNavigation.ApellidoPaterno}".Trim()
                        : null
                })
                .ToListAsync();
        }

        public async Task<EntregaTarea?> GetEntregaById(int id)
        {
            return await _dbContext.Set<EntregaTarea>()
                .FirstOrDefaultAsync(e => e.Id == id && e.Status == StatusEnum.Active);
        }
    }
}
