namespace WebApplication2.Core.DTOs.Admision
{
    public class ReciboResumenDto
    {
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Recargos { get; set; }
        public decimal Total { get; set; }
        public decimal Saldo { get; set; }
        public List<ConceptoReciboDto> Conceptos { get; set; } = new();
    }
}
