namespace WebApplication2.Core.Requests.MultiTenant
{
    public class ActualizarTenantRequest
    {
        public string? Nombre { get; set; }
        public string? NombreCorto { get; set; }
        public string? DominioPersonalizado { get; set; }
        public string? LogoUrl { get; set; }
        public string? ColorPrimario { get; set; }
        public string? ColorSecundario { get; set; }
        public string? EmailContacto { get; set; }
        public string? TelefonoContacto { get; set; }
        public string? DireccionFiscal { get; set; }
        public string? RFC { get; set; }
        public int? IdPlanLicencia { get; set; }
        public DateTime? FechaVencimiento { get; set; }
    }
}
