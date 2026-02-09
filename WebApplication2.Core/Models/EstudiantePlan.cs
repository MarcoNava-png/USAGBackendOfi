namespace WebApplication2.Core.Models;

public partial class EstudiantePlan : BaseEntity
{
    public int IdEstudiantePlan { get; set; }

    public int IdEstudiante { get; set; }

    public int IdPlanEstudios { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public virtual Estudiante IdEstudianteNavigation { get; set; } = null!;

    public virtual PlanEstudios IdPlanEstudiosNavigation { get; set; } = null!;
}