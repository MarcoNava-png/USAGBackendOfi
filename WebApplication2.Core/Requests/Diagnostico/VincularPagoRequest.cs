namespace WebApplication2.Core.Requests.Diagnostico
{
    public class VincularPagoRequest
    {
        public long IdPago { get; set; }
        public List<long> IdsRecibos { get; set; } = new();
    }
}
