using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public TicketSoporteService(
            ApplicationDbContext db,
            IBlobStorageService blobStorage,
            INotificacionInternalService notificaciones,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _db = db;
            _blobStorage = blobStorage;
            _notificaciones = notificaciones;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        private string UrlTicket(int idTicket)
        {
            var baseUrl = _configuration["FrontendUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl)) baseUrl = "https://saciusag.com.mx";
            return $"{baseUrl.TrimEnd('/')}/dashboard/tickets?ticketId={idTicket}";
        }

        private async Task EnviarCorreoTicketAsync(string? email, string asunto, string encabezado, string cuerpo, int idTicket)
        {
            if (string.IsNullOrWhiteSpace(email)) return;
            try
            {
                var html = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;color:#333'>
  <div style='background:#14356F;color:#fff;padding:20px 24px;border-radius:10px 10px 0 0'>
    <h2 style='margin:0;font-size:18px'>{encabezado}</h2>
  </div>
  <div style='background:#f8fafc;padding:24px;border:1px solid #e2e8f0;border-top:none'>
    {cuerpo}
    <p style='text-align:center;margin:28px 0 8px'>
      <a href='{UrlTicket(idTicket)}' style='display:inline-block;background:#14356F;color:#fff;padding:12px 26px;text-decoration:none;border-radius:8px;font-weight:bold'>Ver ticket</a>
    </p>
  </div>
  <div style='text-align:center;padding:16px;color:#94a3b8;font-size:12px'>Sistema Escolar USAG · Soporte</div>
</div>";
                await _emailService.SendEmailAsync(email, asunto, html);
            }
            catch
            {
            }
        }

        private async Task<string?> ObtenerEmailUsuarioAsync(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;
            var user = await _userManager.FindByIdAsync(userId);
            return user?.Email;
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
                AreaDestino = dto.AreaDestino,
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

            var destinatarios = new List<ApplicationUser>();

            if (!string.IsNullOrEmpty(dto.AreaDestino))
            {
                var rolName = dto.AreaDestino.ToLower().Replace(" ", "");
                var usersInRole = await _userManager.GetUsersInRoleAsync(rolName);
                destinatarios.AddRange(usersInRole);
            }

            var admins = await _userManager.GetUsersInRoleAsync("admin");
            foreach (var admin in admins)
            {
                if (!destinatarios.Any(d => d.Id == admin.Id))
                    destinatarios.Add(admin);
            }

            var areaTexto = !string.IsNullOrEmpty(dto.AreaDestino) ? $" para {dto.AreaDestino}" : "";
            var cuerpoCorreo = $@"
    <p>Se registró un nuevo ticket de soporte:</p>
    <p style='background:#fff;border:1px solid #e2e8f0;border-radius:8px;padding:14px'>
      <strong>Folio:</strong> {folio}<br/>
      <strong>Título:</strong> {System.Net.WebUtility.HtmlEncode(dto.Titulo)}<br/>
      <strong>Prioridad:</strong> {ticket.Prioridad}<br/>
      <strong>Creado por:</strong> {System.Net.WebUtility.HtmlEncode(nombreUsuario)}
    </p>
    <p>{System.Net.WebUtility.HtmlEncode(dto.Descripcion)}</p>";

            foreach (var dest in destinatarios)
            {
                await _notificaciones.CrearAsync(
                    dest.Id,
                    $"Nuevo ticket{areaTexto}",
                    $"{nombreUsuario} creó el ticket {folio}: {dto.Titulo}",
                    "ticket",
                    "Soporte",
                    $"/dashboard/tickets?ticketId={ticket.IdTicket}"
                );

                await EnviarCorreoTicketAsync(
                    dest.Email,
                    $"Nuevo ticket {folio}{areaTexto}",
                    $"Nuevo ticket de soporte{areaTexto}",
                    cuerpoCorreo,
                    ticket.IdTicket
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
            {
                var user = await _userManager.FindByIdAsync(userId);
                var userRoles = user != null ? await _userManager.GetRolesAsync(user) : (IList<string>)new List<string>();
                query = query.Where(t => t.UsuarioCreadorId == userId
                    || t.UsuarioAsignadoId == userId
                    || (t.AreaDestino != null && userRoles.Contains(t.AreaDestino)));
            }

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

            await ResolverNombresAsync(items);

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

            var dto = MapToDto(ticket);
            await ResolverNombresAsync(new List<TicketResponseDto> { dto });
            return dto;
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

            var cuerpoComentario = $@"
    <p><strong>{System.Net.WebUtility.HtmlEncode(nombreUsuario)}</strong> escribió en el ticket <strong>{ticket.Folio}</strong> ({System.Net.WebUtility.HtmlEncode(ticket.Titulo)}):</p>
    <p style='background:#fff;border:1px solid #e2e8f0;border-radius:8px;padding:14px'>{System.Net.WebUtility.HtmlEncode(comentario.Contenido)}</p>";

            // Notificar al otro usuario
            if (esAdmin)
            {
                await _notificaciones.CrearAsync(
                    ticket.UsuarioCreadorId,
                    "Respuesta en tu ticket",
                    $"Se ha respondido a tu ticket {ticket.Folio}: {ticket.Titulo}",
                    "ticket",
                    "Soporte",
                    $"/dashboard/tickets?ticketId={ticket.IdTicket}"
                );

                await EnviarCorreoTicketAsync(
                    await ObtenerEmailUsuarioAsync(ticket.UsuarioCreadorId),
                    $"Respuesta en tu ticket {ticket.Folio}",
                    "Respondieron a tu ticket",
                    cuerpoComentario,
                    ticket.IdTicket
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
                        $"/dashboard/tickets?ticketId={ticket.IdTicket}"
                    );

                    await EnviarCorreoTicketAsync(
                        admin.Email,
                        $"Nuevo comentario en ticket {ticket.Folio}",
                        "Nuevo comentario en un ticket",
                        cuerpoComentario,
                        ticket.IdTicket
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
                $"/dashboard/tickets?ticketId={ticket.IdTicket}"
            );

            await EnviarCorreoTicketAsync(
                await ObtenerEmailUsuarioAsync(ticket.UsuarioCreadorId),
                $"Tu ticket {ticket.Folio} ahora está {estatusNombre}",
                $"Ticket {estatusNombre}",
                $"<p>Tu ticket <strong>{ticket.Folio}</strong> ({System.Net.WebUtility.HtmlEncode(ticket.Titulo)}) ha sido marcado como <strong>{estatusNombre}</strong>.</p>",
                ticket.IdTicket
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
                TotalEnValidacion = tickets.FirstOrDefault(t => t.Estatus == TicketEstatusEnum.EnValidacion)?.Count ?? 0,
                Total = tickets.Sum(t => t.Count)
            };
        }

        private async Task<string> GenerarFolioAsync()
        {
            var año = DateTime.UtcNow.Year;
            var prefijo = $"TK-{año}-";

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

            return $"{prefijo}{siguiente:D6}";
        }

        private async Task ResolverNombresAsync(List<TicketResponseDto> items)
        {
            var guids = new HashSet<string>();

            foreach (var item in items)
            {
                if (Guid.TryParse(item.NombreCreador, out _))
                    guids.Add(item.NombreCreador);
                if (!string.IsNullOrEmpty(item.NombreAsignado) && Guid.TryParse(item.NombreAsignado, out _))
                    guids.Add(item.NombreAsignado);
                if (!string.IsNullOrEmpty(item.UsuarioCreadorId))
                    guids.Add(item.UsuarioCreadorId);
                if (!string.IsNullOrEmpty(item.UsuarioAsignadoId))
                    guids.Add(item.UsuarioAsignadoId);
                foreach (var c in item.Comentarios)
                {
                    if (Guid.TryParse(c.NombreUsuario, out _))
                        guids.Add(c.NombreUsuario);
                    if (!string.IsNullOrEmpty(c.UsuarioId))
                        guids.Add(c.UsuarioId);
                }
            }

            if (guids.Count == 0) return;

            var usuarios = await _db.Users
                .AsNoTracking()
                .Where(u => guids.Contains(u.Id))
                .Select(u => new { u.Id, Nombre = (u.Nombres + " " + u.Apellidos).Trim() })
                .ToDictionaryAsync(u => u.Id, u => u.Nombre);

            foreach (var item in items)
            {
                if (Guid.TryParse(item.NombreCreador, out _) && usuarios.TryGetValue(item.NombreCreador, out var nc))
                    item.NombreCreador = nc;

                if (string.IsNullOrEmpty(item.NombreAsignado) && !string.IsNullOrEmpty(item.UsuarioAsignadoId)
                    && usuarios.TryGetValue(item.UsuarioAsignadoId, out var na1))
                    item.NombreAsignado = na1;
                else if (!string.IsNullOrEmpty(item.NombreAsignado) && Guid.TryParse(item.NombreAsignado, out _)
                    && usuarios.TryGetValue(item.NombreAsignado, out var na2))
                    item.NombreAsignado = na2;

                if (string.IsNullOrEmpty(item.NombreCreador) && !string.IsNullOrEmpty(item.UsuarioCreadorId)
                    && usuarios.TryGetValue(item.UsuarioCreadorId, out var nc2))
                    item.NombreCreador = nc2;

                foreach (var c in item.Comentarios)
                {
                    if (Guid.TryParse(c.NombreUsuario, out _) && usuarios.TryGetValue(c.NombreUsuario, out var nu))
                        c.NombreUsuario = nu;
                    else if (string.IsNullOrEmpty(c.NombreUsuario) && !string.IsNullOrEmpty(c.UsuarioId)
                        && usuarios.TryGetValue(c.UsuarioId, out var nu2))
                        c.NombreUsuario = nu2;
                }
            }
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
                AreaDestino = t.AreaDestino,
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
