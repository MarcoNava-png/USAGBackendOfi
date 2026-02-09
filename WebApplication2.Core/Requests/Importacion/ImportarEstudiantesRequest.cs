using System.ComponentModel.DataAnnotations;
using WebApplication2.Core.DTOs.Importacion;

namespace WebApplication2.Core.Requests.Importacion
{
    public class ImportarEstudiantesRequest
    {
        [Required]
        public List<ImportarEstudianteDto> Estudiantes { get; set; } = new();

        public bool CrearCatalogosInexistentes { get; set; } = false;

        public bool ActualizarExistentes { get; set; } = false;

        public bool InscribirAGrupo { get; set; } = false;
    }
}
