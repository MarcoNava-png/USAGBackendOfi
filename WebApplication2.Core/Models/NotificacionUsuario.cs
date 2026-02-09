using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Models
{
    public class NotificacionUsuario
    {
        [Key]
        public long IdNotificacion { get; set; }

        [Required]
        [MaxLength(450)]
        public string UsuarioDestinoId { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Mensaje { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; } = "info"; // info, warning, success, error

        [MaxLength(100)]
        public string? Modulo { get; set; }

        [MaxLength(500)]
        public string? UrlAccion { get; set; }

        public bool Leida { get; set; } = false;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaLectura { get; set; }
    }
}
