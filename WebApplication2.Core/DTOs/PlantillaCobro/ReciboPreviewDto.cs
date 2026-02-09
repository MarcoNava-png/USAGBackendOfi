namespace WebApplication2.Core.DTOs.PlantillaCobro
{
    public class ReciboPreviewDto
    {
        public int NumeroRecibo { get; set; }
        public string FechaVencimiento { get; set; } = null!;
        public string MesCorrespondiente { get; set; } = null!;
        public List<ReciboPreviewDetalleDto> Conceptos { get; set; } = new();
        public decimal Subtotal { get; set; }
    }
}
