using WebApplication2.Core.DTOs.Importacion;

namespace WebApplication2.Core.Responses.Importacion
{
    public class ValidarMateriasResponse
    {
        public bool EsValido { get; set; }
        public int TotalRegistros { get; set; }
        public int RegistrosValidos { get; set; }
        public int RegistrosConErrores { get; set; }
        public List<string> PlanesEncontrados { get; set; } = new();
        public List<string> PlanesNoEncontrados { get; set; } = new();
        public List<string> ClavesDuplicadas { get; set; } = new();
        public List<ResultadoImportacionMateria> DetalleValidacion { get; set; } = new();
    }
}
