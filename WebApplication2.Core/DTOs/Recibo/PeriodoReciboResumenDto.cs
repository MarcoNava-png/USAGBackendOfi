namespace WebApplication2.Core.DTOs.Recibo
{
    public class PeriodoReciboResumenDto
    {
        public int IdPeriodoAcademico { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Clave { get; set; }
        public int? Anio { get; set; }
        public int TotalRecibos { get; set; }
    }

    public class PeriodosConRecibosDto
    {
        public List<PeriodoReciboResumenDto> Periodos { get; set; } = new();
        public int SinPeriodo { get; set; }
    }
}
