using WebApplication2.Core.DTOs.Empresa;

namespace WebApplication2.Services.Interfaces
{
    public interface IEmpresaService
    {
        Task<IReadOnlyList<EmpresaDto>> ListarEmpresasAsync(bool? soloActivas = null, CancellationToken ct = default);
        Task<EmpresaDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task<EmpresaDto> CrearEmpresaAsync(CrearEmpresaDto dto, string usuarioCreador, CancellationToken ct = default);
        Task<EmpresaDto> ActualizarEmpresaAsync(int id, ActualizarEmpresaDto dto, string usuarioModificador, CancellationToken ct = default);
        Task<bool> EliminarEmpresaAsync(int id, CancellationToken ct = default);
        Task<bool> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default);
    }
}
