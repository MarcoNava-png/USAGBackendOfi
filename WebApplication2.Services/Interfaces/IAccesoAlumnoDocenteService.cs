using WebApplication2.Core.DTOs.AccesoAlumnoDocente;

namespace WebApplication2.Services.Interfaces;

public interface IAccesoAlumnoDocenteService
{
    Task<AccesosListaDto> ListarAsync(string tipo, string? busqueda, int pagina, int tamanoPagina, CancellationToken ct = default);
    Task<ResetearPasswordResponse> ResetearPasswordAsync(ResetearPasswordRequest request, CancellationToken ct = default);
    Task<bool> DesbloquearCuentaAsync(string userId, CancellationToken ct = default);
    Task<ResetearPasswordResponse> CrearAccesoAsync(CrearAccesoRequest request, CancellationToken ct = default);
}
