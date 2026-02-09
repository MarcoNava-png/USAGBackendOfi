namespace WebApplication2.Core.DTOs
{
    public class EstadisticasAspirantesDto
    {
        public int TotalAspirantes { get; set; }
        public Dictionary<string, int> AspirantesPorEstatus { get; set; } = new();
        public Dictionary<string, int> AspirantesPorPrograma { get; set; } = new();
        public Dictionary<string, int> AspirantesPorMedioContacto { get; set; } = new();
        public int AspirantesConDocumentosPendientes { get; set; }
        public int AspirantesConPagosPendientes { get; set; }
        public int AspirantesConDocumentosCompletos { get; set; }
        public int AspirantesConPagosCompletos { get; set; }
    }
}
