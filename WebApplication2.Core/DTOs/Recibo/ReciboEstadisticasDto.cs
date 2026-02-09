namespace WebApplication2.Core.DTOs.Recibo
{
    public class ReciboEstadisticasDto
    {
        public int TotalRecibos { get; set; }
        public decimal SaldoPendiente { get; set; }
        public int RecibosVencidos { get; set; }
        public decimal RecargosAcumulados { get; set; }
        public int RecibosPendientes { get; set; }
        public int RecibosPagados { get; set; }
        public int RecibosParciales { get; set; }
        public decimal TotalCobrado { get; set; }
        public List<EstadisticasPorPeriodoDto> PorPeriodo { get; set; } = new List<EstadisticasPorPeriodoDto>();
    }
}
