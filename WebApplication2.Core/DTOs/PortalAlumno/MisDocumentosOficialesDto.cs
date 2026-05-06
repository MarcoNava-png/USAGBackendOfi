namespace WebApplication2.Core.DTOs.PortalAlumno
{
    public class MisDocumentosOficialesDto
    {
        public List<DocumentoOficialDisponibleDto> Disponibles { get; set; } = new();
        public List<MiSolicitudDocumentoDto> MisSolicitudes { get; set; } = new();
    }

    public class DocumentoOficialDisponibleDto
    {
        public int IdTipoDocumento { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public bool RequierePago { get; set; }
        public int DiasVigencia { get; set; }
    }

    public class MiSolicitudDocumentoDto
    {
        public int IdSolicitud { get; set; }
        public string? FolioSolicitud { get; set; }
        public int IdTipoDocumento { get; set; }
        public string NombreTipoDocumento { get; set; } = string.Empty;
        public string Variante { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaGeneracion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public bool RequierePago { get; set; }
        public long? IdRecibo { get; set; }
        public decimal? MontoRecibo { get; set; }
        public string? EstatusRecibo { get; set; }
        public string? CodigoVerificacion { get; set; }
    }

    public class SolicitarDocumentoRequest
    {
        public int IdTipoDocumento { get; set; }
        public string Variante { get; set; } = "COMPLETO";
        public string? Notas { get; set; }
    }
}
