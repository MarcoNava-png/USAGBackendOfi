namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class PeriodoConEstudiantesDto
    {
        public int IdPeriodoAcademico { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Periodicidad { get; set; } = string.Empty;
        public int Anio { get; set; }
        public bool EsPeriodoActual { get; set; }
        public int TotalEstudiantes { get; set; }
    }
}
