using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Models
{
    public class BitacoraAccion
    {
        [Key]
        public long IdBitacora { get; set; }

        [Required]
        [MaxLength(450)]
        public string UsuarioId { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string NombreUsuario { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Accion { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Modulo { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Entidad { get; set; } = null!;

        [MaxLength(100)]
        public string? EntidadId { get; set; }

        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        public string? DatosAnteriores { get; set; }

        public string? DatosNuevos { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
    }
}
