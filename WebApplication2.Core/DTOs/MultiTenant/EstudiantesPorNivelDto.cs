namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class EstudiantesPorNivelDto
    {
        public string Nivel { get; set; } = null!;
        public int Total { get; set; }
        public decimal Porcentaje { get; set; }
    }
}
