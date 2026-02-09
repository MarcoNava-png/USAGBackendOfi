using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Convenio;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class ConvenioService : IConvenioService
    {
        private readonly ApplicationDbContext _dbContext;

        public ConvenioService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<ConvenioDto>> ListarConveniosAsync(
            bool? soloActivos = null,
            int? idCampus = null,
            int? idPlanEstudios = null,
            CancellationToken ct = default)
        {
            var query = _dbContext.Convenio
                .Include(c => c.ConvenioAlcance)
                    .ThenInclude(a => a.IdCampusNavigation)
                .Include(c => c.ConvenioAlcance)
                    .ThenInclude(a => a.IdPlanEstudiosNavigation)
                .Include(c => c.AspiranteConvenio)
                .Where(c => c.Status == StatusEnum.Active)
                .AsQueryable();

            if (soloActivos.HasValue)
            {
                query = query.Where(c => c.Activo == soloActivos.Value);
            }

            if (idCampus.HasValue || idPlanEstudios.HasValue)
            {
                query = query.Where(c => c.ConvenioAlcance.Any(a =>
                    (!idCampus.HasValue || a.IdCampus == idCampus || a.IdCampus == null) &&
                    (!idPlanEstudios.HasValue || a.IdPlanEstudios == idPlanEstudios || a.IdPlanEstudios == null)));
            }

            var convenios = await query.OrderBy(c => c.Nombre).ToListAsync(ct);

            return convenios.Select(MapToDto).ToList();
        }

        public async Task<ConvenioDto?> ObtenerPorIdAsync(int idConvenio, CancellationToken ct = default)
        {
            var convenio = await _dbContext.Convenio
                .Include(c => c.ConvenioAlcance)
                    .ThenInclude(a => a.IdCampusNavigation)
                .Include(c => c.ConvenioAlcance)
                    .ThenInclude(a => a.IdPlanEstudiosNavigation)
                .Include(c => c.AspiranteConvenio)
                .Where(c => c.Status == StatusEnum.Active)
                .FirstOrDefaultAsync(c => c.IdConvenio == idConvenio, ct);

            return convenio != null ? MapToDto(convenio) : null;
        }

        public async Task<ConvenioDto> CrearConvenioAsync(CrearConvenioDto dto, string usuarioCreador, CancellationToken ct = default)
        {
            var claveExiste = await _dbContext.Convenio
                .AnyAsync(c => c.ClaveConvenio == dto.ClaveConvenio && c.Status == StatusEnum.Active, ct);

            if (claveExiste)
            {
                throw new InvalidOperationException($"Ya existe un convenio con la clave '{dto.ClaveConvenio}'");
            }

            ValidarTipoBeneficio(dto.TipoBeneficio, dto.DescuentoPct, dto.Monto);

            ValidarAplicaA(dto.AplicaA);

            var convenio = new Convenio
            {
                ClaveConvenio = dto.ClaveConvenio,
                Nombre = dto.Nombre,
                TipoBeneficio = dto.TipoBeneficio,
                DescuentoPct = dto.DescuentoPct,
                Monto = dto.Monto,
                VigenteDesde = dto.VigenteDesde,
                VigenteHasta = dto.VigenteHasta,
                AplicaA = dto.AplicaA ?? "TODOS",
                MaxAplicaciones = dto.MaxAplicaciones,
                Activo = dto.Activo,
                CreatedBy = usuarioCreador,
                CreatedAt = DateTime.UtcNow,
                Status = StatusEnum.Active
            };

            await _dbContext.Convenio.AddAsync(convenio, ct);
            await _dbContext.SaveChangesAsync(ct);

            if (dto.Alcances.Any())
            {
                foreach (var alcanceDto in dto.Alcances)
                {
                    var alcance = new ConvenioAlcance
                    {
                        IdConvenio = convenio.IdConvenio,
                        IdCampus = alcanceDto.IdCampus,
                        IdPlanEstudios = alcanceDto.IdPlanEstudios,
                        VigenteDesde = alcanceDto.VigenteDesde,
                        VigenteHasta = alcanceDto.VigenteHasta,
                        CreatedBy = usuarioCreador,
                        CreatedAt = DateTime.UtcNow,
                        Status = StatusEnum.Active
                    };
                    await _dbContext.ConvenioAlcance.AddAsync(alcance, ct);
                }
                await _dbContext.SaveChangesAsync(ct);
            }

            return (await ObtenerPorIdAsync(convenio.IdConvenio, ct))!;
        }

        public async Task<ConvenioDto> ActualizarConvenioAsync(int idConvenio, ActualizarConvenioDto dto, string usuarioModificador, CancellationToken ct = default)
        {
            var convenio = await _dbContext.Convenio
                .Include(c => c.ConvenioAlcance)
                .FirstOrDefaultAsync(c => c.IdConvenio == idConvenio && c.Status == StatusEnum.Active, ct);

            if (convenio == null)
            {
                throw new InvalidOperationException($"No se encontro el convenio con ID {idConvenio}");
            }

            if (convenio.ClaveConvenio != dto.ClaveConvenio)
            {
                var claveExiste = await _dbContext.Convenio
                    .AnyAsync(c => c.ClaveConvenio == dto.ClaveConvenio && c.IdConvenio != idConvenio && c.Status == StatusEnum.Active, ct);

                if (claveExiste)
                {
                    throw new InvalidOperationException($"Ya existe un convenio con la clave '{dto.ClaveConvenio}'");
                }
            }

            ValidarTipoBeneficio(dto.TipoBeneficio, dto.DescuentoPct, dto.Monto);
            ValidarAplicaA(dto.AplicaA);

            convenio.ClaveConvenio = dto.ClaveConvenio;
            convenio.Nombre = dto.Nombre;
            convenio.TipoBeneficio = dto.TipoBeneficio;
            convenio.DescuentoPct = dto.DescuentoPct;
            convenio.Monto = dto.Monto;
            convenio.VigenteDesde = dto.VigenteDesde;
            convenio.VigenteHasta = dto.VigenteHasta;
            convenio.AplicaA = dto.AplicaA ?? "TODOS";
            convenio.MaxAplicaciones = dto.MaxAplicaciones;
            convenio.Activo = dto.Activo;
            convenio.UpdatedBy = usuarioModificador;
            convenio.UpdatedAt = DateTime.UtcNow;

            foreach (var alcance in convenio.ConvenioAlcance.Where(a => a.Status == StatusEnum.Active))
            {
                alcance.Status = StatusEnum.Deleted;
                alcance.UpdatedBy = usuarioModificador;
                alcance.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var alcanceDto in dto.Alcances)
            {
                var alcance = new ConvenioAlcance
                {
                    IdConvenio = convenio.IdConvenio,
                    IdCampus = alcanceDto.IdCampus,
                    IdPlanEstudios = alcanceDto.IdPlanEstudios,
                    VigenteDesde = alcanceDto.VigenteDesde,
                    VigenteHasta = alcanceDto.VigenteHasta,
                    CreatedBy = usuarioModificador,
                    CreatedAt = DateTime.UtcNow,
                    Status = StatusEnum.Active
                };
                await _dbContext.ConvenioAlcance.AddAsync(alcance, ct);
            }

            await _dbContext.SaveChangesAsync(ct);

            return (await ObtenerPorIdAsync(convenio.IdConvenio, ct))!;
        }

        public async Task<bool> EliminarConvenioAsync(int idConvenio, CancellationToken ct = default)
        {
            var convenio = await _dbContext.Convenio
                .FirstOrDefaultAsync(c => c.IdConvenio == idConvenio && c.Status == StatusEnum.Active, ct);

            if (convenio == null)
            {
                return false;
            }

            var tieneAspirantesActivos = await _dbContext.AspiranteConvenio
                .AnyAsync(ac => ac.IdConvenio == idConvenio && ac.Estatus == "Aprobado", ct);

            if (tieneAspirantesActivos)
            {
                throw new InvalidOperationException("No se puede eliminar el convenio porque tiene aspirantes con estatus 'Aprobado'");
            }

            convenio.Status = StatusEnum.Deleted;
            convenio.Activo = false;
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> CambiarEstadoConvenioAsync(int idConvenio, bool activo, CancellationToken ct = default)
        {
            var convenio = await _dbContext.Convenio
                .FirstOrDefaultAsync(c => c.IdConvenio == idConvenio && c.Status == StatusEnum.Active, ct);

            if (convenio == null)
            {
                return false;
            }

            convenio.Activo = activo;
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }

        public async Task<IReadOnlyList<ConvenioDisponibleDto>> ObtenerConveniosDisponiblesParaAspiranteAsync(
            int idAspirante,
            CancellationToken ct = default)
        {
            var aspirante = await _dbContext.Aspirante
                .Include(a => a.IdPlanNavigation)
                .FirstOrDefaultAsync(a => a.IdAspirante == idAspirante && a.Status == StatusEnum.Active, ct);

            if (aspirante == null)
            {
                throw new InvalidOperationException($"No se encontro el aspirante con ID {idAspirante}");
            }

            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var idCampusAspirante = aspirante.IdPlanNavigation?.IdCampus;
            var idPlanAspirante = aspirante.IdPlan;

            var convenios = await _dbContext.Convenio
                .Include(c => c.ConvenioAlcance)
                .Where(c => c.Status == StatusEnum.Active
                         && c.Activo
                         && (!c.VigenteDesde.HasValue || c.VigenteDesde <= hoy)
                         && (!c.VigenteHasta.HasValue || c.VigenteHasta >= hoy))
                .Where(c => c.ConvenioAlcance.Any(a =>
                    a.Status == StatusEnum.Active &&
                    (a.IdCampus == null || a.IdCampus == idCampusAspirante) &&
                    (a.IdPlanEstudios == null || a.IdPlanEstudios == idPlanAspirante) &&
                    (!a.VigenteDesde.HasValue || a.VigenteDesde <= hoy) &&
                    (!a.VigenteHasta.HasValue || a.VigenteHasta >= hoy)))
                .OrderBy(c => c.Nombre)
                .ToListAsync(ct);

            var conveniosAsignados = await _dbContext.AspiranteConvenio
                .Where(ac => ac.IdAspirante == idAspirante)
                .Select(ac => ac.IdConvenio)
                .ToListAsync(ct);

            return convenios
                .Where(c => !conveniosAsignados.Contains(c.IdConvenio))
                .Select(c => new ConvenioDisponibleDto
                {
                    IdConvenio = c.IdConvenio,
                    ClaveConvenio = c.ClaveConvenio,
                    Nombre = c.Nombre,
                    TipoBeneficio = c.TipoBeneficio,
                    DescuentoPct = c.DescuentoPct,
                    Monto = c.Monto,
                    DescripcionBeneficio = ObtenerDescripcionBeneficio(c),
                    AplicaA = c.AplicaA ?? "TODOS",
                    MaxAplicaciones = c.MaxAplicaciones
                })
                .ToList();
        }

        public async Task<AspiranteConvenioDto> AsignarConvenioAspiranteAsync(
            AsignarConvenioAspiranteDto dto,
            string usuarioCreador,
            CancellationToken ct = default)
        {
            var aspirante = await _dbContext.Aspirante
                .Include(a => a.IdPersonaNavigation)
                .FirstOrDefaultAsync(a => a.IdAspirante == dto.IdAspirante && a.Status == StatusEnum.Active, ct);

            if (aspirante == null)
            {
                throw new InvalidOperationException($"No se encontro el aspirante con ID {dto.IdAspirante}");
            }

            var convenio = await _dbContext.Convenio
                .FirstOrDefaultAsync(c => c.IdConvenio == dto.IdConvenio && c.Status == StatusEnum.Active && c.Activo, ct);

            if (convenio == null)
            {
                throw new InvalidOperationException($"No se encontro el convenio con ID {dto.IdConvenio} o no esta activo");
            }

            var yaAsignado = await _dbContext.AspiranteConvenio
                .AnyAsync(ac => ac.IdAspirante == dto.IdAspirante && ac.IdConvenio == dto.IdConvenio, ct);

            if (yaAsignado)
            {
                throw new InvalidOperationException("El convenio ya esta asignado a este aspirante");
            }

            var asignacion = new AspiranteConvenio
            {
                IdAspirante = dto.IdAspirante,
                IdConvenio = dto.IdConvenio,
                FechaAsignacion = DateTime.UtcNow,
                Estatus = "Pendiente",
                Evidencia = dto.Evidencia,
                CreatedBy = usuarioCreador,
                CreatedAt = DateTime.UtcNow,
                Status = StatusEnum.Active
            };

            await _dbContext.AspiranteConvenio.AddAsync(asignacion, ct);
            await _dbContext.SaveChangesAsync(ct);

            return new AspiranteConvenioDto
            {
                IdAspiranteConvenio = asignacion.IdAspiranteConvenio,
                IdAspirante = asignacion.IdAspirante,
                NombreAspirante = $"{aspirante.IdPersonaNavigation?.Nombre} {aspirante.IdPersonaNavigation?.ApellidoPaterno} {aspirante.IdPersonaNavigation?.ApellidoMaterno}".Trim(),
                IdConvenio = convenio.IdConvenio,
                ClaveConvenio = convenio.ClaveConvenio,
                NombreConvenio = convenio.Nombre,
                TipoBeneficio = convenio.TipoBeneficio,
                DescuentoPct = convenio.DescuentoPct,
                Monto = convenio.Monto,
                FechaAsignacion = asignacion.FechaAsignacion,
                Estatus = asignacion.Estatus,
                Evidencia = asignacion.Evidencia
            };
        }

        public async Task<IReadOnlyList<AspiranteConvenioDto>> ObtenerConveniosAspiranteAsync(
            int idAspirante,
            CancellationToken ct = default)
        {
            var asignaciones = await _dbContext.AspiranteConvenio
                .Include(ac => ac.IdAspiranteNavigation)
                    .ThenInclude(a => a.IdPersonaNavigation)
                .Include(ac => ac.IdConvenioNavigation)
                .Where(ac => ac.IdAspirante == idAspirante && ac.Status == StatusEnum.Active)
                .OrderByDescending(ac => ac.FechaAsignacion)
                .ToListAsync(ct);

            return asignaciones.Select(ac => new AspiranteConvenioDto
            {
                IdAspiranteConvenio = ac.IdAspiranteConvenio,
                IdAspirante = ac.IdAspirante,
                NombreAspirante = $"{ac.IdAspiranteNavigation?.IdPersonaNavigation?.Nombre} {ac.IdAspiranteNavigation?.IdPersonaNavigation?.ApellidoPaterno} {ac.IdAspiranteNavigation?.IdPersonaNavigation?.ApellidoMaterno}".Trim(),
                IdConvenio = ac.IdConvenio,
                ClaveConvenio = ac.IdConvenioNavigation?.ClaveConvenio,
                NombreConvenio = ac.IdConvenioNavigation?.Nombre,
                TipoBeneficio = ac.IdConvenioNavigation?.TipoBeneficio,
                DescuentoPct = ac.IdConvenioNavigation?.DescuentoPct,
                Monto = ac.IdConvenioNavigation?.Monto,
                FechaAsignacion = ac.FechaAsignacion,
                Estatus = ac.Estatus,
                Evidencia = ac.Evidencia,
                AplicaA = ac.IdConvenioNavigation?.AplicaA ?? "TODOS",
                MaxAplicaciones = ac.IdConvenioNavigation?.MaxAplicaciones,
                VecesAplicado = ac.VecesAplicado,
                PuedeAplicarse = ac.Estatus == "Aprobado" &&
                    (ac.IdConvenioNavigation == null ||
                     !ac.IdConvenioNavigation.MaxAplicaciones.HasValue ||
                     ac.VecesAplicado < ac.IdConvenioNavigation.MaxAplicaciones.Value)
            }).ToList();
        }

        public async Task<bool> CambiarEstatusConvenioAspiranteAsync(
            int idAspiranteConvenio,
            string nuevoEstatus,
            CancellationToken ct = default)
        {
            var estatusValidos = new[] { "Pendiente", "Aprobado", "Rechazado" };
            if (!estatusValidos.Contains(nuevoEstatus))
            {
                throw new ArgumentException($"Estatus no valido. Valores permitidos: {string.Join(", ", estatusValidos)}");
            }

            var asignacion = await _dbContext.AspiranteConvenio
                .FirstOrDefaultAsync(ac => ac.IdAspiranteConvenio == idAspiranteConvenio, ct);

            if (asignacion == null)
            {
                return false;
            }

            asignacion.Estatus = nuevoEstatus;
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> EliminarConvenioAspiranteAsync(int idAspiranteConvenio, CancellationToken ct = default)
        {
            var asignacion = await _dbContext.AspiranteConvenio
                .FirstOrDefaultAsync(ac => ac.IdAspiranteConvenio == idAspiranteConvenio, ct);

            if (asignacion == null)
            {
                return false;
            }

            asignacion.Status = StatusEnum.Deleted;
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }

        public async Task<CalculoDescuentoConvenioDto> CalcularDescuentoConvenioAsync(
            int idConvenio,
            decimal montoOriginal,
            CancellationToken ct = default)
        {
            var convenio = await _dbContext.Convenio
                .FirstOrDefaultAsync(c => c.IdConvenio == idConvenio && c.Status == StatusEnum.Active, ct);

            if (convenio == null)
            {
                throw new InvalidOperationException($"No se encontro el convenio con ID {idConvenio}");
            }

            decimal descuento = CalcularDescuento(convenio, montoOriginal);

            return new CalculoDescuentoConvenioDto
            {
                IdConvenio = convenio.IdConvenio,
                NombreConvenio = convenio.Nombre,
                TipoBeneficio = convenio.TipoBeneficio,
                MontoOriginal = montoOriginal,
                Descuento = descuento,
                MontoFinal = montoOriginal - descuento
            };
        }

        public async Task<decimal> CalcularDescuentoTotalAspiranteAsync(
            int idAspirante,
            decimal montoOriginal,
            string? tipoConcepto = null,
            CancellationToken ct = default)
        {
            var asignacionesAprobadas = await _dbContext.AspiranteConvenio
                .Include(ac => ac.IdConvenioNavigation)
                .Where(ac => ac.IdAspirante == idAspirante
                          && ac.Estatus == "Aprobado"
                          && ac.Status == StatusEnum.Active
                          && ac.IdConvenioNavigation.Activo)
                .ToListAsync(ct);

            if (!asignacionesAprobadas.Any())
            {
                return 0m;
            }

            decimal descuentoTotal = 0m;

            foreach (var asignacion in asignacionesAprobadas)
            {
                var convenio = asignacion.IdConvenioNavigation;

                if (!string.IsNullOrEmpty(tipoConcepto))
                {
                    var aplicaA = convenio.AplicaA?.ToUpperInvariant() ?? "TODOS";
                    if (aplicaA != "TODOS" && aplicaA != tipoConcepto.ToUpperInvariant())
                    {
                        continue;
                    }
                }

                if (convenio.MaxAplicaciones.HasValue && asignacion.VecesAplicado >= convenio.MaxAplicaciones.Value)
                {
                    continue;
                }

                descuentoTotal += CalcularDescuento(convenio, montoOriginal);
            }

            if (descuentoTotal > montoOriginal)
            {
                descuentoTotal = montoOriginal;
            }

            return descuentoTotal;
        }

        public async Task IncrementarAplicacionesConvenioAsync(
            int idAspirante,
            string? tipoConcepto = null,
            CancellationToken ct = default)
        {
            var asignacionesAprobadas = await _dbContext.AspiranteConvenio
                .Include(ac => ac.IdConvenioNavigation)
                .Where(ac => ac.IdAspirante == idAspirante
                          && ac.Estatus == "Aprobado"
                          && ac.Status == StatusEnum.Active
                          && ac.IdConvenioNavigation.Activo)
                .ToListAsync(ct);

            foreach (var asignacion in asignacionesAprobadas)
            {
                var convenio = asignacion.IdConvenioNavigation;

                if (!string.IsNullOrEmpty(tipoConcepto))
                {
                    var aplicaA = convenio.AplicaA?.ToUpperInvariant() ?? "TODOS";
                    if (aplicaA != "TODOS" && aplicaA != tipoConcepto.ToUpperInvariant())
                    {
                        continue;
                    }
                }

                if (convenio.MaxAplicaciones.HasValue && asignacion.VecesAplicado >= convenio.MaxAplicaciones.Value)
                {
                    continue;
                }

                asignacion.VecesAplicado++;
            }

            await _dbContext.SaveChangesAsync(ct);
        }

        #region Helpers privados

        private static decimal CalcularDescuento(Convenio convenio, decimal montoOriginal)
        {
            decimal descuento = 0m;

            switch (convenio.TipoBeneficio.ToUpperInvariant())
            {
                case "PORCENTAJE":
                    if (convenio.DescuentoPct.HasValue)
                    {
                        descuento = montoOriginal * (convenio.DescuentoPct.Value / 100m);
                    }
                    break;

                case "MONTO":
                    if (convenio.Monto.HasValue)
                    {
                        descuento = convenio.Monto.Value;
                    }
                    break;

                case "EXENCION":
                    descuento = montoOriginal;
                    break;
            }

            if (descuento > montoOriginal)
            {
                descuento = montoOriginal;
            }

            return descuento;
        }

        private static void ValidarTipoBeneficio(string tipoBeneficio, decimal? descuentoPct, decimal? monto)
        {
            var tiposValidos = new[] { "PORCENTAJE", "MONTO", "EXENCION" };

            if (!tiposValidos.Contains(tipoBeneficio.ToUpperInvariant()))
            {
                throw new ArgumentException($"Tipo de beneficio no valido. Valores permitidos: {string.Join(", ", tiposValidos)}");
            }

            if (tipoBeneficio.ToUpperInvariant() == "PORCENTAJE")
            {
                if (!descuentoPct.HasValue)
                {
                    throw new ArgumentException("Debe especificar el porcentaje de descuento");
                }
                if (descuentoPct < 0 || descuentoPct > 100)
                {
                    throw new ArgumentException("El porcentaje debe estar entre 0 y 100");
                }
            }

            if (tipoBeneficio.ToUpperInvariant() == "MONTO")
            {
                if (!monto.HasValue)
                {
                    throw new ArgumentException("Debe especificar el monto de descuento");
                }
                if (monto < 0)
                {
                    throw new ArgumentException("El monto no puede ser negativo");
                }
            }
        }

        private static void ValidarAplicaA(string? aplicaA)
        {
            if (string.IsNullOrEmpty(aplicaA)) return;

            var valoresValidos = new[] { "INSCRIPCION", "COLEGIATURA", "TODOS" };
            if (!valoresValidos.Contains(aplicaA.ToUpperInvariant()))
            {
                throw new ArgumentException($"AplicaA no valido. Valores permitidos: {string.Join(", ", valoresValidos)}");
            }
        }

        private static string ObtenerDescripcionBeneficio(Convenio convenio)
        {
            return convenio.TipoBeneficio.ToUpperInvariant() switch
            {
                "PORCENTAJE" => $"{convenio.DescuentoPct:N0}% de descuento",
                "MONTO" => $"${convenio.Monto:N2} de descuento",
                "EXENCION" => "Exencion total (100%)",
                _ => convenio.TipoBeneficio
            };
        }

        private static ConvenioDto MapToDto(Convenio convenio)
        {
            return new ConvenioDto
            {
                IdConvenio = convenio.IdConvenio,
                ClaveConvenio = convenio.ClaveConvenio,
                Nombre = convenio.Nombre,
                TipoBeneficio = convenio.TipoBeneficio,
                DescuentoPct = convenio.DescuentoPct,
                Monto = convenio.Monto,
                VigenteDesde = convenio.VigenteDesde,
                VigenteHasta = convenio.VigenteHasta,
                AplicaA = convenio.AplicaA ?? "TODOS",
                MaxAplicaciones = convenio.MaxAplicaciones,
                Activo = convenio.Activo,
                AspirantesAsignados = convenio.AspiranteConvenio?.Count(ac => ac.Status == StatusEnum.Active) ?? 0,
                Alcances = convenio.ConvenioAlcance?
                    .Where(a => a.Status == StatusEnum.Active)
                    .Select(a => new ConvenioAlcanceDto
                    {
                        IdConvenioAlcance = a.IdConvenioAlcance,
                        IdConvenio = a.IdConvenio,
                        IdCampus = a.IdCampus,
                        NombreCampus = a.IdCampusNavigation?.Nombre,
                        IdPlanEstudios = a.IdPlanEstudios,
                        NombrePlanEstudios = a.IdPlanEstudiosNavigation?.NombrePlanEstudios,
                        VigenteDesde = a.VigenteDesde,
                        VigenteHasta = a.VigenteHasta
                    })
                    .ToList() ?? new List<ConvenioAlcanceDto>()
            };
        }

        #endregion
    }
}
