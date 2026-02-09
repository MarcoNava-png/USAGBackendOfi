using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Comprobante;
using WebApplication2.Core.DTOs.Pagos;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Pagos;
using WebApplication2.Core.Responses.Pagos;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class PagoService : IPagoService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IDocumentoEstudianteService? _documentoService;
        private readonly IBitacoraAccionService? _bitacora;

        public PagoService(ApplicationDbContext db, IMapper mapper, IDocumentoEstudianteService? documentoService = null, IBitacoraAccionService? bitacora = null)
        {
            _db = db;
            _mapper = mapper;
            _documentoService = documentoService;
            _bitacora = bitacora;
        }

        public async Task<long> RegistrarPagoAsync(RegistrarPagoDto dto, CancellationToken ct)
        {
            var pago = new Pago
            {
                FechaPagoUtc = dto.FechaPagoUtc,
                IdMedioPago = dto.IdMedioPago,
                Monto = dto.Monto,
                Moneda = dto.Moneda,
                Referencia = dto.Referencia,
                Notas = dto.Notas,
                Estatus = dto.estatus
            };

            _db.Pago.Add(pago);
            await _db.SaveChangesAsync(ct);

            return pago.IdPago;
        }

        public async Task<IReadOnlyList<long>> AplicarPagoAsync(AplicarPagoDto dto, CancellationToken ct)
        {
            Console.WriteLine($"\n=== INICIO AplicarPagoAsync ===");
            Console.WriteLine($"IdPago: {dto.IdPago}, Aplicaciones: {dto.Aplicaciones.Count}");

            var pago = await _db.Pago.FindAsync(new object[] { dto.IdPago }, ct);
            if (pago == null)
                throw new InvalidOperationException($"No existe el pago con ID {dto.IdPago}");

            Console.WriteLine($"✓ Pago {dto.IdPago} encontrado");

            var recibosAfectados = new List<long>();
            var aplicaciones = new List<PagoAplicacion>();

            foreach (var aplicacion in dto.Aplicaciones)
            {
                Console.WriteLine($"  - Aplicando {aplicacion.Monto:C} a ReciboDetalle {aplicacion.IdReciboDetalle}");

                var pagoAplicacion = new PagoAplicacion
                {
                    IdPago = dto.IdPago,
                    IdReciboDetalle = aplicacion.IdReciboDetalle,
                    MontoAplicado = aplicacion.Monto
                };

                aplicaciones.Add(pagoAplicacion);
                _db.PagoAplicacion.Add(pagoAplicacion);
            }

            Console.WriteLine($"✓ Guardando {aplicaciones.Count} aplicaciones de pago...");
            await _db.SaveChangesAsync(ct);
            Console.WriteLine($"✓ Aplicaciones guardadas en BD");

            var idsReciboDetalle = dto.Aplicaciones.Select(a => a.IdReciboDetalle).ToList();
            Console.WriteLine($"Buscando recibos afectados para detalles: [{string.Join(", ", idsReciboDetalle)}]");

            var detalles = await _db.ReciboDetalle
                .Where(rd => idsReciboDetalle.Contains(rd.IdReciboDetalle))
                .Select(rd => rd.IdRecibo)
                .Distinct()
                .ToListAsync(ct);

            Console.WriteLine($"✓ Recibos únicos afectados encontrados: {detalles.Count} → [{string.Join(", ", detalles)}]");

            if (detalles.Count == 0)
            {
                Console.WriteLine("⚠️ ADVERTENCIA: No se encontraron recibos afectados. No se actualizará ningún estatus.");
            }
            else
            {
                Console.WriteLine($"Iniciando actualización de estatus para {detalles.Count} recibo(s)...");
                foreach (var idRecibo in detalles)
                {
                    Console.WriteLine($"\n>>> Llamando a ActualizarEstatusReciboAsync para recibo {idRecibo}");
                    await ActualizarEstatusReciboAsync(idRecibo, ct);
                    Console.WriteLine($"<<< ActualizarEstatusReciboAsync completado para recibo {idRecibo}");

                    var reciboActualizado = await _db.Recibo.FindAsync(new object[] { idRecibo }, ct);
                    if (reciboActualizado?.Estatus == EstatusRecibo.PAGADO && _documentoService != null)
                    {
                        Console.WriteLine($"✓ Actualizando estado de solicitudes de documento para recibo {idRecibo}");
                        await _documentoService.ActualizarEstatusPagoAsync(idRecibo);
                    }

                    recibosAfectados.Add(idRecibo);
                }
            }

            Console.WriteLine($"\n=== FIN AplicarPagoAsync - Recibos actualizados: [{string.Join(", ", recibosAfectados)}] ===\n");
            return recibosAfectados;
        }

        public async Task<PagoDto?> ObtenerAsync(long idPago, CancellationToken ct)
        {
            var pago = await _db.Pago
                .Include(p => p.Aplicaciones)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdPago == idPago, ct);

            return pago == null ? null : _mapper.Map<PagoDto>(pago);
        }

        public async Task<IReadOnlyList<PagoDto>> ListarPorFechaAsync(
            DateTime fechaInicio,
            DateTime fechaFin,
            string? usuarioId,
            CancellationToken ct)
        {
            var query = _db.Pago
                .Where(p => p.FechaPagoUtc >= fechaInicio && p.FechaPagoUtc <= fechaFin);

            if (!string.IsNullOrEmpty(usuarioId))
            {
                query = query.Where(p => p.CreatedBy == usuarioId);
            }

            var pagos = await query
                .OrderByDescending(p => p.FechaPagoUtc)
                .AsNoTracking()
                .ToListAsync(ct);

            var pagoIds = pagos.Select(p => p.IdPago).ToList();

            var medioPagoIds = pagos.Select(p => p.IdMedioPago).Distinct().ToList();
            var mediosPago = await _db.Set<Core.Models.MedioPago>()
                .Where(m => medioPagoIds.Contains(m.IdMedioPago))
                .AsNoTracking()
                .ToDictionaryAsync(m => m.IdMedioPago, ct);

            var aplicaciones = await _db.PagoAplicacion
                .Where(a => pagoIds.Contains(a.IdPago))
                .AsNoTracking()
                .ToListAsync(ct);

            var reciboDetalleIds = aplicaciones.Select(a => a.IdReciboDetalle).Distinct().ToList();

            var recibosDetalle = await _db.ReciboDetalle
                .Where(rd => reciboDetalleIds.Contains(rd.IdReciboDetalle))
                .AsNoTracking()
                .ToDictionaryAsync(rd => rd.IdReciboDetalle, ct);

            var reciboIds = recibosDetalle.Values.Select(rd => rd.IdRecibo).Distinct().ToList();

            var recibos = await _db.Recibo
                .Where(r => reciboIds.Contains(r.IdRecibo))
                .AsNoTracking()
                .ToDictionaryAsync(r => r.IdRecibo, ct);

            var estudianteIds = recibos.Values
                .Where(r => r.IdEstudiante.HasValue)
                .Select(r => r.IdEstudiante!.Value)
                .Distinct()
                .ToList();

            var aspiranteIds = recibos.Values
                .Where(r => r.IdAspirante.HasValue)
                .Select(r => r.IdAspirante!.Value)
                .Distinct()
                .ToList();

            var estudiantes = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Where(e => estudianteIds.Contains(e.IdEstudiante))
                .AsNoTracking()
                .ToDictionaryAsync(e => e.IdEstudiante, ct);

            var aspirantes = await _db.Aspirante
                .Include(a => a.IdPersonaNavigation)
                .Where(a => aspiranteIds.Contains(a.IdAspirante))
                .AsNoTracking()
                .ToDictionaryAsync(a => a.IdAspirante, ct);

            var aplicacionesPorPago = aplicaciones.GroupBy(a => a.IdPago).ToDictionary(g => g.Key, g => g.ToList());

            var resultado = pagos.Select(p =>
            {
                var apps = aplicacionesPorPago.ContainsKey(p.IdPago) ? aplicacionesPorPago[p.IdPago] : new List<Core.Models.PagoAplicacion>();
                var primeraAplicacion = apps.FirstOrDefault();

                Core.Models.Recibo? recibo = null;
                Core.Models.ReciboDetalle? detalle = null;

                if (primeraAplicacion != null && recibosDetalle.TryGetValue(primeraAplicacion.IdReciboDetalle, out detalle))
                {
                    recibos.TryGetValue(detalle.IdRecibo, out recibo);
                }

                string? matricula = null;
                string nombrePagador = "";
                int? idEstudiante = null;

                if (recibo?.IdEstudiante != null && estudiantes.TryGetValue(recibo.IdEstudiante.Value, out var est))
                {
                    idEstudiante = recibo.IdEstudiante;
                    matricula = est.Matricula;
                    nombrePagador = est.IdPersonaNavigation != null
                        ? $"{est.IdPersonaNavigation.ApellidoPaterno} {est.IdPersonaNavigation.ApellidoMaterno} {est.IdPersonaNavigation.Nombre}".Trim()
                        : est.Matricula;
                }
                else if (recibo?.IdAspirante != null && aspirantes.TryGetValue(recibo.IdAspirante.Value, out var asp))
                {
                    matricula = $"ASP-{asp.IdAspirante}";
                    nombrePagador = asp.IdPersonaNavigation != null
                        ? $"{asp.IdPersonaNavigation.ApellidoPaterno} {asp.IdPersonaNavigation.ApellidoMaterno} {asp.IdPersonaNavigation.Nombre}".Trim()
                        : $"Aspirante #{asp.IdAspirante}";
                }

                var concepto = detalle?.Descripcion ?? "";

                return new PagoDto
                {
                    IdPago = p.IdPago,
                    FolioPago = p.FolioPago,
                    FechaPagoUtc = p.FechaPagoUtc,
                    IdMedioPago = p.IdMedioPago,
                    Monto = p.Monto,
                    Moneda = p.Moneda,
                    Referencia = p.Referencia,
                    Notas = p.Notas,
                    Estatus = p.Estatus,
                    MedioPago = mediosPago.TryGetValue(p.IdMedioPago, out var mp) ? (mp.Descripcion ?? mp.Clave) : "Desconocido",
                    IdEstudiante = idEstudiante,
                    Matricula = matricula,
                    NombreEstudiante = nombrePagador,
                    Concepto = concepto,
                    FolioRecibo = recibo?.Folio
                };
            }).ToList();

            return resultado;
        }

        private async Task ActualizarEstatusReciboAsync(long idRecibo, CancellationToken ct)
        {
            var recibo = await _db.Recibo
                .Include(r => r.Detalles)
                .ThenInclude(d => d.Aplicaciones)
                .FirstOrDefaultAsync(r => r.IdRecibo == idRecibo, ct);

            if (recibo == null) return;

            var estatusAnterior = recibo.Estatus;

            decimal totalPagado = recibo.Detalles
                .SelectMany(d => d.Aplicaciones)
                .Sum(a => a.MontoAplicado);

            decimal saldoAnterior = recibo.Saldo;
            recibo.Saldo = recibo.Subtotal + recibo.Recargos - recibo.Descuento - totalPagado;

            Console.WriteLine($"=== ACTUALIZANDO RECIBO {idRecibo} ===");
            Console.WriteLine($"Subtotal: {recibo.Subtotal}, Recargos: {recibo.Recargos}, Descuento: {recibo.Descuento}");
            Console.WriteLine($"Total Pagado: {totalPagado}");
            Console.WriteLine($"Saldo Anterior: {saldoAnterior} → Saldo Nuevo: {recibo.Saldo}");

            EstatusRecibo nuevoEstatus;
            if (recibo.Saldo <= 0)
            {
                nuevoEstatus = EstatusRecibo.PAGADO;
                recibo.Saldo = 0;
            }
            else if (totalPagado > 0)
            {
                nuevoEstatus = EstatusRecibo.PARCIAL;
            }
            else
            {
                nuevoEstatus = EstatusRecibo.PENDIENTE;
            }

            Console.WriteLine($"Estatus Anterior: {estatusAnterior} → Estatus Nuevo: {nuevoEstatus}");

            if (recibo.Estatus != nuevoEstatus)
            {
                recibo.Estatus = nuevoEstatus;
                Console.WriteLine($"✓ Estatus actualizado de {estatusAnterior} a {nuevoEstatus}");
            }

            var usuarioActual = recibo.UpdatedBy ?? "SYSTEM";

            var bitacora = new BitacoraRecibo
            {
                IdRecibo = idRecibo,
                Usuario = usuarioActual,
                FechaUtc = DateTime.UtcNow,
                Accion = recibo.Estatus != estatusAnterior
                    ? $"Cambio de estatus: {estatusAnterior} → {nuevoEstatus}"
                    : $"Pago aplicado (estatus sin cambio: {nuevoEstatus})",
                Origen = "PagoService.AplicarPago",
                Notas = $"Total pagado: {totalPagado:C}, Saldo anterior: {saldoAnterior:C}, Saldo nuevo: {recibo.Saldo:C}"
            };

            _db.BitacoraRecibo.Add(bitacora);
            Console.WriteLine($"✓ Bitácora registrada");

            if (recibo.IdAspirante.HasValue && nuevoEstatus == EstatusRecibo.PAGADO)
            {
                Console.WriteLine($"✓ Recibo de aspirante {recibo.IdAspirante.Value} completamente pagado. Actualizando estatus del aspirante...");
                await ActualizarEstatusAspirantePorPagoAsync(recibo.IdAspirante.Value, ct);
            }

            await _db.SaveChangesAsync(ct);
            Console.WriteLine($"✓ Cambios guardados en BD para recibo {idRecibo}");
        }

        public async Task<RegistrarYAplicarPagoResultDto> RegistrarYAplicarPagoAsync(RegistrarYAplicarPagoDto dto, CancellationToken ct)
        {
            Console.WriteLine($"\n=== INICIO RegistrarYAplicarPagoAsync ===");
            Console.WriteLine($"IdRecibo: {dto.IdRecibo}, Monto: {dto.Monto:C}");

            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var recibo = await _db.Recibo
                    .Include(r => r.Detalles)
                        .ThenInclude(d => d.Aplicaciones)
                    .FirstOrDefaultAsync(r => r.IdRecibo == dto.IdRecibo, ct);

                if (recibo == null)
                    throw new InvalidOperationException($"No existe el recibo con ID {dto.IdRecibo}");

                var estatusAnterior = recibo.Estatus.ToString();
                var saldoAnterior = recibo.Saldo;

                Console.WriteLine($"Recibo encontrado: Folio={recibo.Folio}, Saldo={saldoAnterior:C}, Estatus={estatusAnterior}");

                if (!recibo.Detalles.Any())
                {
                    Console.WriteLine("⚠️ El recibo no tiene detalles, creando detalle automático...");
                    var detalleGenerico = new ReciboDetalle
                    {
                        IdRecibo = dto.IdRecibo,
                        IdConceptoPago = 1,
                        Descripcion = "Pago de recibo",
                        Cantidad = 1,
                        PrecioUnitario = recibo.Subtotal
                    };
                    _db.ReciboDetalle.Add(detalleGenerico);
                    await _db.SaveChangesAsync(ct);
                    recibo.Detalles.Add(detalleGenerico);
                    Console.WriteLine($"✓ Detalle creado con IdReciboDetalle: {detalleGenerico.IdReciboDetalle}");
                }

                var pago = new Pago
                {
                    FechaPagoUtc = dto.FechaPagoUtc,
                    IdMedioPago = dto.IdMedioPago,
                    Monto = dto.Monto,
                    Moneda = dto.Moneda,
                    Referencia = dto.Referencia,
                    Notas = dto.Notas,
                    Estatus = dto.Estatus
                };

                _db.Pago.Add(pago);
                await _db.SaveChangesAsync(ct);

                Console.WriteLine($"✓ Pago registrado con IdPago: {pago.IdPago}");

                decimal montoRestante = dto.Monto;
                decimal totalAplicado = 0;

                foreach (var detalle in recibo.Detalles.OrderBy(d => d.IdReciboDetalle))
                {
                    if (montoRestante <= 0) break;

                    decimal pagadoDetalle = detalle.Aplicaciones.Sum(a => a.MontoAplicado);
                    decimal importeDetalle = detalle.Cantidad * detalle.PrecioUnitario;
                    decimal pendienteDetalle = importeDetalle - pagadoDetalle;

                    if (pendienteDetalle <= 0) continue;

                    decimal montoAAplicar = Math.Min(montoRestante, pendienteDetalle);

                    var aplicacion = new PagoAplicacion
                    {
                        IdPago = pago.IdPago,
                        IdReciboDetalle = detalle.IdReciboDetalle,
                        MontoAplicado = montoAAplicar
                    };

                    _db.PagoAplicacion.Add(aplicacion);
                    montoRestante -= montoAAplicar;
                    totalAplicado += montoAAplicar;

                    Console.WriteLine($"  - Aplicando {montoAAplicar:C} a detalle {detalle.IdReciboDetalle} (pendiente: {pendienteDetalle:C})");
                }

                await _db.SaveChangesAsync(ct);
                Console.WriteLine($"✓ Total aplicado: {totalAplicado:C}");

                decimal nuevoSaldo = recibo.Saldo - totalAplicado;
                if (nuevoSaldo < 0) nuevoSaldo = 0;

                EstatusRecibo nuevoEstatus;
                if (nuevoSaldo <= 0)
                {
                    nuevoEstatus = EstatusRecibo.PAGADO;
                }
                else if (totalAplicado > 0)
                {
                    nuevoEstatus = EstatusRecibo.PARCIAL;
                }
                else
                {
                    nuevoEstatus = recibo.Estatus;
                }

                Console.WriteLine($"=== ACTUALIZANDO RECIBO {dto.IdRecibo} ===");
                Console.WriteLine($"Saldo: {recibo.Saldo:C} → {nuevoSaldo:C}");
                Console.WriteLine($"Estatus: {recibo.Estatus} → {nuevoEstatus}");

                await _db.Database.ExecuteSqlRawAsync(
                    "UPDATE Recibo SET Saldo = {0}, Estatus = {1}, UpdatedAt = GETUTCDATE() WHERE IdRecibo = {2}",
                    nuevoSaldo, nuevoEstatus.ToString(), dto.IdRecibo);

                var bitacora = new BitacoraRecibo
                {
                    IdRecibo = dto.IdRecibo,
                    Usuario = recibo.CreatedBy ?? "SYSTEM",
                    FechaUtc = DateTime.UtcNow,
                    Accion = $"Pago aplicado: {estatusAnterior} → {nuevoEstatus}",
                    Origen = "PagoService.RegistrarYAplicarPagoAsync",
                    Notas = $"Pago ID: {pago.IdPago}, Monto: {totalAplicado:C}, Saldo anterior: {saldoAnterior:C}, Saldo nuevo: {nuevoSaldo:C}"
                };
                _db.BitacoraRecibo.Add(bitacora);
                await _db.SaveChangesAsync(ct);

                Console.WriteLine($"✓ Recibo actualizado: Saldo={nuevoSaldo:C}, Estatus={nuevoEstatus}");

                if (recibo.IdAspirante.HasValue && nuevoEstatus == EstatusRecibo.PAGADO)
                {
                    Console.WriteLine($"✓ Recibo de aspirante completamente pagado. Actualizando estatus...");
                    await ActualizarEstatusAspirantePorPagoAsync(recibo.IdAspirante.Value, ct);
                }

                if (nuevoEstatus == EstatusRecibo.PAGADO && _documentoService != null)
                {
                    Console.WriteLine($"✓ Actualizando estado de solicitudes de documento para recibo {dto.IdRecibo}");
                    await _documentoService.ActualizarEstatusPagoAsync(dto.IdRecibo);
                }

                recibo.Saldo = nuevoSaldo;
                recibo.Estatus = nuevoEstatus;

                await transaction.CommitAsync(ct);

                var resultado = new RegistrarYAplicarPagoResultDto
                {
                    IdPago = pago.IdPago,
                    IdRecibo = dto.IdRecibo,
                    MontoAplicado = totalAplicado,
                    SaldoAnterior = saldoAnterior,
                    SaldoNuevo = recibo.Saldo,
                    EstatusReciboAnterior = estatusAnterior,
                    EstatusReciboNuevo = recibo.Estatus.ToString(),
                    ReciboPagadoCompletamente = recibo.Estatus == EstatusRecibo.PAGADO
                };

                Console.WriteLine($"=== FIN RegistrarYAplicarPagoAsync - Estatus: {resultado.EstatusReciboNuevo} ===\n");
                return resultado;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                Console.WriteLine($"❌ ERROR en RegistrarYAplicarPagoAsync: {ex.Message}");
                throw;
            }
        }

        private async Task ActualizarEstatusAspirantePorPagoAsync(int idAspirante, CancellationToken ct)
        {
            Console.WriteLine($"=== ACTUALIZANDO ESTATUS ASPIRANTE {idAspirante} POR PAGO ===");

            var recibosAspirante = await _db.Recibo
                .Where(r => r.IdAspirante == idAspirante && r.Status == Core.Enums.StatusEnum.Active)
                .ToListAsync(ct);

            Console.WriteLine($"Recibos activos del aspirante: {recibosAspirante.Count}");
            foreach (var r in recibosAspirante)
            {
                Console.WriteLine($"  - Recibo {r.IdRecibo}: Estatus={r.Estatus}, Saldo={r.Saldo:C}");
            }

            bool todosPagados = recibosAspirante.Any() && recibosAspirante.All(r => r.Estatus == EstatusRecibo.PAGADO);

            Console.WriteLine($"¿Todos pagados? {todosPagados}");

            if (todosPagados)
            {
                var estatusPagado = await _db.AspiranteEstatus
                    .Where(e => e.Status == Core.Enums.StatusEnum.Active)
                    .Where(e => e.DescEstatus == "Pagado" ||
                                e.DescEstatus.ToUpper().Contains("PAGADO") ||
                                e.DescEstatus.ToUpper().Contains("PAGO COMPLETO"))
                    .FirstOrDefaultAsync(ct);

                if (estatusPagado == null)
                {
                    Console.WriteLine("No se encontró estatus 'Pagado', buscando 'Admitido'...");
                    estatusPagado = await _db.AspiranteEstatus
                        .Where(e => e.Status == Core.Enums.StatusEnum.Active)
                        .Where(e => e.DescEstatus == "Admitido")
                        .FirstOrDefaultAsync(ct);
                }

                if (estatusPagado != null)
                {
                    var aspirante = await _db.Aspirante
                        .Include(a => a.IdAspiranteEstatusNavigation)
                        .FirstOrDefaultAsync(a => a.IdAspirante == idAspirante, ct);

                    if (aspirante != null)
                    {
                        var estatusAnterior = aspirante.IdAspiranteEstatusNavigation?.DescEstatus ?? "N/A";

                        if (aspirante.IdAspiranteEstatus != estatusPagado.IdAspiranteEstatus)
                        {
                            Console.WriteLine($"Cambiando estatus de aspirante: '{estatusAnterior}' → '{estatusPagado.DescEstatus}'");
                            aspirante.IdAspiranteEstatus = estatusPagado.IdAspiranteEstatus;
                            await _db.SaveChangesAsync(ct);
                            Console.WriteLine($"✓ Estatus del aspirante actualizado exitosamente");
                        }
                        else
                        {
                            Console.WriteLine($"Aspirante ya tiene estatus '{estatusPagado.DescEstatus}', no se requiere cambio");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("❌ ADVERTENCIA: No se encontró ningún estatus válido para marcar como 'Pagado'");
                    var estatusDisponibles = await _db.AspiranteEstatus
                        .Where(e => e.Status == Core.Enums.StatusEnum.Active)
                        .Select(e => e.DescEstatus)
                        .ToListAsync(ct);
                    Console.WriteLine($"   Estatus disponibles: {string.Join(", ", estatusDisponibles)}");
                }
            }
            else
            {
                Console.WriteLine("No todos los recibos están pagados, el estatus del aspirante no cambiará");
            }
        }

        public async Task<PagoRegistradoResponse> RegistrarPagoCajaAsync(RegistrarPagoCajaRequest request, string usuarioId, CancellationToken ct)
        {
            Console.WriteLine($"\n=== INICIO RegistrarPagoCajaAsync ===");
            Console.WriteLine($"Recibos: {request.RecibosSeleccionados.Count}, Monto total: {request.Monto:C}");

            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var pago = new Pago
                {
                    FechaPagoUtc = request.FechaPago.ToUniversalTime(),
                    IdMedioPago = request.IdMedioPago,
                    Monto = request.Monto,
                    Moneda = "MXN",
                    Referencia = request.Referencia,
                    Notas = request.Notas,
                    Estatus = EstatusPago.CONFIRMADO,
                    IdUsuarioCaja = usuarioId,
                    IdCaja = request.IdCaja
                };

                _db.Pago.Add(pago);
                await _db.SaveChangesAsync(ct);

                Console.WriteLine($"✓ Pago creado con IdPago: {pago.IdPago}, Folio: {pago.FolioPago}");

                var recibosAfectados = new List<long>();

                foreach (var reciboParaPago in request.RecibosSeleccionados)
                {
                    Console.WriteLine($"\n--- Procesando Recibo {reciboParaPago.IdRecibo} por {reciboParaPago.MontoAplicar:C} ---");

                    var recibo = await _db.Recibo
                        .Include(r => r.Detalles)
                            .ThenInclude(d => d.Aplicaciones)
                        .FirstOrDefaultAsync(r => r.IdRecibo == reciboParaPago.IdRecibo, ct);

                    if (recibo == null)
                    {
                        Console.WriteLine($"⚠️ Recibo {reciboParaPago.IdRecibo} no encontrado, saltando...");
                        continue;
                    }

                    var estatusAnterior = recibo.Estatus;
                    var saldoAnterior = recibo.Saldo;

                    if (!recibo.Detalles.Any())
                    {
                        Console.WriteLine("⚠️ El recibo no tiene detalles, creando detalle automático...");
                        var detalleGenerico = new ReciboDetalle
                        {
                            IdRecibo = recibo.IdRecibo,
                            IdConceptoPago = 1,
                            Descripcion = "Pago de recibo",
                            Cantidad = 1,
                            PrecioUnitario = recibo.Subtotal
                        };
                        _db.ReciboDetalle.Add(detalleGenerico);
                        await _db.SaveChangesAsync(ct);
                        recibo.Detalles.Add(detalleGenerico);
                    }

                    decimal montoRestante = reciboParaPago.MontoAplicar;
                    decimal totalAplicado = 0;

                    foreach (var detalle in recibo.Detalles.OrderBy(d => d.IdReciboDetalle))
                    {
                        if (montoRestante <= 0) break;

                        decimal pagadoDetalle = detalle.Aplicaciones.Sum(a => a.MontoAplicado);
                        decimal importeDetalle = detalle.Cantidad * detalle.PrecioUnitario;
                        decimal pendienteDetalle = importeDetalle - pagadoDetalle;

                        if (pendienteDetalle <= 0) continue;

                        decimal montoAAplicar = Math.Min(montoRestante, pendienteDetalle);

                        var aplicacion = new PagoAplicacion
                        {
                            IdPago = pago.IdPago,
                            IdReciboDetalle = detalle.IdReciboDetalle,
                            MontoAplicado = montoAAplicar
                        };

                        _db.PagoAplicacion.Add(aplicacion);
                        montoRestante -= montoAAplicar;
                        totalAplicado += montoAAplicar;

                        Console.WriteLine($"  - Aplicando {montoAAplicar:C} a detalle {detalle.IdReciboDetalle}");
                    }

                    if (montoRestante > 0 && recibo.Recargos > 0)
                    {
                        var detalleRecargo = recibo.Detalles.FirstOrDefault(d => d.Descripcion.Contains("Recargo"));
                        if (detalleRecargo == null)
                        {
                            detalleRecargo = new ReciboDetalle
                            {
                                IdRecibo = recibo.IdRecibo,
                                IdConceptoPago = 1,
                                Descripcion = "Recargo por mora",
                                Cantidad = 1,
                                PrecioUnitario = recibo.Recargos
                            };
                            _db.ReciboDetalle.Add(detalleRecargo);
                            await _db.SaveChangesAsync(ct);
                        }

                        decimal montoRecargoAAplicar = Math.Min(montoRestante, recibo.Recargos);
                        var aplicacionRecargo = new PagoAplicacion
                        {
                            IdPago = pago.IdPago,
                            IdReciboDetalle = detalleRecargo.IdReciboDetalle,
                            MontoAplicado = montoRecargoAAplicar
                        };
                        _db.PagoAplicacion.Add(aplicacionRecargo);
                        totalAplicado += montoRecargoAAplicar;
                        Console.WriteLine($"  - Aplicando {montoRecargoAAplicar:C} a recargo");
                    }

                    await _db.SaveChangesAsync(ct);

                    decimal nuevoSaldo = recibo.Saldo - totalAplicado;
                    if (nuevoSaldo < 0) nuevoSaldo = 0;

                    EstatusRecibo nuevoEstatus;
                    if (nuevoSaldo <= 0)
                    {
                        nuevoEstatus = EstatusRecibo.PAGADO;
                    }
                    else
                    {
                        nuevoEstatus = EstatusRecibo.PARCIAL;
                    }

                    Console.WriteLine($"Saldo: {saldoAnterior:C} → {nuevoSaldo:C}, Estatus: {estatusAnterior} → {nuevoEstatus}");

                    await _db.Database.ExecuteSqlRawAsync(
                        "UPDATE Recibo SET Saldo = {0}, Estatus = {1}, UpdatedAt = GETUTCDATE() WHERE IdRecibo = {2}",
                        nuevoSaldo, nuevoEstatus.ToString(), recibo.IdRecibo);

                    var bitacora = new BitacoraRecibo
                    {
                        IdRecibo = recibo.IdRecibo,
                        Usuario = usuarioId,
                        FechaUtc = DateTime.UtcNow,
                        Accion = $"Pago desde caja: {estatusAnterior} → {nuevoEstatus}",
                        Origen = "CajaController.RegistrarPago",
                        Notas = $"Pago ID: {pago.IdPago}, Monto: {totalAplicado:C}, Saldo anterior: {saldoAnterior:C}, Saldo nuevo: {nuevoSaldo:C}"
                    };
                    _db.BitacoraRecibo.Add(bitacora);

                    if (recibo.IdAspirante.HasValue && nuevoEstatus == EstatusRecibo.PAGADO)
                    {
                        await ActualizarEstatusAspirantePorPagoAsync(recibo.IdAspirante.Value, ct);
                    }

                    if (nuevoEstatus == EstatusRecibo.PAGADO && _documentoService != null)
                    {
                        Console.WriteLine($"✓ Actualizando estado de solicitudes de documento para recibo {recibo.IdRecibo}");
                        await _documentoService.ActualizarEstatusPagoAsync(recibo.IdRecibo);
                    }

                    recibosAfectados.Add(recibo.IdRecibo);
                }

                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                Console.WriteLine($"\n=== FIN RegistrarPagoCajaAsync - {recibosAfectados.Count} recibos afectados ===\n");

                if (_bitacora != null)
                {
                    await _bitacora.RegistrarAsync(usuarioId, usuarioId, "REGISTRAR_PAGO", "Pagos",
                        "Pago", pago.IdPago.ToString(),
                        $"Pago {pago.FolioPago ?? $"PAG-{pago.IdPago:D8}"} por {pago.Monto:C} aplicado a {recibosAfectados.Count} recibo(s)");
                }

                return new PagoRegistradoResponse
                {
                    IdPago = pago.IdPago,
                    FolioPago = pago.FolioPago ?? $"PAG-{pago.IdPago:D8}",
                    Monto = pago.Monto,
                    RecibosAfectados = recibosAfectados,
                    Comprobante = "disponible"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                Console.WriteLine($"❌ ERROR en RegistrarPagoCajaAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<ComprobantePagoDto?> ObtenerDatosComprobanteAsync(long idPago, CancellationToken ct)
        {
            var pago = await _db.Pago
                .Include(p => p.MedioPago)
                .FirstOrDefaultAsync(p => p.IdPago == idPago, ct);

            if (pago == null)
                return null;

            var aplicaciones = await _db.PagoAplicacion
                .Where(pa => pa.IdPago == idPago)
                .Include(pa => pa.ReciboDetalle)
                    .ThenInclude(rd => rd.Recibo)
                .Include(pa => pa.ReciboDetalle)
                    .ThenInclude(rd => rd.ConceptoPago)
                .ToListAsync(ct);

            Console.WriteLine($"[Comprobante] Pago {idPago} encontrado. Aplicaciones cargadas: {aplicaciones.Count}");

            CajeroComprobanteInfo? cajeroInfo = null;
            var usuarioCajaId = !string.IsNullOrEmpty(pago.IdUsuarioCaja) ? pago.IdUsuarioCaja : pago.CreatedBy;
            if (!string.IsNullOrEmpty(usuarioCajaId))
            {
                var cajero = await _db.Users.FindAsync(new object[] { usuarioCajaId }, ct);
                if (cajero != null)
                {
                    cajeroInfo = new CajeroComprobanteInfo
                    {
                        IdUsuario = cajero.Id,
                        NombreCompleto = $"{cajero.Nombres} {cajero.Apellidos}".Trim()
                    };
                }
            }

            Recibo? primerRecibo = aplicaciones.FirstOrDefault()?.ReciboDetalle?.Recibo;

            if (primerRecibo == null && aplicaciones.Any())
            {
                var primerIdReciboDetalle = aplicaciones.First().IdReciboDetalle;

                primerRecibo = await _db.Recibo
                    .FirstOrDefaultAsync(r => _db.ReciboDetalle
                        .Any(rd => rd.IdReciboDetalle == primerIdReciboDetalle && rd.IdRecibo == r.IdRecibo), ct);

                Console.WriteLine($"[Comprobante] Recibo obtenido por consulta directa: {primerRecibo?.IdRecibo}, IdEstudiante={primerRecibo?.IdEstudiante}, IdAspirante={primerRecibo?.IdAspirante}");
            }

            var estudianteInfo = new EstudianteComprobanteInfo();
            string? nombrePeriodo = null;

            Console.WriteLine($"[Comprobante] Procesando recibo: IdRecibo={primerRecibo?.IdRecibo}, IdEstudiante={primerRecibo?.IdEstudiante}, IdAspirante={primerRecibo?.IdAspirante}");

            if (primerRecibo == null)
            {
                Console.WriteLine($"[Comprobante] ERROR: No se pudo obtener el recibo. Verificando aplicaciones...");
                Console.WriteLine($"[Comprobante] El pago tiene {aplicaciones.Count} aplicaciones");
                foreach (var app in aplicaciones)
                {
                    Console.WriteLine($"[Comprobante]   - Aplicación: IdReciboDetalle={app.IdReciboDetalle}, MontoAplicado={app.MontoAplicado}");
                }
            }

            if (primerRecibo?.IdEstudiante.HasValue == true)
            {
                Console.WriteLine($"[Comprobante] Buscando estudiante con ID: {primerRecibo.IdEstudiante.Value}");
                var estudiante = await _db.Estudiante
                    .Include(e => e.IdPersonaNavigation)
                    .Include(e => e.IdPlanActualNavigation)
                    .FirstOrDefaultAsync(e => e.IdEstudiante == primerRecibo.IdEstudiante.Value, ct);

                if (estudiante != null)
                {
                    var persona = estudiante.IdPersonaNavigation;
                    var nombreCompleto = persona != null
                        ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                        : "N/A";

                    estudianteInfo = new EstudianteComprobanteInfo
                    {
                        IdEstudiante = estudiante.IdEstudiante,
                        Matricula = estudiante.Matricula ?? "N/A",
                        NombreCompleto = nombreCompleto,
                        Email = estudiante.Email,
                        Telefono = persona?.Telefono,
                        Carrera = estudiante.IdPlanActualNavigation?.NombrePlanEstudios
                    };
                    Console.WriteLine($"[Comprobante] Estudiante encontrado: {nombreCompleto}, Matrícula: {estudiante.Matricula}");
                }
                else
                {
                    Console.WriteLine($"[Comprobante] ADVERTENCIA: No se encontró estudiante con ID {primerRecibo.IdEstudiante.Value}");
                }
            }
            else if (primerRecibo?.IdAspirante.HasValue == true)
            {
                Console.WriteLine($"[Comprobante] Buscando aspirante con ID: {primerRecibo.IdAspirante.Value}");
                var aspirante = await _db.Aspirante
                    .Include(a => a.IdPersonaNavigation)
                    .Include(a => a.IdPlanNavigation)
                    .FirstOrDefaultAsync(a => a.IdAspirante == primerRecibo.IdAspirante.Value, ct);

                if (aspirante != null)
                {
                    var persona = aspirante.IdPersonaNavigation;
                    var nombreCompleto = persona != null
                        ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                        : "N/A";

                    estudianteInfo = new EstudianteComprobanteInfo
                    {
                        IdEstudiante = 0,
                        Matricula = $"ASP-{aspirante.IdAspirante:D6}",
                        NombreCompleto = nombreCompleto,
                        Email = persona?.Correo,
                        Telefono = persona?.Telefono,
                        Carrera = aspirante.IdPlanNavigation?.NombrePlanEstudios ?? "Aspirante"
                    };
                    Console.WriteLine($"[Comprobante] Aspirante encontrado: {nombreCompleto}, Folio: ASP-{aspirante.IdAspirante:D6}");
                }
                else
                {
                    Console.WriteLine($"[Comprobante] ADVERTENCIA: No se encontró aspirante con ID {primerRecibo.IdAspirante.Value}");
                }
            }
            else
            {
                Console.WriteLine($"[Comprobante] ADVERTENCIA: El recibo no tiene IdEstudiante ni IdAspirante");
            }

            if (primerRecibo?.IdPeriodoAcademico.HasValue == true)
            {
                var periodo = await _db.PeriodoAcademico.FindAsync(new object[] { primerRecibo.IdPeriodoAcademico.Value }, ct);
                nombrePeriodo = periodo?.Nombre;
                estudianteInfo.PeriodoActual = nombrePeriodo;
            }

            var idsPeriodos = aplicaciones
                .Select(a => a.ReciboDetalle?.Recibo?.IdPeriodoAcademico)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var periodos = await _db.PeriodoAcademico
                .Where(p => idsPeriodos.Contains(p.IdPeriodoAcademico))
                .ToDictionaryAsync(p => p.IdPeriodoAcademico, p => p.Nombre, ct);

            DateTime ahoraLocal;
            try
            {
                var zonaHorariaMexico = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
                ahoraLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaMexico);
            }
            catch
            {
                try
                {
                    var zonaHorariaMexico = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
                    ahoraLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaMexico);
                }
                catch
                {
                    ahoraLocal = DateTime.UtcNow.AddHours(-6);
                }
            }
            var hoy = DateOnly.FromDateTime(ahoraLocal);

            var politica = await _db.RecargoPolitica
                .Where(p => p.Activo)
                .FirstOrDefaultAsync(ct);

            decimal tasaDiaria = politica?.TasaDiaria ?? 0.01m;
            decimal? recargoMaximo = politica?.RecargoMaximo;

            var recibosPagados = new List<ReciboComprobanteInfo>();

            var gruposRecibos = aplicaciones.GroupBy(a => a.ReciboDetalle.IdRecibo);

            foreach (var grupo in gruposRecibos)
            {
                var recibo = grupo.First().ReciboDetalle.Recibo;
                var montoAplicadoAlRecibo = grupo.Sum(a => a.MontoAplicado);

                var conceptos = grupo
                    .Select(a => a.ReciboDetalle?.ConceptoPago?.Descripcion ?? a.ReciboDetalle?.Descripcion)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .ToList();

                var conceptoPrincipal = conceptos.Count > 0
                    ? string.Join(", ", conceptos)
                    : "Concepto de pago";

                string? periodoRecibo = null;
                if (recibo.IdPeriodoAcademico.HasValue && periodos.TryGetValue(recibo.IdPeriodoAcademico.Value, out var np))
                {
                    periodoRecibo = np;
                }

                decimal recargosCalculados = recibo.Recargos;

                bool recargoCondonado = !string.IsNullOrEmpty(recibo.Notas) &&
                    (recibo.Notas.Contains("[RECARGO CONDONADO") || recibo.Notas.Contains("CONDONACION_RECARGO"));

                if (!recargoCondonado)
                {
                    recargoCondonado = await _db.BitacoraRecibo
                        .AnyAsync(b => b.IdRecibo == recibo.IdRecibo && b.Accion == "CONDONACION_RECARGO", ct);
                }

                if (recibo.Estatus != EstatusRecibo.PAGADO && !recargoCondonado && recibo.FechaVencimiento < hoy)
                {
                    var diasVencido = hoy.DayNumber - recibo.FechaVencimiento.DayNumber;

                    if (diasVencido > 0)
                    {
                        recargosCalculados = recibo.Saldo * tasaDiaria * diasVencido;

                        if (recargoMaximo.HasValue && recargosCalculados > recargoMaximo.Value)
                        {
                            recargosCalculados = recargoMaximo.Value;
                        }
                    }
                }

                var recargosReales = Math.Max(recibo.Recargos, recargosCalculados);

                var totalConRecargos = recibo.Subtotal + recargosReales - recibo.Descuento;

                recibosPagados.Add(new ReciboComprobanteInfo
                {
                    IdRecibo = recibo.IdRecibo,
                    Folio = recibo.Folio ?? $"REC-{recibo.IdRecibo}",
                    Concepto = conceptoPrincipal,
                    Periodo = periodoRecibo,
                    MontoOriginal = totalConRecargos,
                    Descuento = recibo.Descuento,
                    Recargos = recargosReales,
                    MontoPagado = montoAplicadoAlRecibo,
                    SaldoAnterior = recibo.Saldo + montoAplicadoAlRecibo,
                    SaldoNuevo = recibo.Saldo,
                    Estatus = recibo.Estatus.ToString()
                });
            }

            return new ComprobantePagoDto
            {
                Pago = new PagoComprobanteInfo
                {
                    IdPago = pago.IdPago,
                    FolioPago = pago.FolioPago ?? $"PAG-{pago.IdPago:D8}",
                    FechaPago = pago.FechaPagoUtc,
                    HoraPago = pago.FechaPagoUtc.ToString("HH:mm:ss"),
                    MedioPago = pago.MedioPago?.Descripcion ?? "N/A",
                    Monto = pago.Monto,
                    Moneda = pago.Moneda,
                    Referencia = pago.Referencia,
                    Notas = pago.Notas
                },
                Estudiante = estudianteInfo,
                RecibosPagados = recibosPagados,
                Institucion = new InstitucionInfo
                {
                    Nombre = "UNIVERSIDAD SAN ANDRÉS DE GUANAJUATO",
                    NombreCorto = "USAG",
                    RFC = "CSA000000XX0"
                },
                Cajero = cajeroInfo
            };
        }
    }
}
