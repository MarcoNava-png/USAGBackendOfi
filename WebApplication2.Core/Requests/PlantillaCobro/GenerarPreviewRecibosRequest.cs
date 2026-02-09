using WebApplication2.Core.DTOs.PlantillaCobro;

namespace WebApplication2.Core.Requests.PlantillaCobro
{
    public class GenerarPreviewRecibosRequest
    {
        public int NumeroRecibos { get; set; } = 4;
        public int DiaVencimiento { get; set; } = 10;
        public DateTime? FechaInicioPeriodo { get; set; }
        public List<PreviewConceptoDto> Conceptos { get; set; } = new();
    }
}
