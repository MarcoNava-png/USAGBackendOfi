using WebApplication2.Core.DTOs.Dashboard;
using WebApplication2.Core.Responses.Dashboard;

namespace WebApplication2.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetDashboardAsync(string userId, string role);

        Task<AdminDashboardDto> GetAdminDashboardAsync();

        Task<DirectorDashboardDto> GetDirectorDashboardAsync();

        Task<FinanzasDashboardDto> GetFinanzasDashboardAsync();

        Task<ControlEscolarDashboardDto> GetControlEscolarDashboardAsync();

        Task<AdmisionesDashboardDto> GetAdmisionesDashboardAsync();

        Task<CoordinadorDashboardDto> GetCoordinadorDashboardAsync(string userId);

        Task<DocenteDashboardDto> GetDocenteDashboardAsync(string userId);

        Task<AlumnoDashboardDto> GetAlumnoDashboardAsync(string userId);
    }
}
