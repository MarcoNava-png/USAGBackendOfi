using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Core.Models.MultiTenant;


[Table("Tenant")]
public class Tenant
{
    [Key]
    public int IdTenant { get; set; }

    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string NombreCorto { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Subdominio { get; set; } = null!;

    [MaxLength(100)]
    public string? DominioPersonalizado { get; set; }

    [Required]
    [MaxLength(100)]
    public string DatabaseName { get; set; } = null!;

    [Required]
    public string ConnectionString { get; set; } = null!;

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(7)]
    public string ColorPrimario { get; set; } = "#14356F";

    [MaxLength(7)]
    public string? ColorSecundario { get; set; }

    [MaxLength(50)]
    public string Timezone { get; set; } = "America/Mexico_City";

    [MaxLength(100)]
    public string? EmailContacto { get; set; }

    [MaxLength(20)]
    public string? TelefonoContacto { get; set; }

    [MaxLength(500)]
    public string? DireccionFiscal { get; set; }

    [MaxLength(13)]
    public string? RFC { get; set; }

    public int IdPlanLicencia { get; set; }

    public DateTime FechaContratacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaVencimiento { get; set; }

    public int MaximoEstudiantes { get; set; } = 500;

    public int MaximoUsuarios { get; set; } = 20;

    public TenantStatus Status { get; set; } = TenantStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastAccessAt { get; set; }

    [MaxLength(450)]
    public string? CreatedBy { get; set; }

    [ForeignKey(nameof(IdPlanLicencia))]
    public virtual PlanLicencia? PlanLicencia { get; set; }
}
