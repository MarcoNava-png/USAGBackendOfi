namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class TenantStatsDto
    {
        public int TotalEstudiantes { get; set; }
        public int EstudiantesActivos { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalProfesores { get; set; }
        public int TotalRecibos { get; set; }
        public decimal IngresosMes { get; set; }
        public decimal AdeudoTotal { get; set; }
        public int AspirantesActivos { get; set; }
    }
}
