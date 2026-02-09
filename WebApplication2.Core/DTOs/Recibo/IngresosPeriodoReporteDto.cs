namespace WebApplication2.Core.DTOs.Recibo
{
    public class IngresosPeriodoReporteDto
    {
        public DateOnly FechaReporte { get; set; }
        public int IdPeriodoAcademico { get; set; }
        public string? NombrePeriodo { get; set; }
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public int TotalPagos { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal TotalOtros { get; set; }
        public List<IngresoPorConceptoDto> PorConcepto { get; set; } = new List<IngresoPorConceptoDto>();
        public List<IngresoDetalleDto> Detalle { get; set; } = new List<IngresoDetalleDto>();
    }
}
