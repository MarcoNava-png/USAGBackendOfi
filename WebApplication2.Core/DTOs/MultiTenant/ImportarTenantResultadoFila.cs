namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class ImportarTenantResultadoFila
    {
        public int Fila { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = null!;
        public string? Url { get; set; }
        public string? AdminEmail { get; set; }
        public string? PasswordTemporal { get; set; }
    }
}
