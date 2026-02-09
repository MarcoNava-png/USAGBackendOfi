using WebApplication2.Core.DTOs.Grupo;

namespace WebApplication2.Core.Responses.Grupo
{
    public class ImportarEstudiantesGrupoResponse
    {
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public string PlanEstudios { get; set; } = string.Empty;
        public int TotalProcesados { get; set; }
        public int Exitosos { get; set; }
        public int Fallidos { get; set; }
        public int PersonasCreadas { get; set; }
        public int EstudiantesCreados { get; set; }
        public int InscripcionesCreadas { get; set; }
        public List<EstudianteImportadoResultDto> Resultados { get; set; } = new();
    }
}
