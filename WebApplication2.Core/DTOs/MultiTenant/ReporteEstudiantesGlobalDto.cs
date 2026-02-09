namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class ReporteEstudiantesGlobalDto
    {
        public int TotalEstudiantes { get; set; }
        public int EstudiantesActivos { get; set; }
        public int EstudiantesInactivos { get; set; }
        public int EstudiantesBaja { get; set; }
        public int NuevosEsteMes { get; set; }
        public int NuevosEsteAnio { get; set; }
        public List<EstudiantesPorTenantDto> EstudiantesPorTenant { get; set; } = new();
        public List<EstudiantesPorNivelDto> DistribucionNivel { get; set; } = new();
    }
}
