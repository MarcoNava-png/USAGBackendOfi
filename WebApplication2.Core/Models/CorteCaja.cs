using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Core.Models
{
    [Table("CorteCaja")]
    public class CorteCaja : BaseEntity
    {
        [Key]
        public int IdCorteCaja { get; set; }

        [Required]
        [MaxLength(50)]
        public string FolioCorteCaja { get; set; } = string.Empty;

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        [MaxLength(450)]
        public string IdUsuarioCaja { get; set; } = string.Empty;

        public int? IdCaja { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoInicial { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEfectivo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTransferencia { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTarjeta { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGeneral { get; set; }

        public bool Cerrado { get; set; }

        public DateTime? FechaCierre { get; set; }

        [MaxLength(450)]
        public string? CerradoPor { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        [ForeignKey("IdUsuarioCaja")]
        public virtual ApplicationUser? UsuarioCaja { get; set; }

        [ForeignKey("CerradoPor")]
        public virtual ApplicationUser? UsuarioCerro { get; set; }
    }
}
