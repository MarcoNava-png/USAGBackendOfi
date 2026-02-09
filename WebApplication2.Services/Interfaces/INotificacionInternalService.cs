using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;

namespace WebApplication2.Services.Interfaces
{
    public interface INotificacionInternalService
    {
        Task CrearAsync(string usuarioId, string titulo, string mensaje, string tipo, string? modulo = null, string? urlAccion = null);

        Task<PagedResult<NotificacionUsuarioDto>> ObtenerAsync(string usuarioId, bool soloNoLeidas, int page, int pageSize, CancellationToken ct);

        Task<int> ContarNoLeidasAsync(string usuarioId, CancellationToken ct);

        Task MarcarLeidaAsync(long idNotificacion, string usuarioId, CancellationToken ct);

        Task MarcarTodasLeidasAsync(string usuarioId, CancellationToken ct);
    }
}
