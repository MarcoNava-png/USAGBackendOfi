namespace WebApplication2.Core.DTOs.Pagos
{
    public class DescuentoAutorizado
    {
        public decimal Monto { get; set; }
        public string AutorizadoPor { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }
}
