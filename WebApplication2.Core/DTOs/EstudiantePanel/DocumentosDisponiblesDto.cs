namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class DocumentosDisponiblesDto
    {
        public List<TipoDocumentoDisponibleDto> TiposDisponibles { get; set; } = new();

        public List<SolicitudDocumentoResumenDto> SolicitudesRecientes { get; set; } = new();

        public int SolicitudesPendientes { get; set; }
        public int SolicitudesGeneradas { get; set; }
        public int DocumentosVigentes { get; set; }
    }
}
