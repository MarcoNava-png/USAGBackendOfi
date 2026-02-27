using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs.Ticket;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;
using TicketEstatusEnum = WebApplication2.Core.Enums.TicketEstatusEnum;
using TicketPrioridadEnum = WebApplication2.Core.Enums.TicketPrioridadEnum;
using TicketCategoriaEnum = WebApplication2.Core.Enums.TicketCategoriaEnum;

namespace WebApplication2.Services
{
    public class TicketSoporteService : ITicketSoporteService
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobStorageService _blobStorage;
        private readonly INotificacionInternalService _notificaciones;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketSoporteService(
            ApplicationDbContext db,
            IBlobStorageService blobStorage,
            INotificacionInternalService notificaciones,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _blobStorage = blobStorage;
            _notificaciones = notificaciones;
            _userManager = userManager;
        }

        public async Task<TicketResponseDto> CrearAsync(CrearTicketDto dto, IFormFile? archivo, string userId, string nombreUsuario, bool esAdmin)
        {
            var folio = await GenerarFolioAsync();

            var ticket = new TicketSoporte
            {
                Folio = folio,
                Titulo = dto.Titulo.Trim(),
                Descripcion = dto.Descripcion.Trim(),
                Prioridad = dto.Prioridad,
                Estatus = TicketEstatusEnum.Abierto,
                Categoria = dto.Categoria,
                UsuarioCreadorId = userId,
                NombreCreador = nombreUsuario
            };

            if (archivo != null)
            {
                var blobName = $"{folio}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{Path.GetExtension(archivo.FileName)}";
                var url = await _blobStorage.UploadFile(archivo, blobName, "tickets");
                ticket.ArchivoAdjuntoUrl = url;
                ticket.ArchivoAdjuntoNombre = archivo.FileName;
            }

            _db.TicketsSoporte.Add(ticket);
            await _db.SaveChangesAsync();

            // Notificar a admins
            var admins = await _userManager.GetUsersInRoleAsync("admin");
            foreach (var admin in admins)
            {
                await _notificaciones.CrearAsync(
                    admin.Id,
                    "Nuevo ticket de soporte",
                    $"{nombreUsuario} creó el ticket {folio}: {dto.Titulo}",
                    "ticket",
                    "Soporte",
                    $"/dashboard/tickets"
                );
            }

            return MapToDto(ticket);
        }

        public async Task<PagedResult<TicketResponseDto>> ListarAsync(TicketFiltroDto filtro, string userId, bool esAdmin)
        {
            if (filtro.Page <= 0) filtro.Page = 1;
            if (filtro.PageSize <= 0) filtro.PageSize = 20;
            if (filtro.PageSize > 100) filtro.PageSize = 100;

            var query = _db.TicketsSoporte
                .AsNoTracking()
                .Where(t => t.Status == StatusEnum.Active);

            if (!esAdmin)
                query = query.Where(t => t.UsuarioCreadorId == userId);

            if (filtro.Estatus.HasValue)
                query = query.Where(t => t.Estatus == filtro.Estatus.Value);

            if (filtro.Prioridad.HasValue)
                query = query.Where(t => t.Prioridad == filtro.Prioridad.Value);

            if (filtro.Categoria.HasValue)
                query = query.Where(t => t.Categoria == filtro.Categoria.Value);

            if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            {
                var busqueda = filtro.Busqueda.Trim().ToLower();
                query = query.Where(t =>
                    t.Folio.ToLower().Contains(busqueda) ||
                    t.Titulo.ToLower().Contains(busqueda) ||
                    t.NombreCreador.ToLower().Contains(busqueda));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((filtro.Page - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .Select(t => MapToDto(t))
                .ToListAsync();

            return new PagedResult<TicketResponseDto>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = filtro.Page,
                PageSize = filtro.PageSize
            };
        }

        public async Task<TicketResponseDto> ObtenerPorIdAsync(int id, string userId, bool esAdmin)
        {
            var ticket = await _db.TicketsSoporte
                .AsNoTracking()
                .Include(t => t.Comentarios.Where(c => c.Status == StatusEnum.Active).OrderBy(c => c.CreatedAt))
                .FirstOrDefaultAsync(t => t.IdTicket == id && t.Status == StatusEnum.Active)
                ?? throw new Exception("Ticket no encontrado.");

            if (!esAdmin && ticket.UsuarioCreadorId != userId)
                throw new Exception("No tiene permiso para ver este ticket.");

            return MapToDto(ticket);
        }

        public async Task<TicketResponseDto> ActualizarAsync(int id, ActualizarTicketDto dto, string userId, bool esAdmin)
        {
            var ticket = await _db.TicketsSoporte
                .FirstOrDefaultAsync(t => t.IdTicket == id && t.Status == StatusEnum.Active)
                ?? throw new Exception("Ticket no encontrado.");

            if (!esAdmin && ticket.UsuarioCreadorId != userId)
                throw new Exception("No tiene permiso para editar este ticket.");

            if (dto.Titulo != null) ticket.Titulo = dto.Titulo.Trim();
            if (dto.Descripcion != null) ticket.Descripcion = dto.Descripcion.Trim();
            if (dto.Prioridad.HasValue) ticket.Prioridad = dto.Prioridad.Value;
            if (dto.Categoria.HasValue) ticket.Categoria = dto.Categoria.Value;

            await _db.SaveChangesAsync();
            return MapToDto(ticket);
        }

        public async Task<TicketComentarioDto> AgregarComentarioAsync(int id, CrearComentarioTicketDto dto, IFormFile? archivo, string userId, string nombreUsuario, bool esAdmin)
        {
            var ticket = await _db.TicketsSoporte
                .FirstOrDefaultAsync(t => t.IdTicket == id && t.Status == StatusEnum.Active)
                ?? throw new Exception("Ticket no encontrado.");

            if (!esAdmin && ticket.UsuarioCreadorId != userId)
                throw new Exception("No tiene permiso para comentar en este ticket.");

            var comentario = new TicketComentario
            {
                IdTicket = id,
                UsuarioId = userId,
                NombreUsuario = nombreUsuario,
                Contenido = dto.Contenido.Trim(),
                EsAdmin = esAdmin
            };

            if (archivo != null)
            {
                var blobName = $"{ticket.Folio}_comment_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{Path.GetExtension(archivo.FileName)}";
                var url = await _blobStorage.UploadFile(archivo, blobName, "tickets");
                comentario.ArchivoAdjuntoUrl = url;
                comentario.ArchivoAdjuntoNombre = archivo.FileName;
            }

            _db.TicketComentarios.Add(comentario);

            // Si es admin respondiendo, cambiar estatus a EnProgreso si estaba Abierto
            if (esAdmin && ticket.Estatus == TicketEstatusEnum.Abierto)
                ticket.Estatus = TicketEstatusEnum.EnProgreso;

            await _db.SaveChangesAsync();

            // Notificar al otro usuario
            if (esAdmin)
            {
                await _notificaciones.CrearAsync(
                    ticket.UsuarioCreadorId,
                    "Respuesta en tu ticket",
                    $"Se ha respondido a tu ticket {ticket.Folio}: {ticket.Titulo}",
                    "ticket",
                    "Soporte",
                    $"/dashboard/tickets"
                );
            }
            else
            {
                var admins = await _userManager.GetUsersInRoleAsync("admin");
                foreach (var admin in admins)
                {
                    await _notificaciones.CrearAsync(
                        admin.Id,
                        "Nuevo comentario en ticket",
                        $"{nombreUsuario} comentó en el ticket {ticket.Folio}",
                        "ticket",
                        "Soporte",
                        $"/dashboard/tickets"
                    );
                }
            }

            return new TicketComentarioDto
            {
                IdComentario = comentario.IdComentario,
                UsuarioId = comentario.UsuarioId,
                NombreUsuario = comentario.NombreUsuario,
                Contenido = comentario.Contenido,
                EsAdmin = comentario.EsAdmin,
                ArchivoAdjuntoUrl = comentario.ArchivoAdjuntoUrl,
                ArchivoAdjuntoNombre = comentario.ArchivoAdjuntoNombre,
                CreatedAt = comentario.CreatedAt
            };
        }

        public async Task CambiarEstatusAsync(int id, TicketEstatusEnum nuevoEstatus, string userId)
        {
            var ticket = await _db.TicketsSoporte
                .FirstOrDefaultAsync(t => t.IdTicket == id && t.Status == StatusEnum.Active)
                ?? throw new Exception("Ticket no encontrado.");

            ticket.Estatus = nuevoEstatus;

            if (nuevoEstatus == TicketEstatusEnum.Cerrado || nuevoEstatus == TicketEstatusEnum.Resuelto)
                ticket.FechaCierre = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Notificar al creador
            var estatusNombre = nuevoEstatus.ToString();
            await _notificaciones.CrearAsync(
                ticket.UsuarioCreadorId,
                $"Ticket {estatusNombre}",
                $"Tu ticket {ticket.Folio} ha sido marcado como {estatusNombre}.",
                "ticket",
                "Soporte",
                $"/dashboard/tickets"
            );
        }

        public async Task AsignarAsync(int id, string usuarioAsignadoId, string nombreAsignado)
        {
            var ticket = await _db.TicketsSoporte
                .FirstOrDefaultAsync(t => t.IdTicket == id && t.Status == StatusEnum.Active)
                ?? throw new Exception("Ticket no encontrado.");

            ticket.UsuarioAsignadoId = usuarioAsignadoId;
            ticket.NombreAsignado = nombreAsignado;

            if (ticket.Estatus == TicketEstatusEnum.Abierto)
                ticket.Estatus = TicketEstatusEnum.EnProgreso;

            await _db.SaveChangesAsync();
        }

        public async Task<TicketEstadisticasDto> ObtenerEstadisticasAsync(string userId, bool esAdmin)
        {
            var query = _db.TicketsSoporte
                .AsNoTracking()
                .Where(t => t.Status == StatusEnum.Active);

            if (!esAdmin)
                query = query.Where(t => t.UsuarioCreadorId == userId);

            var tickets = await query
                .GroupBy(t => t.Estatus)
                .Select(g => new { Estatus = g.Key, Count = g.Count() })
                .ToListAsync();

            return new TicketEstadisticasDto
            {
                TotalAbiertos = tickets.FirstOrDefault(t => t.Estatus == TicketEstatusEnum.Abierto)?.Count ?? 0,
                TotalEnProgreso = tickets.FirstOrDefault(t => t.Estatus == TicketEstatusEnum.EnProgreso)?.Count ?? 0,
                TotalResueltos = tickets.FirstOrDefault(t => t.Estatus == TicketEstatusEnum.Resuelto)?.Count ?? 0,
                TotalCerrados = tickets.FirstOrDefault(t => t.Estatus == TicketEstatusEnum.Cerrado)?.Count ?? 0,
                Total = tickets.Sum(t => t.Count)
            };
        }

        private async Task<string> GenerarFolioAsync()
        {
            var hoy = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefijo = $"TK-{hoy}-";

            var ultimoFolio = await _db.TicketsSoporte
                .AsNoTracking()
                .Where(t => t.Folio.StartsWith(prefijo))
                .OrderByDescending(t => t.Folio)
                .Select(t => t.Folio)
                .FirstOrDefaultAsync();

            var siguiente = 1;
            if (ultimoFolio != null)
            {
                var partes = ultimoFolio.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out var num))
                    siguiente = num + 1;
            }

            return $"{prefijo}{siguiente:D4}";
        }

        private static TicketResponseDto MapToDto(TicketSoporte t)
        {
            return new TicketResponseDto
            {
                IdTicket = t.IdTicket,
                Folio = t.Folio,
                Titulo = t.Titulo,
                Descripcion = t.Descripcion,
                Prioridad = t.Prioridad,
                PrioridadNombre = t.Prioridad.ToString(),
                Estatus = t.Estatus,
                EstatusNombre = t.Estatus.ToString(),
                Categoria = t.Categoria,
                CategoriaNombre = t.Categoria.ToString(),
                UsuarioCreadorId = t.UsuarioCreadorId,
                NombreCreador = t.NombreCreador,
                UsuarioAsignadoId = t.UsuarioAsignadoId,
                NombreAsignado = t.NombreAsignado,
                ArchivoAdjuntoUrl = t.ArchivoAdjuntoUrl,
                ArchivoAdjuntoNombre = t.ArchivoAdjuntoNombre,
                FechaCierre = t.FechaCierre,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Comentarios = t.Comentarios?.Select(c => new TicketComentarioDto
                {
                    IdComentario = c.IdComentario,
                    UsuarioId = c.UsuarioId,
                    NombreUsuario = c.NombreUsuario,
                    Contenido = c.Contenido,
                    EsAdmin = c.EsAdmin,
                    ArchivoAdjuntoUrl = c.ArchivoAdjuntoUrl,
                    ArchivoAdjuntoNombre = c.ArchivoAdjuntoNombre,
                    CreatedAt = c.CreatedAt
                }).ToList() ?? new()
            };
        }
    }
}
