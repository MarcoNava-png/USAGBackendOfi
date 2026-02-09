namespace WebApplication2.Core.DTOs.Pagos
{
    public class RegistrarYAplicarPagoResultDto
    {
        public long IdPago { get; set; }
        public long IdRecibo { get; set; }
        public decimal MontoAplicado { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal SaldoNuevo { get; set; }
        public string EstatusReciboAnterior { get; set; } = string.Empty;
        public string EstatusReciboNuevo { get; set; } = string.Empty;
        public bool ReciboPagadoCompletamente { get; set; }
    }
}
