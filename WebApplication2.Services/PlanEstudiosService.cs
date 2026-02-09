using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class PlanEstudiosService : IPlanEstudioService
    {
        private readonly ApplicationDbContext _dbContext;

        public PlanEstudiosService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<PlanEstudios>> GetPlanesEstudios(int page, int pageSize, int? idCampus = null, bool incluirInactivos = false)
        {
            var query = _dbContext.PlanEstudios
                .Include(p => p.IdCampusNavigation)
                .Include(p => p.IdPeriodicidadNavigation)
                .AsQueryable();

            if (!incluirInactivos)
            {
                query = query.Where(p => p.Status == Core.Enums.StatusEnum.Active);
            }
            else
            {
                query = query.Where(p => p.Status != Core.Enums.StatusEnum.Deleted);
            }

            if (idCampus.HasValue)
            {
                query = query.Where(p => p.IdCampus == idCampus.Value);
            }

            var totalItems = await query.CountAsync();

            var planes = await query
                .OrderBy(p => p.NombrePlanEstudios)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<PlanEstudios>
            {
                TotalItems = totalItems,
                Items = planes,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<PlanEstudios?> GetPlanEstudiosById(int id)
        {
            return await _dbContext.PlanEstudios
                .Include(p => p.IdCampusNavigation)
                .Include(p => p.IdPeriodicidadNavigation)
                .Include(p => p.IdNivelEducativoNavigation)
                .FirstOrDefaultAsync(p => p.IdPlanEstudios == id && p.Status == Core.Enums.StatusEnum.Active);
        }

        public async Task<PlanEstudios> CrearPlanEstudios(PlanEstudios planEstudios)
        {
            var campusExists = await _dbContext.Campus.AnyAsync(c => c.IdCampus == planEstudios.IdCampus && c.Status == Core.Enums.StatusEnum.Active);
            if (!campusExists)
            {
                throw new Exception("El campus seleccionado no existe o no está activo");
            }

            var periodicidadExists = await _dbContext.Periodicidad.AnyAsync(p => p.IdPeriodicidad == planEstudios.IdPeriodicidad);
            if (!periodicidadExists)
            {
                throw new Exception("La periodicidad seleccionada no existe");
            }

            var nivelExists = await _dbContext.NivelEducativo.AnyAsync(n => n.IdNivelEducativo == planEstudios.IdNivelEducativo);
            if (!nivelExists)
            {
                throw new Exception("El nivel educativo seleccionado no existe");
            }

            planEstudios.Status = Core.Enums.StatusEnum.Active;

            await _dbContext.PlanEstudios.AddAsync(planEstudios);
            await _dbContext.SaveChangesAsync();

            return await _dbContext.PlanEstudios
                .Include(p => p.IdCampusNavigation)
                .Include(p => p.IdPeriodicidadNavigation)
                .Include(p => p.IdNivelEducativoNavigation)
                .FirstAsync(p => p.IdPlanEstudios == planEstudios.IdPlanEstudios);
        }

        public async Task<PlanEstudios> ActualizarPlanEstudios(PlanEstudios newPlanEstudios)
        {
            var planEstudios = await _dbContext.PlanEstudios
                .FirstOrDefaultAsync(pe => pe.IdPlanEstudios == newPlanEstudios.IdPlanEstudios);

            if (planEstudios == null)
            {
                throw new Exception("No existe el plan de estudios con el id ingresado");
            }

            planEstudios.ClavePlanEstudios = newPlanEstudios.ClavePlanEstudios;
            planEstudios.NombrePlanEstudios = newPlanEstudios.NombrePlanEstudios;
            planEstudios.RVOE = newPlanEstudios.RVOE;
            planEstudios.PermiteAdelantar = newPlanEstudios.PermiteAdelantar;
            planEstudios.Version = newPlanEstudios.Version;
            planEstudios.DuracionMeses = newPlanEstudios.DuracionMeses;
            planEstudios.MinimaAprobatoriaParcial = newPlanEstudios.MinimaAprobatoriaParcial;
            planEstudios.MinimaAprobatoriaFinal = newPlanEstudios.MinimaAprobatoriaFinal;
            planEstudios.IdPeriodicidad = newPlanEstudios.IdPeriodicidad;
            planEstudios.IdNivelEducativo = newPlanEstudios.IdNivelEducativo;
            planEstudios.IdCampus = newPlanEstudios.IdCampus;
            planEstudios.Status = newPlanEstudios.Status;

            _dbContext.PlanEstudios.Update(planEstudios);

            await _dbContext.SaveChangesAsync();

            return planEstudios;
        }

        public async Task<bool> EliminarPlanEstudios(int id)
        {
            var planEstudios = await _dbContext.PlanEstudios
                .FirstOrDefaultAsync(p => p.IdPlanEstudios == id);

            if (planEstudios == null)
            {
                throw new Exception("No existe el plan de estudios con el id ingresado");
            }

            var estudiantesActivos = await _dbContext.Estudiante
                .CountAsync(e => e.IdPlanActual == id && e.Activo);

            if (estudiantesActivos > 0)
            {
                throw new Exception($"No se puede eliminar el plan de estudios porque tiene {estudiantesActivos} estudiante(s) con este plan activo.");
            }

            var gruposActivos = await _dbContext.Grupo
                .CountAsync(g => g.IdPlanEstudios == id && g.Status == Core.Enums.StatusEnum.Active);

            if (gruposActivos > 0)
            {
                throw new Exception($"No se puede eliminar el plan de estudios porque tiene {gruposActivos} grupo(s) activo(s). Elimine primero los grupos.");
            }

            var aspirantesActivos = await _dbContext.Aspirante
                .CountAsync(a => a.IdPlan == id && a.Status == Core.Enums.StatusEnum.Active);

            if (aspirantesActivos > 0)
            {
                throw new Exception($"No se puede eliminar el plan de estudios porque tiene {aspirantesActivos} aspirante(s) asociado(s).");
            }

            var conveniosActivos = await _dbContext.ConvenioAlcance
                .CountAsync(c => c.IdPlanEstudios == id);

            if (conveniosActivos > 0)
            {
                throw new Exception($"No se puede eliminar el plan de estudios porque tiene {conveniosActivos} convenio(s) asociado(s).");
            }

            var materiasPlan = await _dbContext.MateriaPlan
                .Where(mp => mp.IdPlanEstudios == id)
                .ToListAsync();

            foreach (var mp in materiasPlan)
            {
                mp.Status = Core.Enums.StatusEnum.Deleted;
                mp.UpdatedAt = DateTime.UtcNow;
            }

            var estudiantePlanes = await _dbContext.EstudiantePlan
                .Where(ep => ep.IdPlanEstudios == id)
                .ToListAsync();

            foreach (var ep in estudiantePlanes)
            {
                ep.Status = Core.Enums.StatusEnum.Deleted;
                ep.UpdatedAt = DateTime.UtcNow;
            }

            planEstudios.Status = Core.Enums.StatusEnum.Deleted;
            planEstudios.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<PlanEstudios> ToggleEstado(int id)
        {
            var planEstudios = await _dbContext.PlanEstudios
                .Include(p => p.IdCampusNavigation)
                .Include(p => p.IdPeriodicidadNavigation)
                .FirstOrDefaultAsync(p => p.IdPlanEstudios == id);

            if (planEstudios == null)
            {
                throw new Exception("No existe el plan de estudios con el id ingresado");
            }

            planEstudios.Status = planEstudios.Status == Core.Enums.StatusEnum.Active
                ? Core.Enums.StatusEnum.Disabled
                : Core.Enums.StatusEnum.Active;
            planEstudios.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return planEstudios;
        }
    }
}
