namespace WebApplication2.Core.DTOs.Recibo
{
    public class CarteraVencidaItemDto
    {
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public string? Matricula { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Carrera { get; set; }
        public string? Grupo { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public int DiasVencido { get; set; }
        public decimal Total { get; set; }
        public decimal Saldo { get; set; }
        public decimal Recargos { get; set; }
        public decimal TotalAdeudo { get; set; }
    }
}
