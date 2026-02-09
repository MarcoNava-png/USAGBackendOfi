namespace WebApplication2.Core.DTOs.Recibo
{
    public class IngresoPorConceptoDto
    {
        public int IdConceptoPago { get; set; }
        public string? Clave { get; set; }
        public string? Descripcion { get; set; }
        public int CantidadPagos { get; set; }
        public decimal TotalMonto { get; set; }
    }
}
