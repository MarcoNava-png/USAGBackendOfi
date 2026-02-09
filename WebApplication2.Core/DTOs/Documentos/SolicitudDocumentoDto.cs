namespace WebApplication2.Core.DTOs.Documentos
{
    public class SolicitudDocumentoDto
    {
        public long IdSolicitud { get; set; }
        public string FolioSolicitud { get; set; } = string.Empty;
        public int IdEstudiante { get; set; }
        public string NombreEstudiante { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public int IdTipoDocumento { get; set; }
        public string TipoDocumentoNombre { get; set; } = string.Empty;
        public string TipoDocumentoClave { get; set; } = string.Empty;
        public long? IdRecibo { get; set; }
        public string? FolioRecibo { get; set; }
        public string Variante { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaGeneracion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public Guid CodigoVerificacion { get; set; }
        public string UrlVerificacion { get; set; } = string.Empty;
        public int VecesImpreso { get; set; }
        public string? Notas { get; set; }
        public decimal? Precio { get; set; }
        public bool EstaVigente { get; set; }
        public bool PuedeGenerar { get; set; }
    }
}
