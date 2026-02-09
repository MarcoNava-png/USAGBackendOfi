using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Catalogos;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogosController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public CatalogosController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("generos")]
        public async Task<ActionResult<IEnumerable<Genero>>> GetGeneros()
        {
            var generos = await _dbContext.Genero.AsNoTracking().ToListAsync();

            return Ok(generos);
        }

        [HttpGet("horarios")]
        public async Task<ActionResult<IEnumerable<Horario>>> GetHorarios()
        {
            var horarios = await _dbContext.Turno.AsNoTracking().ToListAsync();

            return Ok(horarios);
        }

        [HttpGet("turnos")]
        public async Task<ActionResult<IEnumerable<Turno>>> GetTurnos()
        {
            var turnos = await _dbContext.Turno.AsNoTracking().ToListAsync();

            return Ok(turnos);
        }

        [HttpGet("dias-semana")]
        public async Task<ActionResult<IEnumerable<DiaSemana>>> GetDiasSemana()
        {
            var diasSemana = await _dbContext.DiaSemana.AsNoTracking().ToListAsync();

            return Ok(diasSemana);
        }

        [HttpGet("estado-civil")]
        public async Task<ActionResult<IEnumerable<EstadoCivil>>> GetEstadoCivil()
        {
            var estadoCivil = await _dbContext.EstadoCivil.AsNoTracking().ToListAsync();

            return Ok(estadoCivil);
        }

        [HttpGet("aspirante-status")]
        public async Task<ActionResult<IEnumerable<AspiranteEstatus>>> GetAspiranteStatus()
        {
            var aspiranteEstatus = await _dbContext.AspiranteEstatus
                .Where(a => a.Status == StatusEnum.Active)
                .AsNoTracking()
                .ToListAsync();

            return Ok(aspiranteEstatus);
        }

        [HttpGet("medios-contacto")]
        public async Task<ActionResult<IEnumerable<MedioContacto>>> GetMediosContacto()
        {
            var mediosContacto = await _dbContext.MedioContacto
                .Where(mc => mc.Status == StatusEnum.Active)
                .AsNoTracking()
                .ToListAsync();

            return Ok(mediosContacto);
        }

        [HttpGet("user-roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetRoles()
        {
            var roles = await _dbContext.Roles.AsNoTracking().Select(r => r.Name).ToListAsync();

            return Ok(roles);
        }

        [HttpGet("niveles-educativos")]
        public async Task<ActionResult<IEnumerable<NivelEducativo>>> GetNivelesEducativos()
        {
            var nivelesEducativos = await _dbContext.NivelEducativo
                .AsNoTracking()
                .Where(ne => ne.Activo)
                .ToListAsync();

            return Ok(nivelesEducativos);
        }

        [HttpGet("periodicidad")]
        public async Task<ActionResult<IEnumerable<Periodicidad>>> GetPeriodicidad()
        {
            var periodicidades = await _dbContext.Periodicidad
                .Where(p => p.Activo)
                .AsNoTracking()
                .ToListAsync();

            return Ok(periodicidades);
        }

        [HttpGet("periodicidad/admin")]
        public async Task<ActionResult<IEnumerable<Periodicidad>>> GetPeriodicidadAdmin()
        {
            var periodicidades = await _dbContext.Periodicidad
                .AsNoTracking()
                .ToListAsync();

            return Ok(periodicidades);
        }

        [HttpPost("periodicidad")]
        public async Task<ActionResult<Periodicidad>> CrearPeriodicidad([FromBody] CrearPeriodicidadRequest request)
        {
            var existe = await _dbContext.Periodicidad
                .AnyAsync(p => p.DescPeriodicidad.ToLower() == request.DescPeriodicidad.ToLower());

            if (existe)
                return BadRequest(new { mensaje = "Ya existe una periodicidad con ese nombre" });

            var periodicidad = new Periodicidad
            {
                DescPeriodicidad = request.DescPeriodicidad,
                PeriodosPorAnio = request.PeriodosPorAnio,
                MesesPorPeriodo = request.MesesPorPeriodo,
                Activo = true
            };

            _dbContext.Periodicidad.Add(periodicidad);
            await _dbContext.SaveChangesAsync();

            return Ok(periodicidad);
        }

        [HttpPut("periodicidad/{id:int}/toggle")]
        public async Task<ActionResult<Periodicidad>> TogglePeriodicidad(int id)
        {
            var periodicidad = await _dbContext.Periodicidad.FindAsync(id);

            if (periodicidad == null)
                return NotFound(new { mensaje = "Periodicidad no encontrada" });

            periodicidad.Activo = !periodicidad.Activo;
            await _dbContext.SaveChangesAsync();

            return Ok(periodicidad);
        }

        [HttpGet("documentos-requisito")]
        public async Task<ActionResult<IEnumerable<DocumentoRequisito>>> GetDocumentosRequisito()
        {
            var documentos = await _dbContext.DocumentoRequisito
                .Where(d => d.Activo)
                .OrderBy(d => d.Orden)
                .AsNoTracking()
                .ToListAsync();

            return Ok(documentos);
        }

        [HttpGet("medios-pago")]
        public async Task<ActionResult<IEnumerable<MedioPago>>> GetMediosPago()
        {
            var mediosPago = await _dbContext.MedioPago
                .Where(mp => mp.Activo)
                .AsNoTracking()
                .ToListAsync();

            return Ok(mediosPago);
        }

        [HttpGet("periodos-academicos")]
        public async Task<ActionResult<IEnumerable<PeriodoAcademico>>> GetPeriodosAcademicos()
        {
            var periodosAcademicos = await _dbContext.PeriodoAcademico
                .Include(pa => pa.IdPeriodicidadNavigation)
                .Where(pa => pa.Status == StatusEnum.Active)
                .OrderByDescending(pa => pa.FechaInicio)
                .AsNoTracking()
                .Select(pa => new
                {
                    pa.IdPeriodoAcademico,
                    pa.Clave,
                    pa.Nombre,
                    Periodicidad = pa.IdPeriodicidadNavigation != null
                        ? pa.IdPeriodicidadNavigation.DescPeriodicidad
                        : null,
                    pa.FechaInicio,
                    pa.FechaFin,
                    pa.EsPeriodoActual
                })
                .ToListAsync();

            return Ok(periodosAcademicos);
        }

        [HttpGet("planes-estudio")]
        public async Task<ActionResult<IEnumerable<PlanEstudios>>> GetPlanesEstudio()
        {
            var planesEstudio = await _dbContext.PlanEstudios
                .Include(pe => pe.IdNivelEducativoNavigation)
                .Include(pe => pe.IdPeriodicidadNavigation)
                .Include(pe => pe.IdCampusNavigation)
                .Where(pe => pe.Status == StatusEnum.Active)
                .AsNoTracking()
                .Select(pe => new
                {
                    pe.IdPlanEstudios,
                    pe.ClavePlanEstudios,
                    pe.NombrePlanEstudios,
                    NivelEducativo = pe.IdNivelEducativoNavigation != null
                        ? pe.IdNivelEducativoNavigation.DescNivelEducativo
                        : null,
                    Periodicidad = pe.IdPeriodicidadNavigation != null
                        ? pe.IdPeriodicidadNavigation.DescPeriodicidad
                        : null,
                    pe.RVOE,
                    pe.DuracionMeses,
                    pe.IdCampus,
                    NombreCampus = pe.IdCampusNavigation != null ? pe.IdCampusNavigation.Nombre : null
                })
                .ToListAsync();

            return Ok(planesEstudio);
        }

        [HttpGet("cuatrimestres/{idPlanEstudios:int}")]
        public async Task<ActionResult<IEnumerable<object>>> GetCuatrimestres(int idPlanEstudios)
        {
            var planExiste = await _dbContext.PlanEstudios
                .AnyAsync(pe => pe.IdPlanEstudios == idPlanEstudios && pe.Status == StatusEnum.Active);

            if (!planExiste)
                return NotFound(new { mensaje = "Plan de estudios no encontrado" });

            var cuatrimestres = await _dbContext.MateriaPlan
                .Where(mp => mp.IdPlanEstudios == idPlanEstudios && mp.Status == StatusEnum.Active)
                .Select(mp => mp.Cuatrimestre)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var resultado = cuatrimestres.Select(c => new
            {
                Numero = c,
                Descripcion = ObtenerNombreCuatrimestre(c)
            }).ToList();

            return Ok(resultado);
        }

        private string ObtenerNombreCuatrimestre(byte numero)
        {
            var nombres = new Dictionary<byte, string>
            {
                { 1, "Primer Cuatrimestre" },
                { 2, "Segundo Cuatrimestre" },
                { 3, "Tercer Cuatrimestre" },
                { 4, "Cuarto Cuatrimestre" },
                { 5, "Quinto Cuatrimestre" },
                { 6, "Sexto Cuatrimestre" },
                { 7, "Séptimo Cuatrimestre" },
                { 8, "Octavo Cuatrimestre" },
                { 9, "Noveno Cuatrimestre" },
                { 10, "Décimo Cuatrimestre" },
                { 11, "Onceavo Cuatrimestre" },
                { 12, "Doceavo Cuatrimestre" }
            };

            return nombres.ContainsKey(numero) ? nombres[numero] : $"Cuatrimestre {numero}";
        }
    }
}
