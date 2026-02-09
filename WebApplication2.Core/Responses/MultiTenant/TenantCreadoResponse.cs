namespace WebApplication2.Core.Responses.MultiTenant
{
    public class TenantCreadoResponse
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = null!;
        public int? IdTenant { get; set; }
        public string? Codigo { get; set; }
        public string? Url { get; set; }
        public string? AdminEmail { get; set; }
        public string? PasswordTemporal { get; set; }
    }
}
