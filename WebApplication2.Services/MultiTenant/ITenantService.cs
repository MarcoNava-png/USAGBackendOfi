using WebApplication2.Core.DTOs.MultiTenant;
using WebApplication2.Core.Models.MultiTenant;
using WebApplication2.Core.Requests.MultiTenant;
using WebApplication2.Core.Responses.MultiTenant;

namespace WebApplication2.Services.MultiTenant;

public interface ITenantService
{
    TenantContext? GetCurrentTenant();

    void SetCurrentTenant(TenantContext tenant);

    Task<Tenant?> GetTenantBySubdomainAsync(string subdomain, CancellationToken ct = default);

    Task<Tenant?> GetTenantByIdAsync(int idTenant, CancellationToken ct = default);

    Task<List<TenantListDto>> ListarTenantsAsync(CancellationToken ct = default);

    Task<TenantDetalleDto?> ObtenerTenantDetalleAsync(int idTenant, CancellationToken ct = default);

    Task<TenantCreadoResponse> CrearTenantAsync(CrearTenantRequest request, string creadoPor, CancellationToken ct = default);

    Task<bool> ActualizarTenantAsync(int idTenant, ActualizarTenantRequest request, CancellationToken ct = default);

    Task<bool> CambiarStatusTenantAsync(int idTenant, TenantStatus nuevoStatus, string? motivo, CancellationToken ct = default);

    Task<DashboardGlobalDto> ObtenerDashboardGlobalAsync(CancellationToken ct = default);

    Task<TenantStatsDto> ObtenerEstadisticasTenantAsync(int idTenant, CancellationToken ct = default);

    Task<List<PlanLicenciaDto>> ListarPlanesAsync(CancellationToken ct = default);

    Task<ImportarTenantsResultado> ImportarTenantsAsync(List<ImportarTenantFila> filas, string creadoPor, CancellationToken ct = default);

    byte[] GenerarPlantillaExcel();

    Task<ReporteIngresosGlobalDto> ObtenerReporteIngresosAsync(int? anio = null, CancellationToken ct = default);

    Task<ReporteEstudiantesGlobalDto> ObtenerReporteEstudiantesAsync(CancellationToken ct = default);

    Task<ReporteUsoSistemaDto> ObtenerReporteUsoSistemaAsync(CancellationToken ct = default);

    Task<ReporteLicenciasDto> ObtenerReporteLicenciasAsync(CancellationToken ct = default);
}
