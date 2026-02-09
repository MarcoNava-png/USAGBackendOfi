using System.ComponentModel.DataAnnotations;
using WebApplication2.Core.DTOs.Importacion;

namespace WebApplication2.Core.Requests.Importacion
{
    public class ImportarMateriasRequest
    {
        [Required]
        public List<ImportarMateriaDto> Materias { get; set; } = new();
        public bool ActualizarExistentes { get; set; } = false;

        public bool CrearRelacionSiExiste { get; set; } = true;
    }
}
