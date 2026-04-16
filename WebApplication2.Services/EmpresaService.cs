using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.Empresa;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class EmpresaService : IEmpresaService
    {
        private readonly ApplicationDbContext _db;

        public EmpresaService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<EmpresaDto>> ListarEmpresasAsync(bool? soloActivas = null, CancellationToken ct = default)
        {
            var query = _db.Empresas
                .Include(e => e.Aspirantes)
                .Where(e => e.Status != StatusEnum.Deleted);

            if (soloActivas.HasValue)
                query = query.Where(e => e.Activo == soloActivas.Value);

            var lista = await query.OrderBy(e => e.Nombre).ToListAsync(ct);
            return lista.Select(MapToDto).ToList();
        }

        public async Task<EmpresaDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            var empresa = await _db.Empresas
                .Include(e => e.Aspirantes)
                .FirstOrDefaultAsync(e => e.IdEmpresa == id && e.Status != StatusEnum.Deleted, ct);

            return empresa == null ? null : MapToDto(empresa);
        }

        public async Task<EmpresaDto> CrearEmpresaAsync(CrearEmpresaDto dto, string usuarioCreador, CancellationToken ct = default)
        {
            var existe = await _db.Empresas
                .AnyAsync(e => e.Nombre == dto.Nombre && e.Status != StatusEnum.Deleted, ct);

            if (existe)
                throw new InvalidOperationException($"Ya existe una empresa con el nombre '{dto.Nombre}'");

            var empresa = new Empresa
            {
                Nombre = dto.Nombre,
                Activo = dto.Activo,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuarioCreador,
                Status = StatusEnum.Active
            };

            _db.Empresas.Add(empresa);
            await _db.SaveChangesAsync(ct);

            return (await ObtenerPorIdAsync(empresa.IdEmpresa, ct))!;
        }

        public async Task<EmpresaDto> ActualizarEmpresaAsync(int id, ActualizarEmpresaDto dto, string usuarioModificador, CancellationToken ct = default)
        {
            var empresa = await _db.Empresas
                .FirstOrDefaultAsync(e => e.IdEmpresa == id && e.Status != StatusEnum.Deleted, ct)
                ?? throw new InvalidOperationException($"No se encontró la empresa con ID {id}");

            if (empresa.Nombre != dto.Nombre)
            {
                var existe = await _db.Empresas
                    .AnyAsync(e => e.Nombre == dto.Nombre && e.IdEmpresa != id && e.Status != StatusEnum.Deleted, ct);
                if (existe)
                    throw new InvalidOperationException($"Ya existe una empresa con el nombre '{dto.Nombre}'");
            }

            empresa.Nombre = dto.Nombre;
            empresa.Activo = dto.Activo;
            empresa.UpdatedAt = DateTime.UtcNow;
            empresa.UpdatedBy = usuarioModificador;

            await _db.SaveChangesAsync(ct);
            return (await ObtenerPorIdAsync(id, ct))!;
        }

        public async Task<bool> EliminarEmpresaAsync(int id, CancellationToken ct = default)
        {
            var empresa = await _db.Empresas
                .FirstOrDefaultAsync(e => e.IdEmpresa == id && e.Status != StatusEnum.Deleted, ct);

            if (empresa == null) return false;

            empresa.Status = StatusEnum.Deleted;
            empresa.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default)
        {
            var empresa = await _db.Empresas
                .FirstOrDefaultAsync(e => e.IdEmpresa == id && e.Status != StatusEnum.Deleted, ct);

            if (empresa == null) return false;

            empresa.Activo = activo;
            empresa.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        private static EmpresaDto MapToDto(Empresa e) => new()
        {
            IdEmpresa = e.IdEmpresa,
            Nombre = e.Nombre,
            Activo = e.Activo,
            CantidadAspirantes = e.Aspirantes?.Count(a => a.Status != StatusEnum.Deleted) ?? 0
        };
    }
}
