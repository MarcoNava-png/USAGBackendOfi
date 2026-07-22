namespace WebApplication2.Core.Models;

public class PreInscripcion : BaseEntity
{
    public int IdPreInscripcion { get; set; }
    public int IdEstudiante { get; set; }
    public int IdPlanEstudios { get; set; }
    public int IdPeriodoAcademicoDestino { get; set; }
    public byte NumeroCuatrimestreObjetivo { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? Nota { get; set; }
    public DateTime FechaApartado { get; set; } = DateTime.UtcNow;

    public virtual Estudiante IdEstudianteNavigation { get; set; } = null!;
    public virtual PlanEstudios IdPlanEstudiosNavigation { get; set; } = null!;
    public virtual PeriodoAcademico IdPeriodoAcademicoDestinoNavigation { get; set; } = null!;
}
