namespace WebApplication2.Core.DTOs.Grupo
{
    public class EstudianteEnGrupoDto
    {
        public int IdEstudianteGrupo { get; set; }
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? PlanEstudios { get; set; }
    }
}
