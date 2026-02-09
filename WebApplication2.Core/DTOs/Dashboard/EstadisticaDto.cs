namespace WebApplication2.Core.DTOs.Dashboard
{
    public class EstadisticaDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Tendencia { get; set; }
        public bool? TendenciaPositiva { get; set; }
    }
}
