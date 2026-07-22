using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.ConfiguracionCalificaciones;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public interface IConfiguracionCalificacionesService
    {
        Task<ConfiguracionCalificacionesDto> ObtenerAsync(CancellationToken ct = default);
        Task<ConfiguracionCalificacionesDto> GuardarAsync(ConfiguracionCalificacionesDto dto, CancellationToken ct = default);
        Task<List<ParcialDto>> ListarParcialesAsync(CancellationToken ct = default);
        Task<ParcialDto> GuardarParcialAsync(ParcialDto dto, CancellationToken ct = default);
        Task<bool> EliminarParcialAsync(int id, CancellationToken ct = default);
    }

    public class ConfiguracionCalificacionesService : IConfiguracionCalificacionesService
    {
        private readonly ApplicationDbContext _db;

        public ConfiguracionCalificacionesService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ConfiguracionCalificacionesDto> ObtenerAsync(CancellationToken ct = default)
        {
            var entidad = await _db.ConfiguracionCalificaciones
                .Where(c => c.Status != StatusEnum.Deleted)
                .OrderBy(c => c.IdConfiguracionCalificaciones)
                .FirstOrDefaultAsync(ct);

            if (entidad == null)
            {
                return new ConfiguracionCalificacionesDto();
            }

            return Mapear(entidad);
        }

        public async Task<ConfiguracionCalificacionesDto> GuardarAsync(ConfiguracionCalificacionesDto dto, CancellationToken ct = default)
        {
            var entidad = await _db.ConfiguracionCalificaciones
                .Where(c => c.Status != StatusEnum.Deleted)
                .OrderBy(c => c.IdConfiguracionCalificaciones)
                .FirstOrDefaultAsync(ct);

            if (entidad == null)
            {
                entidad = new ConfiguracionCalificaciones
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Sistema",
                    Status = StatusEnum.Active,
                };
                _db.ConfiguracionCalificaciones.Add(entidad);
            }
            else
            {
                entidad.UpdatedAt = DateTime.UtcNow;
            }

            entidad.EscalaMaxima = dto.EscalaMaxima;
            entidad.CalificacionMinimaAprobatoria = dto.CalificacionMinimaAprobatoria;
            entidad.Decimales = dto.Decimales;
            entidad.RedondearAlEntero = dto.RedondearAlEntero;

            await _db.SaveChangesAsync(ct);

            return Mapear(entidad);
        }

        public async Task<List<ParcialDto>> ListarParcialesAsync(CancellationToken ct = default)
        {
            return await _db.Parciales
                .Where(p => p.Status != StatusEnum.Deleted)
                .OrderBy(p => p.Orden)
                .Select(p => new ParcialDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Orden = p.Orden,
                })
                .ToListAsync(ct);
        }

        public async Task<ParcialDto> GuardarParcialAsync(ParcialDto dto, CancellationToken ct = default)
        {
            Parciales entidad;

            if (dto.Id > 0)
            {
                entidad = await _db.Parciales
                    .FirstOrDefaultAsync(p => p.Id == dto.Id && p.Status != StatusEnum.Deleted, ct)
                    ?? throw new InvalidOperationException("El parcial no existe");

                entidad.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                entidad = new Parciales
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Sistema",
                    Status = StatusEnum.Active,
                };
                _db.Parciales.Add(entidad);
            }

            entidad.Name = dto.Name;
            entidad.Orden = dto.Orden;

            await _db.SaveChangesAsync(ct);

            return new ParcialDto
            {
                Id = entidad.Id,
                Name = entidad.Name,
                Orden = entidad.Orden,
            };
        }

        public async Task<bool> EliminarParcialAsync(int id, CancellationToken ct = default)
        {
            var entidad = await _db.Parciales
                .FirstOrDefaultAsync(p => p.Id == id && p.Status != StatusEnum.Deleted, ct);

            if (entidad == null)
            {
                return false;
            }

            entidad.Status = StatusEnum.Deleted;
            entidad.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return true;
        }

        private static ConfiguracionCalificacionesDto Mapear(ConfiguracionCalificaciones entidad)
        {
            return new ConfiguracionCalificacionesDto
            {
                IdConfiguracionCalificaciones = entidad.IdConfiguracionCalificaciones,
                EscalaMaxima = entidad.EscalaMaxima,
                CalificacionMinimaAprobatoria = entidad.CalificacionMinimaAprobatoria,
                Decimales = entidad.Decimales,
                RedondearAlEntero = entidad.RedondearAlEntero,
            };
        }
    }
}
