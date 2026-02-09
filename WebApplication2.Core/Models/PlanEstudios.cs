namespace WebApplication2.Core.Models;

public partial class PlanEstudios : BaseEntity
{
    public int IdPlanEstudios { get; set; }

    public string ClavePlanEstudios { get; set; } = null!;

    public string? NombrePlanEstudios { get; set; }

    public string? RVOE { get; set; }

    public bool? PermiteAdelantar { get; set; }

    public string? Version { get; set; }

    public int? IdProgramaEstudios { get; set; }

    public int? DuracionMeses { get; set; }

    public int MinimaAprobatoriaParcial { get; set; }

    public int MinimaAprobatoriaFinal { get; set; }

    public int IdPeriodicidad { get; set; }

    public int IdNivelEducativo { get; set; }

    public int IdCampus { get; set; }

    public virtual ICollection<Aspirante> Aspirante { get; set; } = new List<Aspirante>();

    public virtual ICollection<ConvenioAlcance> ConvenioAlcance { get; set; } = new List<ConvenioAlcance>();

    public virtual ICollection<Estudiante> Estudiante { get; set; } = new List<Estudiante>();

    public virtual ICollection<EstudiantePlan> EstudiantePlan { get; set; } = new List<EstudiantePlan>();

    public virtual ICollection<Grupo> Grupo { get; set; } = new List<Grupo>();

    public virtual Campus IdCampusNavigation { get; set; } = null!;

    public virtual NivelEducativo IdNivelEducativoNavigation { get; set; } = null!;

    public virtual Periodicidad IdPeriodicidadNavigation { get; set; } = null!;

    public virtual ICollection<MateriaPlan> MateriaPlan { get; set; } = new List<MateriaPlan>();
}
