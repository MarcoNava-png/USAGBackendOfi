using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Core.Models
{
    [Table("TiposDocumentoEstudiante")]
    public class TipoDocumentoEstudiante
    {
        [Key]
        public int IdTipoDocumento { get; set; }

        [Required]
        [MaxLength(50)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        public int DiasVigencia { get; set; } = 30;

        public bool RequierePago { get; set; } = true;

        public bool Activo { get; set; } = true;

        public int Orden { get; set; } = 0;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public virtual ICollection<SolicitudDocumento> Solicitudes { get; set; } = new List<SolicitudDocumento>();
    }
}
