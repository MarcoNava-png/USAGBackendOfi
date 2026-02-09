using System.ComponentModel.DataAnnotations;
using WebApplication2.Core.DTOs.Importacion;

namespace WebApplication2.Core.Requests.Importacion
{
    public class ImportarPlanesEstudiosRequest
    {
        [Required]
        public List<ImportarPlanEstudiosDto> Planes { get; set; } = new();

        public bool ActualizarExistentes { get; set; } = false;
    }
}
