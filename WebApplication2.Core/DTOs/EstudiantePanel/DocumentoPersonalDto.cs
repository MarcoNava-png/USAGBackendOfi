namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class DocumentoPersonalDto
    {
        public long IdAspiranteDocumento { get; set; }
        public int IdDocumentoRequisito { get; set; }
        public string ClaveDocumento { get; set; } = string.Empty;
        public string NombreDocumento { get; set; } = string.Empty;
        public string Estatus { get; set; } = string.Empty;
        public DateTime? FechaSubido { get; set; }
        public string? UrlArchivo { get; set; }
        public string? Notas { get; set; }
        public bool EsObligatorio { get; set; }
        public DateTime? FechaValidacion { get; set; }
        public string? ValidadoPor { get; set; }
    }
}
