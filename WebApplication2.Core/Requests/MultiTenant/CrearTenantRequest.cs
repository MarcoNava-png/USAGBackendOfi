namespace WebApplication2.Core.Requests.MultiTenant
{
    public class CrearTenantRequest
    {
        public string Codigo { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public string NombreCorto { get; set; } = null!;

        public string Subdominio { get; set; } = null!;

        public int IdPlanLicencia { get; set; }

        public string? LogoUrl { get; set; }
        public string ColorPrimario { get; set; } = "#14356F";
        public string? ColorSecundario { get; set; }

        public string? EmailContacto { get; set; }
        public string? TelefonoContacto { get; set; }
        public string? DireccionFiscal { get; set; }
        public string? RFC { get; set; }

        public string AdminEmail { get; set; } = null!;
        public string AdminNombre { get; set; } = null!;
        public string AdminPassword { get; set; } = null!;
    }
}
