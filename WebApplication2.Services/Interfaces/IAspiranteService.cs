using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Admision;
using WebApplication2.Core.DTOs.Inscripcion;
using WebApplication2.Core.DTOs.PlantillaCobro;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Aspirante;

namespace WebApplication2.Services.Interfaces
{
    public interface IAspiranteService
    {
        Task<PagedResult<Aspirante>> GetAspirantes(int page, int pageSize, string filter);
        Task<Aspirante> GetAspiranteByPersonaId(int id);
        Task<Aspirante> CrearAspirante(Aspirante aspirante);
        Task<Aspirante> ActualizarAspirante(Aspirante aspirante);
        Task<IEnumerable<AspiranteBitacoraSeguimiento>> GetBitacoraSeguimiento(int aspiranteId);
        Task<AspiranteBitacoraSeguimiento> CrearSeguimiento(AspiranteBitacoraSeguimiento seguimiento);
        Task<Aspirante> GetAspiranteById(int id);
        Task<bool> CancelarAspiranteAsync(int idAspirante, string motivo);
        Task<EstadisticasAspirantesDto> ObtenerEstadisticasAsync(int? periodoId);
        Task<FichaAdmisionDto?> ObtenerFichaCompleta(int aspiranteId, string? usuarioGeneraId = null);
        Task<InscripcionAspiranteResultDto> InscribirAspiranteComoEstudianteAsync(int aspiranteId, InscribirAspiranteRequest request, string? usuarioProcesa = null);
        Task<AspiranteEstatus?> ObtenerEstatusEnProcesoAsync();
        Task<PlantillaCobroDto?> BuscarPlantillaParaAspiranteAsync(int idAspirante, CancellationToken ct);
        Task<IReadOnlyList<ReciboDto>> GenerarRecibosDesdeePlantillaParaAspiranteAsync(int idAspirante, int idPlantillaCobro, bool eliminarPendientes, CancellationToken ct);
        Task<bool> OcultarAspiranteAsync(int idAspirante, string usuarioId);
        Task<ComisionReporteDto> CalcularComisionesAsync(DateTime fechaDesde, DateTime fechaHasta, decimal comisionPorRegistro, decimal porcentajePorPago, string? filtrarPorUsuarioId = null);
    }
}
