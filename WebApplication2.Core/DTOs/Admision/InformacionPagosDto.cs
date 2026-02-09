namespace WebApplication2.Core.DTOs.Admision
{
    public class InformacionPagosDto
    {
        public decimal TotalAPagar { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public List<ReciboResumenDto> Recibos { get; set; } = new();
    }
}
