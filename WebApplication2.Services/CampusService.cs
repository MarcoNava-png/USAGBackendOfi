using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class CampusService : ICampusService
    {
        private readonly ApplicationDbContext _dbContext;

        public CampusService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<Campus>> GetCampuses(int page, int pageSize)
        {
            var query = _dbContext.Campus
                .Include(c => c.IdDireccionNavigation)
                    .ThenInclude(dn => dn.CodigoPostal)
                        .ThenInclude(cp => cp.Municipio)
                            .ThenInclude(m => m.Estado)
                .Where(c => c.Activo && c.Status == Core.Enums.StatusEnum.Active);

            var totalItems = await query.CountAsync();

            var campuses = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Campus>
            {
                TotalItems = totalItems,
                Items = campuses,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<Campus?> GetCampusById(int id)
        {
            return await _dbContext.Campus
                .Include(c => c.IdDireccionNavigation)
                    .ThenInclude(d => d.CodigoPostal)
                        .ThenInclude(cp => cp.Municipio)
                            .ThenInclude(m => m.Estado)
                .FirstOrDefaultAsync(c => c.IdCampus == id && c.Status == Core.Enums.StatusEnum.Active);
        }

        public async Task<Campus> CrearCampus(Campus campus)
        {
            if (campus.IdDireccionNavigation != null)
            {
                await _dbContext.Direccion.AddAsync(campus.IdDireccionNavigation);
                await _dbContext.SaveChangesAsync();

                campus.IdDireccion = campus.IdDireccionNavigation.IdDireccion;
            }

            campus.Activo = true;
            campus.Status = Core.Enums.StatusEnum.Active;

            await _dbContext.Campus.AddAsync(campus);
            await _dbContext.SaveChangesAsync();

            return await _dbContext.Campus
                .Include(c => c.IdDireccionNavigation)
                    .ThenInclude(d => d.CodigoPostal)
                        .ThenInclude(cp => cp.Municipio)
                            .ThenInclude(m => m.Estado)
                .FirstAsync(c => c.IdCampus == campus.IdCampus);
        }

        public async Task<Campus> ActualizarCampus(Campus newCampus)
        {
            var campus = await _dbContext.Campus
                .SingleOrDefaultAsync(p => p.IdCampus == newCampus.IdCampus);

            if (campus == null)
            {
                throw new Exception("No existe Campus con el id ingresado");
            }

            if (newCampus.IdDireccionNavigation != null)
            {
                var direccion = await _dbContext.Direccion.SingleOrDefaultAsync(d => d.IdDireccion == campus.IdDireccion);

                if (direccion != null)
                {
                    direccion.Calle = newCampus.IdDireccionNavigation.Calle;
                    direccion.NumeroExterior = newCampus.IdDireccionNavigation.NumeroExterior;
                    direccion.NumeroInterior = newCampus.IdDireccionNavigation.NumeroInterior;
                    direccion.CodigoPostalId = newCampus.IdDireccionNavigation.CodigoPostalId;

                    _dbContext.Direccion.Update(direccion);
                }
            }

            campus.ClaveCampus = newCampus.ClaveCampus;
            campus.Nombre = newCampus.Nombre;

            campus.Activo = newCampus.Activo;
            campus.Status = newCampus.Status;

            _dbContext.Campus.Update(campus);

            await _dbContext.SaveChangesAsync();

            return campus;
        }

        public async Task<bool> EliminarCampus(int id)
        {
            var campus = await _dbContext.Campus
                .FirstOrDefaultAsync(c => c.IdCampus == id);

            if (campus == null)
            {
                throw new Exception("No existe Campus con el id ingresado");
            }

            var planesActivos = await _dbContext.PlanEstudios
                .CountAsync(p => p.IdCampus == id && p.Status == Core.Enums.StatusEnum.Active);

            if (planesActivos > 0)
            {
                throw new Exception($"No se puede eliminar el campus porque tiene {planesActivos} plan(es) de estudio activo(s). Primero debe eliminar los planes.");
            }

            campus.Activo = false;
            campus.Status = Core.Enums.StatusEnum.Deleted;
            campus.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
