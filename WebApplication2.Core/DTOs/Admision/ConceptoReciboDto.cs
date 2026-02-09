namespace WebApplication2.Core.DTOs.Admision
{
    public class ConceptoReciboDto
    {
        public string Concepto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
