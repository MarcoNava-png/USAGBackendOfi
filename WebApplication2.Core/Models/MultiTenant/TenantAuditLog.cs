using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Core.Models.MultiTenant;

[Table("TenantAuditLog")]
public class TenantAuditLog
{
    [Key]
    public long IdLog { get; set; }

    public int? IdTenant { get; set; }

    [MaxLength(50)]
    public string? TenantCodigo { get; set; }

    public int? IdSuperAdmin { get; set; }

    [Required]
    [MaxLength(50)]
    public string Accion { get; set; } = null!;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public string? Detalles { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IdTenant))]
    public virtual Tenant? Tenant { get; set; }

    [ForeignKey(nameof(IdSuperAdmin))]
    public virtual SuperAdmin? SuperAdmin { get; set; }
}
