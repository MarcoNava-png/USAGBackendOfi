namespace WebApplication2.Core.DTOs.Comprobante
{
    public class ReciboComprobanteInfo
    {
        public long IdRecibo { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Concepto { get; set; } = string.Empty;
        public string? Periodo { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Recargos { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal SaldoNuevo { get; set; }
        public string Estatus { get; set; } = string.Empty;
    }
}
