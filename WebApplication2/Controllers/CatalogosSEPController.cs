using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Models.Titulacion;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Controllers
{
    [Route("api/titulacion/catalogos-sep")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.ACADEMICO}")]
    public class CatalogosSEPController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CatalogosSEPController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("tipos-periodo")]
        public async Task<ActionResult> GetTiposPeriodo(CancellationToken ct) =>
            Ok(await _db.CatalogoTipoPeriodoSEP.Where(c => c.Activo).OrderBy(c => c.IdTipoPeriodo).ToListAsync(ct));

        [HttpGet("tipos-certificacion")]
        public async Task<ActionResult> GetTiposCertificacion(CancellationToken ct) =>
            Ok(await _db.CatalogoTipoCertificacionSEP.Where(c => c.Activo).OrderBy(c => c.IdTipoCertificacion).ToListAsync(ct));

        [HttpGet("cargos")]
        public async Task<ActionResult> GetCargos(CancellationToken ct) =>
            Ok(await _db.CatalogoCargoSEP.Where(c => c.Activo).OrderBy(c => c.IdCargo).ToListAsync(ct));

        [HttpGet("observaciones")]
        public async Task<ActionResult> GetObservaciones(CancellationToken ct) =>
            Ok(await _db.CatalogoObservacionSEP.Where(c => c.Activo).OrderBy(c => c.IdObservacion).ToListAsync(ct));

        [HttpGet("niveles-estudio")]
        public async Task<ActionResult> GetNivelesEstudio(CancellationToken ct) =>
            Ok(await _db.CatalogoNivelEstudiosSEP.Where(c => c.Activo).OrderBy(c => c.Descripcion).ToListAsync(ct));

        [HttpGet("generos")]
        public async Task<ActionResult> GetGeneros(CancellationToken ct) =>
            Ok(await _db.CatalogoGeneroSEP.Where(c => c.Activo).OrderBy(c => c.Descripcion).ToListAsync(ct));

        [HttpGet("tipos-asignatura")]
        public async Task<ActionResult> GetTiposAsignatura(CancellationToken ct) =>
            Ok(await _db.CatalogoTipoAsignaturaSEP.Where(c => c.Activo).OrderBy(c => c.Descripcion).ToListAsync(ct));

        [HttpGet("entidades-federativas")]
        public async Task<ActionResult> GetEntidadesFederativas(CancellationToken ct) =>
            Ok(await _db.CatalogoEntidadFederativaSEP.Where(c => c.Activo).OrderBy(c => c.Nombre).ToListAsync(ct));

        [HttpGet("carreras")]
        public async Task<ActionResult> GetCarreras(CancellationToken ct) =>
            Ok(await _db.CatalogoCarreraSEP.Where(c => c.Activo).OrderBy(c => c.IdCarrera).ToListAsync(ct));

        [HttpGet("carreras-con-plan")]
        public async Task<ActionResult> GetCarrerasConPlan(CancellationToken ct)
        {
            var carreras = await _db.CatalogoCarreraSEP.Where(c => c.Activo).OrderBy(c => c.IdCarrera).ToListAsync(ct);
            var planes = await _db.PlanEstudios
                .Include(p => p.IdPeriodicidadNavigation)
                .Include(p => p.IdCampusNavigation)
                .Where(p => p.IdCarreraSEP != null && p.Status == Core.Enums.StatusEnum.Active)
                .ToListAsync(ct);

            var result = carreras.Select(c =>
            {
                var plan = planes.FirstOrDefault(p => p.IdCarreraSEP == int.Parse(c.IdCarrera));
                var idTipoPeriodoSEP = plan?.IdPeriodicidad switch
                {
                    1 => "93",
                    2 => "91",
                    3 => "262",
                    _ => null
                };
                var tipoPeriodoSEP = plan?.IdPeriodicidad switch
                {
                    1 => "CUATRIMESTRE",
                    2 => "SEMESTRE",
                    3 => "ANUAL",
                    _ => null
                };

                return new
                {
                    c.Id,
                    c.IdNombreInstitucion,
                    c.IdNivelEstudios,
                    c.IdCarrera,
                    c.ClaveCarrera,
                    c.NombreCarrera,
                    planVinculado = plan != null ? new
                    {
                        plan.IdPlanEstudios,
                        plan.ClavePlanEstudios,
                        plan.RVOE,
                        fechaExpedicionRvoe = plan.FechaExpedicionRvoe?.ToString("yyyy-MM-dd"),
                        plan.MinimaAprobatoriaFinal,
                        plan.MinimaAprobatoriaParcial,
                        periodicidad = plan.IdPeriodicidadNavigation?.DescPeriodicidad,
                        idPeriodicidad = plan.IdPeriodicidad,
                        idTipoPeriodoSEP,
                        tipoPeriodoSEP,
                        campus = plan.IdCampusNavigation?.Nombre,
                        plan.IdCampus,
                    } : null
                };
            });

            return Ok(result);
        }

        [HttpPut("carreras/{id:int}")]
        public async Task<ActionResult> UpdateCarrera(int id, [FromBody] CatalogoCarreraSEP request, CancellationToken ct)
        {
            var carrera = await _db.CatalogoCarreraSEP.FindAsync(new object[] { id }, ct);
            if (carrera == null) return NotFound();
            carrera.IdNombreInstitucion = request.IdNombreInstitucion;
            carrera.IdNivelEstudios = request.IdNivelEstudios;
            carrera.IdCarrera = request.IdCarrera;
            carrera.ClaveCarrera = request.ClaveCarrera;
            carrera.NombreCarrera = request.NombreCarrera;
            await _db.SaveChangesAsync(ct);
            return Ok(carrera);
        }

        [HttpGet("asignaturas/{idCarrera:int}")]
        public async Task<ActionResult> GetAsignaturas(int idCarrera, CancellationToken ct) =>
            Ok(await _db.CatalogoAsignaturaSEP
                .Where(a => a.IdCarrera == idCarrera && a.Activo)
                .OrderBy(a => a.IdAsignatura)
                .ToListAsync(ct));

        [HttpPut("asignaturas/{id:int}")]
        public async Task<ActionResult> UpdateAsignatura(int id, [FromBody] CatalogoAsignaturaSEP request, CancellationToken ct)
        {
            var asig = await _db.CatalogoAsignaturaSEP.FindAsync(new object[] { id }, ct);
            if (asig == null) return NotFound();
            asig.ClaveAsignatura = request.ClaveAsignatura;
            asig.Nombre = request.Nombre;
            asig.Creditos = request.Creditos;
            asig.IdTipoAsignatura = request.IdTipoAsignatura;
            asig.TipoAsignatura = request.TipoAsignatura;
            await _db.SaveChangesAsync(ct);
            return Ok(asig);
        }

        [HttpPost("asignaturas")]
        public async Task<ActionResult> CreateAsignatura([FromBody] CatalogoAsignaturaSEP request, CancellationToken ct)
        {
            request.Activo = true;
            _db.CatalogoAsignaturaSEP.Add(request);
            await _db.SaveChangesAsync(ct);
            return Ok(request);
        }

        [HttpDelete("asignaturas/{id:int}")]
        public async Task<ActionResult> DeleteAsignatura(int id, CancellationToken ct)
        {
            var asig = await _db.CatalogoAsignaturaSEP.FindAsync(new object[] { id }, ct);
            if (asig == null) return NotFound();
            asig.Activo = false;
            await _db.SaveChangesAsync(ct);
            return Ok();
        }

        [HttpPut("asignaturas/lote")]
        public async Task<ActionResult> UpdateAsignaturasLote([FromBody] List<CatalogoAsignaturaSEP> asignaturas, CancellationToken ct)
        {
            foreach (var req in asignaturas)
            {
                var asig = await _db.CatalogoAsignaturaSEP.FindAsync(new object[] { req.Id }, ct);
                if (asig == null) continue;
                asig.Creditos = req.Creditos;
                asig.IdTipoAsignatura = req.IdTipoAsignatura;
                asig.TipoAsignatura = req.TipoAsignatura;
            }
            await _db.SaveChangesAsync(ct);
            return Ok(new { mensaje = $"{asignaturas.Count} asignaturas actualizadas" });
        }

        [HttpGet("resumen")]
        public async Task<ActionResult> GetResumen(CancellationToken ct)
        {
            return Ok(new
            {
                tiposPeriodo = await _db.CatalogoTipoPeriodoSEP.CountAsync(c => c.Activo, ct),
                tiposCertificacion = await _db.CatalogoTipoCertificacionSEP.CountAsync(c => c.Activo, ct),
                cargos = await _db.CatalogoCargoSEP.CountAsync(c => c.Activo, ct),
                observaciones = await _db.CatalogoObservacionSEP.CountAsync(c => c.Activo, ct),
                nivelesEstudio = await _db.CatalogoNivelEstudiosSEP.CountAsync(c => c.Activo, ct),
                generos = await _db.CatalogoGeneroSEP.CountAsync(c => c.Activo, ct),
                tiposAsignatura = await _db.CatalogoTipoAsignaturaSEP.CountAsync(c => c.Activo, ct),
                entidadesFederativas = await _db.CatalogoEntidadFederativaSEP.CountAsync(c => c.Activo, ct)
            });
        }
    }
}
