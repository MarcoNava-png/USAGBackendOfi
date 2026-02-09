namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class PeriodoAcademicoDetalleDto
    {
        public int IdPeriodoAcademico { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public bool EsActual { get; set; }
        public decimal PromedioDelPeriodo { get; set; }
        public int CreditosDelPeriodo { get; set; }
        public List<MateriaDetalleDto> Materias { get; set; } = new();
        public EstadisticasPeriodoDto Estadisticas { get; set; } = new();
    }
}
