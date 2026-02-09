namespace WebApplication2.Core.DTOs.Dashboard
{
    public class AlertaDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string? Link { get; set; }
        public DateTime? Fecha { get; set; }
    }
}
