namespace WebApplication2.Core.Models;

public partial class Periodicidad
{
    public int IdPeriodicidad { get; set; }

    public string DescPeriodicidad { get; set; } = null!;

    public byte PeriodosPorAnio { get; set; }

    public byte MesesPorPeriodo { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<PeriodoAcademico> PeriodoAcademico { get; set; } = new List<PeriodoAcademico>();

    public virtual ICollection<PlanEstudios> PlanEstudios { get; set; } = new List<PlanEstudios>();
}
