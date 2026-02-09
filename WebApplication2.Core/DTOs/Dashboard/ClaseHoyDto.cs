namespace WebApplication2.Core.DTOs.Dashboard
{
    public class ClaseHoyDto
    {
        public int IdGrupoMateria { get; set; }
        public string Materia { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int TotalEstudiantes { get; set; }
    }
}
