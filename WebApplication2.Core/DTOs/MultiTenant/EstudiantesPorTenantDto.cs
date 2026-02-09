namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class EstudiantesPorTenantDto
    {
        public int IdTenant { get; set; }
        public string Codigo { get; set; } = null!;
        public string NombreCorto { get; set; } = null!;
        public string ColorPrimario { get; set; } = "#14356F";
        public int TotalEstudiantes { get; set; }
        public int Activos { get; set; }
        public int NuevosEsteMes { get; set; }
        public int CapacidadMaxima { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
    }
}
