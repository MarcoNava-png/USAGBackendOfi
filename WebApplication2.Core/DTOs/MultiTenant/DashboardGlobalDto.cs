namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class DashboardGlobalDto
    {
        public int TotalTenants { get; set; }
        public int TenantsActivos { get; set; }
        public int TenantsPendientes { get; set; }
        public int TenantsSuspendidos { get; set; }
        public int TotalEstudiantesGlobal { get; set; }
        public int TotalUsuariosGlobal { get; set; }
        public decimal IngresosMesGlobal { get; set; }
        public List<TenantListDto> UltimosTenantsCreados { get; set; } = new();
        public List<TenantListDto> TenantsConProblemas { get; set; } = new();
    }
}
