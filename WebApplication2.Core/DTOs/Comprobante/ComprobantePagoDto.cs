namespace WebApplication2.Core.DTOs.Comprobante
{
    public class ComprobantePagoDto
    {
        public PagoComprobanteInfo Pago { get; set; } = new();
        public EstudianteComprobanteInfo Estudiante { get; set; } = new();
        public List<ReciboComprobanteInfo> RecibosPagados { get; set; } = new();
        public InstitucionInfo Institucion { get; set; } = new();
        public CajeroComprobanteInfo? Cajero { get; set; }
    }
}
