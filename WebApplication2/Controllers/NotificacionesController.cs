using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.DTOs.MultiTenant;
using WebApplication2.Core.Requests.MultiTenant;
using WebApplication2.Services.MultiTenant;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/admin/notificaciones")]
[Authorize(Roles = "SuperAdmin")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _notificacionService;
    private readonly ILogger<NotificacionesController> _logger;

    public NotificacionesController(
        INotificacionService notificacionService,
        ILogger<NotificacionesController> logger)
    {
        _notificacionService = notificacionService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<NotificacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NotificacionDto>>> GetNotificaciones(
        [FromQuery] bool soloNoLeidas = false,
        [FromQuery] int limite = 50,
        CancellationToken ct = default)
    {
        var notificaciones = await _notificacionService.ObtenerNotificacionesAsync(soloNoLeidas, limite, ct);
        return Ok(notificaciones);
    }

    [HttpGet("contador")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetContadorNoLeidas(CancellationToken ct)
    {
        var contador = await _notificacionService.ObtenerContadorNoLeidasAsync(ct);
        return Ok(new { count = contador });
    }

    [HttpPost("{id:int}/leer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> MarcarComoLeida(int id, CancellationToken ct)
    {
        await _notificacionService.MarcarComoLeidaAsync(id, ct);
        return Ok(new { mensaje = "Notificacion marcada como leida" });
    }

    [HttpPost("leer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> MarcarVariasComoLeidas(
        [FromBody] MarcarLeidasRequest request,
        CancellationToken ct)
    {
        foreach (var id in request.Ids)
        {
            await _notificacionService.MarcarComoLeidaAsync(id, ct);
        }
        return Ok(new { mensaje = $"{request.Ids.Count} notificaciones marcadas como leidas" });
    }

    [HttpPost("leer-todas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> MarcarTodasComoLeidas(CancellationToken ct)
    {
        await _notificacionService.MarcarTodasComoLeidasAsync(ct);
        return Ok(new { mensaje = "Todas las notificaciones marcadas como leidas" });
    }

    [HttpPost("verificar-vencimientos")]
    [ProducesResponseType(typeof(ResultadoEnvioNotificacionesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoEnvioNotificacionesDto>> VerificarVencimientos(CancellationToken ct)
    {
        _logger.LogInformation("Iniciando verificacion manual de vencimientos");
        var resultado = await _notificacionService.VerificarVencimientosAsync(ct);
        _logger.LogInformation("Verificacion completada: {Creadas} notificaciones creadas, {Errores} errores",
            resultado.NotificacionesCreadas, resultado.Errores);
        return Ok(resultado);
    }
}
