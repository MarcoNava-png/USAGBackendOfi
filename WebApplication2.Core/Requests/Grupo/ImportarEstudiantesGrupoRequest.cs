using WebApplication2.Core.DTOs.Grupo;

namespace WebApplication2.Core.Requests.Grupo
{
    public class ImportarEstudiantesGrupoRequest
    {
        public int IdGrupo { get; set; }
        public List<EstudianteImportarDto> Estudiantes { get; set; } = new();
        public string? Observaciones { get; set; }
    }
}
