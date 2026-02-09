namespace WebApplication2.Core.DTOs.Recibo
{
    public class ReciboParaCobroDto
    {
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public int? IdAspirante { get; set; }
        public int? IdEstudiante { get; set; }
        public int? IdPeriodoAcademico { get; set; }
        public int? IdGrupo { get; set; }
        public int? IdPlantillaCobro { get; set; }
        public string FechaEmision { get; set; } = string.Empty;
        public string FechaVencimiento { get; set; } = string.Empty;
        public int Estatus { get; set; }
        public string? EstatusNombre { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DescuentoBeca { get; set; }
        public decimal Descuento { get; set; }
        public decimal DescuentoAdicional { get; set; }
        public decimal Recargos { get; set; }
        public decimal Total { get; set; }
        public decimal Saldo { get; set; }
        public string? Notas { get; set; }
        public string? CreadoPor { get; set; }
        public string? FechaCreacion { get; set; }
        public string? NombrePeriodo { get; set; }
        public string? CodigoGrupo { get; set; }
        public List<ReciboDetalleParaCobroDto> Detalles { get; set; } = new();
    }
}
