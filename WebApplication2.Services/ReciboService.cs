using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Recibo;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Pagos;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class ReciboService : IReciboService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConvenioService _convenioService;
        private readonly IBecaService _becaService;

        public ReciboService(ApplicationDbContext db, IMapper mapper, IConvenioService convenioService, IBecaService becaService)
        {
            _db = db;
            _mapper = mapper;
            _convenioService = convenioService;
            _becaService = becaService;
        }

        private async Task<string> GenerarFolioAsync(CancellationToken ct = default)
        {
            var año = DateTime.UtcNow.Year;
            var prefijo = $"REC-{año}-";

            var ultimoFolio = await _db.Recibo
                .Where(r => r.Folio != null && r.Folio.StartsWith(prefijo))
                .OrderByDescending(r => r.Folio)
                .Select(r => r.Folio)
                .FirstOrDefaultAsync(ct);

            int siguienteNumero = 1;

            if (ultimoFolio != null)
            {
                var numeroStr = ultimoFolio.Substring(prefijo.Length);
                if (int.TryParse(numeroStr, out int numero))
                {
                    siguienteNumero = numero + 1;
                }
            }

            return $"{prefijo}{siguienteNumero:D6}";
        }

        public async Task<IReadOnlyList<ReciboDto>> GenerarRecibosAsync(GenerarRecibosDto dto, CancellationToken ct)
        {
            var asignacion = await _db.PlanPagoAsignacion
                .Include(a => a.PlanPago)
                .ThenInclude(p => p.Detalles)
                .FirstOrDefaultAsync(a => a.IdEstudiante == dto.IdEstudiante
                                       && a.PlanPago.IdPeriodoAcademico == dto.IdPeriodoAcademico, ct);

            if (asignacion == null)
                throw new InvalidOperationException("El estudiante no tiene plan asignado para el periodo.");

            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            var nuevos = new List<Recibo>();

            if (dto.estrategia == EstrategiaEmisionEnum.por_mes)
            {
                foreach (var d in asignacion.PlanPago.Detalles.OrderBy(x => x.MesOffset).ThenBy(x => x.Orden))
                {
                    var r = new Recibo
                    {
                        Folio = await GenerarFolioAsync(ct),
                        IdEstudiante = dto.IdEstudiante,
                        IdPeriodoAcademico = dto.IdPeriodoAcademico,
                        FechaEmision = hoy,
                        FechaVencimiento = new DateOnly(hoy.Year, hoy.Month, Math.Min(dto.DiaVencimiento, (byte)28)),
                        Estatus = EstatusRecibo.PENDIENTE
                    };
                    _db.Recibo.Add(r);
                    await _db.SaveChangesAsync(ct);

                    var rd = new ReciboDetalle
                    {
                        IdRecibo = r.IdRecibo,
                        IdConceptoPago = d.IdConceptoPago,
                        Descripcion = d.Descripcion,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.Importe,
                        RefTabla = "PlanPagoDetalle",
                        RefId = d.IdPlanPagoDetalle
                    };
                    _db.ReciboDetalle.Add(rd);

                    r.Subtotal += d.Cantidad * d.Importe;
                    r.Saldo = r.Subtotal;

                    nuevos.Add(r);
                }
            }
            else
            {
                var r = new Recibo
                {
                    Folio = await GenerarFolioAsync(ct),
                    IdEstudiante = dto.IdEstudiante,
                    IdPeriodoAcademico = dto.IdPeriodoAcademico,
                    FechaEmision = hoy,
                    FechaVencimiento = new DateOnly(hoy.Year, hoy.Month, Math.Min(dto.DiaVencimiento, (byte)28)),
                    Estatus = EstatusRecibo.PENDIENTE
                };
                _db.Recibo.Add(r);
                await _db.SaveChangesAsync(ct);

                foreach (var d in asignacion.PlanPago.Detalles)
                {
                    _db.ReciboDetalle.Add(new ReciboDetalle
                    {
                        IdRecibo = r.IdRecibo,
                        IdConceptoPago = d.IdConceptoPago,
                        Descripcion = d.Descripcion,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.Importe,
                        RefTabla = "PlanPagoDetalle",
                        RefId = d.IdPlanPagoDetalle
                    });
                    r.Subtotal += d.Cantidad * d.Importe;
                }
                r.Saldo = r.Subtotal;
                nuevos.Add(r);
            }

            await _db.SaveChangesAsync(ct);

            await AplicarBecasARecibosAsync(nuevos, dto.IdEstudiante, ct);

            var ids = nuevos.Select(x => x.IdRecibo).ToArray();
            var recs = await _db.Recibo
                .Include(r => r.Detalles)
                .Where(r => ids.Contains(r.IdRecibo))
                .ToListAsync(ct);

            return _mapper.Map<IReadOnlyList<ReciboDto>>(recs);
        }

        private async Task AplicarBecasARecibosAsync(List<Recibo> recibos, int idEstudiante, CancellationToken ct)
        {
            foreach (var recibo in recibos)
            {
                if (!recibo.Detalles.Any())
                {
                    await _db.Entry(recibo).Collection(r => r.Detalles).LoadAsync(ct);
                }

                decimal descuentoTotal = 0m;

                foreach (var detalle in recibo.Detalles)
                {
                    var importeLinea = detalle.Cantidad * detalle.PrecioUnitario;
                    var descuentoLinea = await _becaService.CalcularDescuentoPorBecasAsync(
                        idEstudiante,
                        detalle.IdConceptoPago,
                        importeLinea,
                        recibo.FechaVencimiento,
                        ct);

                    descuentoTotal += descuentoLinea;
                }

                if (descuentoTotal > 0)
                {
                    recibo.Descuento = descuentoTotal;
                    recibo.Saldo = recibo.Subtotal - recibo.Descuento + recibo.Recargos;
                }
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task<ReciboDto?> ObtenerAsync(long idRecibo, CancellationToken ct)
        {
            var r = await _db.Recibo
                .Include(x => x.Detalles)
                .FirstOrDefaultAsync(x => x.IdRecibo == idRecibo, ct);
            return r == null ? null : _mapper.Map<ReciboDto>(r);
        }

        public async Task<IReadOnlyList<ReciboDto>> ListarPorPeriodoAsync(int idPeriodoAcademico, int? idEstudiante, CancellationToken ct)
        {
            var q = _db.Recibo.Include(r => r.Detalles)
                .Where(r => r.IdPeriodoAcademico == idPeriodoAcademico);

            if (idEstudiante.HasValue) q = q.Where(r => r.IdEstudiante == idEstudiante.Value);

            var list = await q.AsNoTracking().ToListAsync(ct);
            return _mapper.Map<IReadOnlyList<ReciboDto>>(list);
        }

        public async Task<IReadOnlyList<ReciboDto>> ListarPorAspiranteAsync(int idAspirante, CancellationToken ct)
        {
            var list = await _db.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.IdAspirante == idAspirante)
                .AsNoTracking()
                .ToListAsync(ct);

            Console.WriteLine($"=== ListarPorAspiranteAsync ===");
            Console.WriteLine($"Aspirante ID: {idAspirante}");
            Console.WriteLine($"Total recibos: {list.Count}");
            foreach (var recibo in list)
            {
                Console.WriteLine($"  Recibo {recibo.IdRecibo} - Detalles: {recibo.Detalles?.Count ?? 0}");
                if (recibo.Detalles != null)
                {
                    foreach (var detalle in recibo.Detalles)
                    {
                        Console.WriteLine($"    - {detalle.IdReciboDetalle}: {detalle.Descripcion} x{detalle.Cantidad} = {detalle.Importe}");
                    }
                }
            }

            var result = _mapper.Map<IReadOnlyList<ReciboDto>>(list);

            Console.WriteLine($"Después del mapeo:");
            Console.WriteLine($"Total DTOs: {result.Count}");
            foreach (var dto in result)
            {
                Console.WriteLine($"  DTO Recibo {dto.IdRecibo} - Detalles: {dto.Detalles?.Count ?? 0}");
            }

            return result;
        }

        public async Task<int> RecalcularRecargosAsync(int idPeriodoAcademico, DateOnly? fechaCorte, CancellationToken ct)
        {
            var hoy = fechaCorte ?? DateOnly.FromDateTime(DateTime.UtcNow);

            var recargosPolitica = await _db.RecargoPolitica
                .Where(p => p.Activo).FirstOrDefaultAsync(ct);
            if (recargosPolitica == null) return 0;

            var pendientes = await _db.Recibo
                .Where(r => r.IdPeriodoAcademico == idPeriodoAcademico &&
                            (r.Estatus == EstatusRecibo.PENDIENTE || r.Estatus == EstatusRecibo.PARCIAL))
                .ToListAsync(ct);

            var actualizados = 0;
            foreach (var r in pendientes)
            {
                if (r.FechaVencimiento.Day <= recargosPolitica.DiaFinGracia) continue;

                var diasMora = Math.Max(0, (hoy.ToDateTime(TimeOnly.MinValue) - r.FechaVencimiento.ToDateTime(TimeOnly.MinValue)).Days);
                if (recargosPolitica.TopeDiasMora.HasValue)
                    diasMora = Math.Min(diasMora, recargosPolitica.TopeDiasMora.Value);

                if (diasMora <= 0) continue;

                var recargo = r.Saldo * recargosPolitica.TasaDiaria * diasMora;
                if (recargosPolitica.RecargoMinimo.HasValue) recargo = Math.Max(recargo, recargosPolitica.RecargoMinimo.Value);
                if (recargosPolitica.RecargoMaximo.HasValue) recargo = Math.Min(recargo, recargosPolitica.RecargoMaximo.Value);

                r.Recargos = Math.Round(recargo, 2);
                if (r.Saldo > 0 && r.FechaVencimiento.ToDateTime(TimeOnly.MinValue) < hoy.ToDateTime(TimeOnly.MinValue))
                    r.Estatus = EstatusRecibo.VENCIDO;

                actualizados++;
            }

            await _db.SaveChangesAsync(ct);
            return actualizados;
        }

        public async Task<ReciboDto> GenerarReciboAspiranteAsync(int idAspirante, decimal monto, string concepto, int diasVencimiento, CancellationToken ct)
        {
            var aspirante = await _db.Aspirante.FindAsync(new object[] { idAspirante }, ct);
            if (aspirante == null)
                throw new InvalidOperationException($"No se encontró el aspirante con ID {idAspirante}");

            var conceptoPago = await _db.ConceptoPago.FirstOrDefaultAsync(c => c.Clave == "INSC", ct);
            if (conceptoPago == null)
            {
                conceptoPago = new ConceptoPago
                {
                    Clave = "INSC",
                    Descripcion = "Inscripción",
                    Activo = true
                };
                _db.ConceptoPago.Add(conceptoPago);
                await _db.SaveChangesAsync(ct);
            }

            string tipoConcepto = concepto.ToUpperInvariant().Contains("INSCRIPCI") ? "INSCRIPCION" : "COLEGIATURA";

            decimal descuentoConvenio = await _convenioService.CalcularDescuentoTotalAspiranteAsync(idAspirante, monto, tipoConcepto, ct);

            var fechaEmision = DateOnly.FromDateTime(DateTime.UtcNow);
            var fechaVencimiento = fechaEmision.AddDays(diasVencimiento);
            var saldoFinal = monto - descuentoConvenio;

            var recibo = new Recibo
            {
                Folio = await GenerarFolioAsync(ct),
                IdAspirante = idAspirante,
                FechaEmision = fechaEmision,
                FechaVencimiento = fechaVencimiento,
                Estatus = EstatusRecibo.PENDIENTE,
                Subtotal = monto,
                Descuento = descuentoConvenio,
                Recargos = 0,
                Saldo = saldoFinal,
                Notas = descuentoConvenio > 0
                    ? $"Recibo de {concepto} - Descuento por convenio: ${descuentoConvenio:N2}"
                    : $"Recibo de {concepto} generado automáticamente"
            };

            _db.Recibo.Add(recibo);
            await _db.SaveChangesAsync(ct);

            var detalle = new ReciboDetalle
            {
                IdRecibo = recibo.IdRecibo,
                IdConceptoPago = conceptoPago.IdConceptoPago,
                Descripcion = concepto,
                Cantidad = 1,
                PrecioUnitario = monto
            };

            _db.ReciboDetalle.Add(detalle);
            await _db.SaveChangesAsync(ct);

            if (descuentoConvenio > 0)
            {
                await _convenioService.IncrementarAplicacionesConvenioAsync(idAspirante, tipoConcepto, ct);
            }

            var reciboCreado = await _db.Recibo
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.IdRecibo == recibo.IdRecibo, ct);

            return _mapper.Map<ReciboDto>(reciboCreado);
        }

        public async Task<RecalcularDescuentosResultDto> RecalcularDescuentosConvenioAspiranteAsync(int idAspirante, CancellationToken ct)
        {
            var recibos = await _db.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.IdAspirante == idAspirante
                    && (r.Estatus == EstatusRecibo.PENDIENTE
                        || r.Estatus == EstatusRecibo.PARCIAL
                        || r.Estatus == EstatusRecibo.VENCIDO))
                .ToListAsync(ct);

            var resultado = new RecalcularDescuentosResultDto();

            foreach (var recibo in recibos)
            {
                var descuentoAnterior = recibo.Descuento;
                var subtotal = recibo.Subtotal;

                string tipoConcepto = "COLEGIATURA";
                var primerDetalle = recibo.Detalles.FirstOrDefault();
                if (primerDetalle != null)
                {
                    var desc = primerDetalle.Descripcion?.ToUpperInvariant() ?? "";
                    if (desc.Contains("INSCRIPCI") || desc.Contains("FICHA"))
                        tipoConcepto = "INSCRIPCION";
                }

                var descuentoNuevo = await _convenioService.CalcularDescuentoTotalAspiranteAsync(
                    idAspirante, subtotal, tipoConcepto, ct);

                var totalConDescuentoAnterior = subtotal - descuentoAnterior + recibo.Recargos;
                var pagosRealizados = totalConDescuentoAnterior - recibo.Saldo;
                if (pagosRealizados < 0) pagosRealizados = 0;

                var nuevoTotal = subtotal - descuentoNuevo + recibo.Recargos;
                var nuevoSaldo = nuevoTotal - pagosRealizados;
                if (nuevoSaldo < 0) nuevoSaldo = 0;

                recibo.Descuento = descuentoNuevo;
                recibo.Saldo = nuevoSaldo;

                resultado.Detalle.Add(new ReciboDescuentoResumenDto
                {
                    IdRecibo = recibo.IdRecibo,
                    Folio = recibo.Folio,
                    SubtotalOriginal = subtotal,
                    DescuentoAnterior = descuentoAnterior,
                    DescuentoNuevo = descuentoNuevo,
                    SaldoNuevo = nuevoSaldo
                });

                if (descuentoAnterior != descuentoNuevo)
                {
                    resultado.RecibosActualizados++;
                    resultado.DescuentoTotalAplicado += descuentoNuevo;
                }
            }

            await _db.SaveChangesAsync(ct);
            return resultado;
        }

        public async Task<ReciboDto> GenerarReciboAspiranteConConceptoAsync(int idAspirante, int idConceptoPago, int diasVencimiento, CancellationToken ct)
        {
            var concepto = await _db.ConceptoPago
                .Include(c => c.Precios)
                .FirstOrDefaultAsync(c => c.IdConceptoPago == idConceptoPago, ct);

            if (concepto == null)
                throw new InvalidOperationException($"No se encontró el ConceptoPago con ID {idConceptoPago}");

            var aspirante = await _db.Aspirante
                .Include(a => a.IdPlanNavigation)
                .FirstOrDefaultAsync(a => a.IdAspirante == idAspirante, ct);

            if (aspirante == null)
                throw new InvalidOperationException($"No se encontró el aspirante con ID {idAspirante}");

            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            var idPlan = aspirante.IdPlan;
            var idCampus = aspirante.IdPlanNavigation?.IdCampus;

            var precio = concepto.Precios
                .Where(p => p.Activo && p.VigenciaDesde <= hoy && (p.VigenciaHasta == null || p.VigenciaHasta >= hoy))
                .OrderByDescending(p => p.IdPlanEstudios.HasValue && p.IdCampus.HasValue ? 3
                    : p.IdPlanEstudios.HasValue ? 2
                    : p.IdCampus.HasValue ? 1
                    : 0)
                .ThenByDescending(p => p.VigenciaDesde)
                .FirstOrDefault(p =>
                    (p.IdPlanEstudios == null || p.IdPlanEstudios == idPlan) &&
                    (p.IdCampus == null || p.IdCampus == idCampus));

            if (precio == null)
                throw new InvalidOperationException($"No se encontró un precio vigente para el concepto '{concepto.Nombre}'");

            return await GenerarReciboAspiranteAsync(idAspirante, precio.Importe, concepto.Nombre ?? concepto.Clave, diasVencimiento, ct);
        }

        public async Task<int> RepararRecibosSinDetallesAsync(CancellationToken ct)
        {
            var recibosSinDetalles = await _db.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.Detalles.Count == 0)
                .ToListAsync(ct);

            if (recibosSinDetalles.Count == 0)
            {
                Console.WriteLine("No hay recibos sin detalles para reparar");
                return 0;
            }

            Console.WriteLine($"=== REPARANDO {recibosSinDetalles.Count} RECIBOS SIN DETALLES ===");

            var conceptoPago = await _db.ConceptoPago.FirstOrDefaultAsync(c => c.Clave == "INSC", ct);
            if (conceptoPago == null)
            {
                Console.WriteLine("Creando concepto de pago INSC");
                conceptoPago = new ConceptoPago
                {
                    Clave = "INSC",
                    Descripcion = "Inscripción",
                    Activo = true
                };
                _db.ConceptoPago.Add(conceptoPago);
                await _db.SaveChangesAsync(ct);
            }

            var reparados = 0;
            foreach (var recibo in recibosSinDetalles)
            {
                Console.WriteLine($"Reparando recibo {recibo.IdRecibo} - Subtotal: {recibo.Subtotal}");

                var detalle = new ReciboDetalle
                {
                    IdRecibo = recibo.IdRecibo,
                    IdConceptoPago = conceptoPago.IdConceptoPago,
                    Descripcion = recibo.Notas ?? "Cuota de Inscripción",
                    Cantidad = 1,
                    PrecioUnitario = recibo.Subtotal
                };

                _db.ReciboDetalle.Add(detalle);
                reparados++;
            }

            Console.WriteLine($"Guardando {reparados} detalles de recibo...");
            await _db.SaveChangesAsync(ct);

            _db.ChangeTracker.Clear();

            Console.WriteLine($"Reparación completada. {reparados} detalles creados");

            return reparados;
        }

        public async Task<bool> EliminarReciboAsync(long idRecibo, CancellationToken ct)
        {
            var recibo = await _db.Recibo
                .Include(r => r.Detalles)
                .ThenInclude(d => d.Aplicaciones)
                .FirstOrDefaultAsync(r => r.IdRecibo == idRecibo, ct);

            if (recibo == null)
                throw new InvalidOperationException($"No se encontró el recibo con ID {idRecibo}");

            var tienePagos = recibo.Detalles.Any(d => d.Aplicaciones.Any());
            if (tienePagos)
                throw new InvalidOperationException("No se puede eliminar un recibo que tiene pagos aplicados");

            _db.ReciboDetalle.RemoveRange(recibo.Detalles);

            _db.Recibo.Remove(recibo);

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<ReciboPdfDto?> ObtenerParaPdfAsync(long idRecibo, CancellationToken ct)
        {
            var recibo = await _db.Recibo
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.IdRecibo == idRecibo, ct);

            if (recibo == null)
                return null;

            string? matricula = null;
            string nombreCompleto = "Sin información";
            string carrera = "N/A";

            if (recibo.IdEstudiante.HasValue)
            {
                var estudiante = await _db.Estudiante
                    .Include(e => e.IdPersonaNavigation)
                    .Include(e => e.EstudiantePlan)
                        .ThenInclude(ep => ep.IdPlanEstudiosNavigation)
                    .FirstOrDefaultAsync(e => e.IdEstudiante == recibo.IdEstudiante, ct);

                if (estudiante != null)
                {
                    matricula = estudiante.Matricula;
                    var persona = estudiante.IdPersonaNavigation;
                    if (persona != null)
                    {
                        nombreCompleto = $"{persona.ApellidoPaterno} {persona.ApellidoMaterno} {persona.Nombre}".Trim();
                    }
                    var plan = estudiante.EstudiantePlan?.FirstOrDefault()?.IdPlanEstudiosNavigation;
                    if (plan != null)
                    {
                        carrera = plan.NombrePlanEstudios ?? "N/A";
                    }
                }
            }

            string periodo = "N/A";
            if (recibo.IdPeriodoAcademico.HasValue)
            {
                var periodoAcademico = await _db.PeriodoAcademico
                    .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == recibo.IdPeriodoAcademico, ct);
                if (periodoAcademico != null)
                {
                    periodo = periodoAcademico.Nombre ?? "N/A";
                }
            }

            DateTime? fechaPago = null;
            var detalleIds = recibo.Detalles.Select(d => d.IdReciboDetalle).ToList();
            if (detalleIds.Any())
            {
                var primerPago = await _db.PagoAplicacion
                    .Include(pa => pa.Pago)
                    .Where(pa => detalleIds.Contains(pa.IdReciboDetalle))
                    .OrderBy(pa => pa.Pago.FechaPagoUtc)
                    .FirstOrDefaultAsync(ct);

                if (primerPago != null)
                {
                    fechaPago = primerPago.Pago.FechaPagoUtc;
                }
            }

            decimal recargosCalculados = recibo.Recargos;

            Console.WriteLine($"[PDF] Recibo {recibo.Folio}: Usando recargos de BD=${recibo.Recargos}");

            var totalConRecargos = recibo.Subtotal - recibo.Descuento + recargosCalculados;

            return new ReciboPdfDto
            {
                IdRecibo = recibo.IdRecibo,
                Folio = recibo.Folio,
                Matricula = matricula,
                NombreEstudiante = nombreCompleto,
                Carrera = carrera,
                Periodo = periodo,
                FechaEmision = recibo.FechaEmision,
                FechaVencimiento = recibo.FechaVencimiento,
                Subtotal = recibo.Subtotal,
                Descuento = recibo.Descuento,
                Recargos = recargosCalculados,
                Total = totalConRecargos,
                Saldo = recibo.Saldo + recargosCalculados,
                Notas = recibo.Notas,
                EstaPagado = recibo.Estatus == EstatusRecibo.PAGADO || recibo.Saldo == 0,
                FechaPago = fechaPago,
                Detalles = recibo.Detalles.Select((d, idx) => new ReciboDetallePdfDto
                {
                    Numero = idx + 1,
                    Descripcion = d.Descripcion,
                    Cantidad = (int)d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Importe = d.Importe
                }).ToList(),
                Institucion = new InstitucionPdfDto()
            };
        }

        public async Task<ReciboBusquedaResultadoDto> BuscarRecibosAsync(ReciboBusquedaFiltrosDto filtros, CancellationToken ct)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var query = _db.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.Status == StatusEnum.Active)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtros.Folio))
            {
                var folioLower = filtros.Folio.Trim().ToLower();
                query = query.Where(r => r.Folio != null && r.Folio.ToLower().Contains(folioLower));
            }

            if (filtros.Estatus.HasValue)
            {
                query = query.Where(r => r.Estatus == filtros.Estatus.Value);
            }

            if (filtros.SoloVencidos)
            {
                query = query.Where(r => r.FechaVencimiento < hoy && r.Estatus != EstatusRecibo.PAGADO && r.Estatus != EstatusRecibo.CANCELADO);
            }

            if (filtros.SoloPagados)
            {
                query = query.Where(r => r.Estatus == EstatusRecibo.PAGADO);
            }

            if (filtros.SoloPendientes)
            {
                query = query.Where(r => r.Estatus == EstatusRecibo.PENDIENTE || r.Estatus == EstatusRecibo.PARCIAL || r.Estatus == EstatusRecibo.VENCIDO);
            }

            if (filtros.IdPeriodoAcademico.HasValue)
            {
                query = query.Where(r => r.IdPeriodoAcademico == filtros.IdPeriodoAcademico.Value);
            }

            if (filtros.FechaEmisionDesde.HasValue)
            {
                query = query.Where(r => r.FechaEmision >= filtros.FechaEmisionDesde.Value);
            }
            if (filtros.FechaEmisionHasta.HasValue)
            {
                query = query.Where(r => r.FechaEmision <= filtros.FechaEmisionHasta.Value);
            }

            if (filtros.FechaVencimientoDesde.HasValue)
            {
                query = query.Where(r => r.FechaVencimiento >= filtros.FechaVencimientoDesde.Value);
            }
            if (filtros.FechaVencimientoHasta.HasValue)
            {
                query = query.Where(r => r.FechaVencimiento <= filtros.FechaVencimientoHasta.Value);
            }

            List<int>? estudianteIds = null;
            List<int>? aspiranteIds = null;
            if (!string.IsNullOrWhiteSpace(filtros.Matricula))
            {
                var matriculaLower = filtros.Matricula.Trim().ToLower();

                estudianteIds = await _db.Estudiante
                    .Where(e => e.Matricula.ToLower().Contains(matriculaLower))
                    .Select(e => e.IdEstudiante)
                    .ToListAsync(ct);

                if (matriculaLower.Contains("asp-") || matriculaLower.All(char.IsDigit))
                {
                    var numeroStr = matriculaLower.Replace("asp-", "").Trim();
                    if (int.TryParse(numeroStr, out int idAspirante))
                    {
                        aspiranteIds = new List<int> { idAspirante };
                    }
                }

                if (estudianteIds.Count == 0 && aspiranteIds == null)
                {
                    estudianteIds = await _db.Estudiante
                        .Include(e => e.IdPersonaNavigation)
                        .Where(e => e.IdPersonaNavigation != null &&
                            (e.IdPersonaNavigation.Nombre.ToLower().Contains(matriculaLower) ||
                             e.IdPersonaNavigation.ApellidoPaterno.ToLower().Contains(matriculaLower) ||
                             e.IdPersonaNavigation.ApellidoMaterno.ToLower().Contains(matriculaLower)))
                        .Select(e => e.IdEstudiante)
                        .Take(100)
                        .ToListAsync(ct);

                    aspiranteIds = await _db.Aspirante
                        .Include(a => a.IdPersonaNavigation)
                        .Where(a => a.IdPersonaNavigation != null &&
                            (a.IdPersonaNavigation.Nombre.ToLower().Contains(matriculaLower) ||
                             a.IdPersonaNavigation.ApellidoPaterno.ToLower().Contains(matriculaLower) ||
                             a.IdPersonaNavigation.ApellidoMaterno.ToLower().Contains(matriculaLower)))
                        .Select(a => a.IdAspirante)
                        .Take(100)
                        .ToListAsync(ct);
                }

                if ((estudianteIds != null && estudianteIds.Count > 0) || (aspiranteIds != null && aspiranteIds.Count > 0))
                {
                    query = query.Where(r =>
                        (r.IdEstudiante.HasValue && estudianteIds != null && estudianteIds.Contains(r.IdEstudiante.Value)) ||
                        (r.IdAspirante.HasValue && aspiranteIds != null && aspiranteIds.Contains(r.IdAspirante.Value)));
                }
                else
                {
                    return new ReciboBusquedaResultadoDto
                    {
                        Recibos = new List<ReciboExtendidoDto>(),
                        TotalRegistros = 0,
                        PaginaActual = filtros.Pagina,
                        TotalPaginas = 0,
                        TamanioPagina = filtros.TamanioPagina
                    };
                }
            }

            var totalRegistros = await query.CountAsync(ct);

            var recibos = await query
                .OrderByDescending(r => r.FechaEmision)
                .ThenByDescending(r => r.IdRecibo)
                .Skip((filtros.Pagina - 1) * filtros.TamanioPagina)
                .Take(filtros.TamanioPagina)
                .ToListAsync(ct);

            var periodosIds = recibos.Where(r => r.IdPeriodoAcademico.HasValue).Select(r => r.IdPeriodoAcademico!.Value).Distinct().ToList();
            var periodos = await _db.PeriodoAcademico
                .Where(p => periodosIds.Contains(p.IdPeriodoAcademico))
                .ToDictionaryAsync(p => p.IdPeriodoAcademico, p => p.Nombre, ct);

            var estudianteIdsRecibos = recibos.Where(r => r.IdEstudiante.HasValue).Select(r => r.IdEstudiante!.Value).Distinct().ToList();
            var estudiantes = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Include(e => e.IdPlanActualNavigation)
                .Include(e => e.EstudianteGrupo.OrderByDescending(eg => eg.FechaInscripcion).Take(1))
                    .ThenInclude(eg => eg.IdGrupoNavigation)
                .Where(e => estudianteIdsRecibos.Contains(e.IdEstudiante))
                .ToDictionaryAsync(e => e.IdEstudiante, ct);

            var aspiranteIdsRecibos = recibos.Where(r => r.IdAspirante.HasValue).Select(r => r.IdAspirante!.Value).Distinct().ToList();
            var aspirantes = await _db.Aspirante
                .Include(a => a.IdPersonaNavigation)
                .Include(a => a.IdPlanNavigation)
                .Where(a => aspiranteIdsRecibos.Contains(a.IdAspirante))
                .ToDictionaryAsync(a => a.IdAspirante, ct);

            var recibosDto = recibos.Select(r =>
            {
                var diasVencido = Math.Max(0, hoy.DayNumber - r.FechaVencimiento.DayNumber);
                var estaVencido = r.FechaVencimiento < hoy && r.Estatus != EstatusRecibo.PAGADO && r.Estatus != EstatusRecibo.CANCELADO;

                string? matricula = null;
                string? nombreCompleto = null;
                string? carrera = null;
                string? planEstudios = null;
                string? grupo = null;
                string? email = null;
                string? telefono = null;
                string tipoPersona = "Estudiante";

                if (r.IdEstudiante.HasValue && estudiantes.TryGetValue(r.IdEstudiante.Value, out var est))
                {
                    matricula = est.Matricula;
                    var persona = est.IdPersonaNavigation;
                    nombreCompleto = persona != null
                        ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                        : "N/A";
                    carrera = est.IdPlanActualNavigation?.NombrePlanEstudios;
                    planEstudios = est.IdPlanActualNavigation?.NombrePlanEstudios;
                    var grupoActual = est.EstudianteGrupo?.FirstOrDefault()?.IdGrupoNavigation;
                    grupo = grupoActual?.CodigoGrupo ?? grupoActual?.NombreGrupo;
                    email = est.Email ?? persona?.Correo;
                    telefono = persona?.Telefono;
                    tipoPersona = "Estudiante";
                }
                else if (r.IdAspirante.HasValue && aspirantes.TryGetValue(r.IdAspirante.Value, out var asp))
                {
                    matricula = $"ASP-{asp.IdAspirante:D6}";
                    var persona = asp.IdPersonaNavigation;
                    nombreCompleto = persona != null
                        ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                        : "N/A";
                    carrera = asp.IdPlanNavigation?.NombrePlanEstudios ?? "Aspirante";
                    planEstudios = asp.IdPlanNavigation?.NombrePlanEstudios;
                    grupo = null;
                    email = persona?.Correo;
                    telefono = persona?.Telefono;
                    tipoPersona = "Aspirante";
                }

                return new ReciboExtendidoDto
                {
                    IdRecibo = r.IdRecibo,
                    Folio = r.Folio,
                    IdAspirante = r.IdAspirante,
                    IdEstudiante = r.IdEstudiante,
                    IdPeriodoAcademico = r.IdPeriodoAcademico,
                    NombrePeriodo = r.IdPeriodoAcademico.HasValue && periodos.TryGetValue(r.IdPeriodoAcademico.Value, out var np) ? np : null,
                    FechaEmision = r.FechaEmision,
                    FechaVencimiento = r.FechaVencimiento,
                    Estatus = r.Estatus.ToString(),
                    Subtotal = r.Subtotal,
                    Descuento = r.Descuento,
                    Recargos = r.Recargos,
                    Total = r.Total,
                    Saldo = r.Saldo,
                    Notas = r.Notas,
                    DiasVencido = estaVencido ? diasVencido : 0,
                    EstaVencido = estaVencido,
                    Matricula = matricula,
                    NombreCompleto = nombreCompleto,
                    Carrera = carrera,
                    PlanEstudios = planEstudios,
                    Grupo = grupo,
                    Email = email,
                    Telefono = telefono,
                    TipoPersona = tipoPersona,
                    Detalles = r.Detalles.Select(d => new ReciboLineaDto
                    {
                        IdReciboDetalle = d.IdReciboDetalle,
                        IdConceptoPago = d.IdConceptoPago,
                        Descripcion = d.Descripcion,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Importe = d.Importe
                    }).ToList()
                };
            }).ToList();

            var totalSaldoPendiente = recibosDto.Where(r => r.Estatus != "PAGADO" && r.Estatus != "CANCELADO").Sum(r => r.Saldo);
            var totalRecargos = recibosDto.Sum(r => r.Recargos);
            var totalVencidos = recibosDto.Count(r => r.EstaVencido);
            var totalPagados = recibosDto.Count(r => r.Estatus == "PAGADO");
            var totalPendientes = recibosDto.Count(r => r.Estatus == "PENDIENTE" || r.Estatus == "PARCIAL" || r.Estatus == "VENCIDO");

            return new ReciboBusquedaResultadoDto
            {
                Recibos = recibosDto,
                TotalRegistros = totalRegistros,
                PaginaActual = filtros.Pagina,
                TotalPaginas = (int)Math.Ceiling((double)totalRegistros / filtros.TamanioPagina),
                TamanioPagina = filtros.TamanioPagina,
                TotalSaldoPendiente = totalSaldoPendiente,
                TotalRecargos = totalRecargos,
                TotalVencidos = totalVencidos,
                TotalPagados = totalPagados,
                TotalPendientes = totalPendientes
            };
        }

        public async Task<ReciboEstadisticasDto> ObtenerEstadisticasAsync(int? idPeriodoAcademico, CancellationToken ct)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var query = _db.Recibo.Where(r => r.Status == StatusEnum.Active);

            if (idPeriodoAcademico.HasValue)
            {
                query = query.Where(r => r.IdPeriodoAcademico == idPeriodoAcademico.Value);
            }

            var recibos = await query.ToListAsync(ct);

            var totalRecibos = recibos.Count;
            var saldoPendiente = recibos.Where(r => r.Estatus != EstatusRecibo.PAGADO && r.Estatus != EstatusRecibo.CANCELADO).Sum(r => r.Saldo);
            var recibosVencidos = recibos.Count(r => r.FechaVencimiento < hoy && r.Estatus != EstatusRecibo.PAGADO && r.Estatus != EstatusRecibo.CANCELADO);
            var recargosAcumulados = recibos.Sum(r => r.Recargos);
            var recibosPendientes = recibos.Count(r => r.Estatus == EstatusRecibo.PENDIENTE);
            var recibosPagados = recibos.Count(r => r.Estatus == EstatusRecibo.PAGADO);
            var recibosParciales = recibos.Count(r => r.Estatus == EstatusRecibo.PARCIAL);
            var totalCobrado = recibos.Where(r => r.Estatus == EstatusRecibo.PAGADO).Sum(r => r.Total);

            var periodosIds = recibos.Where(r => r.IdPeriodoAcademico.HasValue).Select(r => r.IdPeriodoAcademico!.Value).Distinct().ToList();
            var periodos = await _db.PeriodoAcademico
                .Where(p => periodosIds.Contains(p.IdPeriodoAcademico))
                .ToDictionaryAsync(p => p.IdPeriodoAcademico, p => p.Nombre, ct);

            var estadisticasPorPeriodo = recibos
                .Where(r => r.IdPeriodoAcademico.HasValue)
                .GroupBy(r => r.IdPeriodoAcademico!.Value)
                .Select(g => new EstadisticasPorPeriodoDto
                {
                    IdPeriodoAcademico = g.Key,
                    NombrePeriodo = periodos.TryGetValue(g.Key, out var np) ? np : null,
                    TotalRecibos = g.Count(),
                    SaldoPendiente = g.Where(r => r.Estatus != EstatusRecibo.PAGADO && r.Estatus != EstatusRecibo.CANCELADO).Sum(r => r.Saldo),
                    RecibosVencidos = g.Count(r => r.FechaVencimiento < hoy && r.Estatus != EstatusRecibo.PAGADO && r.Estatus != EstatusRecibo.CANCELADO)
                })
                .OrderByDescending(e => e.IdPeriodoAcademico)
                .ToList();

            return new ReciboEstadisticasDto
            {
                TotalRecibos = totalRecibos,
                SaldoPendiente = saldoPendiente,
                RecibosVencidos = recibosVencidos,
                RecargosAcumulados = recargosAcumulados,
                RecibosPendientes = recibosPendientes,
                RecibosPagados = recibosPagados,
                RecibosParciales = recibosParciales,
                TotalCobrado = totalCobrado,
                PorPeriodo = estadisticasPorPeriodo
            };
        }

        public async Task<ReciboBusquedaResultadoDto> BuscarPorMatriculaAsync(string matricula, CancellationToken ct)
        {
            return await BuscarRecibosAsync(new ReciboBusquedaFiltrosDto
            {
                Matricula = matricula,
                TamanioPagina = 1000
            }, ct);
        }

        public async Task<byte[]> ExportarExcelAsync(ReciboBusquedaFiltrosDto filtros, CancellationToken ct)
        {
            filtros.Pagina = 1;
            filtros.TamanioPagina = 10000;

            var resultado = await BuscarRecibosAsync(filtros, ct);
            var estadisticas = await ObtenerEstadisticasAsync(filtros.IdPeriodoAcademico, ct);

            string nombrePeriodo = "Todos los periodos";
            if (filtros.IdPeriodoAcademico.HasValue)
            {
                var periodo = await _db.PeriodoAcademico
                    .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == filtros.IdPeriodoAcademico.Value, ct);
                if (periodo != null)
                    nombrePeriodo = periodo.Nombre;
            }

            using var workbook = new ClosedXML.Excel.XLWorkbook();

            var wsResumen = workbook.Worksheets.Add("Resumen");

            var colorAzulOscuro = ClosedXML.Excel.XLColor.FromHtml("#003366");
            var colorAzulClaro = ClosedXML.Excel.XLColor.FromHtml("#0088CC");
            var colorVerde = ClosedXML.Excel.XLColor.FromHtml("#28A745");
            var colorRojo = ClosedXML.Excel.XLColor.FromHtml("#DC3545");
            var colorAmarillo = ClosedXML.Excel.XLColor.FromHtml("#FFC107");
            var colorGrisClaro = ClosedXML.Excel.XLColor.FromHtml("#F5F5F5");

            wsResumen.Cell("A1").Value = "UNIVERSIDAD SAN ANDRÉS DE GUANAJUATO";
            wsResumen.Range("A1:F1").Merge();
            wsResumen.Cell("A1").Style.Font.Bold = true;
            wsResumen.Cell("A1").Style.Font.FontSize = 16;
            wsResumen.Cell("A1").Style.Font.FontColor = colorAzulOscuro;
            wsResumen.Cell("A1").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            wsResumen.Cell("A2").Value = "REPORTE DE RECIBOS";
            wsResumen.Range("A2:F2").Merge();
            wsResumen.Cell("A2").Style.Font.Bold = true;
            wsResumen.Cell("A2").Style.Font.FontSize = 14;
            wsResumen.Cell("A2").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            wsResumen.Cell("A3").Value = $"Periodo: {nombrePeriodo}";
            wsResumen.Range("A3:F3").Merge();
            wsResumen.Cell("A3").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            wsResumen.Cell("A4").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            wsResumen.Range("A4:F4").Merge();
            wsResumen.Cell("A4").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            wsResumen.Cell("A4").Style.Font.FontColor = ClosedXML.Excel.XLColor.Gray;

            int fila = 6;
            wsResumen.Cell(fila, 1).Value = "ESTADÍSTICAS GENERALES";
            wsResumen.Range(fila, 1, fila, 3).Merge();
            wsResumen.Cell(fila, 1).Style.Font.Bold = true;
            wsResumen.Cell(fila, 1).Style.Fill.BackgroundColor = colorAzulOscuro;
            wsResumen.Cell(fila, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            fila++;

            var indicadores = new[]
            {
                ("Total Recibos", estadisticas.TotalRecibos.ToString()),
                ("Recibos Pagados", estadisticas.RecibosPagados.ToString()),
                ("Recibos Pendientes", estadisticas.RecibosPendientes.ToString()),
                ("Recibos Parciales", estadisticas.RecibosParciales.ToString()),
                ("Recibos Vencidos", estadisticas.RecibosVencidos.ToString()),
            };

            foreach (var (nombre, valor) in indicadores)
            {
                wsResumen.Cell(fila, 1).Value = nombre;
                wsResumen.Cell(fila, 2).Value = valor;
                wsResumen.Cell(fila, 1).Style.Font.Bold = true;
                fila++;
            }

            fila++;
            wsResumen.Cell(fila, 1).Value = "MONTOS";
            wsResumen.Range(fila, 1, fila, 3).Merge();
            wsResumen.Cell(fila, 1).Style.Font.Bold = true;
            wsResumen.Cell(fila, 1).Style.Fill.BackgroundColor = colorAzulOscuro;
            wsResumen.Cell(fila, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            fila++;

            var montos = new[]
            {
                ("Total Cobrado", estadisticas.TotalCobrado, colorVerde),
                ("Saldo Pendiente", estadisticas.SaldoPendiente, colorRojo),
                ("Recargos Acumulados", estadisticas.RecargosAcumulados, colorAmarillo),
            };

            foreach (var (nombre, valor, color) in montos)
            {
                wsResumen.Cell(fila, 1).Value = nombre;
                wsResumen.Cell(fila, 2).Value = valor;
                wsResumen.Cell(fila, 2).Style.NumberFormat.Format = "$#,##0.00";
                wsResumen.Cell(fila, 1).Style.Font.Bold = true;
                wsResumen.Cell(fila, 2).Style.Font.FontColor = color;
                fila++;
            }

            wsResumen.Column(1).Width = 25;
            wsResumen.Column(2).Width = 20;

            var wsDetalle = workbook.Worksheets.Add("Detalle Recibos");

            var headers = new[] { "Folio", "Estudiante", "Matrícula", "Periodo", "Fecha Emisión", "Fecha Vencimiento",
                                   "Días Vencido", "Estatus", "Subtotal", "Descuento", "Recargos", "Total", "Saldo" };

            for (int i = 0; i < headers.Length; i++)
            {
                wsDetalle.Cell(1, i + 1).Value = headers[i];
                wsDetalle.Cell(1, i + 1).Style.Font.Bold = true;
                wsDetalle.Cell(1, i + 1).Style.Fill.BackgroundColor = colorAzulOscuro;
                wsDetalle.Cell(1, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                wsDetalle.Cell(1, i + 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            }

            int filaDetalle = 2;
            foreach (var recibo in resultado.Recibos)
            {
                wsDetalle.Cell(filaDetalle, 1).Value = recibo.Folio ?? $"#${recibo.IdRecibo}";
                wsDetalle.Cell(filaDetalle, 2).Value = recibo.NombreCompleto ?? "-";
                wsDetalle.Cell(filaDetalle, 3).Value = recibo.Matricula ?? "-";
                wsDetalle.Cell(filaDetalle, 4).Value = recibo.NombrePeriodo ?? "-";
                wsDetalle.Cell(filaDetalle, 5).Value = recibo.FechaEmision.ToString("dd/MM/yyyy");
                wsDetalle.Cell(filaDetalle, 6).Value = recibo.FechaVencimiento.ToString("dd/MM/yyyy");
                wsDetalle.Cell(filaDetalle, 7).Value = recibo.DiasVencido;
                wsDetalle.Cell(filaDetalle, 8).Value = recibo.Estatus;
                wsDetalle.Cell(filaDetalle, 9).Value = recibo.Subtotal;
                wsDetalle.Cell(filaDetalle, 10).Value = recibo.Descuento;
                wsDetalle.Cell(filaDetalle, 11).Value = recibo.Recargos;
                wsDetalle.Cell(filaDetalle, 12).Value = recibo.Total;
                wsDetalle.Cell(filaDetalle, 13).Value = recibo.Saldo;

                wsDetalle.Cell(filaDetalle, 9).Style.NumberFormat.Format = "$#,##0.00";
                wsDetalle.Cell(filaDetalle, 10).Style.NumberFormat.Format = "$#,##0.00";
                wsDetalle.Cell(filaDetalle, 11).Style.NumberFormat.Format = "$#,##0.00";
                wsDetalle.Cell(filaDetalle, 12).Style.NumberFormat.Format = "$#,##0.00";
                wsDetalle.Cell(filaDetalle, 13).Style.NumberFormat.Format = "$#,##0.00";

                var estatusCell = wsDetalle.Cell(filaDetalle, 8);
                switch (recibo.Estatus.ToUpper())
                {
                    case "PAGADO":
                        estatusCell.Style.Font.FontColor = colorVerde;
                        break;
                    case "PENDIENTE":
                    case "PARCIAL":
                        estatusCell.Style.Font.FontColor = colorAmarillo;
                        break;
                    case "VENCIDO":
                    case "CANCELADO":
                        estatusCell.Style.Font.FontColor = colorRojo;
                        break;
                }

                if (recibo.EstaVencido)
                {
                    wsDetalle.Cell(filaDetalle, 7).Style.Font.FontColor = colorRojo;
                    wsDetalle.Cell(filaDetalle, 7).Style.Font.Bold = true;
                }

                if (filaDetalle % 2 == 0)
                {
                    for (int i = 1; i <= headers.Length; i++)
                    {
                        wsDetalle.Cell(filaDetalle, i).Style.Fill.BackgroundColor = colorGrisClaro;
                    }
                }

                filaDetalle++;
            }

            filaDetalle++;
            wsDetalle.Cell(filaDetalle, 8).Value = "TOTALES:";
            wsDetalle.Cell(filaDetalle, 8).Style.Font.Bold = true;
            wsDetalle.Cell(filaDetalle, 9).Value = resultado.Recibos.Sum(r => r.Subtotal);
            wsDetalle.Cell(filaDetalle, 10).Value = resultado.Recibos.Sum(r => r.Descuento);
            wsDetalle.Cell(filaDetalle, 11).Value = resultado.Recibos.Sum(r => r.Recargos);
            wsDetalle.Cell(filaDetalle, 12).Value = resultado.Recibos.Sum(r => r.Total);
            wsDetalle.Cell(filaDetalle, 13).Value = resultado.Recibos.Sum(r => r.Saldo);

            for (int i = 9; i <= 13; i++)
            {
                wsDetalle.Cell(filaDetalle, i).Style.NumberFormat.Format = "$#,##0.00";
                wsDetalle.Cell(filaDetalle, i).Style.Font.Bold = true;
                wsDetalle.Cell(filaDetalle, i).Style.Fill.BackgroundColor = colorAzulClaro;
                wsDetalle.Cell(filaDetalle, i).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }

            wsDetalle.Column(1).Width = 20;
            wsDetalle.Column(2).Width = 35;
            wsDetalle.Column(3).Width = 15;
            wsDetalle.Column(4).Width = 30;
            wsDetalle.Column(5).Width = 15;
            wsDetalle.Column(6).Width = 15;
            wsDetalle.Column(7).Width = 12;
            wsDetalle.Column(8).Width = 12;
            wsDetalle.Column(9).Width = 15;
            wsDetalle.Column(10).Width = 12;
            wsDetalle.Column(11).Width = 12;
            wsDetalle.Column(12).Width = 15;
            wsDetalle.Column(13).Width = 15;

            wsDetalle.Range(1, 1, filaDetalle - 1, headers.Length).SetAutoFilter();

            var wsAdeudos = workbook.Worksheets.Add("Adeudos");

            var recibosConAdeudo = resultado.Recibos
                .Where(r => r.Saldo > 0 && r.Estatus != "CANCELADO")
                .OrderByDescending(r => r.Saldo)
                .ToList();

            wsAdeudos.Cell("A1").Value = "REPORTE DE ADEUDOS";
            wsAdeudos.Range("A1:G1").Merge();
            wsAdeudos.Cell("A1").Style.Font.Bold = true;
            wsAdeudos.Cell("A1").Style.Font.FontSize = 14;
            wsAdeudos.Cell("A1").Style.Fill.BackgroundColor = colorRojo;
            wsAdeudos.Cell("A1").Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            wsAdeudos.Cell("A1").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            wsAdeudos.Cell("A2").Value = $"Total de adeudos: {recibosConAdeudo.Count} | Monto total: {recibosConAdeudo.Sum(r => r.Saldo):C}";
            wsAdeudos.Range("A2:G2").Merge();
            wsAdeudos.Cell("A2").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            var headersAdeudo = new[] { "Folio", "Estudiante", "Matrícula", "Días Vencido", "Estatus", "Total Recibo", "Saldo Pendiente" };
            for (int i = 0; i < headersAdeudo.Length; i++)
            {
                wsAdeudos.Cell(4, i + 1).Value = headersAdeudo[i];
                wsAdeudos.Cell(4, i + 1).Style.Font.Bold = true;
                wsAdeudos.Cell(4, i + 1).Style.Fill.BackgroundColor = colorAzulOscuro;
                wsAdeudos.Cell(4, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }

            int filaAdeudo = 5;
            foreach (var recibo in recibosConAdeudo)
            {
                wsAdeudos.Cell(filaAdeudo, 1).Value = recibo.Folio ?? $"#{recibo.IdRecibo}";
                wsAdeudos.Cell(filaAdeudo, 2).Value = recibo.NombreCompleto ?? "-";
                wsAdeudos.Cell(filaAdeudo, 3).Value = recibo.Matricula ?? "-";
                wsAdeudos.Cell(filaAdeudo, 4).Value = recibo.DiasVencido;
                wsAdeudos.Cell(filaAdeudo, 5).Value = recibo.Estatus;
                wsAdeudos.Cell(filaAdeudo, 6).Value = recibo.Total;
                wsAdeudos.Cell(filaAdeudo, 7).Value = recibo.Saldo;

                wsAdeudos.Cell(filaAdeudo, 6).Style.NumberFormat.Format = "$#,##0.00";
                wsAdeudos.Cell(filaAdeudo, 7).Style.NumberFormat.Format = "$#,##0.00";
                wsAdeudos.Cell(filaAdeudo, 7).Style.Font.FontColor = colorRojo;
                wsAdeudos.Cell(filaAdeudo, 7).Style.Font.Bold = true;

                if (recibo.DiasVencido > 0)
                {
                    wsAdeudos.Cell(filaAdeudo, 4).Style.Font.FontColor = colorRojo;
                    wsAdeudos.Cell(filaAdeudo, 4).Style.Font.Bold = true;
                }

                filaAdeudo++;
            }

            wsAdeudos.Column(1).Width = 20;
            wsAdeudos.Column(2).Width = 35;
            wsAdeudos.Column(3).Width = 15;
            wsAdeudos.Column(4).Width = 12;
            wsAdeudos.Column(5).Width = 12;
            wsAdeudos.Column(6).Width = 15;
            wsAdeudos.Column(7).Width = 18;

            if (recibosConAdeudo.Count > 0)
            {
                wsAdeudos.Range(4, 1, filaAdeudo - 1, headersAdeudo.Length).SetAutoFilter();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<ReciboDto> CancelarReciboAsync(long idRecibo, string usuario, string? motivo, CancellationToken ct)
        {
            var recibo = await _db.Recibo
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Aplicaciones)
                .Include(r => r.Bitacora)
                .FirstOrDefaultAsync(r => r.IdRecibo == idRecibo, ct);

            if (recibo == null)
                throw new InvalidOperationException($"No se encontró el recibo con ID {idRecibo}");

            if (recibo.Estatus == EstatusRecibo.CANCELADO)
                throw new InvalidOperationException("El recibo ya se encuentra cancelado");

            var tienePagos = recibo.Detalles.Any(d => d.Aplicaciones.Any());
            if (tienePagos)
                throw new InvalidOperationException("No se puede cancelar un recibo que tiene pagos aplicados. Use la opción de reversar primero.");

            var estatusAnterior = recibo.Estatus;

            recibo.Estatus = EstatusRecibo.CANCELADO;
            recibo.Saldo = 0;

            var bitacora = new BitacoraRecibo
            {
                IdRecibo = recibo.IdRecibo,
                TipoRecibo = recibo.IdAspirante.HasValue ? "Aspirante" : "Estudiante",
                Usuario = usuario,
                FechaUtc = DateTime.UtcNow,
                Accion = "CANCELACION",
                Origen = "Sistema",
                Notas = $"Recibo cancelado. Estado anterior: {estatusAnterior}. Motivo: {motivo ?? "No especificado"}"
            };
            _db.BitacoraRecibo.Add(bitacora);

            await _db.SaveChangesAsync(ct);

            Console.WriteLine($"[ReciboService] Recibo {recibo.Folio} cancelado por {usuario}. Motivo: {motivo}");

            return _mapper.Map<ReciboDto>(recibo);
        }

        public async Task<ReciboDto> ReversarReciboAsync(long idRecibo, string usuario, string? motivo, CancellationToken ct)
        {
            var recibo = await _db.Recibo
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Aplicaciones)
                        .ThenInclude(a => a.Pago)
                .Include(r => r.Bitacora)
                .FirstOrDefaultAsync(r => r.IdRecibo == idRecibo, ct);

            if (recibo == null)
                throw new InvalidOperationException($"No se encontró el recibo con ID {idRecibo}");

            if (recibo.Estatus == EstatusRecibo.CANCELADO)
                throw new InvalidOperationException("No se puede reversar un recibo cancelado");

            var aplicaciones = recibo.Detalles.SelectMany(d => d.Aplicaciones).ToList();
            var totalPagosAplicados = aplicaciones.Sum(a => a.MontoAplicado);

            var estatusAnterior = recibo.Estatus;
            var saldoAnterior = recibo.Saldo;

            if (!aplicaciones.Any())
            {
                if (recibo.Estatus != EstatusRecibo.PENDIENTE)
                {
                    recibo.Estatus = EstatusRecibo.PENDIENTE;
                    recibo.Saldo = recibo.Subtotal - recibo.Descuento + recibo.Recargos;

                    var bitacoraSimple = new BitacoraRecibo
                    {
                        IdRecibo = recibo.IdRecibo,
                        TipoRecibo = recibo.IdAspirante.HasValue ? "Aspirante" : "Estudiante",
                        Usuario = usuario,
                        FechaUtc = DateTime.UtcNow,
                        Accion = "REVERSION_ESTADO",
                        Origen = "Sistema",
                        Notas = $"Estado restablecido a PENDIENTE. Estado anterior: {estatusAnterior}. Motivo: {motivo ?? "No especificado"}"
                    };
                    _db.BitacoraRecibo.Add(bitacoraSimple);

                    await _db.SaveChangesAsync(ct);
                }

                return _mapper.Map<ReciboDto>(recibo);
            }

            var pagosAfectados = aplicaciones.Select(a => a.Pago).Distinct().ToList();

            _db.PagoAplicacion.RemoveRange(aplicaciones);

            recibo.Saldo = recibo.Subtotal - recibo.Descuento + recibo.Recargos;

            recibo.Estatus = EstatusRecibo.PENDIENTE;

            foreach (var pago in pagosAfectados)
            {
                var montoReversado = aplicaciones.Where(a => a.IdPago == pago.IdPago).Sum(a => a.MontoAplicado);

                Console.WriteLine($"[ReciboService] Pago {pago.IdPago} afectado - Monto reversado: {montoReversado:C}");
            }

            var bitacora = new BitacoraRecibo
            {
                IdRecibo = recibo.IdRecibo,
                TipoRecibo = recibo.IdAspirante.HasValue ? "Aspirante" : "Estudiante",
                Usuario = usuario,
                FechaUtc = DateTime.UtcNow,
                Accion = "REVERSION",
                Origen = "Sistema",
                Notas = $"Recibo reversado. Estado anterior: {estatusAnterior}. Saldo anterior: {saldoAnterior:C}. " +
                        $"Pagos reversados: {aplicaciones.Count}. Monto total reversado: {totalPagosAplicados:C}. " +
                        $"Nuevo saldo: {recibo.Saldo:C}. Motivo: {motivo ?? "No especificado"}"
            };
            _db.BitacoraRecibo.Add(bitacora);

            await _db.SaveChangesAsync(ct);

            Console.WriteLine($"[ReciboService] Recibo {recibo.Folio} reversado por {usuario}. " +
                            $"Aplicaciones eliminadas: {aplicaciones.Count}. Monto reversado: {totalPagosAplicados:C}");

            return _mapper.Map<ReciboDto>(recibo);
        }

        public async Task<ReciboDto?> BuscarPorFolioAsync(string folio, CancellationToken ct)
        {
            var recibo = await _db.Recibo
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.Folio == folio, ct);

            return recibo == null ? null : _mapper.Map<ReciboDto>(recibo);
        }

        public async Task<CarteraVencidaReporteDto> ObtenerCarteraVencidaAsync(int? idPeriodoAcademico, int? diasVencidoMinimo, CancellationToken ct)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var diasMinimo = diasVencidoMinimo ?? 1;

            var query = _db.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.Status == StatusEnum.Active)
                .Where(r => r.Estatus != EstatusRecibo.PAGADO && r.Estatus != EstatusRecibo.CANCELADO)
                .Where(r => r.Saldo > 0)
                .Where(r => r.FechaVencimiento < hoy);

            if (idPeriodoAcademico.HasValue)
            {
                query = query.Where(r => r.IdPeriodoAcademico == idPeriodoAcademico.Value);
            }

            var recibosVencidos = await query.ToListAsync(ct);

            recibosVencidos = recibosVencidos
                .Where(r => (hoy.DayNumber - r.FechaVencimiento.DayNumber) >= diasMinimo)
                .ToList();

            var estudianteIds = recibosVencidos.Where(r => r.IdEstudiante.HasValue).Select(r => r.IdEstudiante!.Value).Distinct().ToList();
            var aspiranteIds = recibosVencidos.Where(r => r.IdAspirante.HasValue).Select(r => r.IdAspirante!.Value).Distinct().ToList();

            var estudiantes = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Include(e => e.IdPlanActualNavigation)
                .Include(e => e.EstudianteGrupo.OrderByDescending(eg => eg.FechaInscripcion).Take(1))
                    .ThenInclude(eg => eg.IdGrupoNavigation)
                .Where(e => estudianteIds.Contains(e.IdEstudiante))
                .ToDictionaryAsync(e => e.IdEstudiante, ct);

            var aspirantes = await _db.Aspirante
                .Include(a => a.IdPersonaNavigation)
                .Include(a => a.IdPlanNavigation)
                .Where(a => aspiranteIds.Contains(a.IdAspirante))
                .ToDictionaryAsync(a => a.IdAspirante, ct);

            string? nombrePeriodo = null;
            if (idPeriodoAcademico.HasValue)
            {
                var periodo = await _db.PeriodoAcademico.FindAsync(new object[] { idPeriodoAcademico.Value }, ct);
                nombrePeriodo = periodo?.Nombre;
            }

            var detalle = recibosVencidos.Select(r =>
            {
                var diasVencido = hoy.DayNumber - r.FechaVencimiento.DayNumber;

                string? matricula = null;
                string? nombreCompleto = null;
                string? carrera = null;
                string? grupo = null;
                string? email = null;
                string? telefono = null;

                if (r.IdEstudiante.HasValue && estudiantes.TryGetValue(r.IdEstudiante.Value, out var est))
                {
                    matricula = est.Matricula;
                    var persona = est.IdPersonaNavigation;
                    nombreCompleto = persona != null
                        ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                        : "N/A";
                    carrera = est.IdPlanActualNavigation?.NombrePlanEstudios;
                    var grupoActual = est.EstudianteGrupo?.FirstOrDefault()?.IdGrupoNavigation;
                    grupo = grupoActual?.CodigoGrupo ?? grupoActual?.NombreGrupo;
                    email = est.Email ?? persona?.Correo;
                    telefono = persona?.Telefono ?? persona?.Celular;
                }
                else if (r.IdAspirante.HasValue && aspirantes.TryGetValue(r.IdAspirante.Value, out var asp))
                {
                    matricula = $"ASP-{asp.IdAspirante:D6}";
                    var persona = asp.IdPersonaNavigation;
                    nombreCompleto = persona != null
                        ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                        : "N/A";
                    carrera = asp.IdPlanNavigation?.NombrePlanEstudios ?? "Aspirante";
                    email = persona?.Correo;
                    telefono = persona?.Telefono ?? persona?.Celular;
                }

                return new CarteraVencidaItemDto
                {
                    IdRecibo = r.IdRecibo,
                    Folio = r.Folio,
                    Matricula = matricula,
                    NombreCompleto = nombreCompleto,
                    Carrera = carrera,
                    Grupo = grupo,
                    Email = email,
                    Telefono = telefono,
                    FechaEmision = r.FechaEmision,
                    FechaVencimiento = r.FechaVencimiento,
                    DiasVencido = diasVencido,
                    Total = r.Total,
                    Saldo = r.Saldo,
                    Recargos = r.Recargos,
                    TotalAdeudo = r.Saldo + r.Recargos
                };
            }).OrderByDescending(x => x.DiasVencido).ThenByDescending(x => x.Saldo).ToList();

            return new CarteraVencidaReporteDto
            {
                FechaReporte = hoy,
                IdPeriodoAcademico = idPeriodoAcademico,
                NombrePeriodo = nombrePeriodo,
                TotalRecibosVencidos = detalle.Count,
                TotalSaldoVencido = detalle.Sum(x => x.Saldo),
                TotalRecargos = detalle.Sum(x => x.Recargos),
                TotalAdeudo = detalle.Sum(x => x.TotalAdeudo),
                Detalle = detalle
            };
        }

        public async Task<byte[]> ExportarCarteraVencidaExcelAsync(int? idPeriodoAcademico, int? diasVencidoMinimo, CancellationToken ct)
        {
            var reporte = await ObtenerCarteraVencidaAsync(idPeriodoAcademico, diasVencidoMinimo, ct);

            using var workbook = new ClosedXML.Excel.XLWorkbook();

            var colorRojo = ClosedXML.Excel.XLColor.FromHtml("#DC3545");
            var colorAzulOscuro = ClosedXML.Excel.XLColor.FromHtml("#003366");
            var colorGrisClaro = ClosedXML.Excel.XLColor.FromHtml("#F5F5F5");

            var ws = workbook.Worksheets.Add("Cartera Vencida");

            ws.Cell("A1").Value = "REPORTE DE CARTERA VENCIDA";
            ws.Range("A1:J1").Merge();
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 16;
            ws.Cell("A1").Style.Font.FontColor = colorRojo;
            ws.Cell("A1").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            ws.Cell("A2").Value = reporte.NombrePeriodo ?? "Todos los periodos";
            ws.Range("A2:J2").Merge();
            ws.Cell("A2").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            ws.Cell("A3").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            ws.Range("A3:J3").Merge();
            ws.Cell("A3").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            ws.Cell("A3").Style.Font.FontColor = ClosedXML.Excel.XLColor.Gray;

            int fila = 5;
            ws.Cell(fila, 1).Value = "RESUMEN";
            ws.Range(fila, 1, fila, 4).Merge();
            ws.Cell(fila, 1).Style.Font.Bold = true;
            ws.Cell(fila, 1).Style.Fill.BackgroundColor = colorRojo;
            ws.Cell(fila, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            fila++;

            ws.Cell(fila, 1).Value = "Total Recibos Vencidos:";
            ws.Cell(fila, 2).Value = reporte.TotalRecibosVencidos;
            ws.Cell(fila, 1).Style.Font.Bold = true;
            fila++;

            ws.Cell(fila, 1).Value = "Saldo Vencido:";
            ws.Cell(fila, 2).Value = reporte.TotalSaldoVencido;
            ws.Cell(fila, 2).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(fila, 1).Style.Font.Bold = true;
            fila++;

            ws.Cell(fila, 1).Value = "Recargos Acumulados:";
            ws.Cell(fila, 2).Value = reporte.TotalRecargos;
            ws.Cell(fila, 2).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(fila, 1).Style.Font.Bold = true;
            fila++;

            ws.Cell(fila, 1).Value = "TOTAL ADEUDO:";
            ws.Cell(fila, 2).Value = reporte.TotalAdeudo;
            ws.Cell(fila, 2).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(fila, 1).Style.Font.Bold = true;
            ws.Cell(fila, 2).Style.Font.Bold = true;
            ws.Cell(fila, 2).Style.Font.FontColor = colorRojo;
            fila += 2;

            var headers = new[] { "Folio", "Matrícula", "Estudiante", "Carrera", "Grupo", "Email", "Teléfono", "Días Vencido", "Saldo", "Recargos", "Total Adeudo" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(fila, i + 1).Value = headers[i];
                ws.Cell(fila, i + 1).Style.Font.Bold = true;
                ws.Cell(fila, i + 1).Style.Fill.BackgroundColor = colorAzulOscuro;
                ws.Cell(fila, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }
            fila++;

            foreach (var item in reporte.Detalle)
            {
                ws.Cell(fila, 1).Value = item.Folio ?? $"#{item.IdRecibo}";
                ws.Cell(fila, 2).Value = item.Matricula ?? "-";
                ws.Cell(fila, 3).Value = item.NombreCompleto ?? "-";
                ws.Cell(fila, 4).Value = item.Carrera ?? "-";
                ws.Cell(fila, 5).Value = item.Grupo ?? "-";
                ws.Cell(fila, 6).Value = item.Email ?? "-";
                ws.Cell(fila, 7).Value = item.Telefono ?? "-";
                ws.Cell(fila, 8).Value = item.DiasVencido;
                ws.Cell(fila, 9).Value = item.Saldo;
                ws.Cell(fila, 10).Value = item.Recargos;
                ws.Cell(fila, 11).Value = item.TotalAdeudo;

                ws.Cell(fila, 8).Style.Font.FontColor = colorRojo;
                ws.Cell(fila, 8).Style.Font.Bold = true;
                ws.Cell(fila, 9).Style.NumberFormat.Format = "$#,##0.00";
                ws.Cell(fila, 10).Style.NumberFormat.Format = "$#,##0.00";
                ws.Cell(fila, 11).Style.NumberFormat.Format = "$#,##0.00";
                ws.Cell(fila, 11).Style.Font.FontColor = colorRojo;
                ws.Cell(fila, 11).Style.Font.Bold = true;

                if (fila % 2 == 0)
                {
                    for (int i = 1; i <= headers.Length; i++)
                        ws.Cell(fila, i).Style.Fill.BackgroundColor = colorGrisClaro;
                }

                fila++;
            }

            ws.Column(1).Width = 18;
            ws.Column(2).Width = 15;
            ws.Column(3).Width = 30;
            ws.Column(4).Width = 25;
            ws.Column(5).Width = 12;
            ws.Column(6).Width = 25;
            ws.Column(7).Width = 15;
            ws.Column(8).Width = 12;
            ws.Column(9).Width = 15;
            ws.Column(10).Width = 12;
            ws.Column(11).Width = 15;

            if (reporte.Detalle.Count > 0)
            {
                ws.Range(10, 1, fila - 1, headers.Length).SetAutoFilter();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<IngresosPeriodoReporteDto> ObtenerIngresosAsync(int idPeriodoAcademico, DateOnly? fechaInicio, DateOnly? fechaFin, CancellationToken ct)
        {
            var periodo = await _db.PeriodoAcademico.FindAsync(new object[] { idPeriodoAcademico }, ct);
            if (periodo == null)
                throw new InvalidOperationException($"No se encontró el periodo académico con ID {idPeriodoAcademico}");

            var recibosDelPeriodo = await _db.Recibo
                .Where(r => r.IdPeriodoAcademico == idPeriodoAcademico)
                .Select(r => r.IdRecibo)
                .ToListAsync(ct);

            var query = _db.PagoAplicacion
                .Include(pa => pa.Pago)
                    .ThenInclude(p => p.MedioPago)
                .Include(pa => pa.ReciboDetalle)
                    .ThenInclude(rd => rd.Recibo)
                .Include(pa => pa.ReciboDetalle)
                    .ThenInclude(rd => rd.ConceptoPago)
                .Where(pa => recibosDelPeriodo.Contains(pa.ReciboDetalle.IdRecibo));

            if (fechaInicio.HasValue)
            {
                var fechaInicioDateTime = fechaInicio.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(pa => pa.Pago.FechaPagoUtc >= fechaInicioDateTime);
            }
            if (fechaFin.HasValue)
            {
                var fechaFinDateTime = fechaFin.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(pa => pa.Pago.FechaPagoUtc <= fechaFinDateTime);
            }

            var aplicaciones = await query.ToListAsync(ct);

            var pagosUnicos = aplicaciones.Select(a => a.Pago).DistinctBy(p => p.IdPago).ToList();

            var recibosInfo = aplicaciones.Select(a => a.ReciboDetalle.Recibo).DistinctBy(r => r.IdRecibo).ToList();
            var estudianteIds = recibosInfo.Where(r => r.IdEstudiante.HasValue).Select(r => r.IdEstudiante!.Value).Distinct().ToList();
            var aspiranteIds = recibosInfo.Where(r => r.IdAspirante.HasValue).Select(r => r.IdAspirante!.Value).Distinct().ToList();

            var estudiantes = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Where(e => estudianteIds.Contains(e.IdEstudiante))
                .ToDictionaryAsync(e => e.IdEstudiante, ct);

            var aspirantes = await _db.Aspirante
                .Include(a => a.IdPersonaNavigation)
                .Where(a => aspiranteIds.Contains(a.IdAspirante))
                .ToDictionaryAsync(a => a.IdAspirante, ct);

            decimal totalEfectivo = 0, totalTarjeta = 0, totalTransferencia = 0, totalOtros = 0;
            foreach (var pago in pagosUnicos)
            {
                var monto = aplicaciones.Where(a => a.IdPago == pago.IdPago).Sum(a => a.MontoAplicado);
                var metodo = (pago.MedioPago?.Clave ?? pago.MedioPago?.Descripcion ?? "").ToUpperInvariant();

                if (metodo.Contains("EFECT"))
                    totalEfectivo += monto;
                else if (metodo.Contains("TARJ") || metodo.Contains("CARD"))
                    totalTarjeta += monto;
                else if (metodo.Contains("TRANS") || metodo.Contains("SPEI"))
                    totalTransferencia += monto;
                else
                    totalOtros += monto;
            }

            var porConcepto = aplicaciones
                .GroupBy(a => new { a.ReciboDetalle.IdConceptoPago, a.ReciboDetalle.ConceptoPago?.Clave, a.ReciboDetalle.ConceptoPago?.Descripcion })
                .Select(g => new IngresoPorConceptoDto
                {
                    IdConceptoPago = g.Key.IdConceptoPago,
                    Clave = g.Key.Clave,
                    Descripcion = g.Key.Descripcion ?? g.First().ReciboDetalle.Descripcion,
                    CantidadPagos = g.Select(a => a.IdPago).Distinct().Count(),
                    TotalMonto = g.Sum(a => a.MontoAplicado)
                })
                .OrderByDescending(x => x.TotalMonto)
                .ToList();

            var detalle = pagosUnicos.Select(pago =>
            {
                var aplicacionesPago = aplicaciones.Where(a => a.IdPago == pago.IdPago).ToList();
                var recibo = aplicacionesPago.First().ReciboDetalle.Recibo;
                var conceptos = string.Join(", ", aplicacionesPago.Select(a => a.ReciboDetalle.Descripcion).Distinct());

                string? matricula = null;
                string? nombreCompleto = null;

                if (recibo.IdEstudiante.HasValue && estudiantes.TryGetValue(recibo.IdEstudiante.Value, out var est))
                {
                    matricula = est.Matricula;
                    var persona = est.IdPersonaNavigation;
                    nombreCompleto = persona != null
                        ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                        : "N/A";
                }
                else if (recibo.IdAspirante.HasValue && aspirantes.TryGetValue(recibo.IdAspirante.Value, out var asp))
                {
                    matricula = $"ASP-{asp.IdAspirante:D6}";
                    var persona = asp.IdPersonaNavigation;
                    nombreCompleto = persona != null
                        ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                        : "N/A";
                }

                return new IngresoDetalleDto
                {
                    IdPago = pago.IdPago,
                    FolioRecibo = recibo.Folio,
                    Matricula = matricula,
                    NombreCompleto = nombreCompleto,
                    FechaPago = pago.FechaPagoUtc,
                    MetodoPago = pago.MedioPago?.Descripcion ?? pago.MedioPago?.Clave,
                    Referencia = pago.Referencia,
                    Monto = aplicacionesPago.Sum(a => a.MontoAplicado),
                    Conceptos = conceptos
                };
            }).OrderByDescending(x => x.FechaPago).ToList();

            return new IngresosPeriodoReporteDto
            {
                FechaReporte = DateOnly.FromDateTime(DateTime.Now),
                IdPeriodoAcademico = idPeriodoAcademico,
                NombrePeriodo = periodo.Nombre,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalPagos = pagosUnicos.Count,
                TotalIngresos = aplicaciones.Sum(a => a.MontoAplicado),
                TotalEfectivo = totalEfectivo,
                TotalTarjeta = totalTarjeta,
                TotalTransferencia = totalTransferencia,
                TotalOtros = totalOtros,
                PorConcepto = porConcepto,
                Detalle = detalle
            };
        }

        public async Task<byte[]> ExportarIngresosExcelAsync(int idPeriodoAcademico, DateOnly? fechaInicio, DateOnly? fechaFin, CancellationToken ct)
        {
            var reporte = await ObtenerIngresosAsync(idPeriodoAcademico, fechaInicio, fechaFin, ct);

            using var workbook = new ClosedXML.Excel.XLWorkbook();

            var colorVerde = ClosedXML.Excel.XLColor.FromHtml("#28A745");
            var colorAzulOscuro = ClosedXML.Excel.XLColor.FromHtml("#003366");
            var colorGrisClaro = ClosedXML.Excel.XLColor.FromHtml("#F5F5F5");

            var wsResumen = workbook.Worksheets.Add("Resumen");

            wsResumen.Cell("A1").Value = "REPORTE DE INGRESOS";
            wsResumen.Range("A1:D1").Merge();
            wsResumen.Cell("A1").Style.Font.Bold = true;
            wsResumen.Cell("A1").Style.Font.FontSize = 16;
            wsResumen.Cell("A1").Style.Font.FontColor = colorVerde;
            wsResumen.Cell("A1").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            wsResumen.Cell("A2").Value = reporte.NombrePeriodo ?? "N/A";
            wsResumen.Range("A2:D2").Merge();
            wsResumen.Cell("A2").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            if (reporte.FechaInicio.HasValue || reporte.FechaFin.HasValue)
            {
                var rangoFechas = $"Del {reporte.FechaInicio?.ToString("dd/MM/yyyy") ?? "inicio"} al {reporte.FechaFin?.ToString("dd/MM/yyyy") ?? "fin"}";
                wsResumen.Cell("A3").Value = rangoFechas;
                wsResumen.Range("A3:D3").Merge();
                wsResumen.Cell("A3").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            }

            wsResumen.Cell("A4").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            wsResumen.Range("A4:D4").Merge();
            wsResumen.Cell("A4").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            wsResumen.Cell("A4").Style.Font.FontColor = ClosedXML.Excel.XLColor.Gray;

            int fila = 6;
            wsResumen.Cell(fila, 1).Value = "TOTALES";
            wsResumen.Range(fila, 1, fila, 2).Merge();
            wsResumen.Cell(fila, 1).Style.Font.Bold = true;
            wsResumen.Cell(fila, 1).Style.Fill.BackgroundColor = colorVerde;
            wsResumen.Cell(fila, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            fila++;

            var totales = new[]
            {
                ("Total Pagos", reporte.TotalPagos.ToString(), false),
                ("TOTAL INGRESOS", reporte.TotalIngresos.ToString("C"), true),
                ("Efectivo", reporte.TotalEfectivo.ToString("C"), false),
                ("Tarjeta", reporte.TotalTarjeta.ToString("C"), false),
                ("Transferencia", reporte.TotalTransferencia.ToString("C"), false),
                ("Otros", reporte.TotalOtros.ToString("C"), false),
            };

            foreach (var (nombre, valor, destacar) in totales)
            {
                wsResumen.Cell(fila, 1).Value = nombre;
                wsResumen.Cell(fila, 2).Value = valor;
                wsResumen.Cell(fila, 1).Style.Font.Bold = true;
                if (destacar)
                {
                    wsResumen.Cell(fila, 2).Style.Font.Bold = true;
                    wsResumen.Cell(fila, 2).Style.Font.FontColor = colorVerde;
                    wsResumen.Cell(fila, 2).Style.Font.FontSize = 14;
                }
                fila++;
            }

            fila += 2;
            wsResumen.Cell(fila, 1).Value = "POR CONCEPTO";
            wsResumen.Range(fila, 1, fila, 3).Merge();
            wsResumen.Cell(fila, 1).Style.Font.Bold = true;
            wsResumen.Cell(fila, 1).Style.Fill.BackgroundColor = colorAzulOscuro;
            wsResumen.Cell(fila, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            fila++;

            wsResumen.Cell(fila, 1).Value = "Concepto";
            wsResumen.Cell(fila, 2).Value = "Pagos";
            wsResumen.Cell(fila, 3).Value = "Monto";
            for (int i = 1; i <= 3; i++)
            {
                wsResumen.Cell(fila, i).Style.Font.Bold = true;
                wsResumen.Cell(fila, i).Style.Fill.BackgroundColor = colorGrisClaro;
            }
            fila++;

            foreach (var concepto in reporte.PorConcepto)
            {
                wsResumen.Cell(fila, 1).Value = concepto.Descripcion ?? concepto.Clave ?? $"ID:{concepto.IdConceptoPago}";
                wsResumen.Cell(fila, 2).Value = concepto.CantidadPagos;
                wsResumen.Cell(fila, 3).Value = concepto.TotalMonto;
                wsResumen.Cell(fila, 3).Style.NumberFormat.Format = "$#,##0.00";
                fila++;
            }

            wsResumen.Column(1).Width = 25;
            wsResumen.Column(2).Width = 15;
            wsResumen.Column(3).Width = 18;

            var wsDetalle = workbook.Worksheets.Add("Detalle Pagos");

            var headers = new[] { "ID Pago", "Fecha", "Folio Recibo", "Matrícula", "Estudiante", "Método", "Referencia", "Monto", "Conceptos" };
            for (int i = 0; i < headers.Length; i++)
            {
                wsDetalle.Cell(1, i + 1).Value = headers[i];
                wsDetalle.Cell(1, i + 1).Style.Font.Bold = true;
                wsDetalle.Cell(1, i + 1).Style.Fill.BackgroundColor = colorAzulOscuro;
                wsDetalle.Cell(1, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }

            int filaDetalle = 2;
            foreach (var pago in reporte.Detalle)
            {
                wsDetalle.Cell(filaDetalle, 1).Value = pago.IdPago;
                wsDetalle.Cell(filaDetalle, 2).Value = pago.FechaPago.ToString("dd/MM/yyyy HH:mm");
                wsDetalle.Cell(filaDetalle, 3).Value = pago.FolioRecibo ?? "-";
                wsDetalle.Cell(filaDetalle, 4).Value = pago.Matricula ?? "-";
                wsDetalle.Cell(filaDetalle, 5).Value = pago.NombreCompleto ?? "-";
                wsDetalle.Cell(filaDetalle, 6).Value = pago.MetodoPago ?? "-";
                wsDetalle.Cell(filaDetalle, 7).Value = pago.Referencia ?? "-";
                wsDetalle.Cell(filaDetalle, 8).Value = pago.Monto;
                wsDetalle.Cell(filaDetalle, 9).Value = pago.Conceptos ?? "-";

                wsDetalle.Cell(filaDetalle, 8).Style.NumberFormat.Format = "$#,##0.00";

                if (filaDetalle % 2 == 0)
                {
                    for (int i = 1; i <= headers.Length; i++)
                        wsDetalle.Cell(filaDetalle, i).Style.Fill.BackgroundColor = colorGrisClaro;
                }

                filaDetalle++;
            }

            filaDetalle++;
            wsDetalle.Cell(filaDetalle, 7).Value = "TOTAL:";
            wsDetalle.Cell(filaDetalle, 7).Style.Font.Bold = true;
            wsDetalle.Cell(filaDetalle, 8).Value = reporte.TotalIngresos;
            wsDetalle.Cell(filaDetalle, 8).Style.NumberFormat.Format = "$#,##0.00";
            wsDetalle.Cell(filaDetalle, 8).Style.Font.Bold = true;
            wsDetalle.Cell(filaDetalle, 8).Style.Font.FontColor = colorVerde;

            wsDetalle.Column(1).Width = 10;
            wsDetalle.Column(2).Width = 18;
            wsDetalle.Column(3).Width = 18;
            wsDetalle.Column(4).Width = 15;
            wsDetalle.Column(5).Width = 30;
            wsDetalle.Column(6).Width = 15;
            wsDetalle.Column(7).Width = 20;
            wsDetalle.Column(8).Width = 15;
            wsDetalle.Column(9).Width = 40;

            if (reporte.Detalle.Count > 0)
            {
                wsDetalle.Range(1, 1, filaDetalle - 2, headers.Length).SetAutoFilter();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
