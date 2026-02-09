using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Core.Models.MultiTenant;

[Table("SuperAdminTenant")]
public class SuperAdminTenant
{
    [Key]
    public int IdSuperAdminTenant { get; set; }

    public int IdSuperAdmin { get; set; }

    public int IdTenant { get; set; }

    [Required]
    [MaxLength(20)]
    public string Rol { get; set; } = "Admin";

    public DateTime AsignadoEn { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IdSuperAdmin))]
    public virtual SuperAdmin SuperAdmin { get; set; } = null!;

    [ForeignKey(nameof(IdTenant))]
    public virtual Tenant Tenant { get; set; } = null!;
}
