namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class PeriodoActualPanelDto
    {
        public int IdPeriodoAcademico { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Clave { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public bool EsActual { get; set; }
    }
}
