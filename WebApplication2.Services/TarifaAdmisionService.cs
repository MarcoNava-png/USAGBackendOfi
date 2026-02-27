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

        public TarifaAdmisionService(
            ApplicationDbContext db,
            IReciboService reciboService,
            IConvenioService convenioService,
            IPlantillaCobroService plantillaCobroService)
        {
            _db = db;
            _reciboService = reciboService;
            _convenioService = convenioService;
            _plantillaCobroService = plantillaCobroService;
        }

        public async Task<IReadOnlyList<TarifaAdmisionDto>> ListarTarifasAsync(bool? soloActivas = null, CancellationToken ct = default)
        {
            var query = _db.TarifasAdmision
                .Include(t => t.IdPlanEstudiosNavigation)
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .Where(t => t.Status != StatusEnum.Deleted);

            if (soloActivas.HasValue)
                query = query.Where(t => t.Activo == soloActivas.Value);

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

            tarifa.Nombre = dto.Nombre;
            tarifa.AplicaConvenioMensualidad = dto.AplicaConvenioMensualidad;
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
            int idAspirante, int idTarifaAdmision, bool pagoCompleto, CancellationToken ct = default)
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

            foreach (var detalle in tarifa.Detalles.OrderBy(d => d.Orden))
            {
                var esMensualidad = detalle.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.COLEGIATURA;

                if (esMensualidad && pagoCompleto)
                    continue;

                var descuento = 0m;
                if (tarifa.AplicaConvenioMensualidad && esMensualidad)
                {
                    descuento = await _convenioService.CalcularDescuentoTotalAspiranteAsync(
                        idAspirante, detalle.Monto, "COLEGIATURA", ct);
                }

                var recibo = await _reciboService.GenerarReciboAspiranteConConceptoYMontoAsync(
                    idAspirante, detalle.IdConceptoPago, detalle.Monto, descuento, 7, ct);

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

            // Busca el primer detalle cuyo nombre o clave normalizado contiene alguna de las palabras clave
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

            var conceptos = new List<CotizacionConceptoDto>
            {
                new() {
                    Nombre = "FICHA DE ADMISIÓN",
                    Valor  = ObtenerValor(d => Contiene(d, "FICHA"))
                },
                new() {
                    Nombre = "INSCRIPCIÓN",
                    Valor  = ObtenerValor(d =>
                        d.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.INSCRIPCION
                        && !Contiene(d, "REINSC"))
                },
                new() {
                    Nombre = "REINSCRIPCIÓN",
                    Valor  = ObtenerValor(d => Contiene(d, "REINSC"))
                },
                new() {
                    Nombre = "EXAMEN DE ADMISIÓN",
                    Valor  = ObtenerValor(d => d.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.EXAMEN)
                },
                new() {
                    Nombre = "PROPEDÉUTICO",
                    Valor  = ObtenerValor(d => Contiene(d, "PROP"))
                },
                new() {
                    Nombre = "MENSUALIDADES",
                    Valor  = ObtenerValor(d => d.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.COLEGIATURA)
                },
                new() {
                    Nombre = "SEGURO ESTUDIANTIL",
                    Valor  = ObtenerValor(d => d.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.SEGURO)
                },
                new() {
                    Nombre = "CREDENCIAL ESTUDIANTIL",
                    Valor  = ObtenerValor(d => d.IdConceptoPagoNavigation?.Tipo == Core.Enums.ConceptoTipoEnum.CREDENCIAL, "No")
                },
                new() {
                    Nombre = "CONVENIO",
                    Valor  = tarifa.AplicaConvenioMensualidad ? "SÍ" : "NO"
                },
            };

            return new CotizacionAdmisionPdfDto
            {
                NombreAspirante = nombreAspirante,
                Licenciatura    = tarifa.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "N/A",
                ClavePlan       = tarifa.IdPlanEstudiosNavigation?.ClavePlanEstudios ?? "",
                NombreTarifa    = tarifa.Nombre,
                Fecha           = DateOnly.FromDateTime(DateTime.UtcNow),
                Conceptos       = conceptos,
                Institucion     = new Core.DTOs.Recibo.InstitucionPdfDto()
            };
        }

        private static TarifaAdmisionDto MapToDto(TarifaAdmision t) => new()
        {
            IdTarifaAdmision = t.IdTarifaAdmision,
            IdPlanEstudios = t.IdPlanEstudios,
            NombrePlanEstudios = t.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "",
            ClavePlanEstudios = t.IdPlanEstudiosNavigation?.ClavePlanEstudios ?? "",
            Nombre = t.Nombre,
            AplicaConvenioMensualidad = t.AplicaConvenioMensualidad,
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
