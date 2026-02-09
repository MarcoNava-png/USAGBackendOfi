namespace WebApplication2.Core.DTOs.Catalogos
{
    public class PeriodoAcademicoCatalogoDto
    {
        public int IdPeriodoAcademico { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Periodicidad { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public bool EsPeriodoActual { get; set; }
    }
}
