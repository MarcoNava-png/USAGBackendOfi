namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class ReporteUsoSistemaDto
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int LoginsMes { get; set; }
        public int LoginsHoy { get; set; }
        public List<UsoPorTenantDto> UsoPorTenant { get; set; } = new();
        public List<ActividadRecienteDto> ActividadReciente { get; set; } = new();
    }
}
