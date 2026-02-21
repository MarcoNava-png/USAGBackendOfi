namespace WebApplication2.Core.DTOs
{
    public class ComisionReporteDto
    {
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public decimal ComisionPorRegistro { get; set; }
        public decimal PorcentajePorPago { get; set; }
        public decimal TotalComisionesGlobal { get; set; }
        public List<UsuarioComisionDto> Comisiones { get; set; } = new();
    }

    public class UsuarioComisionDto
    {
        public string UsuarioId { get; set; } = "";
        public string NombreUsuario { get; set; } = "";
        public int TotalRegistros { get; set; }
        public decimal ComisionRegistros { get; set; }
        public decimal TotalPagosRecibidos { get; set; }
        public decimal ComisionPagos { get; set; }
        public decimal TotalComision { get; set; }
        public List<AspiranteComisionDetalleDto> Detalle { get; set; } = new();
    }

    public class AspiranteComisionDetalleDto
    {
        public int IdAspirante { get; set; }
        public string NombreCompleto { get; set; } = "";
        public DateTime FechaRegistro { get; set; }
        public string Estatus { get; set; } = "";
        public decimal TotalPagado { get; set; }
        public decimal ComisionGenerada { get; set; }
    }
}
