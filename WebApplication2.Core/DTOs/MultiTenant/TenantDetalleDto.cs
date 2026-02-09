using WebApplication2.Core.Models.MultiTenant;

namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class TenantDetalleDto
    {
        public int IdTenant { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string NombreCorto { get; set; } = null!;
        public string Subdominio { get; set; } = null!;
        public string? DominioPersonalizado { get; set; }
        public string Url => $"https://{Subdominio}.saciusag.com.mx";

        public string? LogoUrl { get; set; }
        public string ColorPrimario { get; set; } = "#14356F";
        public string? ColorSecundario { get; set; }
        public string Timezone { get; set; } = "America/Mexico_City";

        public string? EmailContacto { get; set; }
        public string? TelefonoContacto { get; set; }
        public string? DireccionFiscal { get; set; }
        public string? RFC { get; set; }

        public int IdPlanLicencia { get; set; }
        public string NombrePlan { get; set; } = null!;
        public int MaximoEstudiantes { get; set; }
        public int MaximoUsuarios { get; set; }
        public DateTime FechaContratacion { get; set; }
        public DateTime? FechaVencimiento { get; set; }

        public TenantStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastAccessAt { get; set; }

        public TenantStatsDto? Estadisticas { get; set; }
    }
}
