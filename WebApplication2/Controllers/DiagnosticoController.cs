using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Diagnostico;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Controllers
{
    [Route("api/diagnostico")]
    [ApiController]
    [AllowAnonymous]
    public class DiagnosticoController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public DiagnosticoController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("recibos-pagados")]
        public async Task<IActionResult> RecibosPagados()
        {
            var recibos = await _db.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.Estatus == EstatusRecibo.PAGADO || r.Saldo == 0)
                .OrderByDescending(r => r.UpdatedAt)
                .Take(20)
                .Select(r => new
                {
                    r.IdRecibo,
                    r.Folio,
                    r.IdEstudiante,
                    r.IdAspirante,
                    r.Total,
                    r.Saldo,
                    Estatus = r.Estatus.ToString(),
                    NumDetalles = r.Detalles.Count,
                    r.UpdatedAt
                })
                .ToListAsync();

            return Ok(recibos);
        }

        [HttpGet("pagos-con-aplicaciones")]
        public async Task<IActionResult> PagosConAplicaciones()
        {
            var aplicaciones = await _db.PagoAplicacion.ToListAsync();

            var pagoIdsConApps = aplicaciones.Select(a => a.IdPago).Distinct().ToList();

            var pagos = await _db.Pago
                .Where(p => pagoIdsConApps.Contains(p.IdPago))
                .OrderByDescending(p => p.IdPago)
                .Take(20)
                .ToListAsync();

            var resultado = new List<object>();

            foreach (var pago in pagos)
            {
                var apps = aplicaciones.Where(a => a.IdPago == pago.IdPago).ToList();
                var recibosInfo = new List<object>();

                foreach (var app in apps)
                {
                    var detalle = await _db.ReciboDetalle
                        .Include(rd => rd.Recibo)
                        .FirstOrDefaultAsync(rd => rd.IdReciboDetalle == app.IdReciboDetalle);

                    if (detalle?.Recibo != null)
                    {
                        recibosInfo.Add(new
                        {
                            IdRecibo = detalle.IdRecibo,
                            Folio = detalle.Recibo.Folio,
                            MontoAplicado = app.MontoAplicado,
                            IdEstudiante = detalle.Recibo.IdEstudiante,
                            IdAspirante = detalle.Recibo.IdAspirante
                        });
                    }
                }

                resultado.Add(new
                {
                    IdPago = pago.IdPago,
                    Monto = pago.Monto,
                    FechaPago = pago.FechaPagoUtc,
                    NumAplicaciones = apps.Count,
                    Recibos = recibosInfo
                });
            }

            return Ok(new
            {
                TotalPagosConAplicaciones = pagoIdsConApps.Count,
                Pagos = resultado
            });
        }

        [HttpPost("vincular-pago-manual")]
        public async Task<IActionResult> VincularPagoManual([FromBody] VincularPagoRequest request)
        {
            var pago = await _db.Pago
                .Include(p => p.Aplicaciones)
                .FirstOrDefaultAsync(p => p.IdPago == request.IdPago);

            if (pago == null)
                return NotFound(new { message = "Pago no encontrado" });

            if (pago.Aplicaciones.Any())
                return BadRequest(new { message = "El pago ya tiene aplicaciones" });

            var resultados = new List<object>();
            decimal totalAplicado = 0;

            foreach (var idRecibo in request.IdsRecibos)
            {
                var recibo = await _db.Recibo
                    .Include(r => r.Detalles)
                    .FirstOrDefaultAsync(r => r.IdRecibo == idRecibo);

                if (recibo == null || !recibo.Detalles.Any())
                {
                    resultados.Add(new { IdRecibo = idRecibo, Error = "Recibo no encontrado o sin detalles" });
                    continue;
                }

                var detalle = recibo.Detalles.First();
                var montoAAplicar = recibo.Total;

                var aplicacion = new PagoAplicacion
                {
                    IdPago = pago.IdPago,
                    IdReciboDetalle = detalle.IdReciboDetalle,
                    MontoAplicado = montoAAplicar
                };
                _db.PagoAplicacion.Add(aplicacion);
                totalAplicado += montoAAplicar;

                resultados.Add(new
                {
                    IdRecibo = idRecibo,
                    Folio = recibo.Folio,
                    MontoAplicado = montoAAplicar,
                    Vinculado = true
                });
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                IdPago = pago.IdPago,
                MontoPago = pago.Monto,
                TotalAplicado = totalAplicado,
                Diferencia = pago.Monto - totalAplicado,
                Resultados = resultados
            });
        }

        [HttpGet("recibos-para-pago/{idPago}")]
        public async Task<IActionResult> RecibosCercanosAPago(long idPago)
        {
            var pago = await _db.Pago.FindAsync(idPago);
            if (pago == null)
                return NotFound(new { message = "Pago no encontrado" });

            var fechaMin = pago.FechaPagoUtc.AddMinutes(-10);
            var fechaMax = pago.FechaPagoUtc.AddMinutes(10);

            var recibosCercanos = await _db.Recibo
                .Include(r => r.Detalles)
                .Where(r => (r.Estatus == EstatusRecibo.PAGADO || r.Saldo == 0))
                .Where(r => r.UpdatedAt >= fechaMin && r.UpdatedAt <= fechaMax)
                .Select(r => new
                {
                    r.IdRecibo,
                    r.Folio,
                    r.Total,
                    r.IdEstudiante,
                    r.IdAspirante,
                    r.UpdatedAt,
                    TieneDetalles = r.Detalles.Any()
                })
                .ToListAsync();

            return Ok(new
            {
                Pago = new { pago.IdPago, pago.Monto, pago.FechaPagoUtc },
                SumaRecibos = recibosCercanos.Sum(r => r.Total),
                RecibosCercanos = recibosCercanos
            });
        }

        [HttpPost("vincular-pagos")]
        public async Task<IActionResult> VincularPagosHuerfanos()
        {
            var resultados = new List<object>();

            var pagosHuerfanos = await _db.Pago
                .Include(p => p.Aplicaciones)
                .Where(p => !p.Aplicaciones.Any() && p.Estatus == EstatusPago.CONFIRMADO)
                .OrderBy(p => p.FechaPagoUtc)
                .ToListAsync();

            foreach (var pago in pagosHuerfanos)
            {

                var fechaMin = pago.FechaPagoUtc.AddMinutes(-5);
                var fechaMax = pago.FechaPagoUtc.AddMinutes(5);

                var reciboCandidato = await _db.Recibo
                    .Include(r => r.Detalles)
                    .Where(r => r.Estatus == EstatusRecibo.PAGADO || r.Saldo == 0)
                    .Where(r => r.UpdatedAt >= fechaMin && r.UpdatedAt <= fechaMax)
                    .Where(r => Math.Abs(r.Total - pago.Monto) < 1) 
                    .FirstOrDefaultAsync();

                if (reciboCandidato != null && reciboCandidato.Detalles.Any())
                {
                    var yaVinculado = await _db.PagoAplicacion
                        .AnyAsync(pa => reciboCandidato.Detalles.Select(d => d.IdReciboDetalle).Contains(pa.IdReciboDetalle));

                    if (!yaVinculado)
                    {
                        var detalle = reciboCandidato.Detalles.First();
                        var aplicacion = new PagoAplicacion
                        {
                            IdPago = pago.IdPago,
                            IdReciboDetalle = detalle.IdReciboDetalle,
                            MontoAplicado = pago.Monto
                        };
                        _db.PagoAplicacion.Add(aplicacion);

                        resultados.Add(new
                        {
                            Vinculado = true,
                            IdPago = pago.IdPago,
                            Monto = pago.Monto,
                            IdRecibo = reciboCandidato.IdRecibo,
                            FolioRecibo = reciboCandidato.Folio,
                            IdEstudiante = reciboCandidato.IdEstudiante,
                            IdAspirante = reciboCandidato.IdAspirante
                        });
                    }
                    else
                    {
                        resultados.Add(new
                        {
                            Vinculado = false,
                            IdPago = pago.IdPago,
                            Monto = pago.Monto,
                            Razon = "El recibo candidato ya tiene aplicaciones de otro pago"
                        });
                    }
                }
                else
                {
                    resultados.Add(new
                    {
                        Vinculado = false,
                        IdPago = pago.IdPago,
                        Monto = pago.Monto,
                        FechaPago = pago.FechaPagoUtc,
                        Razon = "No se encontró recibo candidato por monto y fecha"
                    });
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                TotalPagosHuerfanos = pagosHuerfanos.Count,
                Vinculados = resultados.Count(r => ((dynamic)r).Vinculado == true),
                Resultados = resultados
            });
        }

        [HttpGet("pagos")]
        public async Task<IActionResult> DiagnosticoPagos([FromQuery] long? idPagoDesde = null)
        {
            try
            {
                var query = _db.Pago.AsQueryable();
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
                    .Include(p => p.Aplicaciones)
                    .OrderByDescending(p => p.IdPago)
                    .ToListAsync();

                var diagnostico = new List<object>();

                foreach (var pago in pagos)
                {
                    var apps = pago.Aplicaciones.ToList();
                    var detalles = new List<object>();

                    foreach (var app in apps)
                    {
                        var detalle = await _db.ReciboDetalle
                            .Include(rd => rd.Recibo)
                            .FirstOrDefaultAsync(rd => rd.IdReciboDetalle == app.IdReciboDetalle);

                        if (detalle != null)
                        {
                            string? nombreEstudiante = null;
                            string? matricula = null;

                            if (detalle.Recibo?.IdEstudiante != null)
                            {
                                var estudiante = await _db.Estudiante
                                    .Include(e => e.IdPersonaNavigation)
                                    .FirstOrDefaultAsync(e => e.IdEstudiante == detalle.Recibo.IdEstudiante);

                                if (estudiante != null)
                                {
                                    matricula = estudiante.Matricula;
                                    nombreEstudiante = estudiante.IdPersonaNavigation != null
                                        ? $"{estudiante.IdPersonaNavigation.Nombre} {estudiante.IdPersonaNavigation.ApellidoPaterno}"
                                        : null;
                                }
                            }

                            detalles.Add(new
                            {
                                IdPagoAplicacion = app.IdPagoAplicacion,
                                IdReciboDetalle = app.IdReciboDetalle,
                                MontoAplicado = app.MontoAplicado,
                                Descripcion = detalle.Descripcion,
                                IdRecibo = detalle.IdRecibo,
                                FolioRecibo = detalle.Recibo?.Folio,
                                IdEstudiante = detalle.Recibo?.IdEstudiante,
                                IdAspirante = detalle.Recibo?.IdAspirante,
                                Matricula = matricula,
                                NombreEstudiante = nombreEstudiante
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
                    Mensaje = pagos.All(p => p.Aplicaciones.Count == 0)
                        ? "PROBLEMA: Los pagos no tienen PagoAplicacion vinculadas. Los pagos se registraron sin crear las aplicaciones."
                        : "Los pagos tienen aplicaciones vinculadas.",
                    Pagos = diagnostico
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error en diagnóstico", error = ex.Message, stack = ex.StackTrace });
            }
        }

        [HttpPost("vincular-todos-auto")]
        public async Task<IActionResult> VincularTodosAutomaticamente()
        {
            var resultados = new List<object>();

            var pagosHuerfanos = await _db.Pago
                .Include(p => p.Aplicaciones)
                .Where(p => !p.Aplicaciones.Any() && p.Estatus == EstatusPago.CONFIRMADO)
                .OrderBy(p => p.FechaPagoUtc)
                .ToListAsync();

            foreach (var pago in pagosHuerfanos)
            {
                var fechaMin = pago.FechaPagoUtc.AddSeconds(-2);
                var fechaMax = pago.FechaPagoUtc.AddSeconds(2);

                var recibosCercanos = await _db.Recibo
                    .Include(r => r.Detalles)
                    .Where(r => (r.Estatus == EstatusRecibo.PAGADO || r.Saldo == 0))
                    .Where(r => r.UpdatedAt >= fechaMin && r.UpdatedAt <= fechaMax)
                    .ToListAsync();

                if (recibosCercanos.Any())
                {
                    var aplicacionesCreadas = new List<object>();
                    decimal montoRestante = pago.Monto;

                    foreach (var recibo in recibosCercanos.OrderBy(r => r.UpdatedAt))
                    {
                        if (!recibo.Detalles.Any()) continue;

                        var yaVinculado = await _db.PagoAplicacion
                            .AnyAsync(pa => recibo.Detalles.Select(d => d.IdReciboDetalle).Contains(pa.IdReciboDetalle));

                        if (yaVinculado) continue;

                        var detalle = recibo.Detalles.First();
                        var montoAAplicar = Math.Min(recibo.Total, montoRestante);

                        if (montoAAplicar <= 0) continue;

                        var aplicacion = new PagoAplicacion
                        {
                            IdPago = pago.IdPago,
                            IdReciboDetalle = detalle.IdReciboDetalle,
                            MontoAplicado = montoAAplicar
                        };
                        _db.PagoAplicacion.Add(aplicacion);
                        montoRestante -= montoAAplicar;

                        aplicacionesCreadas.Add(new
                        {
                            IdRecibo = recibo.IdRecibo,
                            Folio = recibo.Folio,
                            Total = recibo.Total,
                            MontoAplicado = montoAAplicar,
                            IdEstudiante = recibo.IdEstudiante,
                            IdAspirante = recibo.IdAspirante
                        });
                    }

                    if (aplicacionesCreadas.Any())
                    {
                        resultados.Add(new
                        {
                            Vinculado = true,
                            IdPago = pago.IdPago,
                            MontoPago = pago.Monto,
                            FechaPago = pago.FechaPagoUtc,
                            RecibosVinculados = aplicacionesCreadas.Count,
                            Aplicaciones = aplicacionesCreadas
                        });
                    }
                    else
                    {
                        resultados.Add(new
                        {
                            Vinculado = false,
                            IdPago = pago.IdPago,
                            MontoPago = pago.Monto,
                            Razon = "Los recibos cercanos ya están vinculados a otros pagos"
                        });
                    }
                }
                else
                {
                    resultados.Add(new
                    {
                        Vinculado = false,
                        IdPago = pago.IdPago,
                        MontoPago = pago.Monto,
                        Razon = "No se encontraron recibos actualizados en el mismo momento"
                    });
                }
            }

            await _db.SaveChangesAsync();

            var vinculados = resultados.Count(r => ((dynamic)r).Vinculado == true);

            return Ok(new
            {
                TotalPagosHuerfanos = pagosHuerfanos.Count,
                Vinculados = vinculados,
                NoVinculados = pagosHuerfanos.Count - vinculados,
                Resultados = resultados
            });
        }
    }
}
