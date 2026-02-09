namespace WebApplication2.Core.DTOs
{
    public class PeriodoAcademicoDto
    {
        public int IdPeriodoAcademico { get; set; }

        public string Clave { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public int IdPeriodicidad { get; set; }

        public string Periodicidad { get; set; }

        public DateOnly FechaInicio { get; set; }

        public DateOnly FechaFin { get; set; }

        public bool EsPeriodoActual { get; set; }
    }
}
