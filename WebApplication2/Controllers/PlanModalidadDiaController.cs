using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanModalidadDiaController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public PlanModalidadDiaController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int idPlanEstudios, [FromQuery] int idModalidad)
        {
            var dias = await _dbContext.PlanModalidadDia
                .Where(d => d.IdPlanEstudios == idPlanEstudios && d.IdModalidad == idModalidad)
                .Include(d => d.IdDiaSemanaNavigation)
                .Include(d => d.IdPlanEstudiosNavigation)
                .Include(d => d.IdModalidadNavigation)
                .OrderBy(d => d.Grupo)
                .ThenBy(d => d.IdDiaSemana)
                .Select(d => new
                {
                    d.IdPlanModalidadDia,
                    d.IdPlanEstudios,
                    NombrePlan = d.IdPlanEstudiosNavigation.NombrePlanEstudios,
                    d.IdModalidad,
                    NombreModalidad = d.IdModalidadNavigation.DescModalidad,
                    d.Grupo,
                    d.IdDiaSemana,
                    NombreDia = d.IdDiaSemanaNavigation.Nombre
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(dias);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var dias = await _dbContext.PlanModalidadDia
                .Include(d => d.IdDiaSemanaNavigation)
                .Include(d => d.IdPlanEstudiosNavigation)
                .Include(d => d.IdModalidadNavigation)
                .OrderBy(d => d.IdPlanEstudios)
                .ThenBy(d => d.IdModalidad)
                .ThenBy(d => d.Grupo)
                .ThenBy(d => d.IdDiaSemana)
                .Select(d => new
                {
                    d.IdPlanModalidadDia,
                    d.IdPlanEstudios,
                    NombrePlan = d.IdPlanEstudiosNavigation.NombrePlanEstudios,
                    d.IdModalidad,
                    NombreModalidad = d.IdModalidadNavigation.DescModalidad,
                    d.Grupo,
                    d.IdDiaSemana,
                    NombreDia = d.IdDiaSemanaNavigation.Nombre
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(dias);
        }

        /// <summary>
        /// Crea o actualiza los días de un grupo específico para plan+modalidad.
        /// Si grupo = 0 o no se envía, se crea un grupo nuevo automáticamente.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Upsert([FromBody] UpsertPlanModalidadDiaRequest request)
        {
            if (request.DiasIds.Count == 0)
                return BadRequest(new { mensaje = "Debe seleccionar al menos un día" });

            int grupo = request.Grupo;

            if (grupo <= 0)
            {
                // Obtener el máximo grupo existente para esta combinación y crear uno nuevo
                var maxGrupo = await _dbContext.PlanModalidadDia
                    .Where(d => d.IdPlanEstudios == request.IdPlanEstudios && d.IdModalidad == request.IdModalidad)
                    .Select(d => (int?)d.Grupo)
                    .MaxAsync() ?? 0;

                grupo = maxGrupo + 1;
            }
            else
            {
                // Eliminar días existentes de este grupo específico
                var existentes = await _dbContext.PlanModalidadDia
                    .Where(d => d.IdPlanEstudios == request.IdPlanEstudios
                        && d.IdModalidad == request.IdModalidad
                        && d.Grupo == grupo)
                    .ToListAsync();

                _dbContext.PlanModalidadDia.RemoveRange(existentes);
            }

            var nuevos = request.DiasIds.Select(diaId => new PlanModalidadDia
            {
                IdPlanEstudios = request.IdPlanEstudios,
                IdModalidad = request.IdModalidad,
                Grupo = grupo,
                IdDiaSemana = (byte)diaId
            }).ToList();

            await _dbContext.PlanModalidadDia.AddRangeAsync(nuevos);
            await _dbContext.SaveChangesAsync();

            return Ok(new { mensaje = "Días actualizados correctamente", grupo });
        }

        /// <summary>
        /// Elimina un grupo completo de días para plan+modalidad.
        /// </summary>
        [HttpDelete("{idPlanEstudios}/{idModalidad}/{grupo}")]
        public async Task<IActionResult> EliminarGrupo(int idPlanEstudios, int idModalidad, int grupo)
        {
            var existentes = await _dbContext.PlanModalidadDia
                .Where(d => d.IdPlanEstudios == idPlanEstudios
                    && d.IdModalidad == idModalidad
                    && d.Grupo == grupo)
                .ToListAsync();

            if (existentes.Count == 0)
                return NotFound(new { mensaje = "Grupo no encontrado" });

            _dbContext.PlanModalidadDia.RemoveRange(existentes);
            await _dbContext.SaveChangesAsync();

            return Ok(new { mensaje = "Grupo eliminado correctamente" });
        }
    }

    public class UpsertPlanModalidadDiaRequest
    {
        public int IdPlanEstudios { get; set; }
        public int IdModalidad { get; set; }
        public int Grupo { get; set; }
        public List<int> DiasIds { get; set; } = new();
    }
}
