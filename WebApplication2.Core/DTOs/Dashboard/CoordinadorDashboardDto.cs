namespace WebApplication2.Core.DTOs.Dashboard
{
    public class CoordinadorDashboardDto
    {
        public decimal AsistenciaPromedio { get; set; }
        public List<GrupoAsistenciaDto> GruposEnRiesgo { get; set; } = new();

        public int CalificacionesPendientes { get; set; }
        public decimal TasaReprobacionPorMateria { get; set; }

        public List<DocentePendienteDto> DocentesConEntregasPendientes { get; set; } = new();
        public int TotalDocentes { get; set; }

        public int GruposAsignados { get; set; }
        public List<GrupoResumenDto> MisGrupos { get; set; } = new();

        public List<AlertaDto> Alertas { get; set; } = new();
        public List<AccionRapidaDto> AccionesRapidas { get; set; } = new();
    }
}
