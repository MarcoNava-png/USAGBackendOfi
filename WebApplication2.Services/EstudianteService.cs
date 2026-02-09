using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class EstudianteService : IEstudianteService
    {
        private readonly ApplicationDbContext _dbContext;

        public EstudianteService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<Estudiante>> GetEstudiantes(int page, int pageSize)
        {
            var totalItems = await _dbContext.Estudiante
                .Include(d => d.IdPersonaNavigation)
                .Include(d => d.IdPlanActualNavigation)
                .Where(d => d.Status == Core.Enums.StatusEnum.Active)
                .CountAsync();

            var items = await _dbContext.Estudiante
                .Include(d => d.IdPersonaNavigation)
                .Include(d => d.IdPlanActualNavigation)
                .Where(d => d.Status == Core.Enums.StatusEnum.Active)
                .OrderBy(d => d.IdPersonaNavigation.ApellidoPaterno)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Estudiante>
            {
                TotalItems = totalItems,
                Items = items,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<Estudiante> GetEstudianteDetalle(int id)
        {
            var totalItems = await _dbContext.Estudiante
                .Where(d => d.Status == Core.Enums.StatusEnum.Active)
                .CountAsync();

            var estudiante = await _dbContext.Estudiante
                .Include(d => d.IdPersonaNavigation)
                .Include(d => d.Inscripcion)
                .ThenInclude(i => i.IdGrupoMateriaNavigation)
                .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                .ThenInclude(mp => mp.IdMateriaNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == id && e.Status == Core.Enums.StatusEnum.Active);

            if (estudiante == null)
            {
                throw new Exception("No se ha encontrado estudiante con el id ingreado.");
            }

            return estudiante;
        }

        public async Task<Estudiante?> GetEstudianteByMatricula(string matricula)
        {
            return await _dbContext.Estudiante
                .Include(d => d.IdPersonaNavigation)
                .Include(d => d.IdPlanActualNavigation)
                .FirstOrDefaultAsync(e => e.Matricula == matricula && e.Status == Core.Enums.StatusEnum.Active);
        }

        public async Task<Estudiante> CrearEstudiante(Estudiante estudiante)
        {
            await _dbContext.Estudiante.AddAsync(estudiante);
            await _dbContext.SaveChangesAsync();

            return estudiante;
        }

        public async Task<Estudiante> ActualizarEstudiante(Estudiante newEstudiante)
        {
            var estudiante = await _dbContext.Estudiante
                .FirstOrDefaultAsync(e => e.IdEstudiante == newEstudiante.IdEstudiante);

            if (estudiante == null)
            {
                throw new Exception("No existe estudiante con el id ingresado");
            }

            estudiante.Matricula = newEstudiante.Matricula;
            estudiante.UsuarioId = newEstudiante.UsuarioId;
            estudiante.Email = newEstudiante.Email;

            _dbContext.Estudiante.Update(estudiante);

            await _dbContext.SaveChangesAsync();

            return estudiante;
        }

        public async Task<(bool TienePendientes, int CantidadPendientes, decimal MontoPendiente)> ValidarPagosPendientesAsync(
            int idEstudiante,
            int? idPeriodoAcademico = null)
        {
            var query = _dbContext.Recibo
                .Where(r => r.IdEstudiante == idEstudiante
                         && r.Status == Core.Enums.StatusEnum.Active
                         && r.Estatus != Core.Enums.EstatusRecibo.PAGADO
                         && r.Estatus != Core.Enums.EstatusRecibo.CANCELADO
                         && r.Estatus != Core.Enums.EstatusRecibo.BONIFICADO);

            if (idPeriodoAcademico.HasValue)
            {
                query = query.Where(r => r.IdPeriodoAcademico == idPeriodoAcademico.Value);
            }

            var recibosPendientes = await query.ToListAsync();

            var cantidadPendientes = recibosPendientes.Count;
            var montoPendiente = recibosPendientes.Sum(r => r.Saldo);

            return (cantidadPendientes > 0, cantidadPendientes, montoPendiente);
        }

        public async Task<bool> PuedeInscribirseAsync(int idEstudiante, int? idPeriodoAcademico = null)
        {
            var (tienePendientes, _, _) = await ValidarPagosPendientesAsync(idEstudiante, idPeriodoAcademico);
            return !tienePendientes;
        }

        public async Task<PagedResult<Estudiante>> GetEstudiantesSinGrupo(int idPlanEstudios, int idPeriodoAcademico, int page, int pageSize)
        {
            var gruposDelPeriodo = await _dbContext.Grupo
                .Where(g => g.IdPlanEstudios == idPlanEstudios
                         && g.IdPeriodoAcademico == idPeriodoAcademico
                         && g.Status == Core.Enums.StatusEnum.Active)
                .Select(g => g.IdGrupo)
                .ToListAsync();

            var estudiantesConInscripcion = await _dbContext.Inscripcion
                .Where(i => i.Status == Core.Enums.StatusEnum.Active
                         && i.IdGrupoMateriaNavigation != null
                         && gruposDelPeriodo.Contains(i.IdGrupoMateriaNavigation.IdGrupo))
                .Select(i => i.IdEstudiante)
                .Distinct()
                .ToListAsync();

            var estudiantesEnGrupoDirecto = await _dbContext.EstudianteGrupo
                .Where(eg => eg.Status == Core.Enums.StatusEnum.Active
                          && gruposDelPeriodo.Contains(eg.IdGrupo))
                .Select(eg => eg.IdEstudiante)
                .Distinct()
                .ToListAsync();

            var estudiantesYaInscritos = estudiantesConInscripcion
                .Union(estudiantesEnGrupoDirecto)
                .Distinct()
                .ToHashSet();

            var query = _dbContext.Estudiante
                .Include(d => d.IdPersonaNavigation)
                .Include(d => d.IdPlanActualNavigation)
                .Where(d => d.Status == Core.Enums.StatusEnum.Active
                         && d.Activo
                         && (d.IdPlanActual == idPlanEstudios || d.IdPlanActual == null)
                         && !estudiantesYaInscritos.Contains(d.IdEstudiante));

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(d => d.IdPersonaNavigation.ApellidoPaterno)
                .ThenBy(d => d.IdPersonaNavigation.ApellidoMaterno)
                .ThenBy(d => d.IdPersonaNavigation.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Estudiante>
            {
                TotalItems = totalItems,
                Items = items,
                PageNumber = page,
                PageSize = pageSize
            };
        }
    }
}
