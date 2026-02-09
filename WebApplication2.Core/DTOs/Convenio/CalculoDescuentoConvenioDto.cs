namespace WebApplication2.Core.DTOs.Convenio
{
    public class CalculoDescuentoConvenioDto
    {
        public int IdConvenio { get; set; }
        public string NombreConvenio { get; set; } = null!;
        public string TipoBeneficio { get; set; } = null!;
        public decimal MontoOriginal { get; set; }
        public decimal Descuento { get; set; }
        public decimal MontoFinal { get; set; }
    }
}
