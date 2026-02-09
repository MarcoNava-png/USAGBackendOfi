using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificacionUsuarioController : ControllerBase
    {
        private readonly INotificacionInternalService _svc;

        public NotificacionUsuarioController(INotificacionInternalService svc)
        {
            _svc = svc;
        }

        private string GetUserId() =>
            User.FindFirst("userId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        [HttpGet]
        public async Task<ActionResult<PagedResult<NotificacionUsuarioDto>>> Get(
            [FromQuery] bool soloNoLeidas = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _svc.ObtenerAsync(userId, soloNoLeidas, page, pageSize, ct);
            return Ok(result);
        }

        [HttpGet("no-leidas")]
        public async Task<ActionResult> GetNoLeidas(CancellationToken ct)
        {
            var userId = GetUserId();
            var count = await _svc.ContarNoLeidasAsync(userId, ct);
            return Ok(new { count });
        }

        [HttpPut("{id:long}/leer")]
        public async Task<ActionResult> MarcarLeida(long id, CancellationToken ct)
        {
            var userId = GetUserId();
            await _svc.MarcarLeidaAsync(id, userId, ct);
            return Ok(new { mensaje = "Notificacion marcada como leida" });
        }

        [HttpPut("leer-todas")]
        public async Task<ActionResult> MarcarTodasLeidas(CancellationToken ct)
        {
            var userId = GetUserId();
            await _svc.MarcarTodasLeidasAsync(userId, ct);
            return Ok(new { mensaje = "Todas las notificaciones marcadas como leidas" });
        }
    }
}
