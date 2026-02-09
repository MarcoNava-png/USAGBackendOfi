namespace WebApplication2.Core.DTOs.PlantillaCobro
{
    public class PreviewConceptoDto
    {
        public string Descripcion { get; set; } = null!;
        public decimal Cantidad { get; set; } = 1;
        public decimal PrecioUnitario { get; set; }
        public int? AplicaEnRecibo { get; set; }
    }
}
