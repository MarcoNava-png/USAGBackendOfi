using WebApplication2.Core.DTOs.Importacion;

namespace WebApplication2.Core.Responses.Importacion
{
    public class ValidarImportacionResponse
    {
        public bool EsValido { get; set; }
        public int TotalRegistros { get; set; }
        public int RegistrosValidos { get; set; }
        public int RegistrosConErrores { get; set; }
        public List<string> CampusEncontrados { get; set; } = new();
        public List<string> CampusNoEncontrados { get; set; } = new();
        public List<string> CursosEncontrados { get; set; } = new();
        public List<string> CursosNoEncontrados { get; set; } = new();
        public List<string> MatriculasDuplicadas { get; set; } = new();
        public List<ResultadoImportacionEstudiante> DetalleValidacion { get; set; } = new();
    }
}
