namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class ActividadRecienteDto
    {
        public int IdTenant { get; set; }
        public string TenantNombre { get; set; } = null!;
        public string Usuario { get; set; } = null!;
        public string Accion { get; set; } = null!;
        public DateTime Fecha { get; set; }
    }
}
