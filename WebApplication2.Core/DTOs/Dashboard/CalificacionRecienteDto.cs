namespace WebApplication2.Core.DTOs.Dashboard
{
    public class CalificacionRecienteDto
    {
        public string Materia { get; set; } = string.Empty;
        public string TipoEvaluacion { get; set; } = string.Empty;
        public decimal Calificacion { get; set; }
        public DateTime Fecha { get; set; }
    }
}
