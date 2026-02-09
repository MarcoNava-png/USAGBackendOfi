using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models
{
    [Table("SolicitudesDocumento")]
    public class SolicitudDocumento
    {
        [Key]
        public long IdSolicitud { get; set; }

        [Required]
        [MaxLength(20)]
        public string FolioSolicitud { get; set; } = string.Empty;

        [Required]
        public int IdEstudiante { get; set; }

        [Required]
        public int IdTipoDocumento { get; set; }

        public long? IdRecibo { get; set; }

        public VarianteDocumento Variante { get; set; } = VarianteDocumento.COMPLETO;

        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        public DateTime? FechaGeneracion { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        public EstatusSolicitudDocumento Estatus { get; set; } = EstatusSolicitudDocumento.PENDIENTE_PAGO;

        public Guid CodigoVerificacion { get; set; } = Guid.NewGuid();

        public int VecesImpreso { get; set; } = 0;

        [MaxLength(500)]
        public string? Notas { get; set; }

        [MaxLength(450)]
        public string? UsuarioSolicita { get; set; }

        [MaxLength(450)]
        public string? UsuarioGenera { get; set; }

        public DateTime? FechaModificacion { get; set; }

        public DateTime? FechaEntrega { get; set; }

        [MaxLength(450)]
        public string? UsuarioEntrega { get; set; }

        [ForeignKey("IdEstudiante")]
        public virtual Estudiante? Estudiante { get; set; }

        [ForeignKey("IdTipoDocumento")]
        public virtual TipoDocumentoEstudiante? TipoDocumento { get; set; }

        [ForeignKey("IdRecibo")]
        public virtual Recibo? Recibo { get; set; }

        [ForeignKey("UsuarioSolicita")]
        public virtual ApplicationUser? UsuarioSolicitante { get; set; }

        [ForeignKey("UsuarioGenera")]
        public virtual ApplicationUser? UsuarioGenerador { get; set; }
    }
}
