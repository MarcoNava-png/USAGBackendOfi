using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs.Admision
{
    public class DocumentoDto
    {
        public string Clave { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool EsObligatorio { get; set; }
        public EstatusDocumentoEnum Estatus { get; set; }
        public DateTime? FechaSubida { get; set; }
        public string? UrlArchivo { get; set; }
        public string? Notas { get; set; }
    }
}
