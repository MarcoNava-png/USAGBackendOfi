namespace WebApplication2.Core.DTOs.Documentos
{
    public class SolicitudesPendientesDto
    {
        public int TotalPendientesPago { get; set; }
        public int TotalListosGenerar { get; set; }
        public int TotalGenerados { get; set; }
        public int TotalVencidos { get; set; }
        public int TotalCancelados { get; set; }
        public int TotalEntregados { get; set; }
        public List<SolicitudResumenDto> Solicitudes { get; set; } = new();
    }

    public class SolicitudResumenDto
    {
        public long IdSolicitud { get; set; }
        public string FolioSolicitud { get; set; } = string.Empty;
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreEstudiante { get; set; } = string.Empty;
        public int IdTipoDocumento { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string TipoDocumentoClave { get; set; } = string.Empty;
        public string Variante { get; set; } = string.Empty;
        public string Estatus { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaGeneracion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal? PrecioDocumento { get; set; }
        public long? IdRecibo { get; set; }
        public string? FolioRecibo { get; set; }
        public string? EstatusRecibo { get; set; }
        public bool PuedeGenerar { get; set; }
        public string? UsuarioSolicita { get; set; }
        public string? UsuarioGenera { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string? UsuarioEntrega { get; set; }
        public bool PuedeMarcarEntregado { get; set; }
    }
}
