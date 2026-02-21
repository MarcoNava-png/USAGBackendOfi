using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.PlantillaCobro;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Inscripcion;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public class InscripcionService : IInscripcionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IReciboService _reciboService;
        private readonly IBecaService _becaService;
        private readonly IPlantillaCobroService _plantillaCobroService;

        public InscripcionService(
            ApplicationDbContext dbContext,
            IReciboService reciboService,
            IBecaService becaService,
            IPlantillaCobroService plantillaCobroService)
        {
            _dbContext = dbContext;
            _reciboService = reciboService;
            _becaService = becaService;
            _plantillaCobroService = plantillaCobroService;
        }

        public async Task<Inscripcion> CrearInscripcion(Inscripcion inscripcion)
        {
            await _dbContext.AddAsync(inscripcion);
            await _dbContext.SaveChangesAsync();

            return inscripcion;
        }

        public async Task<bool> EsNuevoIngresoAsync(int idEstudiante, int idPeriodoAcademico, CancellationToken ct = default)
        {
            var tieneInscripcionesPrevias = await _dbContext.Inscripcion
                .AnyAsync(i => i.IdEstudiante == idEstudiante
                            && i.Status == StatusEnum.Active, ct);

            if (!tieneInscripcionesPrevias)
                return true;

            var tieneInscripcionEnEstePeriodo = await _dbContext.Inscripcion
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm.IdGrupoNavigation)
                .AnyAsync(i => i.IdEstudiante == idEstudiante
                            && i.IdGrupoMateriaNavigation.IdGrupoNavigation.IdPeriodoAcademico == idPeriodoAcademico
                            && i.Status == StatusEnum.Active, ct);

            return !tieneInscripcionEnEstePeriodo;
        }

        public async Task<InscripcionConPagosDto> InscribirConRecibosAutomaticosAsync(
            InscribirConRecibosRequest request,
            CancellationToken ct = default)
        {
            var grupoMateria = await _dbContext.GrupoMateria
                .Include(gm => gm.IdGrupoNavigation)
                    .ThenInclude(g => g.IdPeriodoAcademicoNavigation)
                        .ThenInclude(p => p.IdPeriodicidadNavigation)
                .Include(gm => gm.IdMateriaPlanNavigation)
                    .ThenInclude(mp => mp.IdMateriaNavigation)
                .FirstOrDefaultAsync(gm => gm.IdGrupoMateria == request.IdGrupoMateria
                                        && gm.Status == StatusEnum.Active, ct);

            if (grupoMateria == null)
                throw new InvalidOperationException($"No se encontro el grupo-materia con ID {request.IdGrupoMateria}");

            var periodoAcademico = grupoMateria.IdGrupoNavigation.IdPeriodoAcademicoNavigation;
            var periodicidad = periodoAcademico.IdPeriodicidadNavigation;

            var estudiante = await _dbContext.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == request.IdEstudiante
                                       && e.Status == StatusEnum.Active, ct);

            if (estudiante == null)
                throw new InvalidOperationException($"No se encontro el estudiante con ID {request.IdEstudiante}");

            bool esNuevoIngreso = request.EsNuevoIngreso ??
                                 await EsNuevoIngresoAsync(request.IdEstudiante, periodoAcademico.IdPeriodoAcademico, ct);

            var inscripcion = new Inscripcion
            {
                IdEstudiante = request.IdEstudiante,
                IdGrupoMateria = request.IdGrupoMateria,
                FechaInscripcion = request.FechaInscripcion,
                Estado = ((int)EstadoInscripcionEnum.Inscrito).ToString(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "sistema",
                Status = StatusEnum.Active
            };

            await _dbContext.Inscripcion.AddAsync(inscripcion, ct);
            await _dbContext.SaveChangesAsync(ct);

            var planPago = await ObtenerOCrearPlanPagoAsync(
                periodoAcademico.IdPeriodoAcademico,
                periodicidad,
                esNuevoIngreso,
                ct);

            await AsignarPlanPagoSiNoExisteAsync(planPago.IdPlanPago, estudiante.IdEstudiante, ct);

            var recibos = await GenerarRecibosParaInscripcionAsync(
                estudiante.IdEstudiante,
                periodoAcademico.IdPeriodoAcademico,
                planPago.IdPlanPago,
                esNuevoIngreso,
                periodicidad.MesesPorPeriodo,
                periodoAcademico.FechaInicio,
                ct);

            var resultado = new InscripcionConPagosDto
            {
                IdInscripcion = inscripcion.IdInscripcion,
                IdEstudiante = estudiante.IdEstudiante,
                Matricula = estudiante.Matricula,
                NombreEstudiante = $"{estudiante.IdPersonaNavigation.Nombre} {estudiante.IdPersonaNavigation.ApellidoPaterno}",
                IdGrupoMateria = grupoMateria.IdGrupoMateria,
                NombreMateria = grupoMateria.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                EsNuevoIngreso = esNuevoIngreso,
                TipoInscripcion = esNuevoIngreso ? "Nuevo Ingreso" : "Re-inscripcion",
                CantidadRecibosGenerados = recibos.Count,
                MontoTotalRecibos = recibos.Sum(r => r.Total),
                Recibos = recibos
            };

            return resultado;
        }

        private async Task<PlanPago> ObtenerOCrearPlanPagoAsync(
            int idPeriodoAcademico,
            Periodicidad periodicidad,
            bool incluyeInscripcion,
            CancellationToken ct)
        {
            var planExistente = await _dbContext.PlanPago
                .Include(pp => pp.Detalles)
                .FirstOrDefaultAsync(pp => pp.IdPeriodoAcademico == idPeriodoAcademico
                                        && pp.Activo
                                        && pp.Status == StatusEnum.Active, ct);

            if (planExistente != null)
                return planExistente;

            var periodoAcad = await _dbContext.PeriodoAcademico
                .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == idPeriodoAcademico, ct);

            var planPago = new PlanPago
            {
                Nombre = $"Plan {periodoAcad.Nombre}",
                IdPeriodicidad = periodicidad.IdPeriodicidad,
                IdPeriodoAcademico = idPeriodoAcademico,
                IdModalidadPlan = _dbContext.ModalidadPlan.First(m => m.DescModalidadPlan == "Con Título").IdModalidadPlan,
                Moneda = "MXN",
                Activo = true,
                VigenciaDesde = periodoAcad.FechaInicio,
                VigenciaHasta = periodoAcad.FechaFin,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "sistema",
                Status = StatusEnum.Active
            };

            await _dbContext.PlanPago.AddAsync(planPago, ct);
            await _dbContext.SaveChangesAsync(ct);

            await AgregarDetallesPlanPagoAsync(planPago.IdPlanPago, periodicidad.MesesPorPeriodo, incluyeInscripcion, ct);

            return planPago;
        }

        private async Task AgregarDetallesPlanPagoAsync(
            int idPlanPago,
            byte mesesPorPeriodo,
            bool incluyeInscripcion,
            CancellationToken ct)
        {
            int orden = 1;

            var conceptoInscripcion = await _dbContext.ConceptoPago
                .FirstOrDefaultAsync(c => c.Clave == "INSCRIPCION" && c.Activo, ct);

            var conceptoColegiatura = await _dbContext.ConceptoPago
                .FirstOrDefaultAsync(c => c.Clave == "COLEGIATURA" && c.Activo, ct);

            if (conceptoColegiatura == null)
                throw new InvalidOperationException("No se encontro el concepto de COLEGIATURA");

            var precioColegiatura = await _dbContext.ConceptoPrecio
                .Where(cp => cp.IdConceptoPago == conceptoColegiatura.IdConceptoPago
                          && cp.Activo
                          && cp.VigenciaDesde <= DateOnly.FromDateTime(DateTime.UtcNow)
                          && cp.VigenciaHasta >= DateOnly.FromDateTime(DateTime.UtcNow))
                .OrderByDescending(cp => cp.VigenciaDesde)
                .FirstOrDefaultAsync(ct);

            if (precioColegiatura == null)
                throw new InvalidOperationException("No se encontro precio vigente para COLEGIATURA");

            if (incluyeInscripcion && conceptoInscripcion != null)
            {
                var precioInscripcion = await _dbContext.ConceptoPrecio
                    .Where(cp => cp.IdConceptoPago == conceptoInscripcion.IdConceptoPago
                              && cp.Activo
                              && cp.VigenciaDesde <= DateOnly.FromDateTime(DateTime.UtcNow)
                              && cp.VigenciaHasta >= DateOnly.FromDateTime(DateTime.UtcNow))
                    .OrderByDescending(cp => cp.VigenciaDesde)
                    .FirstOrDefaultAsync(ct);

                if (precioInscripcion != null)
                {
                    var detalleInscripcion = new PlanPagoDetalle
                    {
                        IdPlanPago = idPlanPago,
                        Orden = orden++,
                        IdConceptoPago = conceptoInscripcion.IdConceptoPago,
                        Descripcion = "Inscripcion",
                        Cantidad = 1,
                        Importe = precioInscripcion.Importe,
                        EsInscripcion = true,
                        EsMensualidad = false,
                        MesOffset = 0,
                        DiaPago = 5,
                        PintaInternet = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "sistema",
                        Status = StatusEnum.Active
                    };

                    await _dbContext.PlanPagoDetalle.AddAsync(detalleInscripcion, ct);
                }
            }

            for (int mes = 0; mes < mesesPorPeriodo; mes++)
            {
                var detalleColegiatura = new PlanPagoDetalle
                {
                    IdPlanPago = idPlanPago,
                    Orden = orden++,
                    IdConceptoPago = conceptoColegiatura.IdConceptoPago,
                    Descripcion = $"Colegiatura {mes + 1}",
                    Cantidad = 1,
                    Importe = precioColegiatura.Importe,
                    EsInscripcion = false,
                    EsMensualidad = true,
                    MesOffset = mes,
                    DiaPago = 5,
                    PintaInternet = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "sistema",
                    Status = StatusEnum.Active
                };

                await _dbContext.PlanPagoDetalle.AddAsync(detalleColegiatura, ct);
            }

            await _dbContext.SaveChangesAsync(ct);
        }

        private async Task AsignarPlanPagoSiNoExisteAsync(int idPlanPago, int idEstudiante, CancellationToken ct)
        {
            var yaAsignado = await _dbContext.PlanPagoAsignacion
                .AnyAsync(ppa => ppa.IdPlanPago == idPlanPago
                              && ppa.IdEstudiante == idEstudiante, ct);

            if (!yaAsignado)
            {
                var asignacion = new PlanPagoAsignacion
                {
                    IdPlanPago = idPlanPago,
                    IdEstudiante = idEstudiante,
                    FechaAsignacionUtc = DateTime.UtcNow,
                    Observaciones = "Asignacion automatica al inscribirse"
                };

                await _dbContext.PlanPagoAsignacion.AddAsync(asignacion, ct);
                await _dbContext.SaveChangesAsync(ct);
            }
        }

        private async Task<List<ReciboDto>> GenerarRecibosParaInscripcionAsync(
            int idEstudiante,
            int idPeriodoAcademico,
            int idPlanPago,
            bool esNuevoIngreso,
            byte mesesPorPeriodo,
            DateOnly fechaInicioPeriodo,
            CancellationToken ct)
        {
            var estudiante = await _dbContext.Estudiante
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

            if (estudiante?.IdPlanActual != null)
            {
                var inscripcionActual = await _dbContext.Inscripcion
                    .Include(i => i.IdGrupoMateriaNavigation)
                        .ThenInclude(gm => gm.IdGrupoNavigation)
                    .Where(i => i.IdEstudiante == idEstudiante && i.Status == StatusEnum.Active)
                    .OrderByDescending(i => i.FechaInscripcion)
                    .FirstOrDefaultAsync(ct);

                if (inscripcionActual != null)
                {
                    var numeroCuatrimestre = inscripcionActual.IdGrupoMateriaNavigation.IdGrupoNavigation.NumeroCuatrimestre;

                    var plantilla = await _plantillaCobroService.BuscarPlantillaActivaAsync(
                        estudiante.IdPlanActual.Value,
                        numeroCuatrimestre,
                        idPeriodoAcademico,
                        null,
                        null,
                        ct);

                    if (plantilla != null && plantilla.Detalles != null && plantilla.Detalles.Any())
                    {
                        return await GenerarRecibosDesdeePlantillaAsync(
                            idEstudiante,
                            idPeriodoAcademico,
                            plantilla,
                            fechaInicioPeriodo,
                            ct);
                    }
                }
            }

            return await GenerarRecibosDesdeePlanPagoAsync(
                idEstudiante,
                idPeriodoAcademico,
                idPlanPago,
                esNuevoIngreso,
                fechaInicioPeriodo,
                ct);
        }

        private async Task<List<ReciboDto>> GenerarRecibosDesdeePlantillaAsync(
            int idEstudiante,
            int idPeriodoAcademico,
            PlantillaCobroDto plantilla,
            DateOnly fechaInicioPeriodo,
            CancellationToken ct)
        {
            var recibos = new List<ReciboDto>();
            var detallesOrdenados = plantilla.Detalles!
                .OrderBy(d => d.AplicaEnRecibo ?? 0)
                .ThenBy(d => d.Orden)
                .ToList();

            if (plantilla.EstrategiaEmision == 0)
            {
                var grupos = detallesOrdenados.GroupBy(d => d.AplicaEnRecibo ?? 1);

                foreach (var grupo in grupos)
                {
                    var fechaVencimiento = CalcularFechaVencimiento(
                        fechaInicioPeriodo,
                        (grupo.Key - 1),
                        (byte)plantilla.DiaVencimiento);

                    var recibo = await CrearReciboDesdeDetallesPlantillaAsync(
                        idEstudiante,
                        idPeriodoAcademico,
                        grupo.ToList(),
                        fechaVencimiento,
                        ct);

                    recibos.Add(recibo);
                }
            }
            else
            {
                var fechaVencimiento = CalcularFechaVencimiento(
                    fechaInicioPeriodo,
                    0,
                    (byte)plantilla.DiaVencimiento);

                var recibo = await CrearReciboDesdeDetallesPlantillaAsync(
                    idEstudiante,
                    idPeriodoAcademico,
                    detallesOrdenados,
                    fechaVencimiento,
                    ct);

                recibos.Add(recibo);
            }

            return recibos;
        }

        private async Task<ReciboDto> CrearReciboDesdeDetallesPlantillaAsync(
            int idEstudiante,
            int idPeriodoAcademico,
            List<PlantillaCobroDetalleDto> detalles,
            DateOnly fechaVencimiento,
            CancellationToken ct)
        {
            decimal subtotal = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            decimal descuentoTotal = 0;
            foreach (var detalle in detalles)
            {
                var descuento = await _becaService.CalcularDescuentoPorBecasAsync(
                    idEstudiante,
                    detalle.IdConceptoPago,
                    detalle.Cantidad * detalle.PrecioUnitario,
                    fechaVencimiento,
                    ct);
                descuentoTotal += descuento;
            }

            var recibo = new Recibo
            {
                Folio = await GenerarFolioUnicoAsync(ct),
                IdEstudiante = idEstudiante,
                IdPeriodoAcademico = idPeriodoAcademico,
                FechaEmision = DateOnly.FromDateTime(DateTime.UtcNow),
                FechaVencimiento = fechaVencimiento,
                Estatus = EstatusRecibo.PENDIENTE,
                Subtotal = subtotal,
                Descuento = descuentoTotal,
                Recargos = 0,
                Saldo = subtotal - descuentoTotal,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "sistema",
                Status = StatusEnum.Active
            };

            await _dbContext.Recibo.AddAsync(recibo, ct);
            await _dbContext.SaveChangesAsync(ct);

            var reciboLineas = new List<ReciboLineaDto>();

            foreach (var detalle in detalles)
            {
                var reciboDetalle = new ReciboDetalle
                {
                    IdRecibo = recibo.IdRecibo,
                    IdConceptoPago = detalle.IdConceptoPago,
                    Descripcion = detalle.Descripcion,
                    Cantidad = (int)detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    RefTabla = "PlantillaCobroDetalle",
                    RefId = detalle.IdPlantillaDetalle,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "sistema",
                    Status = StatusEnum.Active
                };

                await _dbContext.ReciboDetalle.AddAsync(reciboDetalle, ct);

                reciboLineas.Add(new ReciboLineaDto
                {
                    Descripcion = detalle.Descripcion,
                    Cantidad = (int)detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Importe = detalle.Cantidad * detalle.PrecioUnitario
                });
            }

            await _dbContext.SaveChangesAsync(ct);

            return new ReciboDto
            {
                IdRecibo = recibo.IdRecibo,
                Folio = recibo.Folio,
                FechaEmision = recibo.FechaEmision,
                FechaVencimiento = recibo.FechaVencimiento,
                estatus = recibo.Estatus,
                Subtotal = recibo.Subtotal,
                Descuento = recibo.Descuento,
                Recargos = recibo.Recargos,
                Total = subtotal - descuentoTotal,
                Saldo = recibo.Saldo,
                Detalles = reciboLineas
            };
        }

        private async Task<List<ReciboDto>> GenerarRecibosDesdeePlanPagoAsync(
            int idEstudiante,
            int idPeriodoAcademico,
            int idPlanPago,
            bool esNuevoIngreso,
            DateOnly fechaInicioPeriodo,
            CancellationToken ct)
        {
            var recibos = new List<ReciboDto>();

            var detallesPlan = await _dbContext.PlanPagoDetalle
                .Include(ppd => ppd.ConceptoPago)
                .Where(ppd => ppd.IdPlanPago == idPlanPago && ppd.Status == StatusEnum.Active)
                .OrderBy(ppd => ppd.Orden)
                .ToListAsync(ct);

            var detallesAGenerar = esNuevoIngreso
                ? detallesPlan
                : detallesPlan.Where(d => !d.EsInscripcion).ToList();

            foreach (var detalle in detallesAGenerar)
            {
                var fechaVencimiento = CalcularFechaVencimiento(fechaInicioPeriodo, detalle.MesOffset, detalle.DiaPago ?? 5);

                var descuentoPorBecas = await _becaService.CalcularDescuentoPorBecasAsync(
                    idEstudiante,
                    detalle.IdConceptoPago,
                    detalle.Importe,
                    fechaVencimiento,
                    ct);

                var saldoFinal = detalle.Importe - descuentoPorBecas;

                var recibo = new Recibo
                {
                    IdEstudiante = idEstudiante,
                    IdPeriodoAcademico = idPeriodoAcademico,
                    FechaEmision = DateOnly.FromDateTime(DateTime.UtcNow),
                    FechaVencimiento = fechaVencimiento,
                    Estatus = EstatusRecibo.PENDIENTE,
                    Subtotal = detalle.Importe,
                    Descuento = descuentoPorBecas,
                    Recargos = 0,
                    Saldo = saldoFinal,
                    Notas = detalle.EsInscripcion ? "Inscripcion" : $"Colegiatura mes {detalle.MesOffset + 1}",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "sistema",
                    Status = StatusEnum.Active
                };

                recibo.Folio = await GenerarFolioUnicoAsync(ct);

                await _dbContext.Recibo.AddAsync(recibo, ct);
                await _dbContext.SaveChangesAsync(ct);

                var reciboDetalle = new ReciboDetalle
                {
                    IdRecibo = recibo.IdRecibo,
                    IdConceptoPago = detalle.IdConceptoPago,
                    Descripcion = detalle.Descripcion,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.Importe,
                    RefTabla = "PlanPagoDetalle",
                    RefId = detalle.IdPlanPagoDetalle,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "sistema",
                    Status = StatusEnum.Active
                };

                await _dbContext.ReciboDetalle.AddAsync(reciboDetalle, ct);
                await _dbContext.SaveChangesAsync(ct);

                recibos.Add(new ReciboDto
                {
                    IdRecibo = recibo.IdRecibo,
                    Folio = recibo.Folio,
                    FechaEmision = recibo.FechaEmision,
                    FechaVencimiento = recibo.FechaVencimiento,
                    estatus = recibo.Estatus,
                    Subtotal = recibo.Subtotal,
                    Descuento = recibo.Descuento,
                    Recargos = recibo.Recargos,
                    Total = recibo.Subtotal - recibo.Descuento + recibo.Recargos,
                    Saldo = recibo.Saldo,
                    Detalles = new List<ReciboLineaDto>
                    {
                        new ReciboLineaDto
                        {
                            Descripcion = reciboDetalle.Descripcion,
                            Cantidad = reciboDetalle.Cantidad,
                            PrecioUnitario = reciboDetalle.PrecioUnitario,
                            Importe = reciboDetalle.Cantidad * reciboDetalle.PrecioUnitario
                        }
                    }
                });
            }

            return recibos;
        }

        private DateOnly CalcularFechaVencimiento(DateOnly fechaInicioPeriodo, int mesOffset, byte diaPago)
        {
            var fecha = fechaInicioPeriodo.AddMonths(mesOffset);

            int diaMaximo = DateTime.DaysInMonth(fecha.Year, fecha.Month);
            int diaFinal = Math.Min(diaPago, diaMaximo);

            return new DateOnly(fecha.Year, fecha.Month, diaFinal);
        }

        private async Task<string> GenerarFolioUnicoAsync(CancellationToken ct)
        {
            var anio = DateTime.UtcNow.Year;
            var ultimoFolio = await _dbContext.Recibo
                .Where(r => r.Folio.StartsWith($"REC-{anio}-"))
                .OrderByDescending(r => r.Folio)
                .Select(r => r.Folio)
                .FirstOrDefaultAsync(ct);

            int secuencial = 1;
            if (!string.IsNullOrEmpty(ultimoFolio))
            {
                var partes = ultimoFolio.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int numero))
                {
                    secuencial = numero + 1;
                }
            }

            return $"REC-{anio}-{secuencial:D6}";
        }

        public async Task<List<Inscripcion>> GetInscripcionesByEstudianteAsync(int idEstudiante)
        {
            return await _dbContext.Inscripcion
                .Include(i => i.IdGrupoMateriaNavigation)
                    .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(i => i.IdGrupoMateriaNavigation.IdGrupoNavigation)
                .Where(i => i.IdEstudiante == idEstudiante && i.Status == StatusEnum.Active)
                .OrderByDescending(i => i.FechaInscripcion)
                .ToListAsync();
        }
    }
}
