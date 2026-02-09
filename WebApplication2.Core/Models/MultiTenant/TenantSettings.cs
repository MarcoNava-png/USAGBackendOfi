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
    }
}
