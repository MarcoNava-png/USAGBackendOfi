using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.DTOs.Importacion
{
    public class ImportarCampusDto
    {
        [Required(ErrorMessage = "La clave del campus es requerida")]
        public string ClaveCampus { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del campus es requerido")]
        public string Nombre { get; set; } = string.Empty;

        public string? Calle { get; set; }

        public string? NumeroExterior { get; set; }

        public string? NumeroInterior { get; set; }

        public string? CodigoPostal { get; set; }

        public string? Colonia { get; set; }

        public string? Telefono { get; set; }
    }
}
