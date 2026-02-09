namespace WebApplication2.Core.Responses.Pagos
{
    public class PagoRegistradoResponse
    {
        public long IdPago { get; set; }
        public string FolioPago { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public List<long> RecibosAfectados { get; set; } = new();
        public string? Comprobante { get; set; }
    }
}
