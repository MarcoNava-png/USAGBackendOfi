namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class MateriaResumenDto
    {
        public string ClaveMateria { get; set; } = string.Empty;
        public string NombreMateria { get; set; } = string.Empty;
        public decimal? CalificacionFinal { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public string? Periodo { get; set; }
    }
}
