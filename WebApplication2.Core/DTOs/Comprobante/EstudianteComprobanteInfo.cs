namespace WebApplication2.Core.DTOs.Comprobante
{
    public class EstudianteComprobanteInfo
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Carrera { get; set; }
        public string? PeriodoActual { get; set; }
    }
}
