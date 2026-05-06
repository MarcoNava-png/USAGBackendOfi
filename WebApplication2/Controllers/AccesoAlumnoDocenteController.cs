using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.AccesoAlumnoDocente;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/accesos-alumnos-docentes")]
[Authorize]
public class AccesoAlumnoDocenteController : ControllerBase
{
    private readonly IAccesoAlumnoDocenteService _service;

    public AccesoAlumnoDocenteController(IAccesoAlumnoDocenteService service)
    {
        _service = service;
    }

    private bool TienePermiso(string tipo)
    {
        if (User.IsInRole(Rol.SUPER_ADMIN) || User.IsInRole(Rol.ADMIN)) return true;
        if (tipo.Equals("alumno", StringComparison.OrdinalIgnoreCase))
            return User.IsInRole(Rol.CONTROL_ESCOLAR) || User.IsInRole(Rol.DIRECTOR);
        if (tipo.Equals("docente", StringComparison.OrdinalIgnoreCase))
            return User.IsInRole(Rol.COORDINADOR) || User.IsInRole(Rol.ACADEMICO) || User.IsInRole(Rol.DIRECTOR);
        return false;
    }

    [HttpGet]
    public async Task<ActionResult<AccesosListaDto>> Listar(
        [FromQuery] string tipo = "alumno",
        [FromQuery] string? busqueda = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        CancellationToken ct = default)
    {
        if (!TienePermiso(tipo))
            return Forbid();

        var resultado = await _service.ListarAsync(tipo, busqueda, pagina, tamanoPagina, ct);
        return Ok(resultado);
    }

    [HttpPost("resetear-password")]
    public async Task<ActionResult<ResetearPasswordResponse>> ResetearPassword([FromBody] ResetearPasswordRequest request, CancellationToken ct)
    {
        if (!User.IsInRole(Rol.SUPER_ADMIN) && !User.IsInRole(Rol.ADMIN) &&
            !User.IsInRole(Rol.CONTROL_ESCOLAR) && !User.IsInRole(Rol.COORDINADOR) &&
            !User.IsInRole(Rol.ACADEMICO) && !User.IsInRole(Rol.DIRECTOR))
            return Forbid();

        var resultado = await _service.ResetearPasswordAsync(request, ct);
        if (!resultado.Exito) return BadRequest(resultado);
        return Ok(resultado);
    }

    [HttpPost("crear-acceso")]
    public async Task<ActionResult<ResetearPasswordResponse>> CrearAcceso([FromBody] CrearAccesoRequest request, CancellationToken ct)
    {
        if (!TienePermiso(request.Tipo))
            return Forbid();

        var resultado = await _service.CrearAccesoAsync(request, ct);
        if (!resultado.Exito) return BadRequest(resultado);
        return Ok(resultado);
    }

    [HttpPost("{userId}/desbloquear")]
    public async Task<ActionResult> Desbloquear([FromRoute] string userId, CancellationToken ct)
    {
        if (!User.IsInRole(Rol.SUPER_ADMIN) && !User.IsInRole(Rol.ADMIN) &&
            !User.IsInRole(Rol.CONTROL_ESCOLAR) && !User.IsInRole(Rol.COORDINADOR) &&
            !User.IsInRole(Rol.ACADEMICO) && !User.IsInRole(Rol.DIRECTOR))
            return Forbid();

        var exito = await _service.DesbloquearCuentaAsync(userId, ct);
        if (!exito) return BadRequest(new { mensaje = "No se pudo desbloquear la cuenta" });
        return Ok(new { mensaje = "Cuenta desbloqueada" });
    }
}
