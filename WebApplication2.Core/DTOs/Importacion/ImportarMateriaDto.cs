using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.DTOs.Importacion
{
    public class ImportarMateriaDto
    {
        [Required(ErrorMessage = "La clave de la materia es requerida")]
        public string Clave { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la materia es requerido")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El plan de estudios es requerido")]
        public string PlanEstudios { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave del campus es requerida")]
        public string ClaveCampus { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cuatrimestre es requerido")]
        public string Cuatrimestre { get; set; } = string.Empty;

        public decimal? Creditos { get; set; }

        public byte? HorasTeoria { get; set; }

        public byte? HorasPractica { get; set; }

        public string? EsOptativa { get; set; }

        public string? Tipo { get; set; }
    }
}
