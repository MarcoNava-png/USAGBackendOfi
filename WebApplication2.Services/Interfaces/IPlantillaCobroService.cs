using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.PlantillaCobro;
using WebApplication2.Core.Requests.Recibo;
using WebApplication2.Core.Responses.Recibo;
using WebApplication2.Core.Requests.PlantillaCobro;
using WebApplication2.Core.Responses.PlantillaCobro;

namespace WebApplication2.Services.Interfaces
{
    public interface IPlantillaCobroService
    {
        Task<IReadOnlyList<PlantillaCobroDto>> ListarPlantillasAsync(
            int? idPlanEstudios = null,
            int? numeroCuatrimestre = null,
            bool? soloActivas = null,
            int? idPeriodoAcademico = null,
            CancellationToken ct = default);

        Task<PlantillaCobroDto?> ObtenerPlantillaPorIdAsync(int id, CancellationToken ct = default);

        Task<PlantillaCobroDto?> BuscarPlantillaActivaAsync(
            int idPlanEstudios,
            int numeroCuatrimestre,
            int? idPeriodoAcademico = null,
            int? idTurno = null,
            int? idModalidad = null,
            CancellationToken ct = default);

        Task<PlantillaCobroDto> CrearPlantillaAsync(
            CreatePlantillaCobroDto dto,
            string usuarioCreador,
            CancellationToken ct = default);

        Task<PlantillaCobroDto> ActualizarPlantillaAsync(
            int id,
            UpdatePlantillaCobroDto dto,
            string usuarioModificador,
            CancellationToken ct = default);

        Task CambiarEstadoPlantillaAsync(int id, bool esActiva, CancellationToken ct = default);

        Task EliminarPlantillaAsync(int id, CancellationToken ct = default);

        Task<PlantillaCobroDto> DuplicarPlantillaAsync(
            int id,
            CreatePlantillaCobroDto? cambios,
            string usuarioCreador,
            CancellationToken ct = default);

        Task<IReadOnlyList<int>> ObtenerCuatrimestresPorPlanAsync(int idPlanEstudios, CancellationToken ct = default);

        Task<GenerarRecibosMasivosResult> GenerarRecibosMasivosAsync(
            GenerarRecibosMasivosRequest request,
            string usuarioCreador,
            CancellationToken ct = default);

        PreviewRecibosResponse GenerarPreviewRecibos(GenerarPreviewRecibosRequest request);
    }
}
