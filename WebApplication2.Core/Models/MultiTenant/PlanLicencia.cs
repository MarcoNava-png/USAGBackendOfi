using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Core.Models.MultiTenant;

[Table("PlanLicencia")]
public class PlanLicencia
{
    [Key]
    public int IdPlanLicencia { get; set; }

    [Required]
    [MaxLength(50)]
    public string Nombre { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Codigo { get; set; } = null!;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioMensual { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecioAnual { get; set; }

    public int MaxEstudiantes { get; set; }

    public int MaxUsuarios { get; set; }

    public int MaxCampus { get; set; } = 1;

    public bool IncluyeSoporte { get; set; }

    public bool IncluyeReportes { get; set; }

    public bool IncluyeAPI { get; set; }

    public bool IncluyeFacturacion { get; set; }

    public string? Caracteristicas { get; set; }

    public bool Activo { get; set; } = true;

    public int Orden { get; set; }

    public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}
