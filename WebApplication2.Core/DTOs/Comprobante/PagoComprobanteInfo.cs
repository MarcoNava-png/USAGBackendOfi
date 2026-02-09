namespace WebApplication2.Core.DTOs.Comprobante
{
    public class PagoComprobanteInfo
    {
        public long IdPago { get; set; }
        public string FolioPago { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; }
        public string HoraPago { get; set; } = string.Empty;
        public string MedioPago { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Moneda { get; set; } = "MXN";
        public string? Referencia { get; set; }
        public string? Notas { get; set; }
    }
}
