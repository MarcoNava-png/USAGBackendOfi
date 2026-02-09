using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Caja;
using WebApplication2.Core.DTOs.Comprobante;
using WebApplication2.Core.DTOs.Pagos;
using WebApplication2.Core.DTOs.Recibo;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Pagos;
using WebApplication2.Core.Responses.Caja;
using WebApplication2.Core.Responses.Pagos;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class CajaService : ICajaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly string _logoPath;

        private static readonly string ColorAzulOscuro = "#003366";
        private static readonly string ColorAzulClaro = "#0088CC";
        private static readonly string ColorGris = "#666666";
        private static readonly string ColorGrisClaro = "#F5F5F5";

        public CajaService(ApplicationDbContext context, IAuthService authService, IWebHostEnvironment env)
        {
            _context = context;
            _authService = authService;

            _logoPath = Path.Combine(env.ContentRootPath, "..", "Logousag.png");
            if (!File.Exists(_logoPath))
            {
                _logoPath = Path.Combine(env.ContentRootPath, "Logousag.png");
            }
        }

        public async Task<RecibosParaCobroDto> BuscarRecibosParaCobroAsync(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
            {
                return new RecibosParaCobroDto { Recibos = new List<ReciboParaCobroDto>() };
            }

            var criterioLower = criterio.Trim().ToLower();

            var reciboPorFolio = await _context.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.Folio != null && r.Folio.ToLower().Contains(criterioLower))
                .Where(r => r.Estatus == EstatusRecibo.PENDIENTE || r.Estatus == EstatusRecibo.PARCIAL || r.Estatus == EstatusRecibo.VENCIDO)
                .FirstOrDefaultAsync();

            if (reciboPorFolio != null && reciboPorFolio.IdEstudiante.HasValue)
            {
                return await ObtenerRecibosDeEstudiante(reciboPorFolio.IdEstudiante.Value);
            }

            if (criterioLower.StartsWith("doc-"))
            {
                var solicitudDoc = await _context.SolicitudesDocumento
                    .Where(s => s.FolioSolicitud.ToLower().Contains(criterioLower) && s.IdRecibo.HasValue)
                    .FirstOrDefaultAsync();

                if (solicitudDoc != null && solicitudDoc.IdRecibo.HasValue)
                {
                    var reciboDoc = await _context.Recibo
                        .Where(r => r.IdRecibo == solicitudDoc.IdRecibo.Value)
                        .Where(r => r.Estatus == EstatusRecibo.PENDIENTE || r.Estatus == EstatusRecibo.PARCIAL || r.Estatus == EstatusRecibo.VENCIDO)
                        .FirstOrDefaultAsync();

                    if (reciboDoc != null && reciboDoc.IdEstudiante.HasValue)
                    {
                        return await ObtenerRecibosDeEstudiante(reciboDoc.IdEstudiante.Value);
                    }
                }
            }

            var estudiantes = await _context.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Where(e => e.Activo &&
                    (e.Matricula.ToLower().Contains(criterioLower) ||
                     (e.IdPersonaNavigation != null &&
                      (e.IdPersonaNavigation.Nombre.ToLower().Contains(criterioLower) ||
                       e.IdPersonaNavigation.ApellidoPaterno.ToLower().Contains(criterioLower) ||
                       e.IdPersonaNavigation.ApellidoMaterno.ToLower().Contains(criterioLower) ||
                       (e.IdPersonaNavigation.Nombre + " " + e.IdPersonaNavigation.ApellidoPaterno + " " + e.IdPersonaNavigation.ApellidoMaterno).ToLower().Contains(criterioLower)))))
                .Take(10)
                .ToListAsync();

            if (estudiantes.Count == 0)
            {
                return new RecibosParaCobroDto
                {
                    Recibos = new List<ReciboParaCobroDto>(),
                    TotalAdeudo = 0
                };
            }

            if (estudiantes.Count == 1)
            {
                return await ObtenerRecibosDeEstudiante(estudiantes[0].IdEstudiante);
            }

            return new RecibosParaCobroDto
            {
                Multiple = true,
                Estudiantes = estudiantes.Select(e => new EstudianteInfoDto
                {
                    IdEstudiante = e.IdEstudiante,
                    Matricula = e.Matricula,
                    NombreCompleto = e.IdPersonaNavigation != null
                        ? $"{e.IdPersonaNavigation.ApellidoPaterno} {e.IdPersonaNavigation.ApellidoMaterno} {e.IdPersonaNavigation.Nombre}".Trim()
                        : "Sin nombre",
                    Email = e.Email ?? e.IdPersonaNavigation?.Correo,
                    Telefono = e.IdPersonaNavigation?.Telefono
                }).ToList(),
                Recibos = new List<ReciboParaCobroDto>()
            };
        }

        private async Task<RecibosParaCobroDto> ObtenerRecibosDeEstudiante(int idEstudiante)
        {
            var estudiante = await _context.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante);

            if (estudiante == null)
            {
                return new RecibosParaCobroDto { Recibos = new List<ReciboParaCobroDto>() };
            }

            var recibos = await _context.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.IdEstudiante == idEstudiante)
                .Where(r => r.Estatus == EstatusRecibo.PENDIENTE || r.Estatus == EstatusRecibo.PARCIAL || r.Estatus == EstatusRecibo.VENCIDO)
                .OrderBy(r => r.FechaVencimiento)
                .ToListAsync();

            var periodosIds = recibos.Where(r => r.IdPeriodoAcademico.HasValue).Select(r => r.IdPeriodoAcademico!.Value).Distinct().ToList();
            var periodos = await _context.PeriodoAcademico.Where(p => periodosIds.Contains(p.IdPeriodoAcademico)).ToDictionaryAsync(p => p.IdPeriodoAcademico, p => p.Nombre);

            var recibosDto = recibos.Select(r => new ReciboParaCobroDto
            {
                IdRecibo = r.IdRecibo,
                Folio = r.Folio,
                IdAspirante = r.IdAspirante,
                IdEstudiante = r.IdEstudiante,
                IdPeriodoAcademico = r.IdPeriodoAcademico,
                IdGrupo = null,
                IdPlantillaCobro = null,
                FechaEmision = r.FechaEmision.ToString("yyyy-MM-dd"),
                FechaVencimiento = r.FechaVencimiento.ToString("yyyy-MM-dd"),
                Estatus = (int)r.Estatus,
                Subtotal = r.Subtotal,
                DescuentoBeca = r.Descuento,
                Descuento = r.Descuento,
                DescuentoAdicional = 0,
                Recargos = r.Recargos,
                Total = r.Total,
                Saldo = r.Saldo,
                Notas = r.Notas,
                CreadoPor = r.CreatedBy,
                FechaCreacion = r.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                NombrePeriodo = r.IdPeriodoAcademico.HasValue && periodos.ContainsKey(r.IdPeriodoAcademico.Value) ? periodos[r.IdPeriodoAcademico.Value] : null,
                CodigoGrupo = null,
                Detalles = r.Detalles.Select(d => new ReciboDetalleParaCobroDto
                {
                    IdReciboDetalle = d.IdReciboDetalle,
                    IdConceptoPago = d.IdConceptoPago,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Importe = d.Importe,
                    DescuentoBeca = r.Descuento > 0 && r.Subtotal > 0 ? (decimal?)Math.Round(d.Importe * (r.Descuento / r.Subtotal), 2) : null,
                    ImporteNeto = r.Descuento > 0 && r.Subtotal > 0 ? (decimal?)(d.Importe - Math.Round(d.Importe * (r.Descuento / r.Subtotal), 2)) : null,
                    IdPlantillaDetalle = null,
                    RefTabla = d.RefTabla,
                    RefId = d.RefId
                }).ToList()
            }).ToList();

            return new RecibosParaCobroDto
            {
                Estudiante = new EstudianteInfoDto
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    Matricula = estudiante.Matricula,
                    NombreCompleto = estudiante.IdPersonaNavigation != null
                        ? $"{estudiante.IdPersonaNavigation.ApellidoPaterno} {estudiante.IdPersonaNavigation.ApellidoMaterno} {estudiante.IdPersonaNavigation.Nombre}".Trim()
                        : "Sin nombre",
                    Email = estudiante.Email ?? estudiante.IdPersonaNavigation?.Correo,
                    Telefono = estudiante.IdPersonaNavigation?.Telefono
                },
                Recibos = recibosDto,
                TotalAdeudo = recibosDto.Sum(r => r.Saldo)
            };
        }

        public async Task<RecibosParaCobroDto> BuscarTodosLosRecibosAsync(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
            {
                return new RecibosParaCobroDto { Recibos = new List<ReciboParaCobroDto>() };
            }

            var criterioLower = criterio.Trim().ToLower();

            var reciboPorFolio = await _context.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.Folio != null && r.Folio.ToLower().Contains(criterioLower))
                .FirstOrDefaultAsync();

            if (reciboPorFolio != null && reciboPorFolio.IdEstudiante.HasValue)
            {
                return await ObtenerTodosRecibosDeEstudiante(reciboPorFolio.IdEstudiante.Value);
            }

            if (criterioLower.StartsWith("doc-"))
            {
                var solicitudDoc = await _context.SolicitudesDocumento
                    .Where(s => s.FolioSolicitud.ToLower().Contains(criterioLower) && s.IdRecibo.HasValue)
                    .FirstOrDefaultAsync();

                if (solicitudDoc != null && solicitudDoc.IdRecibo.HasValue)
                {
                    var reciboDoc = await _context.Recibo
                        .Where(r => r.IdRecibo == solicitudDoc.IdRecibo.Value)
                        .FirstOrDefaultAsync();

                    if (reciboDoc != null && reciboDoc.IdEstudiante.HasValue)
                    {
                        return await ObtenerTodosRecibosDeEstudiante(reciboDoc.IdEstudiante.Value);
                    }
                }
            }

            var estudiantes = await _context.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Where(e => e.Activo &&
                    (e.Matricula.ToLower().Contains(criterioLower) ||
                     (e.IdPersonaNavigation != null &&
                      (e.IdPersonaNavigation.Nombre.ToLower().Contains(criterioLower) ||
                       e.IdPersonaNavigation.ApellidoPaterno.ToLower().Contains(criterioLower) ||
                       e.IdPersonaNavigation.ApellidoMaterno.ToLower().Contains(criterioLower) ||
                       (e.IdPersonaNavigation.Nombre + " " + e.IdPersonaNavigation.ApellidoPaterno + " " + e.IdPersonaNavigation.ApellidoMaterno).ToLower().Contains(criterioLower)))))
                .Take(10)
                .ToListAsync();

            if (estudiantes.Count == 0)
            {
                return new RecibosParaCobroDto
                {
                    Recibos = new List<ReciboParaCobroDto>(),
                    TotalAdeudo = 0
                };
            }

            if (estudiantes.Count == 1)
            {
                return await ObtenerTodosRecibosDeEstudiante(estudiantes[0].IdEstudiante);
            }

            return new RecibosParaCobroDto
            {
                Multiple = true,
                Estudiantes = estudiantes.Select(e => new EstudianteInfoDto
                {
                    IdEstudiante = e.IdEstudiante,
                    Matricula = e.Matricula,
                    NombreCompleto = e.IdPersonaNavigation != null
                        ? $"{e.IdPersonaNavigation.ApellidoPaterno} {e.IdPersonaNavigation.ApellidoMaterno} {e.IdPersonaNavigation.Nombre}".Trim()
                        : "Sin nombre",
                    Email = e.Email ?? e.IdPersonaNavigation?.Correo,
                    Telefono = e.IdPersonaNavigation?.Telefono
                }).ToList(),
                Recibos = new List<ReciboParaCobroDto>()
            };
        }

        private async Task<RecibosParaCobroDto> ObtenerTodosRecibosDeEstudiante(int idEstudiante)
        {
            var estudiante = await _context.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante);

            if (estudiante == null)
            {
                return new RecibosParaCobroDto { Recibos = new List<ReciboParaCobroDto>() };
            }

            var recibos = await _context.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.IdEstudiante == idEstudiante)
                .OrderByDescending(r => r.FechaEmision)
                .ThenByDescending(r => r.IdRecibo)
                .ToListAsync();

            var periodosIds = recibos.Where(r => r.IdPeriodoAcademico.HasValue).Select(r => r.IdPeriodoAcademico!.Value).Distinct().ToList();
            var periodos = await _context.PeriodoAcademico.Where(p => periodosIds.Contains(p.IdPeriodoAcademico)).ToDictionaryAsync(p => p.IdPeriodoAcademico, p => p.Nombre);

            var recibosDto = recibos.Select(r => new ReciboParaCobroDto
            {
                IdRecibo = r.IdRecibo,
                Folio = r.Folio,
                IdAspirante = r.IdAspirante,
                IdEstudiante = r.IdEstudiante,
                IdPeriodoAcademico = r.IdPeriodoAcademico,
                IdGrupo = null,
                IdPlantillaCobro = null,
                FechaEmision = r.FechaEmision.ToString("yyyy-MM-dd"),
                FechaVencimiento = r.FechaVencimiento.ToString("yyyy-MM-dd"),
                Estatus = (int)r.Estatus,
                EstatusNombre = r.Estatus.ToString(),
                Subtotal = r.Subtotal,
                DescuentoBeca = r.Descuento,
                Descuento = r.Descuento,
                DescuentoAdicional = 0,
                Recargos = r.Recargos,
                Total = r.Total,
                Saldo = r.Saldo,
                Notas = r.Notas,
                CreadoPor = r.CreatedBy,
                FechaCreacion = r.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                NombrePeriodo = r.IdPeriodoAcademico.HasValue && periodos.ContainsKey(r.IdPeriodoAcademico.Value) ? periodos[r.IdPeriodoAcademico.Value] : null,
                CodigoGrupo = null,
                Detalles = r.Detalles.Select(d => new ReciboDetalleParaCobroDto
                {
                    IdReciboDetalle = d.IdReciboDetalle,
                    IdConceptoPago = d.IdConceptoPago,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Importe = d.Importe,
                    DescuentoBeca = r.Descuento > 0 && r.Subtotal > 0 ? (decimal?)Math.Round(d.Importe * (r.Descuento / r.Subtotal), 2) : null,
                    ImporteNeto = r.Descuento > 0 && r.Subtotal > 0 ? (decimal?)(d.Importe - Math.Round(d.Importe * (r.Descuento / r.Subtotal), 2)) : null,
                    IdPlantillaDetalle = null,
                    RefTabla = d.RefTabla,
                    RefId = d.RefId
                }).ToList()
            }).ToList();

            var pendientes = recibosDto.Where(r => r.Estatus != (int)EstatusRecibo.PAGADO && r.Estatus != (int)EstatusRecibo.CANCELADO);

            return new RecibosParaCobroDto
            {
                Estudiante = new EstudianteInfoDto
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    Matricula = estudiante.Matricula,
                    NombreCompleto = estudiante.IdPersonaNavigation != null
                        ? $"{estudiante.IdPersonaNavigation.ApellidoPaterno} {estudiante.IdPersonaNavigation.ApellidoMaterno} {estudiante.IdPersonaNavigation.Nombre}".Trim()
                        : "Sin nombre",
                    Email = estudiante.Email ?? estudiante.IdPersonaNavigation?.Correo,
                    Telefono = estudiante.IdPersonaNavigation?.Telefono
                },
                Recibos = recibosDto,
                TotalAdeudo = pendientes.Sum(r => r.Saldo),
                TotalPagado = recibosDto.Where(r => r.Estatus == (int)EstatusRecibo.PAGADO).Sum(r => r.Total)
            };
        }

        public async Task<ResumenCorteCajaDto> ObtenerResumenCorteCaja(DateTime fechaInicio, DateTime fechaFin, string? usuarioId = null)
        {
            return await GenerarCorteCajaDetalladoAsync(usuarioId, fechaInicio, fechaFin);
        }

        public async Task<CorteCaja> CerrarCorteCaja(string usuarioId, decimal montoInicial, string? observaciones = null)
        {
            var corteActivo = await ObtenerCorteActivo(usuarioId);

            DateTime fechaInicio;
            DateTime fechaFin = DateTime.UtcNow;

            if (corteActivo != null)
            {
                fechaInicio = corteActivo.FechaInicio;
                _context.CorteCaja.Remove(corteActivo);
            }
            else
            {
                fechaInicio = DateTime.Today.ToUniversalTime();
            }

            var resumen = await ObtenerResumenCorteCaja(fechaInicio, fechaFin, usuarioId);

            var folio = GenerarFolioCorteCaja();

            var corteCaja = new CorteCaja
            {
                FolioCorteCaja = folio,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                IdUsuarioCaja = usuarioId,
                MontoInicial = montoInicial,
                TotalEfectivo = resumen.Totales.Efectivo,
                TotalTransferencia = resumen.Totales.Transferencia,
                TotalTarjeta = resumen.Totales.Tarjeta,
                TotalGeneral = resumen.Totales.Total,
                Cerrado = true,
                FechaCierre = DateTime.UtcNow,
                CerradoPor = usuarioId,
                Observaciones = observaciones
            };

            _context.CorteCaja.Add(corteCaja);

            var pagosIds = resumen.Pagos.Select(p => p.IdPago).ToList();
            var pagos = await _context.Pago.Where(p => pagosIds.Contains(p.IdPago)).ToListAsync();

            foreach (var pago in pagos)
            {
                pago.IdCorteCaja = corteCaja.IdCorteCaja;
            }

            await _context.SaveChangesAsync();

            return corteCaja;
        }

        public async Task<List<CorteCaja>> ObtenerCortesCaja(string? usuarioId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var query = _context.CorteCaja.AsQueryable();

            if (!string.IsNullOrEmpty(usuarioId))
            {
                query = query.Where(c => c.IdUsuarioCaja == usuarioId);
            }

            if (fechaInicio.HasValue)
            {
                query = query.Where(c => c.FechaInicio >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(c => c.FechaFin <= fechaFin.Value);
            }

            return await query
                .OrderByDescending(c => c.FechaFin)
                .ToListAsync();
        }

        public async Task<CorteCaja?> ObtenerCorteCajaPorId(int idCorteCaja)
        {
            return await _context.CorteCaja
                .Include(c => c.UsuarioCaja)
                .FirstOrDefaultAsync(c => c.IdCorteCaja == idCorteCaja);
        }

        public async Task<CorteCaja?> ObtenerCorteActivo(string usuarioId)
        {
            return await _context.CorteCaja
                .Where(c => c.IdUsuarioCaja == usuarioId && !c.Cerrado)
                .FirstOrDefaultAsync();
        }

        private string GenerarFolioCorteCaja()
        {
            var fecha = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"CC-{fecha}-{random}";
        }

        public async Task<List<UsuarioCajeroDto>> ObtenerCajerosAsync()
        {
            var usuariosConCobros = await _context.Pago
                .Where(p => p.IdUsuarioCaja != null && p.Estatus == EstatusPago.CONFIRMADO)
                .GroupBy(p => p.IdUsuarioCaja)
                .Select(g => new
                {
                    IdUsuario = g.Key,
                    TotalCobros = g.Count(),
                    UltimoCobro = g.Max(p => p.FechaPagoUtc)
                })
                .ToListAsync();

            var cajeros = new List<UsuarioCajeroDto>();

            foreach (var usuario in usuariosConCobros)
            {
                if (string.IsNullOrEmpty(usuario.IdUsuario)) continue;

                var user = await _authService.GetUserById(usuario.IdUsuario);
                if (user != null)
                {
                    cajeros.Add(new UsuarioCajeroDto
                    {
                        IdUsuario = usuario.IdUsuario,
                        NombreCompleto = $"{user.Nombres} {user.Apellidos}".Trim(),
                        Email = user.Email,
                        TotalCobros = usuario.TotalCobros,
                        UltimoCobro = usuario.UltimoCobro
                    });
                }
            }

            return cajeros.OrderBy(c => c.NombreCompleto).ToList();
        }

        public async Task<ResumenCorteCajaDto> GenerarCorteCajaDetalladoAsync(string? usuarioId, DateTime fechaInicio, DateTime fechaFin)
        {
            Console.WriteLine($"\n=== GenerarCorteCajaDetalladoAsync ===");
            Console.WriteLine($"Periodo: {fechaInicio} - {fechaFin}, Usuario: {usuarioId ?? "TODOS"}");

            var query = _context.Pago
                .Where(p => p.FechaPagoUtc >= fechaInicio && p.FechaPagoUtc <= fechaFin && p.Estatus == EstatusPago.CONFIRMADO);

            if (!string.IsNullOrEmpty(usuarioId))
            {
                query = query.Where(p => p.IdUsuarioCaja == usuarioId);
            }

            var pagos = await query
                .OrderBy(p => p.FechaPagoUtc)
                .ToListAsync();

            Console.WriteLine($"Pagos encontrados: {pagos.Count}");

            var pagoIds = pagos.Select(p => p.IdPago).ToList();

            var medioPagoIds = pagos.Select(p => p.IdMedioPago).Distinct().ToList();
            var mediosPago = await _context.Set<MedioPago>()
                .Where(m => medioPagoIds.Contains(m.IdMedioPago))
                .ToDictionaryAsync(m => m.IdMedioPago);

            Console.WriteLine($"Medios de pago cargados: {mediosPago.Count}");

            var aplicaciones = await _context.PagoAplicacion
                .Where(a => pagoIds.Contains(a.IdPago))
                .ToListAsync();

            Console.WriteLine($"Aplicaciones encontradas: {aplicaciones.Count}");

            var reciboDetalleIds = aplicaciones.Select(a => a.IdReciboDetalle).Distinct().ToList();

            var recibosDetalle = await _context.ReciboDetalle
                .Where(rd => reciboDetalleIds.Contains(rd.IdReciboDetalle))
                .ToDictionaryAsync(rd => rd.IdReciboDetalle);

            Console.WriteLine($"ReciboDetalle cargados: {recibosDetalle.Count}");

            var reciboIds = recibosDetalle.Values.Select(rd => rd.IdRecibo).Distinct().ToList();

            Console.WriteLine($"Recibos únicos: {reciboIds.Count}");

            var recibos = await _context.Recibo
                .Where(r => reciboIds.Contains(r.IdRecibo))
                .ToDictionaryAsync(r => r.IdRecibo);

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

            Console.WriteLine($"Estudiantes: {estudianteIds.Count}, Aspirantes: {aspiranteIds.Count}");

            var estudiantes = await _context.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Where(e => estudianteIds.Contains(e.IdEstudiante))
                .ToDictionaryAsync(e => e.IdEstudiante);

            var aspirantes = await _context.Aspirante
                .Include(a => a.IdPersonaNavigation)
                .Where(a => aspiranteIds.Contains(a.IdAspirante))
                .ToDictionaryAsync(a => a.IdAspirante);

            var aplicacionesPorPago = aplicaciones.GroupBy(a => a.IdPago).ToDictionary(g => g.Key, g => g.ToList());

            var pagosDetallados = pagos.Select(p =>
            {
                var apps = aplicacionesPorPago.ContainsKey(p.IdPago) ? aplicacionesPorPago[p.IdPago] : new List<PagoAplicacion>();
                var primeraAplicacion = apps.FirstOrDefault();

                Recibo? recibo = null;
                ReciboDetalle? detalle = null;

                if (primeraAplicacion != null && recibosDetalle.TryGetValue(primeraAplicacion.IdReciboDetalle, out detalle))
                {
                    recibos.TryGetValue(detalle.IdRecibo, out recibo);
                }

                string? matricula = null;
                string nombrePagador = "Sin información";

                if (recibo?.IdEstudiante != null && estudiantes.TryGetValue(recibo.IdEstudiante.Value, out var est))
                {
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

                var concepto = detalle?.Descripcion ?? "Sin concepto";

                var medioPagoDescripcion = mediosPago.TryGetValue(p.IdMedioPago, out var mp)
                    ? (mp.Descripcion ?? mp.Clave)
                    : "Desconocido";

                return new PagoDetalladoDto
                {
                    IdPago = p.IdPago,
                    FolioPago = p.FolioPago,
                    FechaPagoUtc = p.FechaPagoUtc,
                    HoraPago = p.FechaPagoUtc.ToLocalTime().ToString("HH:mm:ss"),
                    IdMedioPago = p.IdMedioPago,
                    MedioPago = medioPagoDescripcion,
                    Monto = p.Monto,
                    Moneda = p.Moneda,
                    Referencia = p.Referencia,
                    Notas = p.Notas,
                    Estatus = (int)p.Estatus,
                    EstatusNombre = p.Estatus.ToString(),
                    IdEstudiante = recibo?.IdEstudiante,
                    Matricula = matricula,
                    NombreEstudiante = nombrePagador,
                    Concepto = concepto,
                    FolioRecibo = recibo?.Folio
                };
            }).ToList();

            CajeroInfoDto? cajeroInfo = null;
            if (!string.IsNullOrEmpty(usuarioId))
            {
                var usuario = await _authService.GetUserById(usuarioId);
                if (usuario != null)
                {
                    cajeroInfo = new CajeroInfoDto
                    {
                        IdUsuario = usuarioId,
                        NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}".Trim(),
                        Email = usuario.Email
                    };
                }
            }

            var totales = new TotalesCorteCajaDto
            {
                Cantidad = pagos.Count,
                Efectivo = pagos.Where(p => p.IdMedioPago == 1).Sum(p => p.Monto),
                Transferencia = pagos.Where(p => p.IdMedioPago == 2).Sum(p => p.Monto),
                Tarjeta = pagos.Where(p => p.IdMedioPago == 3).Sum(p => p.Monto),
                Total = pagos.Sum(p => p.Monto)
            };

            Console.WriteLine($"=== FIN GenerarCorteCajaDetalladoAsync ===\n");

            return new ResumenCorteCajaDto
            {
                Cajero = cajeroInfo,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Pagos = pagosDetallados,
                Totales = totales
            };
        }

        public byte[] GenerarPdfCorteCaja(ResumenCorteCajaDto resumen)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            if (File.Exists(_logoPath))
                            {
                                row.ConstantItem(100).Height(60).Image(_logoPath).FitArea();
                            }
                            else
                            {
                                row.ConstantItem(100).Height(60).Background(ColorGrisClaro)
                                    .AlignCenter().AlignMiddle()
                                    .Text("USAG").FontSize(12).Bold().FontColor(ColorAzulOscuro);
                            }

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().Text("UNIVERSIDAD SAN ANDRÉS DE GUANAJUATO")
                                    .FontSize(14).Bold().FontColor(ColorAzulOscuro);
                                c.Item().AlignCenter().Text("CORTE DE CAJA")
                                    .FontSize(12).SemiBold().FontColor(ColorAzulClaro);
                                c.Item().AlignCenter().Text("Reporte de Ingresos")
                                    .FontSize(9).FontColor(ColorGris);
                            });

                            row.ConstantItem(100);
                        });

                        col.Item().PaddingTop(10).LineHorizontal(2).LineColor(ColorAzulOscuro);

                        col.Item().PaddingTop(10).Background(ColorGrisClaro).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(text =>
                                {
                                    text.Span("Cajero: ").Bold().FontColor(ColorAzulOscuro);
                                    text.Span(resumen.Cajero?.NombreCompleto ?? "Todos los cajeros");
                                });
                                c.Item().PaddingTop(3).Text(text =>
                                {
                                    text.Span("Período: ").Bold().FontColor(ColorAzulOscuro);
                                    text.Span($"{resumen.FechaInicio:dd/MM/yyyy HH:mm} - {resumen.FechaFin:dd/MM/yyyy HH:mm}");
                                });
                            });
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text(text =>
                                {
                                    text.Span("Fecha de generación: ").Bold().FontColor(ColorAzulOscuro);
                                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                                });
                                c.Item().PaddingTop(3).Text(text =>
                                {
                                    text.Span("Total de operaciones: ").Bold().FontColor(ColorAzulOscuro);
                                    text.Span(resumen.Totales.Cantidad.ToString());
                                });
                            });
                        });

                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor(ColorGris);
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Background("#E8F5E9").Padding(10).Column(c =>
                            {
                                c.Item().AlignCenter().Text("Efectivo").FontSize(9).FontColor(ColorGris);
                                c.Item().AlignCenter().Text($"${resumen.Totales.Efectivo:N2}").FontSize(14).Bold().FontColor("#2E7D32");
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Background("#E3F2FD").Padding(10).Column(c =>
                            {
                                c.Item().AlignCenter().Text("Transferencia").FontSize(9).FontColor(ColorGris);
                                c.Item().AlignCenter().Text($"${resumen.Totales.Transferencia:N2}").FontSize(14).Bold().FontColor("#1565C0");
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Background("#F3E5F5").Padding(10).Column(c =>
                            {
                                c.Item().AlignCenter().Text("Tarjeta").FontSize(9).FontColor(ColorGris);
                                c.Item().AlignCenter().Text($"${resumen.Totales.Tarjeta:N2}").FontSize(14).Bold().FontColor("#7B1FA2");
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Background(ColorAzulOscuro).Padding(10).Column(c =>
                            {
                                c.Item().AlignCenter().Text("TOTAL").FontSize(9).FontColor(Colors.White);
                                c.Item().AlignCenter().Text($"${resumen.Totales.Total:N2}").FontSize(14).Bold().FontColor(Colors.White);
                            });
                        });

                        col.Item().Height(15);

                        col.Item().Text("DETALLE DE PAGOS").FontSize(11).Bold().FontColor(ColorAzulOscuro);
                        col.Item().Height(5);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(45);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(65);
                                columns.ConstantColumn(70);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(ColorAzulOscuro).Padding(6).Text("Folio").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(ColorAzulOscuro).Padding(6).Text("Hora").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(ColorAzulOscuro).Padding(6).Text("Estudiante").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(ColorAzulOscuro).Padding(6).Text("Concepto").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(ColorAzulOscuro).Padding(6).Text("Medio").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(ColorAzulOscuro).Padding(6).AlignRight().Text("Monto").FontColor(Colors.White).Bold().FontSize(9);
                            });

                            var index = 0;
                            foreach (var pago in resumen.Pagos)
                            {
                                var bgColor = index % 2 == 0 ? "#FFFFFF" : ColorGrisClaro;

                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor("#E0E0E0").Padding(4).Text(pago.FolioPago ?? "-").FontSize(8);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor("#E0E0E0").Padding(4).Text(pago.HoraPago ?? "-").FontSize(8);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor("#E0E0E0").Padding(4).Column(c =>
                                {
                                    c.Item().Text(pago.NombreEstudiante ?? "-").FontSize(8);
                                    if (!string.IsNullOrEmpty(pago.Matricula))
                                    {
                                        c.Item().Text(pago.Matricula).FontSize(7).FontColor(ColorGris);
                                    }
                                });
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor("#E0E0E0").Padding(4).Text(pago.Concepto ?? "-").FontSize(8);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor("#E0E0E0").Padding(4).Text(pago.MedioPago ?? "-").FontSize(8);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor("#E0E0E0").Padding(4).AlignRight().Text($"${pago.Monto:N2}").FontSize(8).Bold();
                                index++;
                            }
                        });

                        if (resumen.Pagos.Count == 0)
                        {
                            col.Item().Padding(20).AlignCenter().Text("No se encontraron pagos en el período seleccionado")
                                .FontSize(10).Italic().FontColor(ColorGris);
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(ColorGris);
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Universidad San Andrés de Guanajuato").FontSize(8).FontColor(ColorGris);
                                c.Item().Text("www.usag.edu.mx").FontSize(7).FontColor(ColorAzulClaro);
                            });
                            row.RelativeItem().AlignCenter().Text(text =>
                            {
                                text.Span("Documento generado automáticamente").FontSize(7).FontColor(ColorGris);
                            });
                            row.RelativeItem().AlignRight().Text(text =>
                            {
                                text.Span("Página ").FontSize(8).FontColor(ColorGris);
                                text.CurrentPageNumber().FontSize(8).FontColor(ColorGris);
                                text.Span(" de ").FontSize(8).FontColor(ColorGris);
                                text.TotalPages().FontSize(8).FontColor(ColorGris);
                            });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<QuitarRecargoResultado> QuitarRecargoAsync(long idRecibo, string motivo, string nombreUsuario, string usuarioId)
        {
            var recibo = await _context.Recibo
                .FirstOrDefaultAsync(r => r.IdRecibo == idRecibo);

            if (recibo == null)
            {
                return new QuitarRecargoResultado
                {
                    Exitoso = false,
                    Mensaje = "Recibo no encontrado"
                };
            }

            if (recibo.Estatus == Core.Enums.EstatusRecibo.PAGADO)
            {
                return new QuitarRecargoResultado
                {
                    Exitoso = false,
                    Mensaje = "No se puede modificar un recibo ya pagado"
                };
            }

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
            var diasVencido = hoy.DayNumber - recibo.FechaVencimiento.DayNumber;
            decimal recargoCalculado = 0;

            if (diasVencido > 0)
            {
                var politica = await _context.RecargoPolitica
                    .Where(p => p.Activo)
                    .FirstOrDefaultAsync();

                decimal tasaDiaria = politica?.TasaDiaria ?? 0.01m;

                recargoCalculado = recibo.Saldo * tasaDiaria * diasVencido;
                if (politica?.RecargoMaximo.HasValue == true && recargoCalculado > politica.RecargoMaximo.Value)
                {
                    recargoCalculado = politica.RecargoMaximo.Value;
                }
            }

            var recargoAnterior = recibo.Recargos > 0 ? recibo.Recargos : recargoCalculado;

            var bitacora = new BitacoraRecibo
            {
                IdRecibo = recibo.IdRecibo,
                Usuario = nombreUsuario,
                FechaUtc = DateTime.UtcNow,
                Accion = "CONDONACION_RECARGO",
                Notas = $"Recargo condonado de ${recargoAnterior:N2} a $0.00. Motivo: {motivo}"
            };

            _context.BitacoraRecibo.Add(bitacora);

            recibo.Notas = string.IsNullOrEmpty(recibo.Notas)
                ? $"[RECARGO CONDONADO: ${recargoAnterior:N2} - {motivo} - Por: {nombreUsuario} - {DateTime.Now:dd/MM/yyyy HH:mm}]"
                : $"{recibo.Notas}\n[RECARGO CONDONADO: ${recargoAnterior:N2} - {motivo} - Por: {nombreUsuario} - {DateTime.Now:dd/MM/yyyy HH:mm}]";

            recibo.Recargos = 0;

            await _context.SaveChangesAsync();

            return new QuitarRecargoResultado
            {
                Exitoso = true,
                Mensaje = $"Recargo de ${recargoAnterior:N2} condonado exitosamente",
                RecargoCondonado = recargoAnterior
            };
        }

        public async Task<ModificarDetalleResultado> ModificarDetalleReciboAsync(long idRecibo, long idReciboDetalle, decimal nuevoMonto, string motivo, string usuarioId)
        {
            var recibo = await _context.Recibo
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.IdRecibo == idRecibo);

            if (recibo == null)
            {
                return new ModificarDetalleResultado
                {
                    Exitoso = false,
                    Mensaje = "Recibo no encontrado"
                };
            }

            if (recibo.Estatus == Core.Enums.EstatusRecibo.PAGADO)
            {
                return new ModificarDetalleResultado
                {
                    Exitoso = false,
                    Mensaje = "No se puede modificar un recibo ya pagado"
                };
            }

            var detalle = recibo.Detalles.FirstOrDefault(d => d.IdReciboDetalle == idReciboDetalle);
            if (detalle == null)
            {
                return new ModificarDetalleResultado
                {
                    Exitoso = false,
                    Mensaje = "Detalle de recibo no encontrado"
                };
            }

            if (nuevoMonto < 0)
            {
                return new ModificarDetalleResultado
                {
                    Exitoso = false,
                    Mensaje = "El monto no puede ser negativo"
                };
            }

            var montoAnterior = detalle.Importe;

            var usuario = await _authService.GetUserById(usuarioId);
            var nombreUsuario = usuario != null ? $"{usuario.Nombres} {usuario.Apellidos}" : "Sistema";

            var bitacora = new BitacoraRecibo
            {
                IdRecibo = recibo.IdRecibo,
                Usuario = nombreUsuario,
                FechaUtc = DateTime.UtcNow,
                Accion = "MODIFICACION_MONTO",
                Notas = $"Concepto '{detalle.Descripcion}' modificado de ${montoAnterior:N2} a ${nuevoMonto:N2}. Motivo: {motivo}"
            };
            _context.BitacoraRecibo.Add(bitacora);

            var nuevoImporte = nuevoMonto * detalle.Cantidad;

            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE ReciboDetalle SET PrecioUnitario = {0} WHERE IdReciboDetalle = {1}",
                nuevoMonto, detalle.IdReciboDetalle);

            var nuevoSubtotal = recibo.Detalles.Where(d => d.IdReciboDetalle != detalle.IdReciboDetalle).Sum(d => d.Importe) + nuevoImporte;

            var nuevoTotal = nuevoSubtotal - recibo.Descuento + recibo.Recargos;
            var montoPagado = recibo.Total - recibo.Saldo;
            var nuevoSaldo = Math.Max(0, nuevoTotal - montoPagado);

            var nuevaNota = $"[MONTO MODIFICADO: {detalle.Descripcion} de ${montoAnterior:N2} a ${nuevoMonto:N2} - {motivo} - Por: {nombreUsuario} - {DateTime.Now:dd/MM/yyyy HH:mm}]";
            var notaFinal = string.IsNullOrEmpty(recibo.Notas) ? nuevaNota : $"{recibo.Notas}\n{nuevaNota}";

            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Recibo SET Subtotal = {0}, Saldo = {1}, Notas = {2} WHERE IdRecibo = {3}",
                nuevoSubtotal, nuevoSaldo, notaFinal, recibo.IdRecibo);

            await _context.SaveChangesAsync();

            return new ModificarDetalleResultado
            {
                Exitoso = true,
                Mensaje = $"Monto modificado exitosamente de ${montoAnterior:N2} a ${nuevoMonto:N2}",
                MontoAnterior = montoAnterior,
                MontoNuevo = nuevoMonto,
                NuevoTotal = nuevoTotal,
                NuevoSaldo = nuevoSaldo
            };
        }

        public async Task<ModificarRecargoResultado> ModificarRecargoReciboAsync(long idRecibo, decimal nuevoRecargo, string motivo, string usuarioId)
        {
            var recibo = await _context.Recibo
                .FirstOrDefaultAsync(r => r.IdRecibo == idRecibo);

            if (recibo == null)
            {
                return new ModificarRecargoResultado
                {
                    Exitoso = false,
                    Mensaje = "Recibo no encontrado"
                };
            }

            if (recibo.Estatus == Core.Enums.EstatusRecibo.PAGADO)
            {
                return new ModificarRecargoResultado
                {
                    Exitoso = false,
                    Mensaje = "No se puede modificar un recibo ya pagado"
                };
            }

            if (nuevoRecargo < 0)
            {
                return new ModificarRecargoResultado
                {
                    Exitoso = false,
                    Mensaje = "El recargo no puede ser negativo"
                };
            }

            var recargoAnterior = recibo.Recargos;

            var usuario = await _authService.GetUserById(usuarioId);
            var nombreUsuario = usuario != null ? $"{usuario.Nombres} {usuario.Apellidos}" : "Sistema";

            var bitacora = new BitacoraRecibo
            {
                IdRecibo = recibo.IdRecibo,
                Usuario = nombreUsuario,
                FechaUtc = DateTime.UtcNow,
                Accion = "MODIFICACION_RECARGO",
                Notas = $"Recargo modificado de ${recargoAnterior:N2} a ${nuevoRecargo:N2}. Motivo: {motivo}"
            };
            _context.BitacoraRecibo.Add(bitacora);

            var nuevoTotal = recibo.Subtotal - recibo.Descuento + nuevoRecargo;
            var montoPagado = recibo.Total - recibo.Saldo;
            var nuevoSaldo = Math.Max(0, nuevoTotal - montoPagado);

            var nuevaNota = $"[RECARGO MODIFICADO: de ${recargoAnterior:N2} a ${nuevoRecargo:N2} - {motivo} - Por: {nombreUsuario} - {DateTime.Now:dd/MM/yyyy HH:mm}]";
            var notaFinal = string.IsNullOrEmpty(recibo.Notas) ? nuevaNota : $"{recibo.Notas}\n{nuevaNota}";

            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Recibo SET Recargos = {0}, Saldo = {1}, Notas = {2} WHERE IdRecibo = {3}",
                nuevoRecargo, nuevoSaldo, notaFinal, recibo.IdRecibo);

            await _context.SaveChangesAsync();

            return new ModificarRecargoResultado
            {
                Exitoso = true,
                Mensaje = $"Recargo modificado exitosamente de ${recargoAnterior:N2} a ${nuevoRecargo:N2}",
                RecargoAnterior = recargoAnterior,
                RecargoNuevo = nuevoRecargo,
                NuevoTotal = nuevoTotal
            };
        }
    }
}
