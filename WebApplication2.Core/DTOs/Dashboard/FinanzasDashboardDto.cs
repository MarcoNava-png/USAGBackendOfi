namespace WebApplication2.Core.DTOs.Dashboard
{
    public class FinanzasDashboardDto
    {
        public decimal IngresosDia { get; set; }
        public decimal IngresosSemana { get; set; }
        public decimal IngresosMes { get; set; }
        public int PagosHoy { get; set; }

        public decimal DeudaTotal { get; set; }
        public int TotalMorosos { get; set; }
        public List<MorosoDto> TopMorosos { get; set; } = new();

        public decimal TotalBecasDelMes { get; set; }
        public decimal TotalDescuentosDelMes { get; set; }
        public int EstudiantesConBeca { get; set; }

        public int RecibosPendientes { get; set; }
        public int RecibosVencidos { get; set; }
        public int RecibosPagados { get; set; }

        public List<AlertaDto> Alertas { get; set; } = new();
        public List<AccionRapidaDto> AccionesRapidas { get; set; } = new();
    }
}
