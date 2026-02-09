namespace WebApplication2.Core.DTOs.PlantillaCobro
{
    public class ReciboPreviewDetalleDto
    {
        public string Concepto { get; set; } = null!;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
    }
}
