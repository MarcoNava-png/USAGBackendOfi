using Microsoft.AspNetCore.Http;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs.Ticket;
using WebApplication2.Core.Enums;

namespace WebApplication2.Services.Interfaces
{
    public interface ITicketSoporteService
    {
        Task<TicketResponseDto> CrearAsync(CrearTicketDto dto, IFormFile? archivo, string userId, string nombreUsuario, bool esAdmin);
        Task<PagedResult<TicketResponseDto>> ListarAsync(TicketFiltroDto filtro, string userId, bool esAdmin);
        Task<TicketResponseDto> ObtenerPorIdAsync(int id, string userId, bool esAdmin);
        Task<TicketResponseDto> ActualizarAsync(int id, ActualizarTicketDto dto, string userId, bool esAdmin);
        Task<TicketComentarioDto> AgregarComentarioAsync(int id, CrearComentarioTicketDto dto, IFormFile? archivo, string userId, string nombreUsuario, bool esAdmin);
        Task CambiarEstatusAsync(int id, TicketEstatusEnum nuevoEstatus, string userId);
        Task AsignarAsync(int id, string usuarioAsignadoId, string nombreAsignado);
        Task<TicketEstadisticasDto> ObtenerEstadisticasAsync(string userId, bool esAdmin);
    }
}
