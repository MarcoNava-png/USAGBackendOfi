using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Core.Models.MultiTenant;


[Table("Notificacion")]
public class Notificacion
{
    [Key]
    public int IdNotificacion { get; set; }

    [Required]
    [StringLength(50)]
    public string Tipo { get; set; } = null!; 

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = null!;

    [Required]
    public string Mensaje { get; set; } = null!;

    public int? IdTenant { get; set; }

    [StringLength(20)]
    public string? TenantCodigo { get; set; }

    [StringLength(200)]
    public string? TenantNombre { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public bool Leida { get; set; }

    public DateTime? FechaLectura { get; set; }

    [StringLength(20)]
    public string Prioridad { get; set; } = "Normal"; 

    [StringLength(500)]
    public string? AccionUrl { get; set; }

    public bool EmailEnviado { get; set; }

    public DateTime? FechaEmailEnviado { get; set; }

    [ForeignKey(nameof(IdTenant))]
    public virtual Tenant? Tenant { get; set; }
}

public static class TipoNotificacion
{
    public const string Vencimiento = "VENCIMIENTO";
    public const string Alerta = "ALERTA";
    public const string Info = "INFO";
    public const string Sistema = "SISTEMA";
}

public static class PrioridadNotificacion
{
    public const string Baja = "Baja";
    public const string Normal = "Normal";
    public const string Alta = "Alta";
    public const string Critica = "Critica";
}
