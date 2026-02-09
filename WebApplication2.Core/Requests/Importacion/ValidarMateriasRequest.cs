using WebApplication2.Core.DTOs.Importacion;

namespace WebApplication2.Core.Requests.Importacion
{
    public class ValidarMateriasRequest
    {
        public List<ImportarMateriaDto> Materias { get; set; } = new();
    }
}
