namespace WebApplication2.Core.DTOs.Grupo
{
    public class EstudianteGrupoResultDto
    {
        public int IdEstudianteGrupo { get; set; }
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public DateTime FechaInscripcion { get; set; }
        public string Estado { get; set; } = "Inscrito";
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
    }
}
