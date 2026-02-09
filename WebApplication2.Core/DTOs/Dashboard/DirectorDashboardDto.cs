namespace WebApplication2.Core.DTOs.Dashboard
{
    public class DirectorDashboardDto
    {
        public int EstudiantesActivos { get; set; }
        public string TendenciaEstudiantes { get; set; } = string.Empty;
        public int InscripcionesDelMes { get; set; }
        public int BajasDelMes { get; set; }

        public decimal PorcentajeMorosidad { get; set; }
        public decimal IngresosMensuales { get; set; }

        public decimal PromedioGeneral { get; set; }
        public decimal TasaReprobacion { get; set; }
        public decimal AsistenciaGlobal { get; set; }

        public List<ProgramaResumenDto> ProgramasResumen { get; set; } = new();
        public List<AlertaDto> Alertas { get; set; } = new();
    }
}
