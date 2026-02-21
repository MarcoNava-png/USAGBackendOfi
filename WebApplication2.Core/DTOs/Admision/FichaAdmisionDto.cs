namespace WebApplication2.Core.DTOs.Admision
{
    public class FichaAdmisionDto
    {
        public int IdAspirante { get; set; }
        public string Folio { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public string EstatusActual { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public DatosPersonalesDto DatosPersonales { get; set; } = new();
        public DatosContactoDto DatosContacto { get; set; } = new();
        public InformacionAcademicaDto InformacionAcademica { get; set; } = new();
        public List<DocumentoDto> Documentos { get; set; } = new();
        public DatosSocioeconomicosDto DatosSocioeconomicos { get; set; } = new();
        public InformacionPagosDto InformacionPagos { get; set; } = new();
        public SeguimientoDto Seguimiento { get; set; } = new();
        public MetadataGeneracionDto Metadata { get; set; } = new();
    }
}
