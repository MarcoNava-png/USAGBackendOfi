using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Controllers
{
    [Route("api/seguimiento-egresados")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.ACADEMICO},{Rol.FINANZAS}")]
    public class SeguimientoEgresadoController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SeguimientoEgresadoController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult> Listar(
            [FromQuery] string? programa,
            [FromQuery] string? busqueda,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var query = _db.SeguimientoEgresados
                .Where(s => s.Status == StatusEnum.Active)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(programa))
                query = query.Where(s => s.ProgramaAcademico == programa);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var term = busqueda.Trim();
                query = query.Where(s => EF.Functions.Collate(s.NombreCompleto, "SQL_Latin1_General_CP1_CI_AI").Contains(term)
                    || (s.Matricula != null && s.Matricula.Contains(term)));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(s => s.NombreCompleto)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return Ok(new { items, total, page, pageSize });
        }

        [HttpGet("programas")]
        public async Task<ActionResult> ListarProgramas(CancellationToken ct)
        {
            var programas = await _db.SeguimientoEgresados
                .Where(s => s.Status == StatusEnum.Active)
                .Select(s => s.ProgramaAcademico)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync(ct);

            return Ok(programas);
        }

        [HttpGet("estadisticas")]
        public async Task<ActionResult> Estadisticas(CancellationToken ct)
        {
            var all = await _db.SeguimientoEgresados.Where(s => s.Status == StatusEnum.Active).ToListAsync(ct);

            return Ok(new
            {
                total = all.Count,
                titulados = all.Count(s => s.EstatusTitulacion == "Títulado"),
                enProceso = all.Count(s => s.EstatusTitulacion == "En Proceso"),
                expedienteCompleto = all.Count(s => s.Expediente == "Completo"),
                expedienteIncompleto = all.Count(s => s.Expediente == "Incompleto"),
                servicioSocialLiberado = all.Count(s => s.LiberacionServicioSocial == "Liberado"),
                certificadoEntregado = all.Count(s => s.EstatusCertificado == "Entregado"),
                tituloElectronicoEntregado = all.Count(s => s.EstatusTituloElectronico == "Entregado"),
                tituloFisicoEntregado = all.Count(s => s.EstatusTituloFisico == "Entregado"),
                cedulaEntregada = all.Count(s => s.TramiteCedula == "Entregada")
            });
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Actualizar(int id, [FromBody] ActualizarSeguimientoRequest request, CancellationToken ct)
        {
            var registro = await _db.SeguimientoEgresados.FindAsync(new object[] { id }, ct);
            if (registro == null) return NotFound();

            var userRole = User.Claims.FirstOrDefault(c => c.Type.Contains("role"))?.Value ?? "";

            if (request.Expediente != null && EsRolPermitido(userRole, "expediente"))
                registro.Expediente = request.Expediente;

            if (request.PagoTitulacion != null && EsRolPermitido(userRole, "pago_titulacion"))
                registro.PagoTitulacion = request.PagoTitulacion;

            if (request.LiberacionServicioSocial != null && EsRolPermitido(userRole, "servicio_social"))
                registro.LiberacionServicioSocial = request.LiberacionServicioSocial;

            if (request.EstatusTitulacion != null && EsRolPermitido(userRole, "titulacion"))
                registro.EstatusTitulacion = request.EstatusTitulacion;

            if (request.EstatusCertificado != null && EsRolPermitido(userRole, "certificado"))
                registro.EstatusCertificado = request.EstatusCertificado;

            if (request.EstatusTituloElectronico != null && EsRolPermitido(userRole, "titulo_electronico"))
                registro.EstatusTituloElectronico = request.EstatusTituloElectronico;

            if (request.EstatusTituloFisico != null && EsRolPermitido(userRole, "titulo_fisico"))
                registro.EstatusTituloFisico = request.EstatusTituloFisico;

            if (request.PagoCedula != null && EsRolPermitido(userRole, "pago_cedula"))
                registro.PagoCedula = request.PagoCedula;

            if (request.TramiteCedula != null && EsRolPermitido(userRole, "tramite_cedula"))
                registro.TramiteCedula = request.TramiteCedula;

            if (request.Observaciones != null)
                registro.Observaciones = request.Observaciones;

            registro.UpdatedAt = DateTime.UtcNow;
            registro.UpdatedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            await _db.SaveChangesAsync(ct);
            return Ok(registro);
        }

        private static bool EsRolPermitido(string rol, string campo) => campo switch
        {
            "expediente" => rol is "admin" or "controlescolar" or "director",
            "pago_titulacion" => rol is "admin" or "finanzas" or "director",
            "servicio_social" => rol is "admin" or "coordinador" or "academico" or "director",
            "titulacion" => rol is "admin" or "academico" or "director",
            "certificado" => rol is "admin" or "controlescolar" or "director",
            "titulo_electronico" => rol is "admin" or "controlescolar" or "director",
            "titulo_fisico" => rol is "admin" or "controlescolar" or "director",
            "pago_cedula" => rol is "admin" or "finanzas" or "director",
            "tramite_cedula" => rol is "admin" or "controlescolar" or "director",
            _ => rol is "admin"
        };
    }

    public class ActualizarSeguimientoRequest
    {
        public string? Expediente { get; set; }
        public string? PagoTitulacion { get; set; }
        public string? LiberacionServicioSocial { get; set; }
        public string? EstatusTitulacion { get; set; }
        public string? EstatusCertificado { get; set; }
        public string? EstatusTituloElectronico { get; set; }
        public string? EstatusTituloFisico { get; set; }
        public string? PagoCedula { get; set; }
        public string? TramiteCedula { get; set; }
        public string? Observaciones { get; set; }
    }
}
