using WebApplication2.Core.DTOs.Importacion;

namespace WebApplication2.Core.Responses.Importacion
{
    public class ImportarMateriasResponse
    {
        public int TotalProcesados { get; set; }
        public int MateriasCreadas { get; set; }
        public int MateriasActualizadas { get; set; }
        public int RelacionesCreadas { get; set; }
        public int Fallidos { get; set; }
        public List<ResultadoImportacionMateria> Resultados { get; set; } = new();
        public List<string> PlanesNoEncontrados { get; set; } = new();
    }
}
