namespace WebApplication2.Core.Models;

public partial class Estudiante : BaseEntity
{
    public int IdEstudiante { get; set; }

    public string Matricula { get; set; } = null!;

    public int IdPersona { get; set; }

    public string? Email { get; set; }

    public DateOnly FechaIngreso { get; set; }

    public int? IdPlanActual { get; set; }

    public bool Activo { get; set; }

    public string? UsuarioId { get; set; }

    public virtual ICollection<EstudiantePlan> EstudiantePlan { get; set; } = new List<EstudiantePlan>();

    public virtual Persona IdPersonaNavigation { get; set; } = null!;

    public virtual PlanEstudios? IdPlanActualNavigation { get; set; }

    public virtual ICollection<Inscripcion> Inscripcion { get; set; } = new List<Inscripcion>();

    public virtual ICollection<EstudianteGrupo> EstudianteGrupo { get; set; } = new List<EstudianteGrupo>();

    public virtual ApplicationUser Usuario { get; set; }
}
