namespace WebApplication2.Core.Requests.PeriodoAcademico
{
    public class PeriodoAcademicoRequest
    {
        public string Clave { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public int IdPeriodicidad { get; set; }

        public DateOnly FechaInicio { get; set; }

        public DateOnly FechaFin { get; set; }

        public DateTime? FechaLimiteParcial1 { get; set; }
        public DateTime? FechaLimiteParcial2 { get; set; }
        public DateTime? FechaLimiteParcial3 { get; set; }
    }
}
