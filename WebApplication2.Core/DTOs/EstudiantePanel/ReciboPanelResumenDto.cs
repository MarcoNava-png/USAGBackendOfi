namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class ReciboPanelResumenDto
    {
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public string? Concepto { get; set; }
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Recargos { get; set; }
        public decimal Total { get; set; }
        public decimal Saldo { get; set; }
        public int? DiasVencido { get; set; }
        public bool EstaVencido { get; set; }
        public string? NombrePeriodo { get; set; }
    }
}
