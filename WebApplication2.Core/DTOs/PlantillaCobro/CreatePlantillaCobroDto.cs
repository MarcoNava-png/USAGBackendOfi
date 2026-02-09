using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.DTOs.PlantillaCobro
{
    public class CreatePlantillaCobroDto
    {
        [Required(ErrorMessage = "El nombre de la plantilla es requerido")]
        [StringLength(200)]
        public string NombrePlantilla { get; set; } = null!;

        [Required(ErrorMessage = "El plan de estudios es requerido")]
        public int IdPlanEstudios { get; set; }

        [Required(ErrorMessage = "El número de cuatrimestre es requerido")]
        [Range(1, 12, ErrorMessage = "El cuatrimestre debe estar entre 1 y 12")]
        public int NumeroCuatrimestre { get; set; }

        public int? IdPeriodoAcademico { get; set; }
        public int? IdTurno { get; set; }
        public int? IdModalidad { get; set; }

        [Required(ErrorMessage = "La fecha de vigencia es requerida")]
        public DateTime FechaVigenciaInicio { get; set; }

        public DateTime? FechaVigenciaFin { get; set; }

        [Required(ErrorMessage = "La estrategia de emisión es requerida")]
        [Range(0, 2, ErrorMessage = "Estrategia inválida")]
        public int EstrategiaEmision { get; set; }

        [Required(ErrorMessage = "El número de recibos es requerido")]
        [Range(1, 12, ErrorMessage = "El número de recibos debe estar entre 1 y 12")]
        public int NumeroRecibos { get; set; }

        [Required(ErrorMessage = "El día de vencimiento es requerido")]
        [Range(1, 31, ErrorMessage = "El día debe estar entre 1 y 31")]
        public int DiaVencimiento { get; set; }

        [Required(ErrorMessage = "Los detalles son requeridos")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un concepto")]
        public List<CreatePlantillaCobroDetalleDto> Detalles { get; set; } = new();
    }
}
