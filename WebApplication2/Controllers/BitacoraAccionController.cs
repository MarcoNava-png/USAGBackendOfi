using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.FINANZAS}")]
    public class BitacoraAccionController : ControllerBase
    {
        private readonly IBitacoraAccionService _svc;

        public BitacoraAccionController(IBitacoraAccionService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<BitacoraAccionDto>>> Get(
            [FromQuery] string? modulo,
            [FromQuery] string? usuario,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] string? busqueda,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var filtro = new BitacoraAccionFiltroDto
            {
                Modulo = modulo,
                Usuario = usuario,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                Busqueda = busqueda,
                Page = page,
                PageSize = pageSize
            };

            var result = await _svc.ConsultarAsync(filtro, ct);
            return Ok(result);
        }
    }
}
