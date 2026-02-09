namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class SolicitudDocumentoResumenDto
    {
        public long IdSolicitud { get; set; }
        public string FolioSolicitud { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public string Variante { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaGeneracion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public bool EstaVigente { get; set; }
        public bool PuedeDescargar { get; set; }
        public Guid? CodigoVerificacion { get; set; }
    }
}
