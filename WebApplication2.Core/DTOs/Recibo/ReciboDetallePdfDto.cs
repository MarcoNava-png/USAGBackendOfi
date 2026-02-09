namespace WebApplication2.Core.DTOs.Recibo
{
    public class ReciboDetallePdfDto
    {
        public int Numero { get; set; }
        public string? Descripcion { get; set; }
        public int Cantidad { get; set; } = 1;
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
    }
}
