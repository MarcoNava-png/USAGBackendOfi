namespace WebApplication2.Core.DTOs.Recibo
{
    public class RecalcularDescuentosResultDto
    {
        public int RecibosActualizados { get; set; }
        public decimal DescuentoTotalAplicado { get; set; }
        public List<ReciboDescuentoResumenDto> Detalle { get; set; } = new();
    }

    public class ReciboDescuentoResumenDto
    {
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public decimal SubtotalOriginal { get; set; }
        public decimal DescuentoAnterior { get; set; }
        public decimal DescuentoNuevo { get; set; }
        public decimal SaldoNuevo { get; set; }
    }
}
