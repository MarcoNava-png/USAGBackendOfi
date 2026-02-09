namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class PlanLicenciaDto
    {
        public int IdPlanLicencia { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public decimal PrecioMensual { get; set; }
        public decimal? PrecioAnual { get; set; }
        public int MaxEstudiantes { get; set; }
        public int MaxUsuarios { get; set; }
        public int MaxCampus { get; set; }
        public bool IncluyeSoporte { get; set; }
        public bool IncluyeReportes { get; set; }
        public bool IncluyeAPI { get; set; }
        public bool IncluyeFacturacion { get; set; }
        public bool Activo { get; set; }
    }
}
