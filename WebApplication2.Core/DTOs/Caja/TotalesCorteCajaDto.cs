namespace WebApplication2.Core.DTOs.Caja
{
    public class TotalesCorteCajaDto
    {
        public int Cantidad { get; set; }
        public decimal Efectivo { get; set; }
        public decimal Transferencia { get; set; }
        public decimal Tarjeta { get; set; }
        public decimal Total { get; set; }
    }
}
