using WebApplication2.Core.DTOs.PlantillaCobro;

namespace WebApplication2.Core.Responses.PlantillaCobro
{
    public class PreviewRecibosResponse
    {
        public List<ReciboPreviewDto> Recibos { get; set; } = new();
        public decimal TotalPrimerRecibo { get; set; }
        public decimal TotalRecibosRegulares { get; set; }
        public decimal TotalGeneral { get; set; }
    }
}
