using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Core.Models.MultiTenant;

[Table("SuperAdmin")]
public class SuperAdmin
{
    [Key]
    public int IdSuperAdmin { get; set; }

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string NombreCompleto { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public bool AccesoTotal { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    [MaxLength(50)]
    public string? LastLoginIP { get; set; }

    public virtual ICollection<SuperAdminTenant> TenantAccess { get; set; } = new List<SuperAdminTenant>();
}
