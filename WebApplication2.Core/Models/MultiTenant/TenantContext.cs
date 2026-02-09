namespace WebApplication2.Core.Models.MultiTenant
{
    public class TenantContext
    {
        public int IdTenant { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Subdominio { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
        public TenantStatus Status { get; set; }
        public TenantSettings Settings { get; set; } = new();
    }
}
