using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class NotificacionInternalService : INotificacionInternalService
    {
        private readonly ApplicationDbContext _db;

        public NotificacionInternalService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CrearAsync(string usuarioId, string titulo, string mensaje, string tipo, string? modulo = null, string? urlAccion = null)
        {
            var notif = new NotificacionUsuario
            {
                UsuarioDestinoId = usuarioId,
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                Modulo = modulo,
                UrlAccion = urlAccion,
                Leida = false,
                FechaCreacion = DateTime.UtcNow
            };

            _db.NotificacionesUsuario.Add(notif);
            await _db.SaveChangesAsync();
        }

        public async Task<PagedResult<NotificacionUsuarioDto>> ObtenerAsync(string usuarioId, bool soloNoLeidas, int page, int pageSize, CancellationToken ct)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var query = _db.NotificacionesUsuario
                .AsNoTracking()
                .Where(n => n.UsuarioDestinoId == usuarioId);

            if (soloNoLeidas)
                query = query.Where(n => !n.Leida);

            var totalItems = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(n => n.FechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificacionUsuarioDto
                {
                    IdNotificacion = n.IdNotificacion,
                    Titulo = n.Titulo,
                    Mensaje = n.Mensaje,
                    Tipo = n.Tipo,
                    Modulo = n.Modulo,
                    UrlAccion = n.UrlAccion,
                    Leida = n.Leida,
                    FechaCreacion = n.FechaCreacion,
                    FechaLectura = n.FechaLectura
                })
                .ToListAsync(ct);

            return new PagedResult<NotificacionUsuarioDto>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<int> ContarNoLeidasAsync(string usuarioId, CancellationToken ct)
        {
            return await _db.NotificacionesUsuario
                .CountAsync(n => n.UsuarioDestinoId == usuarioId && !n.Leida, ct);
        }

        public async Task MarcarLeidaAsync(long idNotificacion, string usuarioId, CancellationToken ct)
        {
            var notif = await _db.NotificacionesUsuario
                .FirstOrDefaultAsync(n => n.IdNotificacion == idNotificacion && n.UsuarioDestinoId == usuarioId, ct);

            if (notif != null && !notif.Leida)
            {
                notif.Leida = true;
                notif.FechaLectura = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }

        public async Task MarcarTodasLeidasAsync(string usuarioId, CancellationToken ct)
        {
            var noLeidas = await _db.NotificacionesUsuario
                .Where(n => n.UsuarioDestinoId == usuarioId && !n.Leida)
                .ToListAsync(ct);

            var ahora = DateTime.UtcNow;
            foreach (var n in noLeidas)
            {
                n.Leida = true;
                n.FechaLectura = ahora;
            }

            if (noLeidas.Count > 0)
                await _db.SaveChangesAsync(ct);
        }
    }
}
