namespace WebApplication2.Core.DTOs.Admision
{
    public class CostoDesglosePdfDto
    {
        public string Concepto { get; set; } = string.Empty;
        public decimal? Monto { get; set; }
        public string? Nota { get; set; }
    }
}
