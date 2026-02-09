using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs.Pagos
{
    public class RegistrarYAplicarPagoDto
    {
        public long IdRecibo { get; set; }
        public DateTime FechaPagoUtc { get; set; } = DateTime.UtcNow;
        public int IdMedioPago { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; } = "MXN";
        public string? Referencia { get; set; }
        public string? Notas { get; set; }
        public EstatusPago Estatus { get; set; } = EstatusPago.CONFIRMADO;
    }
}
