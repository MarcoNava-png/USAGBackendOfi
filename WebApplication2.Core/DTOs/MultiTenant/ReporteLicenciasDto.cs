namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class ReporteLicenciasDto
    {
        public int TotalTenants { get; set; }
        public int TenantsActivos { get; set; }
        public int TenantsPorVencer { get; set; }
        public int TenantsVencidos { get; set; }
        public decimal IngresosRecurrentesMensual { get; set; }
        public List<LicenciaPorVencerDto> ProximosVencimientos { get; set; } = new();
        public List<DistribucionPlanesDto> DistribucionPlanes { get; set; } = new();
    }
}
