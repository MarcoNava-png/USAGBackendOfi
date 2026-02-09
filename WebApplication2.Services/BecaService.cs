using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public class BecaService : IBecaService
    {
        private readonly ApplicationDbContext _dbContext;

        public BecaService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<decimal> CalcularDescuentoPorBecasAsync(
            int idEstudiante,
            int idConceptoPago,
            decimal importeBase,
            DateOnly fechaAplicacion,
            CancellationToken ct = default)
        {
            var becas = await ObtenerBecasActivasAsync(idEstudiante, fechaAplicacion, idConceptoPago, ct);

            if (!becas.Any())
            {
                return 0m;
            }

            decimal descuentoTotal = 0m;

            foreach (var asignacion in becas)
            {
                decimal descuentoBeca = 0m;

                var tipo = asignacion.Beca?.Tipo ?? asignacion.Tipo;
                var valor = asignacion.Beca?.Valor ?? asignacion.Valor;
                var topeMensual = asignacion.Beca?.TopeMensual ?? asignacion.TopeMensual;

                if (tipo == "PORCENTAJE")
                {
                    descuentoBeca = importeBase * (valor / 100m);
                }
                else if (tipo == "MONTO")
                {
                    descuentoBeca = valor;
                }

                if (topeMensual.HasValue && descuentoBeca > topeMensual.Value)
                {
                    descuentoBeca = topeMensual.Value;
                }

                descuentoTotal += descuentoBeca;
            }

            if (descuentoTotal > importeBase)
            {
                descuentoTotal = importeBase;
            }

            return descuentoTotal;
        }

        public async Task<IReadOnlyList<BecaAsignacion>> ObtenerBecasActivasAsync(
            int idEstudiante,
            DateOnly fecha,
            int? idConceptoPago = null,
            CancellationToken ct = default)
        {
            var query = _dbContext.BecaAsignacion
                .Include(b => b.ConceptoPago)
                .Include(b => b.Beca)
                .Where(b => b.IdEstudiante == idEstudiante
                         && b.Activo
                         && b.VigenciaDesde <= fecha
                         && (!b.VigenciaHasta.HasValue || b.VigenciaHasta.Value >= fecha));

            if (idConceptoPago.HasValue)
            {
                query = query.Where(b =>
                    b.IdConceptoPago == idConceptoPago ||
                    b.IdConceptoPago == null ||
                    (b.Beca != null && (b.Beca.IdConceptoPago == idConceptoPago || b.Beca.IdConceptoPago == null)));
            }

            return await query.ToListAsync(ct);
        }

        public async Task<BecaAsignacion> AsignarBecaAsync(
            int idEstudiante,
            int? idConceptoPago,
            string tipo,
            decimal valor,
            DateOnly vigenciaDesde,
            DateOnly? vigenciaHasta,
            decimal? topeMensual,
            string? observaciones,
            CancellationToken ct = default)
        {
            if (tipo != "PORCENTAJE" && tipo != "MONTO")
            {
                throw new ArgumentException("El tipo debe ser 'PORCENTAJE' o 'MONTO'", nameof(tipo));
            }

            if (tipo == "PORCENTAJE" && (valor < 0 || valor > 100))
            {
                throw new ArgumentException("El porcentaje debe estar entre 0 y 100", nameof(valor));
            }

            if (valor < 0)
            {
                throw new ArgumentException("El valor no puede ser negativo", nameof(valor));
            }

            if (vigenciaHasta.HasValue && vigenciaHasta < vigenciaDesde)
            {
                throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio");
            }

            var estudianteExiste = await _dbContext.Estudiante
                .AnyAsync(e => e.IdEstudiante == idEstudiante && e.Status == StatusEnum.Active, ct);

            if (!estudianteExiste)
            {
                throw new InvalidOperationException($"El estudiante con ID {idEstudiante} no existe o no está activo");
            }

            if (idConceptoPago.HasValue)
            {
                var conceptoExiste = await _dbContext.ConceptoPago
                    .AnyAsync(c => c.IdConceptoPago == idConceptoPago.Value && c.Activo, ct);

                if (!conceptoExiste)
                {
                    throw new InvalidOperationException($"El concepto de pago con ID {idConceptoPago} no existe o no está activo");
                }
            }

            var beca = new BecaAsignacion
            {
                IdEstudiante = idEstudiante,
                IdConceptoPago = idConceptoPago,
                Tipo = tipo,
                Valor = valor,
                VigenciaDesde = vigenciaDesde,
                VigenciaHasta = vigenciaHasta,
                TopeMensual = topeMensual,
                Activo = true,
                Observaciones = observaciones
            };

            await _dbContext.BecaAsignacion.AddAsync(beca, ct);
            await _dbContext.SaveChangesAsync(ct);

            await RecalcularDescuentosRecibosAsync(idEstudiante, null, ct);

            return beca;
        }

        public async Task<BecaAsignacion> AsignarBecaDesdeCatalogoAsync(
            int idEstudiante,
            int idBeca,
            DateOnly vigenciaDesde,
            DateOnly? vigenciaHasta,
            string? observaciones,
            int? idPeriodoAcademico = null,
            CancellationToken ct = default)
        {
            var estudianteExiste = await _dbContext.Estudiante
                .AnyAsync(e => e.IdEstudiante == idEstudiante && e.Status == StatusEnum.Active, ct);

            if (!estudianteExiste)
            {
                throw new InvalidOperationException($"El estudiante con ID {idEstudiante} no existe o no está activo");
            }

            var becaCatalogo = await _dbContext.Beca
                .FirstOrDefaultAsync(b => b.IdBeca == idBeca && b.Activo && b.Status == StatusEnum.Active, ct);

            if (becaCatalogo == null)
            {
                throw new InvalidOperationException($"La beca con ID {idBeca} no existe o no está activa");
            }

            DateOnly fechaDesde = vigenciaDesde;
            DateOnly? fechaHasta = vigenciaHasta;

            if (idPeriodoAcademico.HasValue)
            {
                var periodo = await _dbContext.PeriodoAcademico
                    .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == idPeriodoAcademico.Value, ct);

                if (periodo == null)
                {
                    throw new InvalidOperationException($"El período académico con ID {idPeriodoAcademico} no existe");
                }

                fechaDesde = periodo.FechaInicio;
                fechaHasta = periodo.FechaFin;
            }

            if (fechaHasta.HasValue && fechaHasta < fechaDesde)
            {
                throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio");
            }

            var asignacion = new BecaAsignacion
            {
                IdEstudiante = idEstudiante,
                IdBeca = idBeca,
                IdPeriodoAcademico = idPeriodoAcademico,
                IdConceptoPago = becaCatalogo.IdConceptoPago,
                Tipo = becaCatalogo.Tipo,
                Valor = becaCatalogo.Valor,
                TopeMensual = becaCatalogo.TopeMensual,
                VigenciaDesde = fechaDesde,
                VigenciaHasta = fechaHasta,
                Activo = true,
                Observaciones = observaciones
            };

            await _dbContext.BecaAsignacion.AddAsync(asignacion, ct);
            await _dbContext.SaveChangesAsync(ct);

            await RecalcularDescuentosRecibosAsync(idEstudiante, idPeriodoAcademico, ct);

            await _dbContext.Entry(asignacion).Reference(a => a.Beca).LoadAsync(ct);
            if (idPeriodoAcademico.HasValue)
            {
                await _dbContext.Entry(asignacion).Reference(a => a.PeriodoAcademico).LoadAsync(ct);
            }

            return asignacion;
        }

        public async Task<BecaAsignacion?> ActualizarBecaAsignacionAsync(
            long idBecaAsignacion,
            DateOnly? vigenciaDesde,
            DateOnly? vigenciaHasta,
            string? observaciones,
            bool? activo,
            int? idPeriodoAcademico,
            CancellationToken ct = default)
        {
            var asignacion = await _dbContext.BecaAsignacion
                .Include(b => b.Beca)
                .Include(b => b.PeriodoAcademico)
                .Include(b => b.ConceptoPago)
                .FirstOrDefaultAsync(b => b.IdBecaAsignacion == idBecaAsignacion, ct);

            if (asignacion == null)
            {
                return null;
            }

            var idEstudianteOriginal = asignacion.IdEstudiante;

            if (idPeriodoAcademico.HasValue)
            {
                var periodo = await _dbContext.PeriodoAcademico
                    .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == idPeriodoAcademico.Value, ct);

                if (periodo == null)
                {
                    throw new InvalidOperationException($"El período académico con ID {idPeriodoAcademico} no existe");
                }

                asignacion.IdPeriodoAcademico = idPeriodoAcademico;
                asignacion.VigenciaDesde = periodo.FechaInicio;
                asignacion.VigenciaHasta = periodo.FechaFin;
            }
            else
            {
                if (vigenciaDesde.HasValue)
                {
                    asignacion.VigenciaDesde = vigenciaDesde.Value;
                }

                if (vigenciaHasta.HasValue)
                {
                    asignacion.VigenciaHasta = vigenciaHasta.Value;
                }

                if (idPeriodoAcademico == null && asignacion.IdPeriodoAcademico.HasValue)
                {
                    asignacion.IdPeriodoAcademico = null;
                }
            }

            if (asignacion.VigenciaHasta.HasValue && asignacion.VigenciaHasta < asignacion.VigenciaDesde)
            {
                throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio");
            }

            if (observaciones != null)
            {
                asignacion.Observaciones = observaciones;
            }

            if (activo.HasValue)
            {
                asignacion.Activo = activo.Value;
            }

            await _dbContext.SaveChangesAsync(ct);

            await RecalcularDescuentosRecibosAsync(idEstudianteOriginal, asignacion.IdPeriodoAcademico, ct);

            return asignacion;
        }

        public async Task<BecaAsignacion?> ObtenerBecaPorIdAsync(long idBecaAsignacion, CancellationToken ct = default)
        {
            return await _dbContext.BecaAsignacion
                .Include(b => b.ConceptoPago)
                .Include(b => b.Beca)
                .Include(b => b.Estudiante)
                .Include(b => b.PeriodoAcademico)
                .FirstOrDefaultAsync(b => b.IdBecaAsignacion == idBecaAsignacion, ct);
        }

        public async Task<bool> CancelarBecaAsync(long idBecaAsignacion, CancellationToken ct = default)
        {
            var beca = await _dbContext.BecaAsignacion
                .FirstOrDefaultAsync(b => b.IdBecaAsignacion == idBecaAsignacion, ct);

            if (beca == null)
            {
                return false;
            }

            beca.Activo = false;
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }

        public async Task<IReadOnlyList<BecaAsignacion>> ObtenerBecasEstudianteAsync(
            int idEstudiante,
            bool? soloActivas = null,
            CancellationToken ct = default)
        {
            var query = _dbContext.BecaAsignacion
                .Include(b => b.ConceptoPago)
                .Include(b => b.Beca)
                .Include(b => b.PeriodoAcademico)
                .Where(b => b.IdEstudiante == idEstudiante);

            if (soloActivas.HasValue)
            {
                query = query.Where(b => b.Activo == soloActivas.Value);
            }

            return await query.OrderByDescending(b => b.VigenciaDesde).ToListAsync(ct);
        }

        public async Task<int> RecalcularDescuentosRecibosAsync(
            int idEstudiante,
            int? idPeriodoAcademico = null,
            CancellationToken ct = default)
        {
            var query = _dbContext.Recibo
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.ConceptoPago)
                .Where(r => r.IdEstudiante == idEstudiante
                         && r.Estatus == EstatusRecibo.PENDIENTE
                         && r.Status == StatusEnum.Active);

            if (idPeriodoAcademico.HasValue)
            {
                query = query.Where(r => r.IdPeriodoAcademico == idPeriodoAcademico.Value);
            }

            var recibos = await query.ToListAsync(ct);
            int recibosActualizados = 0;

            foreach (var recibo in recibos)
            {
                decimal nuevoDescuento = 0m;

                foreach (var detalle in recibo.Detalles.Where(d => d.Status == StatusEnum.Active))
                {
                    var descuentoLinea = await CalcularDescuentoPorBecasAsync(
                        idEstudiante,
                        detalle.IdConceptoPago,
                        detalle.PrecioUnitario * detalle.Cantidad,
                        recibo.FechaVencimiento,
                        ct);

                    nuevoDescuento += descuentoLinea;
                }

                if (recibo.Descuento != nuevoDescuento)
                {
                    recibo.Descuento = nuevoDescuento;
                    recibo.Saldo = recibo.Subtotal - recibo.Descuento + recibo.Recargos;
                    recibosActualizados++;
                }
            }

            if (recibosActualizados > 0)
            {
                await _dbContext.SaveChangesAsync(ct);
            }

            return recibosActualizados;
        }
    }
}
