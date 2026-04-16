namespace WebApplication2.Core.Models
{
    public class PagoMetodo : BaseEntity
    {
        public long IdPagoMetodo { get; set; }
        public long IdPago { get; set; }
        public int IdMedioPago { get; set; }
        public decimal Monto { get; set; }
        public string? Referencia { get; set; }

        public Pago Pago { get; set; } = null!;
        public MedioPago MedioPago { get; set; } = null!;
    }
}
