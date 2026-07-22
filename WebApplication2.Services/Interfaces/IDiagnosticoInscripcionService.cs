using WebApplication2.Core.DTOs.Diagnostico;

namespace WebApplication2.Services.Interfaces
{
    public interface IDiagnosticoInscripcionService
    {
        Task<List<InconsistenciaInscripcionDto>> DetectarAsync(int idPeriodoAcademico);
        Task<RepararInscripcionResultDto> RepararAsync(RepararInscripcionRequest req);
    }
}
