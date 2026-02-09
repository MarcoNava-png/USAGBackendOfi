namespace WebApplication2.Core.DTOs.Recibo
{
    public class EstadisticasPorPeriodoDto
    {
        public int IdPeriodoAcademico { get; set; }
        public string? NombrePeriodo { get; set; }
        public int TotalRecibos { get; set; }
        public decimal SaldoPendiente { get; set; }
        public int RecibosVencidos { get; set; }
    }
}
