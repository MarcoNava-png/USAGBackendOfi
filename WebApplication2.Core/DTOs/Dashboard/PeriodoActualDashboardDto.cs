namespace WebApplication2.Core.DTOs.Dashboard
{
    public class PeriodoActualDashboardDto
    {
        public int IdPeriodo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int DiasRestantes { get; set; }
        public bool EsActivo { get; set; }
    }
}
