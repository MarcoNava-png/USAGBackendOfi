namespace WebApplication2.Core.DTOs.Recibo
{
    public class IngresoDetalleDto
    {
        public long IdPago { get; set; }
        public string? FolioRecibo { get; set; }
        public string? Matricula { get; set; }
        public string? NombreCompleto { get; set; }
        public DateTime FechaPago { get; set; }
        public string? MetodoPago { get; set; }
        public string? Referencia { get; set; }
        public decimal Monto { get; set; }
        public string? Conceptos { get; set; }
    }
}
