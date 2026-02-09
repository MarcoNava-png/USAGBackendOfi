using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.DTOs;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanesController : ControllerBase
    {
        private readonly IPlanPagoService _svc;
        public PlanesController(IPlanPagoService svc) { _svc = svc; }

        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromBody] CrearPlanPagoDto dto, CancellationToken ct)
            => Ok(await _svc.CrearPlanAsync(dto, ct));

        [HttpPost("{idPlan:int}/detalles")]
        public async Task<ActionResult<int>> AgregarDetalle(int idPlan, [FromBody] CrearPlanDetalleDto dto, CancellationToken ct)
            => Ok(await _svc.AgregarDetalleAsync(idPlan, dto, ct));

        [HttpPost("asignaciones")]
        public async Task<ActionResult<long>> Asignar([FromBody] AsignarPlanDto dto, CancellationToken ct)
            => Ok(await _svc.AsignarPlanAsync(dto, ct));
    }
}

