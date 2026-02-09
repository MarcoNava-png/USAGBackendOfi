namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class InformacionAcademicaPanelDto
    {
        public int? IdPlanEstudios { get; set; }
        public string? PlanEstudios { get; set; }
        public string? Carrera { get; set; }
        public string? RVOE { get; set; }
        public string? Modalidad { get; set; }
        public DateOnly FechaIngreso { get; set; }
        public string? Campus { get; set; }
        public string? Turno { get; set; }

        public GrupoActualDto? GrupoActual { get; set; }

        public PeriodoActualPanelDto? PeriodoActual { get; set; }

        public string? GradoActual { get; set; }
        public int? SemestreActual { get; set; }
    }
}
