using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.Titulacion;
using WebApplication2.Core.Enums;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/titulacion/certificados")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR}")]
    public class CertificadoElectronicoController : ControllerBase
    {
        private readonly ICertificadoElectronicoService _service;
        private readonly ICertificadoXmlService _xmlService;
        private readonly ITitulosElectronicosSepService _sepService;
        private readonly ApplicationDbContext _db;

        public CertificadoElectronicoController(ICertificadoElectronicoService service, ICertificadoXmlService xmlService, ITitulosElectronicosSepService sepService, ApplicationDbContext db)
        {
            _service = service;
            _xmlService = xmlService;
            _sepService = sepService;
            _db = db;
        }

        [HttpGet("directa")]
        public async Task<ActionResult<List<CertificadoElectronicoListDto>>> ListarDirecta(CancellationToken ct)
        {
            return Ok(await _service.ListarAsync(TipoTitulacionEnum.Directa, ct));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CertificadoElectronicoDetalleDto>> ObtenerDetalle(int id, CancellationToken ct)
        {
            var result = await _service.ObtenerDetalleAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("directa")]
        public async Task<ActionResult<CertificadoElectronicoDetalleDto>> CrearDirecta([FromBody] CrearCertificadoDirectoRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _service.CrearDirectoAsync(request, ct));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CertificadoElectronicoDetalleDto>> Actualizar(int id, [FromBody] CrearCertificadoDirectoRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _service.ActualizarDirectoAsync(id, request, ct));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("estadisticas/directa")]
        public async Task<ActionResult> EstadisticasDirecta(CancellationToken ct)
        {
            var (total, enRevision, xmlGenerados, registrados) = await _service.ObtenerEstadisticasAsync(TipoTitulacionEnum.Directa, ct);
            return Ok(new { total, enRevision, xmlGenerados, registrados });
        }

        [HttpPost("{id:int}/generar-xml")]
        public async Task<ActionResult> GenerarXml(int id, CancellationToken ct)
        {
            try
            {
                var xml = await _xmlService.GenerarXmlAsync(id, ct);
                return Ok(new { xml, mensaje = "XML generado correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id:int}/descargar-xml")]
        public async Task<ActionResult> DescargarXml(int id, CancellationToken ct)
        {
            var cert = await _db.CertificadoElectronico
                .FirstOrDefaultAsync(c => c.Id == id && c.Status == Core.Enums.StatusEnum.Active, ct);
            if (cert == null) return NotFound();
            if (string.IsNullOrEmpty(cert.XmlGenerado))
                return BadRequest(new { error = "El certificado no tiene XML generado" });

            var bytes = System.Text.Encoding.UTF8.GetBytes(cert.XmlGenerado);
            return File(bytes, "application/xml", $"DEC_{cert.NumeroControl}_{cert.FolioControl}.xml");
        }

        [HttpPost("{id:int}/enviar-sep")]
        public async Task<ActionResult> EnviarSep(int id, [FromQuery] string? cveInstitucion, CancellationToken ct)
        {
            try
            {
                var r = await _sepService.EnviarAsync(id, cveInstitucion, ct);
                return Ok(r);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { error = $"No se pudo conectar con el web service de la SEP: {ex.Message}" });
            }
        }

        [HttpPost("{id:int}/consultar-sep")]
        public async Task<ActionResult> ConsultarSep(int id, CancellationToken ct)
        {
            try
            {
                var r = await _sepService.ConsultarAsync(id, ct);
                return Ok(r);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { error = $"No se pudo conectar con el web service de la SEP: {ex.Message}" });
            }
        }

        [HttpPost("{id:int}/descargar-sep")]
        public async Task<ActionResult> DescargarSep(int id, CancellationToken ct)
        {
            try
            {
                var r = await _sepService.DescargarAsync(id, ct);
                return Ok(r);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { error = $"No se pudo conectar con el web service de la SEP: {ex.Message}" });
            }
        }

        [HttpGet("catalogos/carreras-sep")]
        public async Task<ActionResult> GetCarrerasSEP(CancellationToken ct)
        {
            return Ok(await _db.CatalogoCarreraSEP.Where(c => c.Activo).OrderBy(c => c.IdCarrera).ToListAsync(ct));
        }

        [HttpGet("catalogos/asignaturas-sep/{idCarrera:int}")]
        public async Task<ActionResult> GetAsignaturasSEP(int idCarrera, CancellationToken ct)
        {
            return Ok(await _db.CatalogoAsignaturaSEP
                .Where(a => a.IdCarrera == idCarrera && a.Activo)
                .OrderBy(a => a.IdAsignatura)
                .ToListAsync(ct));
        }

        [HttpGet("catalogos/cargos")]
        public async Task<ActionResult> GetCargos(CancellationToken ct)
        {
            return Ok(await _db.CatalogoCargoSEP.Where(c => c.Activo).OrderBy(c => c.IdCargo).ToListAsync(ct));
        }

        [HttpGet("catalogos/tipos-periodo")]
        public async Task<ActionResult> GetTiposPeriodo(CancellationToken ct)
        {
            return Ok(await _db.CatalogoTipoPeriodoSEP.Where(c => c.Activo).OrderBy(c => c.IdTipoPeriodo).ToListAsync(ct));
        }

        [HttpGet("catalogos/tipos-certificacion")]
        public async Task<ActionResult> GetTiposCertificacion(CancellationToken ct)
        {
            return Ok(await _db.CatalogoTipoCertificacionSEP.Where(c => c.Activo).OrderBy(c => c.IdTipoCertificacion).ToListAsync(ct));
        }

        [HttpGet("catalogos/observaciones")]
        public async Task<ActionResult> GetObservaciones(CancellationToken ct)
        {
            return Ok(await _db.CatalogoObservacionSEP.Where(c => c.Activo).OrderBy(c => c.IdObservacion).ToListAsync(ct));
        }

        [HttpGet("catalogos/niveles-estudio")]
        public async Task<ActionResult> GetNivelesEstudio(CancellationToken ct) =>
            Ok(await _db.CatalogoNivelEstudiosSEP.Where(c => c.Activo).OrderBy(c => c.Descripcion).ToListAsync(ct));

        [HttpGet("catalogos/generos")]
        public async Task<ActionResult> GetGeneros(CancellationToken ct) =>
            Ok(await _db.CatalogoGeneroSEP.Where(c => c.Activo).OrderBy(c => c.Descripcion).ToListAsync(ct));

        [HttpGet("catalogos/tipos-asignatura")]
        public async Task<ActionResult> GetTiposAsignatura(CancellationToken ct) =>
            Ok(await _db.CatalogoTipoAsignaturaSEP.Where(c => c.Activo).OrderBy(c => c.Descripcion).ToListAsync(ct));

        [HttpGet("catalogos/entidades-federativas")]
        public async Task<ActionResult> GetEntidadesFederativas(CancellationToken ct) =>
            Ok(await _db.CatalogoEntidadFederativaSEP.Where(c => c.Activo).OrderBy(c => c.Nombre).ToListAsync(ct));
    }
}
