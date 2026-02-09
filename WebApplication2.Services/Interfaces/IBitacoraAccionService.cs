using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;

namespace WebApplication2.Services.Interfaces
{
    public interface IBitacoraAccionService
    {
        Task RegistrarAsync(string usuarioId, string nombreUsuario, string accion, string modulo,
            string entidad, string entidadId, string descripcion,
            string? datosAnteriores = null, string? datosNuevos = null, string? ip = null);

        Task<PagedResult<BitacoraAccionDto>> ConsultarAsync(BitacoraAccionFiltroDto filtro, CancellationToken ct);
    }
}
