namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class ImportarTenantFila
    {
        public int Fila { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string NombreCorto { get; set; } = null!;
        public string Subdominio { get; set; } = null!;
        public string? ColorPrimario { get; set; }
        public string? EmailContacto { get; set; }
        public string? TelefonoContacto { get; set; }
        public int IdPlanLicencia { get; set; }
        public string AdminEmail { get; set; } = null!;
        public string AdminNombre { get; set; } = null!;
    }
}
