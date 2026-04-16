using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.PlanEstudios;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using System.Collections.Generic;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlanEstudiosController : ControllerBase
    {
        private readonly IPlanEstudioService _planEstudioService;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificacionInternalService _notifService;

        public PlanEstudiosController(IPlanEstudioService planEstudioService, IMapper mapper, ApplicationDbContext db, UserManager<ApplicationUser> userManager, INotificacionInternalService notifService)
        {
            _planEstudioService = planEstudioService;
            _mapper = mapper;
            _db = db;
            _userManager = userManager;
            _notifService = notifService;
        }

        [HttpGet]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS},{Rol.ADMISIONES},{Rol.ACADEMICO}")]
        public async Task<ActionResult<PagedResult<PlanEstudioDto>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] int? idCampus = null,
            [FromQuery] bool incluirInactivos = false)
        {
            var pagination = await _planEstudioService.GetPlanesEstudios(page, pageSize, idCampus, incluirInactivos);

            var planesEstudiosDto = _mapper.Map<IEnumerable<PlanEstudioDto>>(pagination.Items);

            var response = new PagedResult<PlanEstudioDto>
            {
                TotalItems = pagination.TotalItems,
                Items = [.. planesEstudiosDto],
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS},{Rol.ADMISIONES},{Rol.ACADEMICO}")]
        public async Task<ActionResult<PlanEstudioDto>> GetById(int id)
        {
            var planEstudios = await _planEstudioService.GetPlanEstudiosById(id);

            if (planEstudios == null)
            {
                return NotFound(new { message = "Plan de estudios no encontrado" });
            }

            var planEstudiosDto = _mapper.Map<PlanEstudioDto>(planEstudios);
            return Ok(planEstudiosDto);
        }

        [HttpPost]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.ACADEMICO},{Rol.CONTROL_ESCOLAR}")]
        public async Task<ActionResult<PlanEstudioDto>> Post([FromBody] PlanEstudiosRequest request)
        {
            try
            {
                var esAdmin = User.IsInRole(Rol.ADMIN);

                var planEstudios = _mapper.Map<PlanEstudios>(request);
                await _planEstudioService.CrearPlanEstudios(planEstudios);

                if (!esAdmin)
                {
                    planEstudios.Status = StatusEnum.Disabled;
                    await _db.SaveChangesAsync();

                    var campus = planEstudios.IdCampus > 0
                        ? await _db.Campus.FindAsync(planEstudios.IdCampus)
                        : null;

                    var solicitud = new SolicitudPlanEstudios
                    {
                        IdPlanEstudios = planEstudios.IdPlanEstudios,
                        ClavePlanEstudios = planEstudios.ClavePlanEstudios,
                        NombrePlanEstudios = planEstudios.NombrePlanEstudios,
                        Campus = campus?.Nombre,
                        Rvoe = planEstudios.RVOE,
                        EstatusSolicitud = "Pendiente",
                        SolicitadoPor = User.Identity?.Name ?? "Sistema",
                        FechaSolicitud = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        Status = StatusEnum.Active
                    };
                    _db.SolicitudesPlanEstudios.Add(solicitud);
                    await _db.SaveChangesAsync();

                    var academicos = await _userManager.GetUsersInRoleAsync("academico");
                    var admins = await _userManager.GetUsersInRoleAsync("admin");
                    var destinatarios = academicos.Union(admins).Select(u => u.Id).Distinct();

                    foreach (var userId in destinatarios)
                    {
                        await _notifService.CrearAsync(
                            userId,
                            "Solicitud de nuevo plan de estudios",
                            $"{User.Identity?.Name} solicita dar de alta el plan '{planEstudios.NombrePlanEstudios}'. Requiere aprobación.",
                            "info",
                            "Academico",
                            "/dashboard/solicitudes-plan"
                        );
                    }
                }

                var planEstudiosDto = _mapper.Map<PlanEstudioDto>(planEstudios);
                return Ok(planEstudiosDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("solicitudes")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.ACADEMICO},{Rol.DIRECTOR}")]
        public async Task<ActionResult> ListarSolicitudes([FromQuery] string? estatus, CancellationToken ct)
        {
            var query = _db.SolicitudesPlanEstudios.Where(s => s.Status == StatusEnum.Active).AsQueryable();
            if (!string.IsNullOrEmpty(estatus)) query = query.Where(s => s.EstatusSolicitud == estatus);

            return Ok(await query.OrderByDescending(s => s.FechaSolicitud).ToListAsync(ct));
        }

        [HttpPut("solicitudes/{id:int}/aprobar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.ACADEMICO},{Rol.DIRECTOR}")]
        public async Task<ActionResult> AprobarSolicitud(int id, [FromBody] ComentarioSolicitudRequest request, CancellationToken ct)
        {
            var solicitud = await _db.SolicitudesPlanEstudios.FirstOrDefaultAsync(s => s.IdSolicitudPlanEstudios == id, ct);
            if (solicitud == null) return NotFound();

            solicitud.EstatusSolicitud = "Aprobada";
            solicitud.AprobadoPor = User.Identity?.Name;
            solicitud.ComentarioRevision = request.Comentario;
            solicitud.FechaResolucion = DateTime.UtcNow;

            var plan = await _db.PlanEstudios.FindAsync(new object[] { solicitud.IdPlanEstudios }, ct);
            if (plan != null) plan.Status = StatusEnum.Active;

            await _db.SaveChangesAsync(ct);
            return Ok(new { mensaje = "Plan de estudios aprobado y activado" });
        }

        [HttpPut("solicitudes/{id:int}/rechazar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.ACADEMICO},{Rol.DIRECTOR}")]
        public async Task<ActionResult> RechazarSolicitud(int id, [FromBody] ComentarioSolicitudRequest request, CancellationToken ct)
        {
            var solicitud = await _db.SolicitudesPlanEstudios.FirstOrDefaultAsync(s => s.IdSolicitudPlanEstudios == id, ct);
            if (solicitud == null) return NotFound();

            solicitud.EstatusSolicitud = "Rechazada";
            solicitud.AprobadoPor = User.Identity?.Name;
            solicitud.ComentarioRevision = request.Comentario;
            solicitud.FechaResolucion = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return Ok(new { mensaje = "Solicitud rechazada" });
        }

        [HttpPut]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        public async Task<IActionResult> Update([FromBody] PlanEstudiosUpdateRequest request)
        {
            try
            {
                var newPlanEstudios = _mapper.Map<PlanEstudios>(request);

                var planEstudios = await _planEstudioService.ActualizarPlanEstudios(newPlanEstudios);

                var planEstudiosDto = _mapper.Map<PlanEstudioDto>(planEstudios);

                return Ok(planEstudiosDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _planEstudioService.EliminarPlanEstudios(id);
                return Ok(new { message = "Plan de estudios eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/toggle")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.ACADEMICO},{Rol.CONTROL_ESCOLAR}")]
        public async Task<ActionResult<PlanEstudioDto>> ToggleEstado(int id)
        {
            try
            {
                var planEstudios = await _planEstudioService.ToggleEstado(id);
                var planEstudiosDto = _mapper.Map<PlanEstudioDto>(planEstudios);
                return Ok(planEstudiosDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/documentos")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.ADMISIONES}")]
        public async Task<ActionResult<List<PlanDocumentoRequisitoDto>>> GetDocumentosPlan(int id)
        {
            var docs = await _planEstudioService.GetDocumentosPlanAsync(id);
            return Ok(docs);
        }

        [HttpPut("{id}/documentos")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR}")]
        public async Task<ActionResult> ActualizarDocumentosPlan(int id, [FromBody] ActualizarDocumentosPlanRequest request)
        {
            try
            {
                await _planEstudioService.ActualizarDocumentosPlanAsync(id, request.Documentos);
                return Ok(new { message = "Documentos del plan actualizados correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("documentos-requisito")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.ADMISIONES}")]
        public async Task<ActionResult<List<DocumentoRequisitoDisponibleDto>>> GetTodosDocumentosRequisito()
        {
            var docs = await _planEstudioService.GetTodosDocumentosRequisitoAsync();
            return Ok(docs);
        }
    }

    public class ComentarioSolicitudRequest
    {
        public string? Comentario { get; set; }
    }
}
