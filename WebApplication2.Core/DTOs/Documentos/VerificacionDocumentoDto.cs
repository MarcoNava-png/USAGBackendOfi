namespace WebApplication2.Core.DTOs.Documentos
{
    public class VerificacionDocumentoDto
    {
        public bool EsValido { get; set; }
        public bool EstaVigente { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? TipoDocumento { get; set; }
        public string? NombreEstudiante { get; set; }
        public string? Matricula { get; set; }
        public string? Carrera { get; set; }
        public DateTime? FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? FolioDocumento { get; set; }
    }
}
