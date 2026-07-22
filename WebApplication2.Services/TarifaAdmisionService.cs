using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.TarifaAdmision;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class TarifaAdmisionService : ITarifaAdmisionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IReciboService _reciboService;
        private readonly IConvenioService _convenioService;
        private readonly IPlantillaCobroService _plantillaCobroService;
        private readonly IInstitucionProvider _institucionProvider;

        public TarifaAdmisionService(
            ApplicationDbContext db,
            IReciboService reciboService,
            IConvenioService convenioService,
            IPlantillaCobroService plantillaCobroService,
            IInstitucionProvider institucionProvider)
        {
            _db = db;
            _reciboService = reciboService;
            _convenioService = convenioService;
            _plantillaCobroService = plantillaCobroService;
            _institucionProvider = institucionProvider;
        }

        public async Task<IReadOnlyList<TarifaAdmisionDto>> ListarTarifasAsync(bool? soloActivas = null, bool? esConvenioEmpresarial = null, CancellationToken ct = default)
        {
            var query = _db.TarifasAdmision
                .Include(t => t.IdPlanEstudiosNavigation)
                    .ThenInclude(p => p.IdCampusNavigation)
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .Where(t => t.Status != StatusEnum.Deleted);

            if (soloActivas.HasValue)
                query = query.Where(t => t.Activo == soloActivas.Value);

            if (esConvenioEmpresarial.HasValue)
                query = query.Where(t => t.EsConvenioEmpresarial == esConvenioEmpresarial.Value);

            var lista = await query.OrderBy(t => t.IdPlanEstudiosNavigation.NombrePlanEstudios).ToListAsync(ct);
            return lista.Select(MapToDto).ToList();
        }

        public async Task<TarifaAdmisionDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            var tarifa = await _db.TarifasAdmision
                .Include(t => t.IdPlanEstudiosNavigation)
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .FirstOrDefaultAsync(t => t.IdTarifaAdmision == id && t.Status != StatusEnum.Deleted, ct);

            return tarifa == null ? null : MapToDto(tarifa);
        }

        public async Task<TarifaAdmisionDto?> ObtenerPorPlanAsync(int idPlanEstudios, CancellationToken ct = default)
        {
            var tarifa = await _db.TarifasAdmision
                .Include(t => t.IdPlanEstudiosNavigation)
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .FirstOrDefaultAsync(t => t.IdPlanEstudios == idPlanEstudios && t.Activo && t.Status != StatusEnum.Deleted, ct);

            return tarifa == null ? null : MapToDto(tarifa);
        }

        public async Task<TarifaAdmisionDto> CrearTarifaAsync(CrearTarifaAdmisionDto dto, string usuarioCreador, CancellationToken ct = default)
        {
            var tarifa = new TarifaAdmision
            {
                IdPlanEstudios = dto.IdPlanEstudios,
                Nombre = dto.Nombre,
                AplicaConvenioMensualidad = dto.AplicaConvenioMensualidad,
                EsConvenioEmpresarial = dto.EsConvenioEmpresarial,
                Activo = dto.Activo,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuarioCreador,
                Status = StatusEnum.Active,
                Detalles = dto.Detalles.Select((d, i) => new TarifaAdmisionDetalle
                {
                    IdConceptoPago = d.IdConceptoPago,
                    Monto = d.Monto,
                    EsAplicable = d.EsAplicable,
                    Notas = d.Notas,
                    Orden = d.Orden > 0 ? d.Orden : i + 1,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = usuarioCreador,
                    Status = StatusEnum.Active
                }).ToList()
            };

            _db.TarifasAdmision.Add(tarifa);
            await _db.SaveChangesAsync(ct);

            return (await ObtenerPorIdAsync(tarifa.IdTarifaAdmision, ct))!;
        }

        public async Task<TarifaAdmisionDto> ActualizarTarifaAsync(int id, ActualizarTarifaAdmisionDto dto, string usuarioModificador, CancellationToken ct = default)
        {
            var tarifa = await _db.TarifasAdmision
                .Include(t => t.Detalles)
                .FirstOrDefaultAsync(t => t.IdTarifaAdmision == id && t.Status != StatusEnum.Deleted, ct)
                ?? throw new InvalidOperationException($"No se encontró la tarifa con ID {id}");

            if (dto.IdPlanEstudios.HasValue && dto.IdPlanEstudios.Value != tarifa.IdPlanEstudios)
            {
                var planExiste = await _db.PlanEstudios
                    .AnyAsync(p => p.IdPlanEstudios == dto.IdPlanEstudios.Value, ct);
                if (!planExiste)
                    throw new InvalidOperationException($"No se encontró el plan de estudios con ID {dto.IdPlanEstudios.Value}");

                tarifa.IdPlanEstudios = dto.IdPlanEstudios.Value;
            }

            tarifa.Nombre = dto.Nombre;
            tarifa.AplicaConvenioMensualidad = dto.AplicaConvenioMensualidad;
            tarifa.EsConvenioEmpresarial = dto.EsConvenioEmpresarial;
            tarifa.Activo = dto.Activo;
            tarifa.UpdatedAt = DateTime.UtcNow;
            tarifa.UpdatedBy = usuarioModificador;

            _db.TarifasAdmisionDetalles.RemoveRange(tarifa.Detalles);

            tarifa.Detalles = dto.Detalles.Select((d, i) => new TarifaAdmisionDetalle
            {
                IdTarifaAdmision = id,
                IdConceptoPago = d.IdConceptoPago,
                Monto = d.Monto,
                EsAplicable = d.EsAplicable,
                Notas = d.Notas,
                Orden = d.Orden > 0 ? d.Orden : i + 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuarioModificador,
                Status = StatusEnum.Active
            }).ToList();

            await _db.SaveChangesAsync(ct);
            return (await ObtenerPorIdAsync(id, ct))!;
        }

        public async Task<bool> EliminarTarifaAsync(int id, CancellationToken ct = default)
        {
            var tarifa = await _db.TarifasAdmision
                .FirstOrDefaultAsync(t => t.IdTarifaAdmision == id && t.Status != StatusEnum.Deleted, ct);

            if (tarifa == null) return false;

            tarifa.Status = StatusEnum.Deleted;
            tarifa.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default)
        {
            var tarifa = await _db.TarifasAdmision
                .FirstOrDefaultAsync(t => t.IdTarifaAdmision == id && t.Status != StatusEnum.Deleted, ct);

            if (tarifa == null) return false;

            tarifa.Activo = activo;
            tarifa.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<GenerarRecibosAdmisionResultDto> GenerarRecibosAsync(
            int idAspirante, int idTarifaAdmision, bool pagoCompleto,
            List<int>? conceptosIncluidos = null, decimal descuentoPorcentaje = 0,
            CancellationToken ct = default)
        {
            var tarifa = await _db.TarifasAdmision
                .Include(t => t.Detalles.Where(d => d.EsAplicable && d.Status != StatusEnum.Deleted))
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .FirstOrDefaultAsync(t => t.IdTarifaAdmision == idTarifaAdmision && t.Status != StatusEnum.Deleted, ct)
                ?? throw new InvalidOperationException($"No se encontró la tarifa con ID {idTarifaAdmision}");

            var aspirante = await _db.Aspirante
                .FirstOrDefaultAsync(a => a.IdAspirante == idAspirante, ct)
                ?? throw new InvalidOperationException($"No se encontró el aspirante con ID {idAspirante}");

            // Validar porcentaje de descuento
            if (descuentoPorcentaje < 0) descuentoPorcentaje = 0;
            if (descuentoPorcentaje > 100) descuentoPorcentaje = 100;

            var resultado = new GenerarRecibosAdmisionResultDto();

            var conceptosYaConRecibo = await _db.Recibo
                .Where(r => r.IdAspirante == idAspirante && r.Estatus != Core.Enums.EstatusRecibo.CANCELADO)
                .SelectMany(r => r.Detalles.Select(d => d.IdConceptoPago))
                .Distinct()
                .ToListAsync(ct);
            var conceptosExistentes = new HashSet<int>(conceptosYaConRecibo);

            foreach (var detalle in tarifa.Detalles.OrderBy(d => d.Orden))
            {
                var esMensualidad = detalle.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.COLEGIATURA;

                if (esMensualidad && pagoCompleto)
                    continue;

                if (conceptosIncluidos != null && !conceptosIncluidos.Contains(detalle.IdConceptoPago))
                    continue;

                if (conceptosExistentes.Contains(detalle.IdConceptoPago))
                {
                    resultado.Advertencias ??= new List<string>();
                    resultado.Advertencias.Add($"{detalle.IdConceptoPagoNavigation?.Nombre ?? "Concepto"} ya tiene recibo generado, se omitió.");
                    continue;
                }

                var monto = detalle.Monto;

                // Aplicar descuento por porcentaje
                var descuentoManual = descuentoPorcentaje > 0
                    ? Math.Round(monto * (descuentoPorcentaje / 100m), 2)
                    : 0m;

                // Calcular descuento de convenio
                var descuentoConvenio = 0m;
                if (tarifa.AplicaConvenioMensualidad && esMensualidad)
                {
                    descuentoConvenio = await _convenioService.CalcularDescuentoTotalAspiranteAsync(
                        idAspirante, monto, "COLEGIATURA", ct);
                }

                var descuentoTotal = descuentoManual + descuentoConvenio;
                // No superar el monto original
                if (descuentoTotal > monto) descuentoTotal = monto;

                var recibo = await _reciboService.GenerarReciboAspiranteConConceptoYMontoAsync(
                    idAspirante, detalle.IdConceptoPago, monto, descuentoTotal, 7, ct);

                resultado.RecibosAdmision.Add(recibo);
            }

            if (pagoCompleto)
            {
                var cuatrimestre = aspirante.CuatrimestreInteres ?? 1;
                var plantilla = await _plantillaCobroService.BuscarPlantillaActivaAsync(
                    aspirante.IdPlan,
                    cuatrimestre,
                    aspirante.IdPeriodoAcademico,
                    aspirante.TurnoId,
                    aspirante.IdModalidad,
                    ct);

                if (plantilla != null && plantilla.Detalles != null)
                {
                    decimal descuentoMensualidad = 0m;
                    if (tarifa.AplicaConvenioMensualidad)
                    {
                        var primerDetalleMensualidad = plantilla.Detalles.FirstOrDefault();
                        if (primerDetalleMensualidad != null)
                        {
                            var montoRef = primerDetalleMensualidad.PrecioUnitario * primerDetalleMensualidad.Cantidad;
                            descuentoMensualidad = await _convenioService.CalcularDescuentoTotalAspiranteAsync(
                                idAspirante, montoRef, "COLEGIATURA", ct);
                        }
                    }

                    foreach (var detalle in plantilla.Detalles.OrderBy(d => d.Orden))
                    {
                        var monto = detalle.PrecioUnitario * detalle.Cantidad;
                        var montoFinal = Math.Max(0, monto - descuentoMensualidad);
                        var descripcion = detalle.Descripcion ?? detalle.NombreConcepto ?? "Mensualidad";

                        var recibo = await _reciboService.GenerarReciboAspiranteAsync(
                            idAspirante, montoFinal, descripcion, plantilla.DiaVencimiento, ct);

                        resultado.RecibosMensualidades.Add(recibo);
                    }
                }
            }

            return resultado;
        }

        public async Task<GenerarRecibosAdmisionResultDto> GenerarRecibosV2Async(
            int idAspirante, int idTarifaAdmision, GenerarRecibosAdmisionRequestV2Dto request,
            CancellationToken ct = default)
        {
            var tarifa = await _db.TarifasAdmision
                .Include(t => t.Detalles.Where(d => d.EsAplicable && d.Status != StatusEnum.Deleted))
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .FirstOrDefaultAsync(t => t.IdTarifaAdmision == idTarifaAdmision && t.Status != StatusEnum.Deleted, ct)
                ?? throw new InvalidOperationException($"No se encontró la tarifa con ID {idTarifaAdmision}");

            var aspirante = await _db.Aspirante
                .FirstOrDefaultAsync(a => a.IdAspirante == idAspirante, ct)
                ?? throw new InvalidOperationException($"No se encontró el aspirante con ID {idAspirante}");

            var resultado = new GenerarRecibosAdmisionResultDto();

            var conceptosYaConRecibo = await _db.Recibo
                .Where(r => r.IdAspirante == idAspirante && r.Estatus != Core.Enums.EstatusRecibo.CANCELADO)
                .SelectMany(r => r.Detalles.Select(d => d.IdConceptoPago))
                .Distinct()
                .ToListAsync(ct);
            var conceptosExistentes = new HashSet<int>(conceptosYaConRecibo);

            var conceptoPromocionMap = request.Conceptos.ToDictionary(c => c.IdConceptoPago, c => c.IdPromocion);

            foreach (var detalle in tarifa.Detalles.OrderBy(d => d.Orden))
            {
                var esMensualidad = detalle.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.COLEGIATURA;

                if (esMensualidad && request.PagoCompleto)
                    continue;

                if (!conceptoPromocionMap.ContainsKey(detalle.IdConceptoPago))
                    continue;

                if (conceptosExistentes.Contains(detalle.IdConceptoPago))
                {
                    resultado.Advertencias ??= new List<string>();
                    resultado.Advertencias.Add($"{detalle.IdConceptoPagoNavigation?.Nombre ?? "Concepto"} ya tiene recibo generado, se omitió.");
                    continue;
                }

                var monto = detalle.Monto;
                var idPromocion = conceptoPromocionMap[detalle.IdConceptoPago];
                var descuentoTotal = 0m;
                string? nombrePromocion = null;

                if (idPromocion.HasValue)
                {
                    var convenio = await _db.Convenio
                        .FirstOrDefaultAsync(c => c.IdConvenio == idPromocion.Value && c.Status != StatusEnum.Deleted, ct);

                    if (convenio != null)
                    {
                        nombrePromocion = convenio.Nombre;
                        descuentoTotal = convenio.TipoBeneficio.ToUpperInvariant() switch
                        {
                            "PORCENTAJE" => convenio.DescuentoPct.HasValue
                                ? Math.Round(monto * (convenio.DescuentoPct.Value / 100m), 2) : 0m,
                            "MONTO" => convenio.Monto ?? 0m,
                            "EXENCION" => monto,
                            _ => 0m
                        };

                        if (descuentoTotal > monto) descuentoTotal = monto;
                    }
                }

                var recibo = await _reciboService.GenerarReciboAspiranteConConceptoYMontoAsync(
                    idAspirante, detalle.IdConceptoPago, monto, descuentoTotal, 7, nombrePromocion, ct);

                if (request.IdEmpresa.HasValue)
                {
                    var reciboEntity = await _db.Recibo
                        .FirstOrDefaultAsync(r => r.IdRecibo == recibo.IdRecibo, ct);
                    if (reciboEntity != null)
                    {
                        reciboEntity.IdEmpresa = request.IdEmpresa;
                    }
                }

                resultado.RecibosAdmision.Add(recibo);
            }

            if (request.IdEmpresa.HasValue && !aspirante.IdEmpresa.HasValue)
            {
                aspirante.IdEmpresa = request.IdEmpresa;
            }

            if (tarifa.EsConvenioEmpresarial)
            {
                aspirante.IdEmpresa ??= request.IdEmpresa;
            }

            await _db.SaveChangesAsync(ct);

            if (request.PagoCompleto)
            {
                var cuatrimestre = aspirante.CuatrimestreInteres ?? 1;
                var plantilla = await _plantillaCobroService.BuscarPlantillaActivaAsync(
                    aspirante.IdPlan, cuatrimestre,
                    aspirante.IdPeriodoAcademico, aspirante.TurnoId, aspirante.IdModalidad, ct);

                if (plantilla?.Detalles != null)
                {
                    foreach (var detalle in plantilla.Detalles.OrderBy(d => d.Orden))
                    {
                        var monto = detalle.PrecioUnitario * detalle.Cantidad;
                        var descripcion = detalle.Descripcion ?? detalle.NombreConcepto ?? "Mensualidad";

                        var recibo = await _reciboService.GenerarReciboAspiranteAsync(
                            idAspirante, monto, descripcion, plantilla.DiaVencimiento, ct);

                        resultado.RecibosMensualidades.Add(recibo);
                    }
                }
            }

            return resultado;
        }

        public async Task<CotizacionAdmisionPdfDto> GenerarCotizacionPdfDtoAsync(
            int idTarifaAdmision, int idAspirante, CancellationToken ct = default)
        {
            var tarifa = await _db.TarifasAdmision
                .Include(t => t.IdPlanEstudiosNavigation)
                .Include(t => t.Detalles.Where(d => d.Status != StatusEnum.Deleted))
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .FirstOrDefaultAsync(t => t.IdTarifaAdmision == idTarifaAdmision && t.Status != StatusEnum.Deleted, ct)
                ?? throw new InvalidOperationException($"No se encontró la tarifa con ID {idTarifaAdmision}");

            var aspirante = await _db.Aspirante
                .Include(a => a.IdPersonaNavigation)
                .Include(a => a.IdPlanNavigation)
                .FirstOrDefaultAsync(a => a.IdAspirante == idAspirante, ct)
                ?? throw new InvalidOperationException($"No se encontró el aspirante con ID {idAspirante}");

            var recibosAspirante = await _db.Recibo
                .Include(r => r.Detalles)
                .Where(r => r.IdAspirante == idAspirante && r.Estatus != Core.Enums.EstatusRecibo.CANCELADO && r.Status != StatusEnum.Deleted)
                .ToListAsync(ct);

            var persona = aspirante.IdPersonaNavigation;
            var nombreAspirante = persona != null
                ? $"{persona.ApellidoPaterno} {persona.ApellidoMaterno} {persona.Nombre}".Trim()
                : "N/A";

            // Normaliza texto eliminando tildes y mayúsculas para comparación
            static string Norm(string s)
            {
                var r = new System.Text.StringBuilder();
                foreach (var c in s.ToUpperInvariant().Normalize(System.Text.NormalizationForm.FormD))
                    if (c < 128) r.Append(c);
                return r.ToString();
            }

            string ObtenerValor(Func<TarifaAdmisionDetalle, bool> predicado, string valorDefecto = "N/A")
            {
                var detalle = tarifa.Detalles.FirstOrDefault(predicado);
                if (detalle == null) return valorDefecto;
                if (!detalle.EsAplicable) return valorDefecto;
                return $"${detalle.Monto:N2}";
            }

            bool Contiene(TarifaAdmisionDetalle d, string keyword)
            {
                var texto = Norm((d.IdConceptoPagoNavigation?.Nombre ?? "") + " " + (d.IdConceptoPagoNavigation?.Clave ?? ""));
                return texto.Contains(keyword);
            }

            string ObtenerNotas(Func<TarifaAdmisionDetalle, bool> predicado)
            {
                var detalle = tarifa.Detalles.FirstOrDefault(predicado);
                if (detalle == null || !detalle.EsAplicable) return "";

                var recibo = recibosAspirante.FirstOrDefault(r =>
                    r.Detalles.Any(d => d.IdConceptoPago == detalle.IdConceptoPago));

                if (recibo == null) return "Pendiente";

                if (recibo.Descuento > 0)
                {
                    var pct = recibo.Subtotal > 0 ? (recibo.Descuento / recibo.Subtotal) * 100m : 0;
                    var pctTxt = pct == 100 ? "100%" : $"{pct:F0}%";
                    var estado = recibo.Estatus switch
                    {
                        Core.Enums.EstatusRecibo.PAGADO => "Pagado",
                        Core.Enums.EstatusRecibo.VENCIDO => "Vencido",
                        _ => "Pendiente"
                    };
                    return $"{estado} · Descuento {pctTxt} - ${recibo.Descuento:N2}";
                }

                return recibo.Estatus switch
                {
                    Core.Enums.EstatusRecibo.PAGADO => "Pagado",
                    Core.Enums.EstatusRecibo.VENCIDO => "Vencido",
                    _ => "Pendiente"
                };
            }

            bool predFicha(TarifaAdmisionDetalle d) => Contiene(d, "FICHA");
            bool predInsc(TarifaAdmisionDetalle d) => d.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.INSCRIPCION && !Contiene(d, "REINSC");
            bool predReinsc(TarifaAdmisionDetalle d) => Contiene(d, "REINSC");
            bool predExamen(TarifaAdmisionDetalle d) => d.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.EXAMEN;
            bool predProp(TarifaAdmisionDetalle d) => Contiene(d, "PROP");
            bool predColeg(TarifaAdmisionDetalle d) => d.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.COLEGIATURA;
            bool predSeguro(TarifaAdmisionDetalle d) => d.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.SEGURO;
            bool predCred(TarifaAdmisionDetalle d) => d.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.CREDENCIAL;

            var conceptos = new List<CotizacionConceptoDto>
            {
                new() { Nombre = "FICHA DE ADMISIÓN", Valor = ObtenerValor(predFicha), Notas = ObtenerNotas(predFicha) },
                new() { Nombre = "INSCRIPCIÓN", Valor = ObtenerValor(predInsc), Notas = ObtenerNotas(predInsc) },
                new() { Nombre = "REINSCRIPCIÓN", Valor = ObtenerValor(predReinsc), Notas = ObtenerNotas(predReinsc) },
                new() { Nombre = "EXAMEN DE ADMISIÓN", Valor = ObtenerValor(predExamen), Notas = ObtenerNotas(predExamen) },
                new() { Nombre = "PROPEDÉUTICO", Valor = ObtenerValor(predProp), Notas = ObtenerNotas(predProp) },
                new() { Nombre = "MENSUALIDADES", Valor = ObtenerValor(predColeg), Notas = ObtenerNotas(predColeg) },
                new() { Nombre = "SEGURO ESTUDIANTIL", Valor = ObtenerValor(predSeguro), Notas = ObtenerNotas(predSeguro) },
                new() { Nombre = "CREDENCIAL ESTUDIANTIL", Valor = ObtenerValor(predCred, "No"), Notas = ObtenerNotas(predCred) },
                new() { Nombre = "PROMOCIÓN", Valor = tarifa.AplicaConvenioMensualidad ? "SÍ" : "NO" },
            };

            return new CotizacionAdmisionPdfDto
            {
                NombreAspirante = nombreAspirante,
                Licenciatura    = tarifa.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "N/A",
                ClavePlan       = tarifa.IdPlanEstudiosNavigation?.ClavePlanEstudios ?? "",
                NombreTarifa    = tarifa.Nombre,
                Fecha           = DateOnly.FromDateTime(DateTime.UtcNow),
                Conceptos       = conceptos,
                Institucion     = _institucionProvider.ObtenerPdf()
            };
        }

        public async Task<CotizacionAdmisionPdfDto> GenerarCotizacionPdfDtoV2Async(
            int idTarifaAdmision, int idAspirante, CotizacionAdmisionRequestDto request, CancellationToken ct = default)
        {
            var tarifa = await _db.TarifasAdmision
                .Include(t => t.IdPlanEstudiosNavigation)
                .Include(t => t.Detalles.Where(d => d.Status != StatusEnum.Deleted))
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .FirstOrDefaultAsync(t => t.IdTarifaAdmision == idTarifaAdmision && t.Status != StatusEnum.Deleted, ct)
                ?? throw new InvalidOperationException($"No se encontró la tarifa con ID {idTarifaAdmision}");

            var aspirante = await _db.Aspirante
                .Include(a => a.IdPersonaNavigation)
                .Include(a => a.IdPlanNavigation)
                .FirstOrDefaultAsync(a => a.IdAspirante == idAspirante, ct)
                ?? throw new InvalidOperationException($"No se encontró el aspirante con ID {idAspirante}");

            var persona = aspirante.IdPersonaNavigation;
            var nombreAspirante = persona != null
                ? $"{persona.ApellidoPaterno} {persona.ApellidoMaterno} {persona.Nombre}".Trim()
                : "N/A";

            // Cargar promociones referenciadas
            var idsPromocion = request.Conceptos
                .Where(c => c.IdPromocion.HasValue)
                .Select(c => c.IdPromocion!.Value)
                .Distinct()
                .ToList();

            var conveniosDict = new Dictionary<int, Core.Models.Convenio>();
            if (idsPromocion.Count > 0)
            {
                var convenios = await _db.Set<Core.Models.Convenio>()
                    .Where(c => idsPromocion.Contains(c.IdConvenio))
                    .ToListAsync(ct);
                conveniosDict = convenios.ToDictionary(c => c.IdConvenio);
            }

            // Nombre de empresa
            string? nombreEmpresa = null;
            if (request.IdEmpresa.HasValue)
            {
                var empresa = await _db.Empresas.FindAsync(new object[] { request.IdEmpresa.Value }, ct);
                nombreEmpresa = empresa?.Nombre;
            }

            // Crear lookup de promociones por concepto
            var promoPorConcepto = request.Conceptos.ToDictionary(c => c.IdConceptoPago, c => c.IdPromocion);
            var conceptosIncluidos = new HashSet<int>(request.Conceptos.Select(c => c.IdConceptoPago));

            var conceptos = new List<CotizacionConceptoDto>();
            decimal totalOriginal = 0;
            decimal totalDescuento = 0;

            foreach (var detalle in tarifa.Detalles.OrderBy(d => d.Orden))
            {
                var nombreConcepto = detalle.IdConceptoPagoNavigation?.Nombre ?? "Concepto";
                var incluido = conceptosIncluidos.Contains(detalle.IdConceptoPago) && detalle.EsAplicable;
                var monto = detalle.Monto;
                decimal descuento = 0;
                string? nombrePromo = null;

                if (incluido && promoPorConcepto.TryGetValue(detalle.IdConceptoPago, out var idPromo) && idPromo.HasValue)
                {
                    if (conveniosDict.TryGetValue(idPromo.Value, out var convenio))
                    {
                        nombrePromo = $"{convenio.ClaveConvenio} — {convenio.Nombre}";
                        switch (convenio.TipoBeneficio.ToUpper())
                        {
                            case "PORCENTAJE":
                                descuento = Math.Round(monto * (convenio.DescuentoPct ?? 0) / 100m, 2);
                                nombrePromo += $" ({convenio.DescuentoPct}%)";
                                break;
                            case "MONTO":
                                descuento = Math.Min(convenio.Monto ?? 0, monto);
                                nombrePromo += $" (-${convenio.Monto:N2})";
                                break;
                            case "EXENCION":
                                descuento = monto;
                                nombrePromo += " (Exención)";
                                break;
                        }
                    }
                }

                var montoFinal = monto - descuento;

                conceptos.Add(new CotizacionConceptoDto
                {
                    Nombre = nombreConcepto,
                    Valor = incluido ? $"${montoFinal:N2}" : "N/A",
                    Monto = monto,
                    NombrePromocion = nombrePromo,
                    MontoDescuento = descuento,
                    MontoFinal = montoFinal,
                    Incluido = incluido,
                });

                if (incluido)
                {
                    totalOriginal += monto;
                    totalDescuento += descuento;
                }
            }

            return new CotizacionAdmisionPdfDto
            {
                NombreAspirante = nombreAspirante,
                Licenciatura    = tarifa.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "N/A",
                ClavePlan       = tarifa.IdPlanEstudiosNavigation?.ClavePlanEstudios ?? "",
                NombreTarifa    = tarifa.Nombre,
                Fecha           = DateOnly.FromDateTime(DateTime.UtcNow),
                Conceptos       = conceptos,
                Institucion     = _institucionProvider.ObtenerPdf(),
                TotalOriginal   = totalOriginal,
                TotalDescuento  = totalDescuento,
                TotalFinal      = totalOriginal - totalDescuento,
                NombreEmpresa   = nombreEmpresa,
            };
        }

        private static TarifaAdmisionDto MapToDto(TarifaAdmision t) => new()
        {
            IdTarifaAdmision = t.IdTarifaAdmision,
            IdPlanEstudios = t.IdPlanEstudios,
            NombrePlanEstudios = t.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "",
            ClavePlanEstudios = t.IdPlanEstudiosNavigation?.ClavePlanEstudios ?? "",
            NombreCampus = t.IdPlanEstudiosNavigation?.IdCampusNavigation?.Nombre,
            Nombre = t.Nombre,
            AplicaConvenioMensualidad = t.AplicaConvenioMensualidad,
            EsConvenioEmpresarial = t.EsConvenioEmpresarial,
            Activo = t.Activo,
            Detalles = t.Detalles
                .OrderBy(d => d.Orden)
                .Select(d => new TarifaAdmisionDetalleDto
                {
                    IdTarifaAdmisionDetalle = d.IdTarifaAdmisionDetalle,
                    IdConceptoPago = d.IdConceptoPago,
                    ClaveConcepto = d.IdConceptoPagoNavigation?.Clave ?? "",
                    NombreConcepto = d.IdConceptoPagoNavigation?.Nombre ?? "",
                    TipoConcepto = d.IdConceptoPagoNavigation?.Tipo.ToString() ?? "",
                    Monto = d.Monto,
                    EsAplicable = d.EsAplicable,
                    Notas = d.Notas,
                    Orden = d.Orden
                }).ToList()
        };
    }
}
