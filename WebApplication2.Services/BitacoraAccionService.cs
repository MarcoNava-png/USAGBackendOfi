using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class BitacoraAccionService : IBitacoraAccionService
    {
        private readonly ApplicationDbContext _db;

        public BitacoraAccionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task RegistrarAsync(string usuarioId, string nombreUsuario, string accion, string modulo,
            string entidad, string entidadId, string descripcion,
            string? datosAnteriores = null, string? datosNuevos = null, string? ip = null)
        {
            var entry = new BitacoraAccion
            {
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                Accion = accion,
                Modulo = modulo,
                Entidad = entidad,
                EntidadId = entidadId,
                Descripcion = descripcion,
                DatosAnteriores = datosAnteriores,
                DatosNuevos = datosNuevos,
                IpAddress = ip,
                FechaUtc = DateTime.UtcNow
            };

            _db.BitacoraAcciones.Add(entry);
            await _db.SaveChangesAsync();
        }

        public async Task<PagedResult<BitacoraAccionDto>> ConsultarAsync(BitacoraAccionFiltroDto filtro, CancellationToken ct)
        {
            if (filtro.Page <= 0) filtro.Page = 1;
            if (filtro.PageSize <= 0) filtro.PageSize = 20;
            if (filtro.PageSize > 100) filtro.PageSize = 100;

            var query = _db.BitacoraAcciones.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Modulo))
                query = query.Where(b => b.Modulo == filtro.Modulo);

            if (!string.IsNullOrWhiteSpace(filtro.Usuario))
            {
                var u = filtro.Usuario.ToLower();
                query = query.Where(b => b.NombreUsuario.ToLower().Contains(u) || b.UsuarioId.ToLower().Contains(u));
            }

            if (filtro.FechaDesde.HasValue)
                query = query.Where(b => b.FechaUtc >= filtro.FechaDesde.Value);

            if (filtro.FechaHasta.HasValue)
                query = query.Where(b => b.FechaUtc <= filtro.FechaHasta.Value);

            if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            {
                var s = filtro.Busqueda.ToLower();
                query = query.Where(b =>
                    b.Accion.ToLower().Contains(s) ||
                    b.Entidad.ToLower().Contains(s) ||
                    (b.Descripcion != null && b.Descripcion.ToLower().Contains(s)) ||
                    b.NombreUsuario.ToLower().Contains(s)
                );
            }

            var totalItems = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(b => b.FechaUtc)
                .Skip((filtro.Page - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .Select(b => new BitacoraAccionDto
                {
                    IdBitacora = b.IdBitacora,
                    UsuarioId = b.UsuarioId,
                    NombreUsuario = b.NombreUsuario,
                    Accion = b.Accion,
                    Modulo = b.Modulo,
                    Entidad = b.Entidad,
                    EntidadId = b.EntidadId,
                    Descripcion = b.Descripcion,
                    DatosAnteriores = b.DatosAnteriores,
                    DatosNuevos = b.DatosNuevos,
                    IpAddress = b.IpAddress,
                    FechaUtc = b.FechaUtc
                })
                .ToListAsync(ct);

            return new PagedResult<BitacoraAccionDto>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = filtro.Page,
                PageSize = filtro.PageSize
            };
        }
    }
}
