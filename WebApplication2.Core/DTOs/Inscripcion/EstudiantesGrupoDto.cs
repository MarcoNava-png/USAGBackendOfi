namespace WebApplication2.Core.DTOs.Inscripcion
{
    public class EstudiantesGrupoDto
    {
        public int IdGrupo { get; set; }
        public string CodigoGrupo { get; set; } = string.Empty;
        public string NombreGrupo { get; set; } = string.Empty;
        public int TotalEstudiantes { get; set; }
        public List<EstudianteInscritoDto> Estudiantes { get; set; } = new();
    }
}
