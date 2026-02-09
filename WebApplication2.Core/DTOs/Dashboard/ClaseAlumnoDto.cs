namespace WebApplication2.Core.DTOs.Dashboard
{
    public class ClaseAlumnoDto
    {
        public int IdGrupoMateria { get; set; }
        public string Materia { get; set; } = string.Empty;
        public string Profesor { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
    }
}
