namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class UsoPorTenantDto
    {
        public int IdTenant { get; set; }
        public string Codigo { get; set; } = null!;
        public string NombreCorto { get; set; } = null!;
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public int LoginsMes { get; set; }
    }
}
