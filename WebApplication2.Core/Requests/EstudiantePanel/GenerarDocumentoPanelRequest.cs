namespace WebApplication2.Core.Requests.EstudiantePanel
{
    public class GenerarDocumentoPanelRequest
    {
        public int IdEstudiante { get; set; }
        public int IdTipoDocumento { get; set; }
        public string Variante { get; set; } = "COMPLETO";
        public string? Notas { get; set; }
    }
}
