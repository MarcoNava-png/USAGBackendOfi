using WebApplication2.Core.DTOs.Importacion;

namespace WebApplication2.Core.Responses.Importacion
{
    public class ImportarEstudiantesResponse
    {
        public int TotalProcesados { get; set; }
        public int Exitosos { get; set; }
        public int Fallidos { get; set; }
        public int Actualizados { get; set; }
        public List<ResultadoImportacionEstudiante> Resultados { get; set; } = new();
        public List<string> Errores { get; set; } = new();
    }
}
