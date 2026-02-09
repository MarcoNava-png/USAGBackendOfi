using WebApplication2.Core.DTOs.Grupo;

namespace WebApplication2.Core.Responses.Grupo
{
    public class InscribirEstudiantesGrupoResponse
    {
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public int TotalProcesados { get; set; }
        public int Exitosos { get; set; }
        public int Fallidos { get; set; }
        public List<EstudianteGrupoResultDto> Resultados { get; set; } = new();
    }
}
