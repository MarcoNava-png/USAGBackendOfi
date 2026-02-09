namespace WebApplication2.Core.DTOs.Inscripcion
{
    public class ValidacionesInscripcionDto
    {
        public bool DocumentosCompletos { get; set; }
        public bool PagoInscripcionRealizado { get; set; }
        public bool EstatusAspiranteValido { get; set; }
        public List<string> Advertencias { get; set; } = new();
        public List<DocumentoValidacionDto> DetalleDocumentos { get; set; } = new();
    }
}
