namespace WebApplication2.Core.DTOs.Caja
{
    public class PagoDetalladoDto
    {
        public long IdPago { get; set; }
        public string? FolioPago { get; set; }
        public DateTime FechaPagoUtc { get; set; }
        public string? HoraPago { get; set; }
        public int IdMedioPago { get; set; }
        public string? MedioPago { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; } = "MXN";
        public string? Referencia { get; set; }
        public string? Notas { get; set; }
        public int Estatus { get; set; }
        public string? EstatusNombre { get; set; }
        public int? IdEstudiante { get; set; }
        public string? Matricula { get; set; }
        public string? NombreEstudiante { get; set; }
        public string? Concepto { get; set; }
        public string? FolioRecibo { get; set; }
    }
}
