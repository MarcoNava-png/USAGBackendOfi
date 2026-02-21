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
                .OrderBy(d => d.IdDiaSemana)
                .Select(d => new
                {
                    d.IdPlanModalidadDia,
                    d.IdPlanEstudios,
                    NombrePlan = d.IdPlanEstudiosNavigation.NombrePlanEstudios,
                    d.IdModalidad,
                    NombreModalidad = d.IdModalidadNavigation.DescModalidad,
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
                .ThenBy(d => d.IdDiaSemana)
                .Select(d => new
                {
                    d.IdPlanModalidadDia,
                    d.IdPlanEstudios,
                    NombrePlan = d.IdPlanEstudiosNavigation.NombrePlanEstudios,
                    d.IdModalidad,
                    NombreModalidad = d.IdModalidadNavigation.DescModalidad,
                    d.IdDiaSemana,
                    NombreDia = d.IdDiaSemanaNavigation.Nombre
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(dias);
        }

        [HttpPost]
        public async Task<IActionResult> Upsert([FromBody] UpsertPlanModalidadDiaRequest request)
        {
            var existentes = await _dbContext.PlanModalidadDia
                .Where(d => d.IdPlanEstudios == request.IdPlanEstudios && d.IdModalidad == request.IdModalidad)
                .ToListAsync();

            _dbContext.PlanModalidadDia.RemoveRange(existentes);

            var nuevos = request.DiasIds.Select(diaId => new PlanModalidadDia
            {
                IdPlanEstudios = request.IdPlanEstudios,
                IdModalidad = request.IdModalidad,
                IdDiaSemana = (byte)diaId
            }).ToList();

            await _dbContext.PlanModalidadDia.AddRangeAsync(nuevos);
            await _dbContext.SaveChangesAsync();

            return Ok(new { mensaje = "Días actualizados correctamente" });
        }
    }

    public class UpsertPlanModalidadDiaRequest
    {
        public int IdPlanEstudios { get; set; }
        public int IdModalidad { get; set; }
        public List<int> DiasIds { get; set; } = new();
    }
}
