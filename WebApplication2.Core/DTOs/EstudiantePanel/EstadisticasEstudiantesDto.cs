namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class EstadisticasEstudiantesDto
    {
        public int TotalEstudiantes { get; set; }
        public int EstudiantesActivos { get; set; }
        public int EstudiantesConAdeudo { get; set; }
        public int EstudiantesConBeca { get; set; }
        public decimal TotalAdeudoGeneral { get; set; }
        public decimal PromedioGeneralInstitucional { get; set; }
    }
}
