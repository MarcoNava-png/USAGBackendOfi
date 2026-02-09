namespace WebApplication2.Core.DTOs.Dashboard
{
    public class ControlEscolarDashboardDto
    {
        public int InscripcionesHoy { get; set; }
        public int InscripcionesSemana { get; set; }
        public int BajasDelMes { get; set; }
        public int CambiosGrupo { get; set; }

        public List<EstudiantesPorProgramaDto> EstudiantesPorPrograma { get; set; } = new();

        public int DocumentosPendientes { get; set; }
        public int ExpedientesIncompletos { get; set; }

        public int GruposSinProfesor { get; set; }
        public int GruposActivos { get; set; }

        public PeriodoActualDashboardDto? PeriodoActual { get; set; }

        public List<AlertaDto> Alertas { get; set; } = new();
        public List<AccionRapidaDto> AccionesRapidas { get; set; } = new();
    }
}
