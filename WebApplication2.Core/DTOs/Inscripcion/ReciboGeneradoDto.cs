namespace WebApplication2.Core.DTOs.Inscripcion
{
    public class ReciboGeneradoDto
    {
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateOnly FechaVencimiento { get; set; }
    }
}
