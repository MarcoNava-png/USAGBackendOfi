namespace WebApplication2.Core.DTOs.PortalAlumno
{
    public class MisPagosDto
    {
        public decimal TotalAdeudo { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public int RecibosPendientes { get; set; }
        public int RecibosVencidos { get; set; }
        public int RecibosPagados { get; set; }
        public List<MiReciboDto> Recibos { get; set; } = new();
    }

    public class MiReciboDto
    {
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Recargos { get; set; }
        public decimal Total { get; set; }
        public decimal Saldo { get; set; }
        public string? Notas { get; set; }
        public bool Vencido { get; set; }
        public int DiasVencido { get; set; }
        public List<MiReciboDetalleDto> Detalles { get; set; } = new();
        public List<MiPagoAplicadoDto> Pagos { get; set; } = new();
    }

    public class MiReciboDetalleDto
    {
        public long IdReciboDetalle { get; set; }
        public int IdConceptoPago { get; set; }
        public string? Concepto { get; set; }
        public string? Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
    }

    public class MiPagoAplicadoDto
    {
        public long IdPago { get; set; }
        public string? FolioPago { get; set; }
        public DateTime FechaPago { get; set; }
        public string? MedioPago { get; set; }
        public decimal MontoAplicado { get; set; }
        public string? Referencia { get; set; }
    }
}
