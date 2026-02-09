namespace WebApplication2.Core.DTOs.Dashboard
{
    public class AdmisionesDashboardDto
    {
        public int ProspectosHoy { get; set; }
        public int ProspectosSemana { get; set; }
        public int ProspectosDelMes { get; set; }

        public FunnelAdmisionDto Funnel { get; set; } = new();

        public int ConversionesDelMes { get; set; }
        public decimal TasaConversion { get; set; }

        public int CitasHoy { get; set; }
        public int CitasPendientes { get; set; }

        public int DocumentosPendientesAdmision { get; set; }

        public List<AlertaDto> Alertas { get; set; } = new();
        public List<AccionRapidaDto> AccionesRapidas { get; set; } = new();
    }
}
