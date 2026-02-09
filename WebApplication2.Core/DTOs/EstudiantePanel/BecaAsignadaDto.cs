namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class BecaAsignadaDto
    {
        public long IdBecaAsignacion { get; set; }
        public int? IdBeca { get; set; }
        public string? NombreBeca { get; set; }
        public string? ClaveBeca { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string? ConceptoPago { get; set; }
        public decimal? TopeMensual { get; set; }
        public DateOnly VigenciaDesde { get; set; }
        public DateOnly? VigenciaHasta { get; set; }
        public bool Activo { get; set; }
        public bool EstaVigente { get; set; }
        public string? Observaciones { get; set; }

        public string DescripcionDescuento => Tipo == "PORCENTAJE"
            ? $"{Valor}% de descuento"
            : $"${Valor:N2} de descuento";
    }
}
