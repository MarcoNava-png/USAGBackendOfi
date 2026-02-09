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

namespace WebApplication2.Services
{
    public class BecaCatalogoService : IBecaCatalogoService
    {
        private readonly ApplicationDbContext _dbContext;

        public BecaCatalogoService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Beca>> ObtenerTodasAsync(bool? soloActivas = null, CancellationToken ct = default)
        {
            var query = _dbContext.Beca
                .Include(b => b.ConceptoPago)
                .Where(b => b.Status == StatusEnum.Active);

            if (soloActivas.HasValue)
            {
                query = query.Where(b => b.Activo == soloActivas.Value);
            }

            return await query
                .OrderBy(b => b.Nombre)
                .ToListAsync(ct);
        }

        public async Task<Beca?> ObtenerPorIdAsync(int idBeca, CancellationToken ct = default)
        {
            return await _dbContext.Beca
                .Include(b => b.ConceptoPago)
                .FirstOrDefaultAsync(b => b.IdBeca == idBeca && b.Status == StatusEnum.Active, ct);
        }

        public async Task<Beca?> ObtenerPorClaveAsync(string clave, CancellationToken ct = default)
        {
            return await _dbContext.Beca
                .Include(b => b.ConceptoPago)
                .FirstOrDefaultAsync(b => b.Clave == clave && b.Status == StatusEnum.Active, ct);
        }

        public async Task<Beca> CrearAsync(
            string clave,
            string nombre,
            string? descripcion,
            string tipo,
            decimal valor,
            decimal? topeMensual,
            int? idConceptoPago,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(clave))
                throw new ArgumentException("La clave es requerida", nameof(clave));

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es requerido", nameof(nombre));

            if (tipo != "PORCENTAJE" && tipo != "MONTO")
                throw new ArgumentException("El tipo debe ser 'PORCENTAJE' o 'MONTO'", nameof(tipo));

            if (tipo == "PORCENTAJE" && (valor < 0 || valor > 100))
                throw new ArgumentException("El porcentaje debe estar entre 0 y 100", nameof(valor));

            if (valor < 0)
                throw new ArgumentException("El valor no puede ser negativo", nameof(valor));

            if (await ExisteClaveAsync(clave, null, ct))
                throw new InvalidOperationException($"Ya existe una beca con la clave '{clave}'");

            if (idConceptoPago.HasValue)
            {
                var conceptoExiste = await _dbContext.ConceptoPago
                    .AnyAsync(c => c.IdConceptoPago == idConceptoPago.Value && c.Activo, ct);

                if (!conceptoExiste)
                    throw new InvalidOperationException($"El concepto de pago con ID {idConceptoPago} no existe o no está activo");
            }

            var beca = new Beca
            {
                Clave = clave.ToUpperInvariant(),
                Nombre = nombre,
                Descripcion = descripcion,
                Tipo = tipo,
                Valor = valor,
                TopeMensual = topeMensual,
                IdConceptoPago = idConceptoPago,
                Activo = true
            };

            await _dbContext.Beca.AddAsync(beca, ct);
            await _dbContext.SaveChangesAsync(ct);

            return beca;
        }

        public async Task<Beca> ActualizarAsync(
            int idBeca,
            string nombre,
            string? descripcion,
            string tipo,
            decimal valor,
            decimal? topeMensual,
            int? idConceptoPago,
            bool activo,
            CancellationToken ct = default)
        {
            var beca = await _dbContext.Beca
                .FirstOrDefaultAsync(b => b.IdBeca == idBeca && b.Status == StatusEnum.Active, ct);

            if (beca == null)
                throw new InvalidOperationException($"No se encontró la beca con ID {idBeca}");

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es requerido", nameof(nombre));

            if (tipo != "PORCENTAJE" && tipo != "MONTO")
                throw new ArgumentException("El tipo debe ser 'PORCENTAJE' o 'MONTO'", nameof(tipo));

            if (tipo == "PORCENTAJE" && (valor < 0 || valor > 100))
                throw new ArgumentException("El porcentaje debe estar entre 0 y 100", nameof(valor));

            if (valor < 0)
                throw new ArgumentException("El valor no puede ser negativo", nameof(valor));

            if (idConceptoPago.HasValue)
            {
                var conceptoExiste = await _dbContext.ConceptoPago
                    .AnyAsync(c => c.IdConceptoPago == idConceptoPago.Value && c.Activo, ct);

                if (!conceptoExiste)
                    throw new InvalidOperationException($"El concepto de pago con ID {idConceptoPago} no existe o no está activo");
            }

            beca.Nombre = nombre;
            beca.Descripcion = descripcion;
            beca.Tipo = tipo;
            beca.Valor = valor;
            beca.TopeMensual = topeMensual;
            beca.IdConceptoPago = idConceptoPago;
            beca.Activo = activo;

            await _dbContext.SaveChangesAsync(ct);

            return beca;
        }

        public async Task<bool> DesactivarAsync(int idBeca, CancellationToken ct = default)
        {
            var beca = await _dbContext.Beca
                .FirstOrDefaultAsync(b => b.IdBeca == idBeca && b.Status == StatusEnum.Active, ct);

            if (beca == null)
                return false;

            beca.Activo = false;
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> ActivarAsync(int idBeca, CancellationToken ct = default)
        {
            var beca = await _dbContext.Beca
                .FirstOrDefaultAsync(b => b.IdBeca == idBeca && b.Status == StatusEnum.Active, ct);

            if (beca == null)
                return false;

            beca.Activo = true;
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> ExisteClaveAsync(string clave, int? excluirId = null, CancellationToken ct = default)
        {
            var query = _dbContext.Beca
                .Where(b => b.Clave == clave.ToUpperInvariant() && b.Status == StatusEnum.Active);

            if (excluirId.HasValue)
            {
                query = query.Where(b => b.IdBeca != excluirId.Value);
            }

            return await query.AnyAsync(ct);
        }
    }
}
