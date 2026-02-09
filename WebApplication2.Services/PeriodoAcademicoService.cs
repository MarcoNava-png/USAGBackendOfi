using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class PeriodoAcademicoService : IPeriodoAcademicoService
    {
        private readonly ApplicationDbContext _dbContext;

        public PeriodoAcademicoService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<PeriodoAcademico>> GetPeriodosAcademicos(int page, int pageSize)
        {
            var totalItems = await _dbContext.PeriodoAcademico
                .AsNoTracking()
                .Where(p => p.Status == Core.Enums.StatusEnum.Active)
                .Include(p => p.IdPeriodicidadNavigation)
                .CountAsync();

            var periodosAcademicos = await _dbContext.PeriodoAcademico
                .AsNoTracking()
                .Where(p => p.Status == Core.Enums.StatusEnum.Active)
                .Include(p => p.IdPeriodicidadNavigation)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<PeriodoAcademico>
            {
                TotalItems = totalItems,
                Items = periodosAcademicos,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<PeriodoAcademico> CrearPeriodoAcademico(PeriodoAcademico periodoAcademico)
        {
            await _dbContext.PeriodoAcademico.AddAsync(periodoAcademico);
            await _dbContext.SaveChangesAsync();

            return periodoAcademico;
        }

        public async Task<PeriodoAcademico> ActualizarPeriodoAcademico(PeriodoAcademico newPeriodoAcademico)
        {
            var periodoAcademico = await _dbContext.PeriodoAcademico
                .FirstOrDefaultAsync(pe => pe.IdPeriodoAcademico == newPeriodoAcademico.IdPeriodoAcademico);

            if (periodoAcademico == null)
            {
                throw new Exception("No existe el periodo academico con el id ingresado");
            }

            periodoAcademico.Clave = newPeriodoAcademico.Clave;
            periodoAcademico.Nombre = newPeriodoAcademico.Nombre;
            periodoAcademico.IdPeriodicidad = newPeriodoAcademico.IdPeriodicidad;
            periodoAcademico.FechaInicio = newPeriodoAcademico.FechaInicio;
            periodoAcademico.FechaFin = newPeriodoAcademico.FechaFin;
            periodoAcademico.Status = newPeriodoAcademico.Status;

            _dbContext.PeriodoAcademico.Update(periodoAcademico);

            await _dbContext.SaveChangesAsync();

            return periodoAcademico;
        }

        public async Task<PeriodoAcademico?> GetPeriodoActualAsync()
        {
            return await _dbContext.PeriodoAcademico
                .Include(p => p.IdPeriodicidadNavigation)
                .FirstOrDefaultAsync(p => p.EsPeriodoActual
                    && p.Status == Core.Enums.StatusEnum.Active);
        }

        public async Task<PeriodoAcademico> MarcarComoPeriodoActualAsync(int idPeriodoAcademico)
        {
            var periodoNuevo = await _dbContext.PeriodoAcademico
                .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == idPeriodoAcademico);

            if (periodoNuevo == null)
                throw new InvalidOperationException($"No se encontro el periodo academico con ID {idPeriodoAcademico}");

            var periodosActuales = await _dbContext.PeriodoAcademico
                .Where(p => p.EsPeriodoActual)
                .ToListAsync();

            foreach (var periodo in periodosActuales)
            {
                periodo.EsPeriodoActual = false;
            }

            periodoNuevo.EsPeriodoActual = true;

            await _dbContext.SaveChangesAsync();

            return periodoNuevo;
        }

        public bool EsPeriodoActivoPorFechas(PeriodoAcademico periodo, DateOnly? fechaReferencia = null)
        {
            var fecha = fechaReferencia ?? DateOnly.FromDateTime(DateTime.UtcNow);
            return fecha >= periodo.FechaInicio && fecha <= periodo.FechaFin;
        }

        public async Task EliminarPeriodoAcademicoAsync(int idPeriodoAcademico)
        {
            var periodo = await _dbContext.PeriodoAcademico
                .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == idPeriodoAcademico);

            if (periodo == null)
                throw new InvalidOperationException($"No se encontro el periodo academico con ID {idPeriodoAcademico}");

            if (periodo.EsPeriodoActual)
                throw new InvalidOperationException("No se puede eliminar el periodo academico actual. Primero seleccione otro periodo como actual.");

            periodo.Status = Core.Enums.StatusEnum.Disabled;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<PeriodoAcademico?> GetPeriodoAcademicoPorIdAsync(int idPeriodoAcademico)
        {
            return await _dbContext.PeriodoAcademico
                .Include(p => p.IdPeriodicidadNavigation)
                .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == idPeriodoAcademico
                    && p.Status == Core.Enums.StatusEnum.Active);
        }
    }
}
