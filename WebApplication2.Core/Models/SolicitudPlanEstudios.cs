namespace WebApplication2.Core.Models;

public class SolicitudPlanEstudios : BaseEntity
{
    public int IdSolicitudPlanEstudios { get; set; }
    public int IdPlanEstudios { get; set; }
    public string ClavePlanEstudios { get; set; } = null!;
    public string NombrePlanEstudios { get; set; } = null!;
    public string? Campus { get; set; }
    public string? Rvoe { get; set; }
    public string EstatusSolicitud { get; set; } = "Pendiente";
    public string SolicitadoPor { get; set; } = null!;
    public string? AprobadoPor { get; set; }
    public string? ComentarioRevision { get; set; }
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    public DateTime? FechaResolucion { get; set; }

    public virtual PlanEstudios IdPlanEstudiosNavigation { get; set; } = null!;
}
