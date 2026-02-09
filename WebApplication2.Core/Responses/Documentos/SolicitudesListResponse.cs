using WebApplication2.Core.DTOs.Documentos;

namespace WebApplication2.Core.Responses.Documentos
{
    public class SolicitudesListResponse
    {
        public List<SolicitudDocumentoDto> Solicitudes { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}
