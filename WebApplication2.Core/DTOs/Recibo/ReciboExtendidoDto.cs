namespace WebApplication2.Core.DTOs.Recibo
{
    public class ReciboExtendidoDto
    {
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public int? IdAspirante { get; set; }
        public int? IdEstudiante { get; set; }
        public int? IdPeriodoAcademico { get; set; }
        public string? NombrePeriodo { get; set; }
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Recargos { get; set; }
        public decimal Total { get; set; }
        public decimal Saldo { get; set; }
        public string? Notas { get; set; }
        public int DiasVencido { get; set; }
        public bool EstaVencido { get; set; }
        public string? Matricula { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Carrera { get; set; }
        public string? PlanEstudios { get; set; }
        public string? Grupo { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string TipoPersona { get; set; } = "Estudiante";
        public List<ReciboLineaDto> Detalles { get; set; } = new List<ReciboLineaDto>();
    }
}
