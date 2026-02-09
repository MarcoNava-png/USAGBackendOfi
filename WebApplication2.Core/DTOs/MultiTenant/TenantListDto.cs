namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class TenantListDto
    {
        public int IdTenant { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string NombreCorto { get; set; } = null!;
        public string Subdominio { get; set; } = null!;
        public string Url => $"https://{Subdominio}.saciusag.com.mx";
        public string? LogoUrl { get; set; }
        public string ColorPrimario { get; set; } = "#14356F";
        public string Status { get; set; } = null!;
        public string Plan { get; set; } = null!;
        public DateTime FechaContratacion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public DateTime? LastAccessAt { get; set; }
        public TenantStatsDto? Estadisticas { get; set; }
    }
}
