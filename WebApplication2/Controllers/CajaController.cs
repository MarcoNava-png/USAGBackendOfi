using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Caja;
using WebApplication2.Core.DTOs.Pagos;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Caja;
using WebApplication2.Core.Requests.Pagos;
using WebApplication2.Core.Responses.Pagos;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/caja")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.FINANZAS},{Rol.ADMISIONES},{Rol.DIRECTOR}")]
    public class CajaController : ControllerBase
    {
        private readonly ICajaService _cajaService;
        private readonly IAuthService _authService;
        private readonly IPagoService _pagoService;

        public CajaController(ICajaService cajaService, IAuthService authService, IPagoService pagoService)
        {
            _cajaService = cajaService;
            _authService = authService;
            _pagoService = pagoService;
        }

        [HttpPost("pago")]
        public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoCajaRequest request, CancellationToken ct)
        {
            try
            {
                var usuarioId = User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                if (request.RecibosSeleccionados == null || !request.RecibosSeleccionados.Any())
                {
                    return BadRequest(new { message = "Debe seleccionar al menos un recibo para pagar" });
                }

                if (request.Monto <= 0)
                {
                    return BadRequest(new { message = "El monto debe ser mayor a cero" });
                }

                request.IdUsuarioCaja = usuarioId;

                var resultado = await _pagoService.RegistrarPagoCajaAsync(request, usuarioId, ct);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al registrar el pago", error = ex.Message });
            }
        }

        [HttpGet("recibos-pendientes")]
        public async Task<IActionResult> BuscarRecibosParaCobro([FromQuery] string criterio)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(criterio))
                {
                    return BadRequest(new { message = "Debe proporcionar un criterio de búsqueda" });
                }

                var resultado = await _cajaService.BuscarRecibosParaCobroAsync(criterio);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar recibos", error = ex.Message });
            }
        }

        [HttpGet("corte")]
        public async Task<IActionResult> ObtenerResumenCorteCaja(
            [FromQuery] string? fechaInicio = null,
            [FromQuery] string? fechaFin = null,
            [FromQuery] string? usuarioId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(usuarioId))
                {
                    usuarioId = User.FindFirst("userId")?.Value;
                }

                DateTime inicio = string.IsNullOrEmpty(fechaInicio)
                    ? DateTime.Today.ToUniversalTime()
                    : DateTime.Parse(fechaInicio).ToUniversalTime();

                DateTime fin = string.IsNullOrEmpty(fechaFin)
                    ? DateTime.UtcNow
                    : DateTime.Parse(fechaFin).ToUniversalTime();

                var resumen = await _cajaService.ObtenerResumenCorteCaja(inicio, fin, usuarioId);

                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener resumen de corte de caja", error = ex.Message });
            }
        }

        [HttpPost("corte/cerrar")]
        public async Task<IActionResult> CerrarCorteCaja([FromBody] CerrarCorteCajaRequest request)
        {
            try
            {
                var usuarioId = User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var montoInicial = request.MontoInicial ?? 0;

                var corteCaja = await _cajaService.CerrarCorteCaja(usuarioId, montoInicial, request.Observaciones);

                var usuario = await _authService.GetUserById(usuarioId);
                var corteCajaDto = new CorteCajaDto
                {
                    IdCorteCaja = corteCaja.IdCorteCaja,
                    FolioCorteCaja = corteCaja.FolioCorteCaja,
                    FechaInicio = corteCaja.FechaInicio,
                    FechaFin = corteCaja.FechaFin,
                    IdUsuarioCaja = corteCaja.IdUsuarioCaja,
                    IdCaja = corteCaja.IdCaja,
                    MontoInicial = corteCaja.MontoInicial,
                    TotalEfectivo = corteCaja.TotalEfectivo,
                    TotalTransferencia = corteCaja.TotalTransferencia,
                    TotalTarjeta = corteCaja.TotalTarjeta,
                    TotalGeneral = corteCaja.TotalGeneral,
                    Cerrado = corteCaja.Cerrado,
                    FechaCierre = corteCaja.FechaCierre,
                    CerradoPor = corteCaja.CerradoPor,
                    Observaciones = corteCaja.Observaciones,
                    NombreUsuario = usuario != null ? $"{usuario.Nombres} {usuario.Apellidos}" : ""
                };

                return Ok(corteCajaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cerrar corte de caja", error = ex.Message });
            }
        }

        [HttpGet("cortes")]
        public async Task<IActionResult> ObtenerCortesCaja(
            [FromQuery] string? usuarioId = null,
            [FromQuery] string? fechaInicio = null,
            [FromQuery] string? fechaFin = null)
        {
            try
            {
                DateTime? inicio = string.IsNullOrEmpty(fechaInicio)
                    ? null
                    : DateTime.Parse(fechaInicio).ToUniversalTime();

                DateTime? fin = string.IsNullOrEmpty(fechaFin)
                    ? null
                    : DateTime.Parse(fechaFin).ToUniversalTime();

                var cortes = await _cajaService.ObtenerCortesCaja(usuarioId, inicio, fin);

                var cortesDto = new List<CorteCajaDto>();

                foreach (var corte in cortes)
                {
                    var usuario = await _authService.GetUserById(corte.IdUsuarioCaja);
                    cortesDto.Add(new CorteCajaDto
                    {
                        IdCorteCaja = corte.IdCorteCaja,
                        FolioCorteCaja = corte.FolioCorteCaja,
                        FechaInicio = corte.FechaInicio,
                        FechaFin = corte.FechaFin,
                        IdUsuarioCaja = corte.IdUsuarioCaja,
                        IdCaja = corte.IdCaja,
                        MontoInicial = corte.MontoInicial,
                        TotalEfectivo = corte.TotalEfectivo,
                        TotalTransferencia = corte.TotalTransferencia,
                        TotalTarjeta = corte.TotalTarjeta,
                        TotalGeneral = corte.TotalGeneral,
                        Cerrado = corte.Cerrado,
                        FechaCierre = corte.FechaCierre,
                        CerradoPor = corte.CerradoPor,
                        Observaciones = corte.Observaciones,
                        NombreUsuario = usuario != null ? $"{usuario.Nombres} {usuario.Apellidos}" : ""
                    });
                }

                return Ok(cortesDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener cortes de caja", error = ex.Message });
            }
        }

        [HttpGet("cortes/{id}")]
        public async Task<IActionResult> ObtenerCorteCajaPorId(int id)
        {
            try
            {
                var corte = await _cajaService.ObtenerCorteCajaPorId(id);

                if (corte == null)
                {
                    return NotFound(new { message = "Corte de caja no encontrado" });
                }

                var usuario = await _authService.GetUserById(corte.IdUsuarioCaja);
                var corteCajaDto = new CorteCajaDto
                {
                    IdCorteCaja = corte.IdCorteCaja,
                    FolioCorteCaja = corte.FolioCorteCaja,
                    FechaInicio = corte.FechaInicio,
                    FechaFin = corte.FechaFin,
                    IdUsuarioCaja = corte.IdUsuarioCaja,
                    IdCaja = corte.IdCaja,
                    MontoInicial = corte.MontoInicial,
                    TotalEfectivo = corte.TotalEfectivo,
                    TotalTransferencia = corte.TotalTransferencia,
                    TotalTarjeta = corte.TotalTarjeta,
                    TotalGeneral = corte.TotalGeneral,
                    Cerrado = corte.Cerrado,
                    FechaCierre = corte.FechaCierre,
                    CerradoPor = corte.CerradoPor,
                    Observaciones = corte.Observaciones,
                    NombreUsuario = usuario != null ? $"{usuario.Nombres} {usuario.Apellidos}" : ""
                };

                return Ok(corteCajaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener corte de caja", error = ex.Message });
            }
        }

        [HttpGet("cajeros")]
        public async Task<IActionResult> ObtenerCajeros()
        {
            try
            {
                var cajeros = await _cajaService.ObtenerCajerosAsync();
                return Ok(cajeros);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener cajeros", error = ex.Message });
            }
        }

        [HttpPost("corte/generar")]
        public async Task<IActionResult> GenerarCorteCajaDetallado([FromBody] GenerarCorteCajaRequest request)
        {
            try
            {
                var resumen = await _cajaService.GenerarCorteCajaDetalladoAsync(
                    request.IdUsuarioCaja,
                    request.FechaInicio.ToUniversalTime(),
                    request.FechaFin.ToUniversalTime()
                );
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar corte de caja", error = ex.Message });
            }
        }

        [HttpPost("corte/pdf")]
        public async Task<IActionResult> GenerarPdfCorteCaja([FromBody] GenerarCorteCajaRequest request)
        {
            try
            {
                var resumen = await _cajaService.GenerarCorteCajaDetalladoAsync(
                    request.IdUsuarioCaja,
                    request.FechaInicio.ToUniversalTime(),
                    request.FechaFin.ToUniversalTime()
                );

                var pdfBytes = _cajaService.GenerarPdfCorteCaja(resumen);

                var fechaCorte = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivo = $"CorteCaja_{fechaCorte}.pdf";

                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar PDF del corte de caja", error = ex.Message });
            }
        }

        [HttpGet("corte/activo")]
        public async Task<IActionResult> ObtenerCorteActivo()
        {
            try
            {
                var usuarioId = User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var corteActivo = await _cajaService.ObtenerCorteActivo(usuarioId);

                if (corteActivo == null)
                {
                    return Ok((CorteCajaDto?)null);
                }

                var usuario = await _authService.GetUserById(corteActivo.IdUsuarioCaja);
                var corteCajaDto = new CorteCajaDto
                {
                    IdCorteCaja = corteActivo.IdCorteCaja,
                    FolioCorteCaja = corteActivo.FolioCorteCaja,
                    FechaInicio = corteActivo.FechaInicio,
                    FechaFin = corteActivo.FechaFin,
                    IdUsuarioCaja = corteActivo.IdUsuarioCaja,
                    IdCaja = corteActivo.IdCaja,
                    MontoInicial = corteActivo.MontoInicial,
                    TotalEfectivo = corteActivo.TotalEfectivo,
                    TotalTransferencia = corteActivo.TotalTransferencia,
                    TotalTarjeta = corteActivo.TotalTarjeta,
                    TotalGeneral = corteActivo.TotalGeneral,
                    Cerrado = corteActivo.Cerrado,
                    FechaCierre = corteActivo.FechaCierre,
                    CerradoPor = corteActivo.CerradoPor,
                    Observaciones = corteActivo.Observaciones,
                    NombreUsuario = usuario != null ? $"{usuario.Nombres} {usuario.Apellidos}" : ""
                };

                return Ok(corteCajaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener corte activo", error = ex.Message });
            }
        }

        [HttpGet("recibos-todos")]
        public async Task<IActionResult> BuscarTodosLosRecibos([FromQuery] string criterio)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(criterio))
                {
                    return BadRequest(new { message = "Debe proporcionar un criterio de búsqueda" });
                }

                var resultado = await _cajaService.BuscarTodosLosRecibosAsync(criterio);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar recibos", error = ex.Message });
            }
        }

        [HttpGet("comprobante/{idPago}/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerComprobantePdf(long idPago, CancellationToken ct)
        {
            try
            {
                var comprobante = await _pagoService.ObtenerDatosComprobanteAsync(idPago, ct);

                if (comprobante == null)
                {
                    return NotFound(new { message = "Pago no encontrado" });
                }

                var pdfService = HttpContext.RequestServices.GetRequiredService<IPdfService>();
                var pdfBytes = pdfService.GenerarComprobantePago(comprobante);

                var nombreArchivo = $"Comprobante_{comprobante.Pago.FolioPago}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar comprobante", error = ex.Message });
            }
        }

        [HttpGet("comprobante/{idPago}")]
        public async Task<IActionResult> ObtenerDatosComprobante(long idPago, CancellationToken ct)
        {
            try
            {
                var comprobante = await _pagoService.ObtenerDatosComprobanteAsync(idPago, ct);

                if (comprobante == null)
                {
                    return NotFound(new { message = "Pago no encontrado" });
                }

                return Ok(comprobante);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener comprobante", error = ex.Message });
            }
        }

        [HttpPost("recibos/{idRecibo}/quitar-recargo")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS}")]
        public async Task<IActionResult> QuitarRecargo(long idRecibo, [FromBody] QuitarRecargoRequest request)
        {
            try
            {
                var usuarioId = User.FindFirst("userId")?.Value;
                var nombreUsuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var resultado = await _cajaService.QuitarRecargoAsync(idRecibo, request.Motivo, nombreUsuario, usuarioId);

                if (!resultado.Exitoso)
                {
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return Ok(new {
                    message = resultado.Mensaje,
                    recargoCondonado = resultado.RecargoCondonado
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al quitar recargo", error = ex.Message });
            }
        }

        [HttpPut("recibos/{idRecibo}/detalles/{idReciboDetalle}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS}")]
        public async Task<IActionResult> ModificarDetalleRecibo(long idRecibo, long idReciboDetalle, [FromBody] ModificarDetalleRequest request)
        {
            try
            {
                var usuarioId = User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                if (request.NuevoMonto < 0)
                {
                    return BadRequest(new { message = "El monto no puede ser negativo" });
                }

                var resultado = await _cajaService.ModificarDetalleReciboAsync(idRecibo, idReciboDetalle, request.NuevoMonto, request.Motivo, usuarioId);

                if (!resultado.Exitoso)
                {
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al modificar detalle del recibo", error = ex.Message });
            }
        }

        [HttpPut("recibos/{idRecibo}/recargo")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS}")]
        public async Task<IActionResult> ModificarRecargoRecibo(long idRecibo, [FromBody] ModificarRecargoRequest request)
        {
            try
            {
                var usuarioId = User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                if (request.NuevoRecargo < 0)
                {
                    return BadRequest(new { message = "El recargo no puede ser negativo" });
                }

                var resultado = await _cajaService.ModificarRecargoReciboAsync(idRecibo, request.NuevoRecargo, request.Motivo, usuarioId);

                if (!resultado.Exitoso)
                {
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al modificar recargo del recibo", error = ex.Message });
            }
        }

        [HttpGet("diagnostico/pagos")]
        [AllowAnonymous]
        public async Task<IActionResult> DiagnosticoPagos([FromQuery] long? idPagoDesde = null)
        {
            try
            {
                var db = HttpContext.RequestServices.GetRequiredService<WebApplication2.Data.DbContexts.ApplicationDbContext>();

                var query = db.Pago.AsQueryable();
                if (idPagoDesde.HasValue)
                {
                    query = query.Where(p => p.IdPago >= idPagoDesde.Value);
                }
                else
                {
                    query = query.OrderByDescending(p => p.IdPago).Take(20);
                }

                var pagos = await query
                    .Include(p => p.MedioPago)
                    .OrderByDescending(p => p.IdPago)
                    .ToListAsync();

                var diagnostico = new List<object>();

                foreach (var pago in pagos)
                {
                    var apps = await db.PagoAplicacion
                        .Where(pa => pa.IdPago == pago.IdPago)
                        .ToListAsync();

                    var detalles = new List<object>();

                    foreach (var app in apps)
                    {
                        var detalle = await db.ReciboDetalle
                            .Include(rd => rd.Recibo)
                            .FirstOrDefaultAsync(rd => rd.IdReciboDetalle == app.IdReciboDetalle);

                        if (detalle != null)
                        {
                            detalles.Add(new
                            {
                                IdPagoAplicacion = app.IdPagoAplicacion,
                                IdReciboDetalle = app.IdReciboDetalle,
                                MontoAplicado = app.MontoAplicado,
                                Descripcion = detalle.Descripcion,
                                IdRecibo = detalle.IdRecibo,
                                FolioRecibo = detalle.Recibo?.Folio,
                                IdEstudiante = detalle.Recibo?.IdEstudiante,
                                IdAspirante = detalle.Recibo?.IdAspirante
                            });
                        }
                    }

                    diagnostico.Add(new
                    {
                        IdPago = pago.IdPago,
                        FolioPago = pago.FolioPago,
                        Monto = pago.Monto,
                        FechaPago = pago.FechaPagoUtc,
                        IdMedioPago = pago.IdMedioPago,
                        MedioPago = pago.MedioPago?.Descripcion ?? "NO CARGADO",
                        NumAplicaciones = apps.Count,
                        Aplicaciones = detalles
                    });
                }

                return Ok(new
                {
                    TotalPagos = pagos.Count,
                    Pagos = diagnostico
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error en diagnóstico", error = ex.Message, stack = ex.StackTrace });
            }
        }
    }
}
