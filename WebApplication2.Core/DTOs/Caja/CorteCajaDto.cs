namespace WebApplication2.Core.DTOs.Caja
{
    public class CorteCajaDto
    {
        public int IdCorteCaja { get; set; }
        public string FolioCorteCaja { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string IdUsuarioCaja { get; set; } = string.Empty;
        public int? IdCaja { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalGeneral { get; set; }
        public bool Cerrado { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string? CerradoPor { get; set; }
        public string? Observaciones { get; set; }
        public string? NombreUsuario { get; set; }
        public int CantidadPagos { get; set; }
    }
}
