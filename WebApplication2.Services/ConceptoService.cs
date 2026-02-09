using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class ConceptoService : IConceptoService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public ConceptoService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db; _mapper = mapper;
        }

        private static ConceptoTipoEnum ParseTipoEnum(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return ConceptoTipoEnum.OTRO;

            return tipo.ToUpperInvariant() switch
            {
                "INSCRIPCION" => ConceptoTipoEnum.INSCRIPCION,
                "COLEGIATURA" => ConceptoTipoEnum.COLEGIATURA,
                "EXAMEN" => ConceptoTipoEnum.EXAMEN,
                "CONSTANCIA" => ConceptoTipoEnum.CONSTANCIA,
                "CREDENCIAL" => ConceptoTipoEnum.CREDENCIAL,
                "SEGURO" => ConceptoTipoEnum.SEGURO,
                _ => ConceptoTipoEnum.OTRO
            };
        }

        private static ConceptoDto MapToDto(ConceptoPago entity)
        {
            return new ConceptoDto
            {
                IdConceptoPago = entity.IdConceptoPago,
                Clave = entity.Clave,
                Nombre = entity.Nombre ?? entity.Descripcion ?? entity.Clave,
                Descripcion = entity.Descripcion,
                Tipo = entity.Tipo.ToString(),
                PermiteBeca = entity.PermiteBeca,
                Status = entity.Activo ? 1 : 0,
                ConceptoTipo = entity.Tipo,
                ConceptoAplica = entity.AplicaA,
                EsObligatorio = entity.EsObligatorio,
                PeriodicidadMeses = entity.PeriodicidadMeses
            };
        }

        public async Task<ConceptoDto> CrearConceptoAsync(CrearConceptoDto dto, CancellationToken ct)
        {
            var entity = new ConceptoPago
            {
                Clave = dto.Clave,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion ?? dto.Nombre,
                Tipo = dto.ConceptoTipo ?? ParseTipoEnum(dto.Tipo),
                AplicaA = dto.ConceptoAplica ?? ConceptoAplicaAEnum.Ambos,
                EsObligatorio = dto.EsObligatorio ?? false,
                PeriodicidadMeses = dto.PeriodicidadMeses,
                PermiteBeca = dto.PermiteBeca,
                Activo = true
            };

            _db.ConceptoPago.Add(entity);
            await _db.SaveChangesAsync(ct);
            return MapToDto(entity);
        }

        public async Task<PrecioDto> CrearPrecioAsync(CrearPrecioDto dto, CancellationToken ct)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var existe = await _db.ConceptoPago.AnyAsync(x => x.IdConceptoPago == dto.IdConceptoPago, ct);
            if (!existe) throw new InvalidOperationException("ConceptoPago no existe");

            if (dto.Importe <= 0) throw new ArgumentException("El importe debe ser mayor a 0.", nameof(dto.Importe));
            if (dto.VigenciaHasta.HasValue && dto.VigenciaHasta.Value < dto.VigenciaDesde)
                throw new ArgumentException("VigenciaHasta no puede ser menor que VigenciaDesde.", nameof(dto.VigenciaHasta));

            var entity = _mapper.Map<ConceptoPrecio>(dto);

            entity.Moneda = string.IsNullOrWhiteSpace(entity.Moneda) ? "MXN" : entity.Moneda.ToUpper();
            entity.CreatedAt = DateTime.UtcNow;

            _db.ConceptoPrecio.Add(entity);
            await _db.SaveChangesAsync(ct);

            return _mapper.Map<PrecioDto>(entity);
        }

        public async Task<IReadOnlyList<ConceptoDto>> ListarAsync(bool? soloActivos, int? tipo, string? busqueda, CancellationToken ct)
        {
            var query = _db.ConceptoPago.AsNoTracking();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(x => x.Activo);
            }

            if (tipo.HasValue)
            {
                query = query.Where(x => (int)x.Tipo == tipo.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var busquedaLower = busqueda.ToLower();
                query = query.Where(x =>
                    x.Clave.ToLower().Contains(busquedaLower) ||
                    (x.Nombre != null && x.Nombre.ToLower().Contains(busquedaLower)) ||
                    (x.Descripcion != null && x.Descripcion.ToLower().Contains(busquedaLower))
                );
            }

            var items = await query.OrderBy(x => x.Nombre).ToListAsync(ct);
            return items.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<PrecioDto>> ListarPreciosAsync(int idConceptoPago, CancellationToken ct)
        {
            var items = await _db.ConceptoPrecio.AsNoTracking()
                .Where(x => x.IdConceptoPago == idConceptoPago).ToListAsync(ct);
            return _mapper.Map<IReadOnlyList<PrecioDto>>(items);
        }

        public async Task<ConceptoDto?> ObtenerPorIdAsync(int id, CancellationToken ct)
        {
            var entity = await _db.ConceptoPago.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdConceptoPago == id, ct);

            if (entity == null)
                return null;

            return MapToDto(entity);
        }

        public async Task<ConceptoDto> ActualizarAsync(int id, ActualizarConceptoDto dto, CancellationToken ct)
        {
            var entity = await _db.ConceptoPago.FindAsync(new object[] { id }, ct);

            if (entity == null)
                throw new InvalidOperationException($"Concepto con ID {id} no encontrado");

            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                entity.Nombre = dto.Nombre;

            if (dto.Descripcion != null)
                entity.Descripcion = dto.Descripcion;

            if (!string.IsNullOrWhiteSpace(dto.Tipo))
                entity.Tipo = ParseTipoEnum(dto.Tipo);

            if (dto.PermiteBeca.HasValue)
                entity.PermiteBeca = dto.PermiteBeca.Value;

            if (dto.ConceptoTipo.HasValue)
                entity.Tipo = dto.ConceptoTipo.Value;

            if (dto.ConceptoAplica.HasValue)
                entity.AplicaA = dto.ConceptoAplica.Value;

            if (dto.EsObligatorio.HasValue)
                entity.EsObligatorio = dto.EsObligatorio.Value;

            if (dto.PeriodicidadMeses.HasValue)
                entity.PeriodicidadMeses = dto.PeriodicidadMeses;

            await _db.SaveChangesAsync(ct);
            return MapToDto(entity);
        }

        public async Task CambiarEstadoAsync(int id, bool activo, CancellationToken ct)
        {
            var entity = await _db.ConceptoPago.FindAsync(new object[] { id }, ct);

            if (entity == null)
                throw new InvalidOperationException($"Concepto con ID {id} no encontrado");

            entity.Activo = activo;
            await _db.SaveChangesAsync(ct);
        }

        public async Task EliminarAsync(int id, CancellationToken ct)
        {
            var entity = await _db.ConceptoPago
                .Include(x => x.Precios)
                .FirstOrDefaultAsync(x => x.IdConceptoPago == id, ct);

            if (entity == null)
                throw new InvalidOperationException($"Concepto con ID {id} no encontrado");

            if (entity.Precios.Any())
            {
                _db.ConceptoPrecio.RemoveRange(entity.Precios);
            }

            _db.ConceptoPago.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}
