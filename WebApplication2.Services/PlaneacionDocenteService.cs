using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.DocentePortal;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class PlaneacionDocenteService : IPlaneacionDocenteService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBlobStorageService _blobStorageService;
        private const string Container = "planeaciones";

        public PlaneacionDocenteService(ApplicationDbContext dbContext, IBlobStorageService blobStorageService)
        {
            _dbContext = dbContext;
            _blobStorageService = blobStorageService;
        }

        public async Task<List<PlaneacionDocenteDto>> GetByGrupoMateria(int idGrupoMateria, int profesorId)
        {
            return await _dbContext.Set<PlaneacionDocente>()
                .Where(p => p.IdGrupoMateria == idGrupoMateria
                         && p.IdProfesor == profesorId
                         && p.Status == StatusEnum.Active)
                .OrderByDescending(p => p.FechaSubida)
                .Select(p => new PlaneacionDocenteDto
                {
                    Id = p.Id,
                    IdGrupoMateria = p.IdGrupoMateria,
                    NombreArchivo = p.NombreArchivo,
                    UrlArchivo = p.UrlArchivo,
                    Descripcion = p.Descripcion,
                    TipoArchivo = p.TipoArchivo,
                    TamanoBytes = p.TamanoBytes,
                    FechaSubida = p.FechaSubida
                })
                .ToListAsync();
        }

        public async Task<PlaneacionDocente> Upload(int profesorId, int idGrupoMateria, IFormFile file, string? descripcion)
        {
            var validation = await FileValidator.ValidateAsync(file);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            var extension = Path.GetExtension(file.FileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var blobName = $"{profesorId}/{idGrupoMateria}_{timestamp}{extension}";

            var url = await _blobStorageService.UploadFile(file, blobName, Container);

            var entity = new PlaneacionDocente
            {
                IdProfesor = profesorId,
                IdGrupoMateria = idGrupoMateria,
                NombreArchivo = file.FileName,
                UrlArchivo = url,
                Descripcion = descripcion,
                TipoArchivo = extension,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "sistema",
                Status = StatusEnum.Active
            };

            _dbContext.Set<PlaneacionDocente>().Add(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task Delete(int id, int profesorId)
        {
            var entity = await _dbContext.Set<PlaneacionDocente>()
                .FirstOrDefaultAsync(p => p.Id == id && p.IdProfesor == profesorId && p.Status == StatusEnum.Active);

            if (entity == null)
                throw new InvalidOperationException("Planeación no encontrada");

            // Delete the file
            var blobName = entity.UrlArchivo.Split('/').Last();
            var containerPath = $"{profesorId}";
            try
            {
                await _blobStorageService.DeleteFile($"{containerPath}/{blobName}", Container);
            }
            catch
            {
                // File may not exist, continue with soft delete
            }

            entity.Status = StatusEnum.Deleted;
            entity.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<PlaneacionDocente?> GetById(int id)
        {
            return await _dbContext.Set<PlaneacionDocente>()
                .FirstOrDefaultAsync(p => p.Id == id && p.Status == StatusEnum.Active);
        }
    }
}
