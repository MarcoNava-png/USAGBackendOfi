namespace WebApplication2.Core.Models.MultiTenant
{
    public class TenantSettings
    {
        public string? LogoUrl { get; set; }
        public string ColorPrimario { get; set; } = "#14356F";
        public string? ColorSecundario { get; set; }
        public string Timezone { get; set; } = "America/Mexico_City";
        public int MaxEstudiantes { get; set; }
        public int MaxUsuarios { get; set; }
        public int MaxCampus { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? RFC { get; set; }
        public bool IncluyeReportes { get; set; }
        public bool IncluyeApi { get; set; }
        public bool IncluyeFacturacion { get; set; }
        public bool IncluyeSoporte { get; set; }
    }
}
