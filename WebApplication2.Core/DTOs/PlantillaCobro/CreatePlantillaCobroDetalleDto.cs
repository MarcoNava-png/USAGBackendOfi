using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.DTOs.PlantillaCobro
{
    public class CreatePlantillaCobroDetalleDto
    {
        [Required(ErrorMessage = "El concepto de pago es requerido")]
        public int IdConceptoPago { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(500)]
        public string Descripcion { get; set; } = null!;

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0.01, 999999.99, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [Required(ErrorMessage = "El precio unitario es requerido")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        [Required(ErrorMessage = "El orden es requerido")]
        [Range(1, 100)]
        public int Orden { get; set; }

        public int? AplicaEnRecibo { get; set; }
    }
}
