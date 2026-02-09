namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class TenantPublicInfoDto
    {
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string NombreCorto { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string ColorPrimario { get; set; } = "#14356F";
        public string? ColorSecundario { get; set; }
    }
}
