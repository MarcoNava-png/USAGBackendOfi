using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs.Pagos
{
    public class RegistrarPagoDto
    {
        public DateTime FechaPagoUtc { get; set; }
        public int IdMedioPago { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; } = null!;
        public string? Referencia { get; set; }
        public string? Notas { get; set; }
        public EstatusPago estatus { get; set; } = EstatusPago.CONFIRMADO;
    }
}
