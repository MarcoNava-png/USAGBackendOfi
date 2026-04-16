namespace WebApplication2.Core.Models;

public class SolicitudBaja : BaseEntity
{
    public int IdSolicitudBaja { get; set; }
    public int IdEstudiante { get; set; }
    public string? Matricula { get; set; }
    public string? NombreEstudiante { get; set; }
    public string? Carrera { get; set; }
    public int? TipoBaja { get; set; }
    public int? EstadoBaja { get; set; }
    public string? MotivoBaja { get; set; }
    public decimal MontoAdeudo { get; set; }
    public int RecibosVencidos { get; set; }
    public int RecibosPendientes { get; set; }
    public string EstatusSolicitud { get; set; } = "Pendiente";
    public string? SolicitadoPor { get; set; }
    public string? AutorizadoPor { get; set; }
    public string? ComentarioFinanzas { get; set; }
    public DateTime? FechaAutorizacion { get; set; }
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

    public virtual Estudiante IdEstudianteNavigation { get; set; } = null!;
}
