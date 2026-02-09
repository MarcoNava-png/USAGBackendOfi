using WebApplication2.Core.DTOs.EstudiantePanel;

namespace WebApplication2.Core.Responses.EstudiantePanel
{
    public class BuscarEstudiantesPanelResponse
    {
        public List<EstudianteListaDto> Estudiantes { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalPaginas { get; set; }

        public EstadisticasEstudiantesDto Estadisticas { get; set; } = new();
    }
}
