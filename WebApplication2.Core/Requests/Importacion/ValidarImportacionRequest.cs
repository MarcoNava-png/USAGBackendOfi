using WebApplication2.Core.DTOs.Importacion;

namespace WebApplication2.Core.Requests.Importacion
{
    public class ValidarImportacionRequest
    {
        public List<ImportarEstudianteDto> Estudiantes { get; set; } = new();
    }
}
