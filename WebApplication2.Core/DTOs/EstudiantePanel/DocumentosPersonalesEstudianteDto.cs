namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class DocumentosPersonalesEstudianteDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public int? IdAspirante { get; set; }
        public List<DocumentoPersonalDto> Documentos { get; set; } = new();
        public int TotalDocumentos { get; set; }
        public int DocumentosValidados { get; set; }
        public int DocumentosPendientes { get; set; }
    }
}
