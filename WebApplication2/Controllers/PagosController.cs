using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Pagos;
using WebApplication2.Core.Requests.Pagos;
using WebApplication2.Core.Responses.Pagos;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.FINANZAS},{Rol.ADMISIONES}")]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _svc;
        private readonly IPdfService _pdfService;

        public PagosController(IPagoService svc, IPdfService pdfService)
        {
            _svc = svc;
            _pdfService = pdfService;
        }

        [HttpPost]
        public async Task<ActionResult<long>> Registrar([FromBody] RegistrarPagoDto dto, CancellationToken ct)
            => Ok(await _svc.RegistrarPagoAsync(dto, ct));

        [HttpPost("aplicar")]
        public async Task<ActionResult<IReadOnlyList<long>>> Aplicar([FromBody] AplicarPagoDto dto, CancellationToken ct)
            => Ok(await _svc.AplicarPagoAsync(dto, ct));

        [HttpPost("registrar-y-aplicar")]
        public async Task<ActionResult<RegistrarYAplicarPagoResultDto>> RegistrarYAplicar([FromBody] RegistrarYAplicarPagoDto dto, CancellationToken ct)
        {
            try
            {
                var resultado = await _svc.RegistrarYAplicarPagoAsync(dto, ct);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Error al procesar pago: {ex.Message}" });
            }
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<PagoDto>> Obtener(long id, CancellationToken ct)
        {
            var p = await _svc.ObtenerAsync(id, ct);
            return p is null ? NotFound() : Ok(p);
        }

        [HttpGet("{id:long}/comprobante")]
        public async Task<IActionResult> DescargarComprobante(long id, CancellationToken ct)
        {
            try
            {
                var datosComprobante = await _svc.ObtenerDatosComprobanteAsync(id, ct);

                if (datosComprobante == null)
                {
                    return NotFound(new { Error = $"No se encontró el pago con ID {id}" });
                }

                var pdfBytes = _pdfService.GenerarComprobantePago(datosComprobante);

                var nombreArchivo = $"Comprobante_{datosComprobante.Pago.FolioPago}.pdf";

                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Error al generar comprobante: {ex.Message}" });
            }
        }

        [HttpGet("corte-caja")]
        public async Task<ActionResult<IReadOnlyList<PagoDto>>> CorteCaja(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin,
            [FromQuery] string? usuarioId,
            CancellationToken ct)
        {
            var pagos = await _svc.ListarPorFechaAsync(fechaInicio, fechaFin, usuarioId, ct);
            return Ok(pagos);
        }
    }
}
