namespace WebApplication2.Core.DTOs.Dashboard
{
    public class AdminDashboardDto
    {
        public decimal IngresosDia { get; set; }
        public decimal IngresosMes { get; set; }
        public decimal DeudaTotal { get; set; }
        public decimal PorcentajeMorosidad { get; set; }
        public int TotalMorosos { get; set; }

        public int AspirantesNuevos { get; set; }
        public int ConversionesDelMes { get; set; }
        public int InscripcionesDelMes { get; set; }
        public int BajasDelMes { get; set; }

        public int EstudiantesActivos { get; set; }
        public decimal AsistenciaGlobal { get; set; }
        public decimal PromedioGeneral { get; set; }
        public decimal TasaReprobacion { get; set; }

        public int TotalUsuarios { get; set; }
        public int GruposActivos { get; set; }
        public int ProfesoresActivos { get; set; }

        public List<AlertaDto> Alertas { get; set; } = new();
        public List<AccionRapidaDto> AccionesRapidas { get; set; } = new();
    }
}
