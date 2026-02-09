namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class ResumenEjecutivoDto
    {
        public int TotalEscuelas { get; set; }
        public int EscuelasActivas { get; set; }
        public int TotalEstudiantes { get; set; }
        public int EstudiantesActivos { get; set; }
        public decimal IngresosMesActual { get; set; }
        public decimal IngresosAnioActual { get; set; }
        public decimal AdeudoTotal { get; set; }
        public decimal IngresosRecurrentes { get; set; }
        public int LicenciasPorVencer { get; set; }
        public List<IngresosPorTenantDto> TopEscuelasIngresos { get; set; } = new();
        public List<EstudiantesPorTenantDto> TopEscuelasEstudiantes { get; set; } = new();
    }
}
