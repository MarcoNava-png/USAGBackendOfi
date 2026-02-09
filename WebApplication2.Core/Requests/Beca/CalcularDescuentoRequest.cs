namespace WebApplication2.Core.Requests.Beca
{
    public class CalcularDescuentoRequest
    {
        public int IdEstudiante { get; set; }
        public int IdConceptoPago { get; set; }
        public decimal ImporteBase { get; set; }
        public DateOnly? FechaAplicacion { get; set; }
    }
}
