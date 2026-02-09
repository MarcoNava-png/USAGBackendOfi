namespace WebApplication2.Core.DTOs.Recibo
{
    public class ReciboDetalleParaCobroDto
    {
        public long IdReciboDetalle { get; set; }
        public int IdConceptoPago { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public decimal? DescuentoBeca { get; set; }
        public decimal? ImporteNeto { get; set; }
        public int? IdPlantillaDetalle { get; set; }
        public string? RefTabla { get; set; }
        public long? RefId { get; set; }
    }
}
