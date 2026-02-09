namespace WebApplication2.Core.DTOs.Recibo
{
    public class ReciboPdfDto
    {
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public string? Matricula { get; set; }
        public string? NombreEstudiante { get; set; }
        public string? Carrera { get; set; }
        public string? Periodo { get; set; }
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Recargos { get; set; }
        public decimal Total { get; set; }
        public decimal Saldo { get; set; }
        public string? Notas { get; set; }
        public bool EstaPagado { get; set; }
        public DateTime? FechaPago { get; set; }
        public List<ReciboDetallePdfDto> Detalles { get; set; } = new();
        public InstitucionPdfDto? Institucion { get; set; }
    }
}
