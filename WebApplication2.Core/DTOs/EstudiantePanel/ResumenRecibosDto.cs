namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class ResumenRecibosDto
    {
        public decimal TotalAdeudo { get; set; }
        public decimal TotalPagado { get; set; }
        public int RecibosPendientes { get; set; }
        public int RecibosPagados { get; set; }
        public int RecibosVencidos { get; set; }
        public decimal TotalDescuentosAplicados { get; set; }

        public ReciboPanelResumenDto? ProximoVencimiento { get; set; }

        public List<ReciboPanelResumenDto> UltimosRecibos { get; set; } = new();
    }
}
