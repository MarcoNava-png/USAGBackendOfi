using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.DTOs.Importacion
{
    public class ImportarPlanEstudiosDto
    {
        [Required(ErrorMessage = "La clave del plan es requerida")]
        public string ClavePlanEstudios { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del plan es requerido")]
        public string NombrePlanEstudios { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave del campus es requerida")]
        public string ClaveCampus { get; set; } = string.Empty;

        public string? NivelEducativo { get; set; }

        public string? Periodicidad { get; set; }

        public int? DuracionMeses { get; set; }

        public string? RVOE { get; set; }

        public string? Version { get; set; }
    }
}
