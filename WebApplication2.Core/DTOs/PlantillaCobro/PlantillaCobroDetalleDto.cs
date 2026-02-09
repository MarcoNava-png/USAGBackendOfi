namespace WebApplication2.Core.DTOs.PlantillaCobro
{
    public class PlantillaCobroDetalleDto
    {
        public int IdPlantillaDetalle { get; set; }
        public int IdPlantillaCobro { get; set; }
        public int IdConceptoPago { get; set; }
        public string Descripcion { get; set; } = null!;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Orden { get; set; }
        public int? AplicaEnRecibo { get; set; }

        public string? NombreConcepto { get; set; }
        public string? ClaveConcepto { get; set; }
        public decimal Importe { get; set; }
    }
}
